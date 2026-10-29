// w_PetActionGuardian.h: an OriginSix Guardian mount which walks beside its owner while it's not ridden.
//////////////////////////////////////////////////////////////////////
#pragma once

#include "w_PetAction.h"
#include "Render/Models/ZzzBMD.h"

SmartPointer(PetActionGuardian);

class PetActionGuardian : public PetAction
{
public:
    static PetActionGuardianPtr Make();
    virtual ~PetActionGuardian();

public:
    virtual bool Move(OBJECT* obj, CHARACTER* Owner, int targetKey, double tick, bool bForceRender);

private:
    PetActionGuardian();
};
