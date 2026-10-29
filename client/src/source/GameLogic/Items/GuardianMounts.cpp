#include "stdafx.h"

#include "GameLogic/Items/GuardianMounts.h"

#include "Engine/Object/ZzzInventory.h"
#include "Engine/Object/ZzzCharacter.h"
#include "Engine/Object/ZzzObject.h"
#include "Render/Terrain/ZzzLodTerrain.h"
#include "Render/Sprites/GlobalBitmap.h"
#include "Render/Effects/ZzzEffect.h"
#include "Render/Textures/ZzzOpenglUtil.h"
#include "Core/Text/Utf8.h"

#include <algorithm>
#include <array>
#include <string>
#include <vector>
#include "UI/Inventory/InventoryCtrl.h"
#include "UI/Inventory/MyInventory.h"
#include "UI/Core/WindowSystem.h"

namespace GameLogic::Items
{
namespace
{
// When the hero last mounted or dismounted a Guardian (GetTickCount), 0 if never.
unsigned int LastRideToggle = 0;

// The glow layer of each mesh of each Guardian model, or BITMAP_UNKNOWN.
std::array<std::vector<GLuint>, 9> GuardianGlowTextures;

// The glowing meshes of the S18 models: the Ghost Horse's emissive parts and marks, the Ice Dragon's
// wings and the Ullr's seal and crystals. They are drawn bright over themselves.
bool IsGlowingMesh(const std::wstring& textureFileName)
{
    std::wstring name = textureFileName;
    for (wchar_t& c : name)
    {
        c = static_cast<wchar_t>(towlower(c));
    }

    for (const wchar_t* part : {L"emissive", L"horsemark", L"_wing", L"urseal", L"ursh"})
    {
        if (name.find(part) != std::wstring::npos)
        {
            return true;
        }
    }

    return false;
}

// The glow layer of a mesh: the S18 models have <texture>_render beside a texture.
GLuint LoadGlowTexture(const std::wstring& textureFileName, GLuint meshTexture)
{
    if (IsGlowingMesh(textureFileName))
    {
        return meshTexture;
    }

    const size_t dot = textureFileName.find_last_of(L'.');
    const std::wstring stem = textureFileName.substr(0, dot);
    for (const std::wstring& candidate : {stem + L"_render.jpg", stem + L"_render.tga"})
    {
        const GLuint index = Bitmaps.LoadImage(L"Data\\Skill\\" + candidate, GL_LINEAR, GL_REPEAT);
        if (index != BITMAP_UNKNOWN)
        {
            return index;
        }
    }

    return BITMAP_UNKNOWN;
}
} // namespace

// The glow layers of a Guardian model, loaded once its model is: models may load after the data.
const std::vector<GLuint>& GetGlowTextures(int model)
{
    const BMD& b = Models[model];
    std::vector<GLuint>& glow = GuardianGlowTextures[model - MODEL_GUARDIAN_GHOST_HORSE];
    if (static_cast<int>(glow.size()) != b.NumMeshs)
    {
        glow.assign(b.NumMeshs, BITMAP_UNKNOWN);
        for (int i = 0; i < b.NumMeshs; ++i)
        {
            glow[i] = LoadGlowTexture(Core::Text::FromUtf8(b.Textures[i].FileName), b.IndexTexture[i]);
        }
    }

    return glow;
}

void LoadGuardianGlowTextures()
{
    for (int model = MODEL_GUARDIAN_GHOST_HORSE; model <= MODEL_GUARDIAN_ULLR_RARE; ++model)
    {
        GetGlowTextures(model);
    }
}

ITEM* FindSummonedGuardian(int* index)
{
    if (g_pMyInventory == nullptr)
    {
        return nullptr;
    }

    mu::ui::window::CInventoryCtrl* inventory = g_pMyInventory->GetInventoryCtrl();
    if (inventory == nullptr)
    {
        return nullptr;
    }

    for (size_t i = 0; i < inventory->GetNumberOfItems(); ++i)
    {
        ITEM* item = inventory->GetItem(static_cast<int>(i));
        if (item != nullptr && IsGuardianMountItem(item->Type) && item->HasSkill)
        {
            if (index != nullptr)
            {
                *index = inventory->GetIndexByItem(item);
            }

            return item;
        }
    }

    return nullptr;
}

int GetRiddenGuardianModel(const OBJECT* rider)
{
    for (int i = 0; i < MAX_MOUNTS; ++i)
    {
        const OBJECT& mount = Mounts[i];
        if (mount.Live && mount.Owner == rider && IsGuardianMountModel(mount.Type))
        {
            return mount.Type;
        }
    }

    return -1;
}

bool IsRidingGuardianMount(const OBJECT* rider)
{
    return GetRiddenGuardianModel(rider) >= 0;
}

float GetGuardianRiderRaise(const OBJECT* rider)
{
    for (int i = 0; i < MAX_MOUNTS; ++i)
    {
        const OBJECT& mount = Mounts[i];
        if (mount.Live && mount.Owner == rider && IsGuardianMountModel(mount.Type) && mount.Alpha > 0.01f)
        {
            // The Fenrir walking and running poses already lift the rider for the Fenrir's gait.
            constexpr float MovingLift = 45.f;
            const int action = rider->CurrentAction;
            const bool moving = (action >= PLAYER_FENRIR_WALK && action <= PLAYER_FENRIR_WALK_ONE_LEFT)
                                || (action >= PLAYER_FENRIR_RUN && action <= PLAYER_FENRIR_RUN_ONE_LEFT_ELF)
                                || (action >= PLAYER_RAGE_FENRIR_WALK && action <= PLAYER_RAGE_FENRIR_WALK_TWO_SWORD)
                                || (action >= PLAYER_RAGE_FENRIR_RUN && action <= PLAYER_RAGE_FENRIR_RUN_ONE_LEFT);
            const float raise = GetGuardianRiderRaise(mount.Type);
            return moving ? std::max(0.f, raise - MovingLift) : raise;
        }
    }

    return 0.f;
}

void RenderGuardianModel(OBJECT* o, int state)
{
    RenderObject(o, false, 0, state);
    if (state != 0 || o->Alpha <= 0.01f)
    {
        return;
    }

    // The model's own glow layers, drawn bright over it and pulsing slowly.
    BMD* b = &Models[o->Type];
    const float pulse = 0.75f + 0.25f * static_cast<float>(sin(WorldTime * 0.003));
    const std::vector<GLuint>& glow = GetGlowTextures(o->Type);
    // Unlit, so the glow is as bright as its texture in any light.
    Vector(1.f, 1.f, 1.f, b->BodyLight);
    const bool lightEnable = b->LightEnable;
    b->LightEnable = false;
    b->BeginRender(o->Alpha);

    // The S18 renderer shows the Guardians brighter than the S6 lighting: their own textures, added
    // over them, lift the skin. The Fierce Lions look right as they are, with their _render layers.
    static constexpr float Brightening[] = {0.3f, 0.3f, 0.3f, 0.05f, 0.05f, 0.3f, 0.3f, 0.3f, 0.3f};
    const float brightening = Brightening[o->Type - MODEL_GUARDIAN_GHOST_HORSE] * o->Alpha;
    if (brightening > 0.f)
    {
        Vector(brightening, brightening, brightening, b->BodyLight);
        b->RenderBody(RENDER_TEXTURE | RENDER_BRIGHT, 1.f, -1, 1.f, 0.f, 0.f, -1);
        Vector(1.f, 1.f, 1.f, b->BodyLight);
    }

    for (int i = 0; i < b->NumMeshs && i < static_cast<int>(glow.size()); ++i)
    {
        if (glow[i] != BITMAP_UNKNOWN)
        {
            // Twice, for the strong glow of the S18 models.
            for (int pass = 0; pass < 2; ++pass)
            {
                b->RenderMesh(i, RENDER_TEXTURE | RENDER_BRIGHT, o->Alpha * pulse, -1, 1.f, 0.f, 0.f, static_cast<int>(glow[i]));
            }
        }
    }

    // The aura: a soft chrome sheen in the Guardian's color over the whole model, pulsing with the glow.
    static constexpr float AuraColors[][3] = {
        {0.25f, 0.55f, 1.f},  // Ghost Horse: cyan
        {0.3f, 0.7f, 1.f},    // Ice Dragon: ice blue
        {0.6f, 0.35f, 1.f},   // Ice Dragon (Rare): violet
        {1.f, 0.5f, 0.15f},   // Fierce Lion: orange
        {1.f, 0.2f, 0.2f},    // Fierce Lion (Rare): red
        {1.f, 0.6f, 0.2f},    // Shining Tail: amber
        {0.45f, 0.4f, 1.f},   // Shining Tail (Rare): blue violet
        {0.2f, 0.55f, 1.f},   // Ullr: blue
        {1.f, 0.45f, 0.1f},   // Ullr (Rare): ember
    };
    constexpr float AuraStrength = 0.45f;
    const float* color = AuraColors[o->Type - MODEL_GUARDIAN_GHOST_HORSE];
    const float aura = AuraStrength * pulse * o->Alpha;
    Vector(color[0] * aura, color[1] * aura, color[2] * aura, b->BodyLight);
    b->RenderBody(RENDER_CHROME | RENDER_BRIGHT, 1.f, -1, 1.f, o->BlendMeshTexCoordU, o->BlendMeshTexCoordV, -1, BITMAP_CHROME);
    b->EndRender();
    b->LightEnable = lightEnable;

    // The aura lights: soft lights in the Guardian's color along its body, like the S18 Guardians' aura.
    constexpr int AuraLights = 12;
    constexpr float AuraLightSize = 2.2f;
    constexpr float AuraLightStrength = 0.55f;
    VectorCopy(o->Position, b->BodyOrigin);
    b->BodyScale = o->Scale;
    b->CurrentAction = o->CurrentAction;
    b->Animation(BoneTransform, o->AnimationFrame, o->PriorAnimationFrame, o->PriorAction, o->Angle, o->HeadAngle);
    vec3_t light;
    const float strength = AuraLightStrength * pulse * o->Alpha;
    Vector(color[0] * strength, color[1] * strength, color[2] * strength, light);
    vec3_t relative = {0.f, 0.f, 0.f};
    vec3_t position;
    const int step = std::max(1, b->NumBones / AuraLights);
    for (int bone = 0; bone < b->NumBones; bone += step)
    {
        if (b->Bones[bone].Dummy)
        {
            continue;
        }

        b->TransformPosition(BoneTransform[bone], relative, position, false);
        CreateSprite(BITMAP_LIGHT, position, AuraLightSize, light, o);
    }
}

bool ToggleGuardianRiding()
{
    int index = -1;
    if (FindSummonedGuardian(&index) == nullptr || GetGuardianRideCooldownFraction() > 0.f)
    {
        return false;
    }

    // Using the summoned Guardian on itself asks the server to mount or dismount it.
    SendRequestUse(index, index);
    LastRideToggle = GetTickCount();
    return true;
}

float GetGuardianRideCooldownFraction()
{
    if (LastRideToggle == 0)
    {
        return 0.f;
    }

    const unsigned int elapsed = GetTickCount() - LastRideToggle;
    return elapsed >= GuardianRideCooldownMs ? 0.f : 1.f - static_cast<float>(elapsed) / GuardianRideCooldownMs;
}

bool IsHeroRidingGuardian()
{
    return Hero != nullptr && Hero->Helper.Type == MODEL_HORN_OF_FENRIR && FindSummonedGuardian() != nullptr;
}
} // namespace GameLogic::Items
