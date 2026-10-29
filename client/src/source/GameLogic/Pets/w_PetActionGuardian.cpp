// w_PetActionGuardian.cpp: implementation of the PetActionGuardian class.
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "w_PetActionGuardian.h"
#include "Engine/AI/ZzzAI.h"
#include "Engine/Object/ZzzCharacter.h"
#include "GameLogic/Items/GuardianMounts.h"
#include "Render/Terrain/ZzzLodTerrain.h"

#include <algorithm>

namespace
{
// The Guardian follows its owner on the owner's own path (beside it, it could stand off a bridge or in
// a wall): it stays this far behind, starts walking when the owner is farther than the start distance,
// and stops when it is back at the follow distance, so it does not flicker between the two.
constexpr float FollowDistance = 170.f;
constexpr float StartDistance = 240.f;
// A walking Guardian stops within this of its follow distance (it never gets there exactly).
constexpr float StopTolerance = 10.f;
// Farther than this (e.g. after a teleport), it appears behind its owner.
constexpr float JumpDistance = 1200.f;
// It moves as far as its owner did in the frame, plus this part of the gap to close it smoothly.
constexpr float CatchUp = 0.05f;
constexpr float MinimumSpeed = 2.f;
// An owner step longer than this (per frame) is a teleport or a first frame, not a move.
constexpr float MaximumOwnerStep = 40.f;
// Without a moving owner, it runs to catch up only when it is this far behind.
constexpr float CatchUpRunGap = 500.f;

// Whether the owner runs: its run actions, on foot or riding.
bool IsRunning(const OBJECT* owner)
{
    return (owner->CurrentAction >= PLAYER_RUN && owner->CurrentAction <= PLAYER_RUN_RIDE_WEAPON)
           || owner->CurrentAction == PLAYER_RUN_RIDE_HORSE || owner->CurrentAction == PLAYER_RUN_TWO_HAND_SWORD_TWO
           || (owner->CurrentAction >= PLAYER_FENRIR_RUN && owner->CurrentAction <= PLAYER_FENRIR_RUN_ONE_LEFT_ELF);
}
} // namespace

PetActionGuardianPtr PetActionGuardian::Make()
{
    PetActionGuardianPtr temp(new PetActionGuardian);
    return temp;
}

PetActionGuardian::PetActionGuardian() {}

PetActionGuardian::~PetActionGuardian() {}

bool PetActionGuardian::Move(OBJECT* obj, CHARACTER* Owner, int targetKey, double tick, bool bForceRender)
{
    OBJECT* owner = obj->Owner;
    if (owner == nullptr)
    {
        return FALSE;
    }

    // How far the owner moved since the last frame (the start position keeps its last position).
    float ownerStep = sqrtf((owner->Position[0] - obj->StartPosition[0]) * (owner->Position[0] - obj->StartPosition[0])
                            + (owner->Position[1] - obj->StartPosition[1]) * (owner->Position[1] - obj->StartPosition[1]));
    if (ownerStep > MaximumOwnerStep)
    {
        ownerStep = 0.f;
    }
    VectorCopy(owner->Position, obj->StartPosition);

    const float dx = owner->Position[0] - obj->Position[0];
    const float dy = owner->Position[1] - obj->Position[1];
    const float distance = sqrtf(dx * dx + dy * dy);

    int action = GameLogic::Items::GuardianActionStand;
    if (distance > JumpDistance)
    {
        // Behind the owner, on the way it came.
        vec3_t back = {0.f, FollowDistance, 0.f};
        vec3_t ownerAngle = {0.f, 0.f, owner->Angle[2]};
        float matrix[3][4];
        vec3_t offset;
        AngleMatrix(ownerAngle, matrix);
        VectorRotate(back, matrix, offset);
        VectorAdd(owner->Position, offset, obj->Position);
        obj->Angle[2] = owner->Angle[2];
    }
    else if (distance > (obj->CurrentAction == GameLogic::Items::GuardianActionStand ? StartDistance : FollowDistance + StopTolerance))
    {
        const float gap = distance - FollowDistance;
        // It walks when its owner walks and runs when its owner runs.
        const bool run = ownerStep > 0.f ? IsRunning(owner) : gap > CatchUpRunGap;
        action = run ? GameLogic::Items::GuardianActionRun : GameLogic::Items::GuardianActionWalk;
        const float step = std::min(gap, ownerStep + std::max(MinimumSpeed, gap * CatchUp) * FPS_ANIMATION_FACTOR);
        obj->Position[0] += dx / distance * step;
        obj->Position[1] += dy / distance * step;
        obj->Angle[2] = TurnAngle2(obj->Angle[2], CreateAngle2D(obj->Position, owner->Position), 10.f * FPS_ANIMATION_FACTOR);
    }

    obj->Position[2] = RequestTerrainHeight(obj->Position[0], obj->Position[1]);
    if (obj->CurrentAction != action)
    {
        SetAction(obj, action);
    }

    obj->Velocity = action == GameLogic::Items::GuardianActionStand ? 0.25f : (action == GameLogic::Items::GuardianActionRun ? 0.5f : 0.4f);
    return TRUE;
}
