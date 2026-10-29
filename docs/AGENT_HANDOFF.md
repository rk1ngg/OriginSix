# OriginSix — Agent Handoff & Change Ledger (`AGENT_HANDOFF.md`)

> **Instruction for AI Agents:** Read this file first together with [AGENTS.md](../AGENTS.md), [UPSTREAM_STRATEGY.md](UPSTREAM_STRATEGY.md), and [GUARDIAN_RUUD_UI_PLAN.md](GUARDIAN_RUUD_UI_PLAN.md). Update this file after every completed milestone, upstream sync, or feature step so the next agent has full context.

**Last Updated:** 2026-10-28

---

## 1. Repository & Upstream Snapshot Status

`D:\OriginSix` is a **single Git monorepo** (no nested `.git` directories inside `server/`, `client/`, or `web/`).

| Directory | Upstream Source | Previous Baseline SHA | Current Baseline SHA (Updated 2026-10-28) | Summary of Upstream Changes Pulled |
|---|---|---|---|---|
| `server/` | `https://github.com/MUnique/OpenMU` (`master`) | `cc0ccd268d70303b1b65bce58b178afcb1437fd5` | `c5f6b380ac25ffeee0bf6ee15a5873e8cbd654b6` | **48 merged PRs / 380 commits:** Monster roaming/aggro fixes (`#1322`, `#1316`), Item stat profile formula support (`#1321`), Chaos Mix client success rate & price sync (`#1300`), Illusion Temple (`#1318`, `#1320`), Doppelganger (`#1317`), Kalima (`#1314`), Imperial Guardian (`#1285`), Lucky Items (`#1315`), and EF Core / Admin Panel updates. |
| `client/` | `https://github.com/sven-n/MuMain` (`main`) | `063169014914add35a777178b15d8c9fbaae4b51` | `55a7b4459c18487f6517fdf75d62a360117cc1ed` | **27 merged PRs / 1,062 commits:** BMD Version 14 (`0x0E`) model decryption via `cryptopp` (`#804`), removal of binary `ItemToolTip*.bmd` in favor of JSON item definitions (`Data/Items/*.json`, `#819`–`#836`, `#863`), `std::chrono` gameplay timers (`#847`), UI & pathfinding fixes (`#829`, `#850`, `#854`). |
| `web/` | `https://github.com/mamfloo/open-mu-web` (`master`) | *(none)* | `093908290dd09e21e0058de0309ee1df413c8aee` | Imported Next.js 14 + Prisma + NextAuth OpenMU web portal directly connected to the OpenMU PostgreSQL schema (`account`, `guild`, `data`, `cfg`). |

---

## 2. Local Runtime & Loopback Port Map

All local services bind strictly to loopback addresses:

| Service | Loopback Address / Port | Start / Management Script | Notes |
|---|---|---|---|
| **PostgreSQL 16 (`originsix-local-database-1`)** | `127.0.0.1:5434` (`-> 5432`) | `& D:\OriginSix\scripts\local-server.ps1 -Action Start` | Credentials stored encrypted via Windows DPAPI in `D:\OriginSix\.local\credentials.clixml` (never in plaintext `.env`). |
| **OpenMU Server (`originsix-local-openmu-1`)** | Admin: `http://127.0.0.1:8080`<br>Connect: `127.127.127.127:44406`<br>Game: `127.127.127.127:45901..45906`<br>Chat: `127.127.127.127:45980` | `& D:\OriginSix\scripts\local-server.ps1 -Action Start`<br>Smoke test: `& D:\OriginSix\scripts\test-local-server.ps1` | Running local Docker image **`originsix-openmu:upstream-20261028`** (built from `server/` @ `c5f6b380ac25ffeee0bf6ee15a5873e8cbd654b6` + local anti-cheat mount fix). |
| **OriginSix Web CMS (`web/`)** | `http://127.0.0.1:3080` | `& D:\OriginSix\scripts\local-web.ps1 -Action Dev -Port 3080` | Generates `DATABASE_URL` and `NEXTAUTH_SECRET` in memory at runtime from `.local/credentials.clixml` and `.local/web-secret.clixml`. |
| **MuMain Client (`client/`)** | Connects to `127.127.127.127:44406` | `D:\OriginSix\scripts\play-local.cmd` | Binary at `D:\OriginSix\client\out\build\windows-x64\src\Release\Main.exe`. |

---

## 3. Local Customizations & External Assets Added

### A. Server Customizations (`server/`)
1. **Mount Speed-Hack Anti-Cheat Fix (`server/src/GameLogic/PlugIns/SpeedHackDetectPlugIn.cs:234`):**
   - Scales `WalkSpeedToleranceMs` by `Math.Max(1.0, scaling)` when a player is riding a mount (`Uniria`, `Dinorant`, `Dark Horse`, `Fenrir`, or future Guardians) so mounted movement does not trigger false-positive speed-hack kicks.
   - Verified by `SpeedHackAntiCheatTests` (`22/22 passed`). Documented in [ANTICHEAT_MOUNT_FIX_2026-10-22.md](ANTICHEAT_MOUNT_FIX_2026-10-22.md).

### B. Client Customizations (`client/`)
1. **BMD Version 15 (`0x0F`, LEA-256 ECB) Model Loader (`client/src/source/Render/Models/ZzzBMD.cpp`):**
   - Upstream `sven-n/MuMain` PR `#804` added BMD Version 14 (`0x0E`, ModulusCipher).
   - Because official Webzen Season 14–20 Guardian `.bmd` files in `MU_Red_1_20_61_Full.zip` use **BMD Version 15 (`0x0F`, LEA-256 ECB)**, we added `DecryptLea256Body` (`Version == 0xF`) in `ZzzBMD.cpp`.
   - Verified with `D:\OriginSix\client\out\build\windows-x64\tests\data\Release\model_files_data_tests.exe`: **12/12 test cases passed (`1,423/1,423` assertions passed)**.
2. **125 Extracted Guardian, Seal & Priest James Assets (`client/src/bin/Data/`):**
   - **Source Archive:** `https://github.com/vanessagelra-cell/mu-data/releases/download/v1.0.0/MU_Red_1_20_61_Full.zip` (extracted selectively via HTTP Byte-Range requests; **0** existing Season 6 files were overwritten).
   - **Models & Textures Included:**
     - **1st Guardian (`Ghost Horse`):** `Data/Skill/ghost_horse.bmd`, `Data/Item/Ghosthorse_item_shield.bmd`, `Data/Item/Ghosthorse_item_mask01.bmd`, plus `Ghost_horse_*.OZJ`, `Ghoshorsemark*.OZJ`, `gra3.OZJ`.
     - **2nd Guardian (`Ice Dragon` & `Rare`):** `Data/Skill/icedragon.bmd`, `Data/Skill/icedragon_rare.bmd`, `Data/Item/hs_2GuadianiceCA.bmd`, `Data/Item/hs_2GuadianiceCB.bmd`, `Data/Item/hs_2GuadianiceD.bmd` (Seal), plus `hs_2Guadianice*.OZJ/.OZT`.
     - **3rd Guardian (`Fierce Lion` & `Rare`):** `Data/Skill/fiercelion.bmd`, `Data/Skill/fiercelionR.bmd`, `Data/Item/hs_fiercelion_inB.bmd`, `Data/Item/hs_fiercelion_inR.bmd`, `Data/Item/hs_3Guadianice.bmd` (Seal), plus `hs_fiercelion*.OZJ/.OZT`, `hs_3Guadianice.OZJ`.
     - **4th Guardian (`Shining Tail` & `Rare`):** `Data/Skill/shiningtails.bmd`, `Data/Skill/rare_shiningtails.bmd`, `Data/Item/shiningtails_in.bmd`, `Data/Item/rare_shiningtails_in.bmd`, `Data/Item/tail_inzangmdle.bmd` (Seal), plus `shiningtails*.OZJ/.OZT`, `rare_shiningtails*.OZJ/.OZT`, `rubber_tail*.OZJ`.
     - **5th Guardian (`Ullr` / `Ur` & `Rare`):** `Data/Skill/Ur.bmd`, `Data/Skill/UrUp.bmd`, `Data/Item/ur_inven.bmd`, `Data/Item/urUp_inven.bmd`, `Data/Item/urseal.bmd` (Seal), plus `urseal*.OZJ`, `ursh*.OZJ`, `goms5cha_*.OZJ`, `gom5cha_saddle*.OZJ`.
     - **Priest James NPC & Ruud UI:** `Data/NPC/james.bmd`, `Data/NPC/NPCjames.OZJ`, `Data/Item/James_anvil.bmd`, `Data/Item/Jamesanvil_6master.bmd`, `Data/Interface/RuudShop2DItem_1.OZJ`, `Data/Interface/RuudShopTierType.OZJ`.
   - **How to Remove/Rollback the 125 Asset Files (if needed):**
     - All 125 files are standalone additions in `Data/Skill/`, `Data/Item/`, `Data/NPC/`, and `Data/Interface/`. While untracked, `git clean -fd client/src/bin/Data/Skill client/src/bin/Data/Item client/src/bin/Data/NPC` (after reviewing with `git clean -nd`) removes them immediately without touching any Season 6 files.

### C. Web CMS Customizations (`web/`)
1. **MurenMU (`WeiCMS`) Dark Obsidian & Arcane Gold Redesign:**
   - Redesigned `mamfloo/open-mu-web` in `D:\OriginSix\web\app\` to match the layout and theme of `https://murenmu.com/` (`Cinzel` + `Barlow` typography, `#0b0d13` obsidian background, `#d4af37` arcane gold accents, 3-column layout with Server Status, Top Players, Boss Timers, Castle Siege widget, and starter news fallback when `data.OpenMuWeb_News` table is not present).

---

## 4. Where We Are Now & Next Steps ([GUARDIAN_RUUD_UI_PLAN.md](GUARDIAN_RUUD_UI_PLAN.md))

### Completed:
- [x] Verified official Webzen Guardian tiers 1–5 (`Ghost Horse` → `Ice Dragon` → `Fierce Lion` → `Shining Tail` → `Ullr`), Priest James (`Elveland 32, 239`, `50,000 Ruud` per Seal), and Trainer (`Lorencia 122, 110`) combination rules (`+11=60%`..`+15=100%`, `20 Bless + 20 Soul + 1 Chaos + 1 Creation`).
- [x] Updated `server/` (`MUnique/OpenMU`) and `client/` (`sven-n/MuMain`) to latest upstream `master`/`main` and rebuilt `originsix-openmu:upstream-20261028`.
- [x] Added BMD Version 15 (`0x0F`, LEA-256 ECB) support to `ZzzBMD.cpp` and extracted/verified all 125 Guardian, Seal, and Priest James 3D models and textures (`1,423/1,423` assertions passed in `model_files_data_tests.exe`).

### Next Steps (Pending Implementation):
1. **Phase 2 & 3 — S6-Compatible Guardian & Currency Data Table:**
   - Define exact Season 6 item indices (Group 13 helper indices that do not collide with existing S6 items), S6 level requirements (within the S6 level 400 cap), S6 stat bonuses (Movement Speed, Defense, Damage Reduction / Attack boost scaled for S6), and Ruud currency storage.
2. **Phase 4 — Server Implementation (`server/`):**
   - Register the 5 Guardians, Rare variants, and 5 Seals in `server/src/Persistence/Initialization/VersionSeasonSix/Items/Pets.cs` (gated behind a configuration flag, default OFF until paired).
   - Add Trainer Guardian upgrade crafting logic in `ChaosMixes.cs` / crafting handler (`+11~+15` previous Guardian + Seal + 20 Bless + 20 Soul + 1 Chaos + 1 Creation; reset to `+0` on failure).
   - Add Ruud currency field & Priest James Ruud Shop (`50,000 Ruud` per Seal).
3. **Phase 5 & 6 — Client Implementation (`client/`):**
   - Register paired item definitions and tooltips in `client/src/bin/Data/Items/Group13_Helper.json`.
   - Wire Guardian mount rendering (`ghost_horse.bmd`, `icedragon.bmd`, `fiercelion.bmd`, `shiningtails.bmd`, `Ur.bmd` + Rare variants) and inventory preview models (`Ghosthorse_item_shield.bmd`, etc.).
   - Add S18-style Guardian equipment slot & Ruud Shop RmlUi interface.
