#include "stdafx.h"

#include "doctest.h"

#include "Render/Models/ZzzBMD.h"

#include <cmath>
#include <filesystem>
#include <string>

// The Guardian mounts, their inventory items and seals, and Priest James are version 15
// (0x0F, LEA-256) models from the 2024 MU Korea client. They are local assets, not part of
// the repository, so a missing file skips its check instead of failing it.
namespace
{
const std::filesystem::path DataFolder = std::filesystem::path(MU_TEST_DATA_DIR);

struct ExpectedModel
{
    const wchar_t* Folder;
    const wchar_t* File;
    short Meshes;  // 0: not documented, only checked to be positive
    short Bones;
    short Actions;
};

// Mesh, bone and action counts of the mounts and James as recorded in docs/GUARDIAN_RUUD_UI_PLAN.md.
constexpr ExpectedModel GuardianModels[] = {
    {L"Skill", L"ghost_horse.bmd", 8, 108, 8},
    {L"Skill", L"icedragon.bmd", 3, 150, 8},
    {L"Skill", L"icedragon_rare.bmd", 3, 150, 8},
    {L"Skill", L"fiercelion.bmd", 4, 161, 5},
    {L"Skill", L"fiercelionR.bmd", 4, 161, 5},
    {L"Skill", L"shiningtails.bmd", 4, 162, 5},
    {L"Skill", L"rare_shiningtails.bmd", 4, 168, 5},
    {L"Skill", L"Ur.bmd", 7, 142, 5},
    {L"Skill", L"UrUp.bmd", 8, 142, 5},
    {L"NPC", L"james.bmd", 1, 66, 9},
    {L"Item", L"Ghosthorse_item_shield.bmd", 0, 0, 0},
    {L"Item", L"Ghosthorse_item_mask01.bmd", 0, 0, 0},
    {L"Item", L"hs_2GuadianiceCA.bmd", 0, 0, 0},
    {L"Item", L"hs_2GuadianiceD.bmd", 0, 0, 0},
    {L"Item", L"hs_fiercelion_inB.bmd", 0, 0, 0},
    {L"Item", L"hs_3Guadianice.bmd", 0, 0, 0},
    {L"Item", L"shiningtails_in.bmd", 0, 0, 0},
    {L"Item", L"tail_inzangmdle.bmd", 0, 0, 0},
    {L"Item", L"ur_inven.bmd", 0, 0, 0},
    {L"Item", L"urseal.bmd", 0, 0, 0},
};

bool HasFiniteVertices(const BMD& model)
{
    for (int m = 0; m < model.NumMeshs; ++m)
    {
        const Mesh_t& mesh = model.Meshs[m];
        for (int v = 0; v < mesh.NumVertices; ++v)
        {
            const Vertex_t& vertex = mesh.Vertices[v];
            if (vertex.Node < 0 || vertex.Node >= model.NumBones)
            {
                return false;
            }

            for (const float coordinate : vertex.Position)
            {
                if (!std::isfinite(coordinate) || std::fabs(coordinate) > 100000.f)
                {
                    return false;
                }
            }
        }
    }

    return true;
}
} // namespace

TEST_CASE("Guardian models: the version 15 models load with their documented counts and sane vertices [data][models]")
{
    for (const ExpectedModel& expected : GuardianModels)
    {
        const std::filesystem::path file = DataFolder / expected.Folder / expected.File;
        CAPTURE(file.string());
        if (!std::filesystem::exists(file))
        {
            MESSAGE("skipped, not installed: " << file.string());
            continue;
        }

        BMD model;
        const std::wstring folder = (DataFolder / expected.Folder).wstring() + L"/";
        REQUIRE(model.Open2(folder.c_str(), expected.File));
        CHECK(model.Version == 0xF);
        CHECK(model.NumMeshs > 0);
        CHECK(model.NumBones > 0);
        if (expected.Meshes > 0)
        {
            CHECK(model.NumMeshs == expected.Meshes);
            CHECK(model.NumBones == expected.Bones);
            CHECK(model.NumActions == expected.Actions);
        }

        for (int m = 0; m < model.NumMeshs; ++m)
        {
            CHECK(model.Meshs[m].NumVertices > 0);
            CHECK(model.Meshs[m].NumTriangles > 0);
        }

        CHECK(HasFiniteVertices(model));
    }
}
