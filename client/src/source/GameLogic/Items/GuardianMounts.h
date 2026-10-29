#pragma once

// OriginSix Guardian mounts: helper items 13/146 (Ghost Horse) to 13/154 (Ullr Rare). The server
// equips a Guardian in the inventory (its skill flag marks the active one) and shows it to everyone
// in place of the pet. The client rides it like Fenrir: the character's helper is the Horn of
// Fenrir for every riding rule, while the mount object uses the Guardian's own model.

#include "Core/Globals/_enum.h"

// The mount models use the model slots of the unused helper items 13/400 to 13/408, so adding
// them doesn't renumber the model enum.
constexpr int MODEL_GUARDIAN_GHOST_HORSE = MODEL_HELPER + 400;
constexpr int MODEL_GUARDIAN_ICE_DRAGON = MODEL_HELPER + 401;
constexpr int MODEL_GUARDIAN_ICE_DRAGON_RARE = MODEL_HELPER + 402;
constexpr int MODEL_GUARDIAN_FIERCE_LION = MODEL_HELPER + 403;
constexpr int MODEL_GUARDIAN_FIERCE_LION_RARE = MODEL_HELPER + 404;
constexpr int MODEL_GUARDIAN_SHINING_TAIL = MODEL_HELPER + 405;
constexpr int MODEL_GUARDIAN_SHINING_TAIL_RARE = MODEL_HELPER + 406;
constexpr int MODEL_GUARDIAN_ULLR = MODEL_HELPER + 407;
constexpr int MODEL_GUARDIAN_ULLR_RARE = MODEL_HELPER + 408;

typedef struct tagITEM ITEM;
class OBJECT;

namespace GameLogic::Items
{
constexpr int FirstGuardianMountItem = ITEM_HELPER + 146;
constexpr int LastGuardianMountItem = ITEM_HELPER + 154;

inline bool IsGuardianMountItem(int itemType)
{
    return itemType >= FirstGuardianMountItem && itemType <= LastGuardianMountItem;
}

// The mount model of a Guardian item, or -1 if the item is no Guardian.
inline int GetGuardianMountModel(int itemType)
{
    return IsGuardianMountItem(itemType) ? MODEL_GUARDIAN_GHOST_HORSE + (itemType - FirstGuardianMountItem) : -1;
}

inline bool IsGuardianMountModel(int modelType)
{
    return modelType >= MODEL_GUARDIAN_GHOST_HORSE && modelType <= MODEL_GUARDIAN_ULLR_RARE;
}

// The scale of each Guardian model, from the Ghost Horse to the Ullr (Rare). The Shining Tail is the
// standard size; the others are about as large (the Ullr's model is about seven times larger).
constexpr float GuardianScales[] = {0.72f, 1.12f, 1.12f, 1.1f, 1.1f, 1.08f, 1.08f, 0.216f, 0.216f};

// Guardians glow like +15 items, as a model and in the inventory.
constexpr int GuardianGlowLevel = 15;

// The hip height of each Guardian model and of the Fenrir model, unscaled.
constexpr float GuardianHipHeights[] = {150.f, 98.f, 98.f, 101.f, 101.f, 103.f, 103.f, 690.f, 690.f};
constexpr float FenrirHipHeight = 112.f * 0.9f;

inline float GetGuardianScale(int modelType)
{
    return IsGuardianMountModel(modelType) ? GuardianScales[modelType - MODEL_GUARDIAN_GHOST_HORSE] : 1.f;
}

// How far the rider of a Guardian is raised: the riding poses sit at the Fenrir's hip height, so the
// rider of a Guardian with a higher back is raised by the difference.
inline float GetGuardianRiderRaise(int modelType)
{
    if (!IsGuardianMountModel(modelType))
    {
        return 0.f;
    }

    const int index = modelType - MODEL_GUARDIAN_GHOST_HORSE;
    // The saddle is a little above the hips.
    constexpr float SaddleHeight = 20.f;
    const float raise = GuardianHipHeights[index] * GuardianScales[index] - FenrirHipHeight + SaddleHeight;
    return raise > 0.f ? raise : 0.f;
}

// The actions of the Guardian models: 0 (and 1) stand, 2 walk, 3 run and 4 attack.
constexpr int GuardianActionStand = 0;
constexpr int GuardianActionWalk = 2;
constexpr int GuardianActionRun = 3;
constexpr int GuardianActionAttack = 4;

// The Guardian action for a Fenrir action (stand, walk, run, attack, skill, damage), so a ridden
// Guardian follows the Fenrir rules; other mounts keep the Fenrir action.
inline int ToGuardianMountAction(int modelType, int fenrirAction)
{
    constexpr int Actions[] = {GuardianActionStand, GuardianActionWalk, GuardianActionRun, GuardianActionAttack, GuardianActionAttack, GuardianActionStand};
    return IsGuardianMountModel(modelType) && fenrirAction >= 0 && fenrirAction < 6 ? Actions[fenrirAction] : fenrirAction;
}

// The server sends a ridden Guardian with the first Fenrir variant flag; otherwise it walks beside its owner.
constexpr int GuardianRiddenVariant = 1;
constexpr int GuardianRiddenExcellentFlag = 0x01;

// The summoned Guardian in the inventory, or nullptr; index receives its inventory index.
ITEM* FindSummonedGuardian(int* index = nullptr);

// Whether the object rides a Guardian (its mount is one).
bool IsRidingGuardianMount(const OBJECT* rider);

// The model of the Guardian the object rides, or -1.
int GetRiddenGuardianModel(const OBJECT* rider);

// The running speed of each ridden Guardian, for its movement speed option: from the fastest Fenrir's (19)
// for the Ghost Horse up to 21 for the Ullr; a Rare Guardian runs like its normal one.
constexpr float GuardianRunSpeeds[] = {19.f, 19.5f, 19.5f, 20.f, 20.f, 20.5f, 20.5f, 21.f, 21.f};

inline float GetGuardianRunSpeed(int modelType)
{
    return IsGuardianMountModel(modelType) ? GuardianRunSpeeds[modelType - MODEL_GUARDIAN_GHOST_HORSE] : 0.f;
}

// How far the rider is raised: by the Guardian the rider's object rides, or 0.
float GetGuardianRiderRaise(const OBJECT* rider);

// Loads the glow layers of the Guardian models, after their textures.
void LoadGuardianGlowTextures();

// Renders a ridden or walking Guardian with its +15 glow; state is the object state of RenderObject (e.g. cloaking).
void RenderGuardianModel(OBJECT* o, int state);

// Mounting or dismounting takes this long before the Guardian can be mounted or dismounted again.
constexpr unsigned int GuardianRideCooldownMs = 1000;

// The armor of each Guardian while it is summoned, as the server's AddGuardianMountsPlugIn adds it.
constexpr int GuardianArmors[] = {150, 180, 195, 210, 225, 240, 255, 275, 555};

// The attack and wizardry of each Guardian while it is summoned (only the Ullr (Rare) has them).
constexpr int GuardianAttacks[] = {0, 0, 0, 0, 0, 0, 0, 0, 28};
constexpr int GuardianWizardries[] = {0, 0, 0, 0, 0, 0, 0, 0, 28};

// Ctrl+Z: mounts or dismounts the summoned Guardian. Returns false if no Guardian is summoned or
// the ride cooldown runs.
bool ToggleGuardianRiding();

// The part of the ride cooldown which is left, from 1 (just used) to 0 (ready).
float GetGuardianRideCooldownFraction();

// Whether the hero rides the summoned Guardian (the hero's helper is then the Fenrir it rides like).
bool IsHeroRidingGuardian();
} // namespace GameLogic::Items
