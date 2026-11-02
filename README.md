# SORA: After the Rain

**Status:** pre-production / Unity interaction prototype, **not yet Editor-compiled**.  
**Platform:** Windows PC first. **Unity:** `6000.3.7f1` (Unity 6.3 LTS), URP package `17.3.0`.  
**Working language:** Vietnamese (design) / English (code identifiers).

## Concept

Tokyo slice-of-life / convenience-store management / narrative social simulation. Twin first-year university students **Tei Toshiharu (丁俊陽)** and **Tei Akiko (明子)** (both 18 at game start) have recently moved from the countryside to Tokyo. Both work part-time at a convenience store, hoping to pay bills and meet someone who could change their financial future. Romance/drama grows naturally out of customer interactions and workplace chores.

## What exists

- Two controllable placeholder characters and camera; swap with `Tab`.
- Primitive convenience store/night street scene generator.
- Early NPC/work logic, POS, inventory, dialogue, quicksave.
- Smoke/build scripts and static structure validation.

**Important:** This is a SOURCE prototype, not a tested playable `.exe`. It has NOT been compiled or visually reviewed in Unity Editor. It currently has primitive avatars (not production-quality anime models).

## Quick start (Windows)

1. Install Unity Hub and Unity Editor version **6000.3.7f1**; activate your personal Unity license using Hub.
2. Clone: `git clone https://github.com/haizzdungnay/SORA-After-the-Rain.git`. Install Git LFS before adding art binaries.
3. Add the cloned folder as a project in Unity Hub; open with the pinned version.
4. In Unity, run **SORA > 0. Configure Prototype Project** to create URP assets/settings, then run **SORA > 1. Generate Demo Scene**, open `Assets/SORA/Scenes/SORA_Prototype.unity` and press Play.
5. If Input issues arise, set **Active Input Handling** to **Both** or **Input Manager (Old)** as instructed in `README_START_HERE.md`.
6. Run offline check `python Tools/validate_project.py`. This does not compile C#.

See `README_START_HERE.md` for import troubleshooting, smoke test and Windows build scripts.

## Repo layout

```text
Assets/SORA/Scripts/         Gameplay and Editor scene generator
Packages/                   Unity package manifest
ProjectSettings/            Unity version lock
Docs/GAME_DESIGN_DOCUMENT_v0.4.md  Canonical narrative/design baseline
Docs/CONTRIBUTING_AND_AI.md AI collaboration and branch conventions
Docs/ROADMAP_GITHUB.md      Next tasks and acceptance gates
Tools/                      Unity setup, smoke/build, structural checks
.github/workflows/          Offline check + optional self-hosted Unity build
```

## Development policy

- `main`: stable reviewed snapshots only; prototype changes on `feature/*` branches and pull requests.
- Do not commit Unity generated folders (`Library`, `Temp`, `Logs`, `Builds`) or credentials, licensing data or unlicensed third-party assets.
- For binary assets (FBX/BLEND/PNG/PSD/WAV/video), use Git LFS after installing it (`git lfs install`), with the tracking setup described in `Docs/CONTRIBUTING_AND_AI.md`.
- AI-generated code/model drafts require Unity import, visual review, license verification and acceptance tests before being treated as complete.

## Quality gate

Offline structure validation runs without Unity. Five EditMode tests are supplied for POS, 100 character swaps, delivery/inventory and separate relations; they have NOT been run in Unity here. Compile, EditMode, Play Mode and Windows build acceptance are pending. See [import report](Docs/IMPORT_REPORT_2026-11-02.md) and [CI setup](Docs/CI_SETUP.md).

The repo is PUBLIC at the owner's request. Public visibility does not grant an open-source license; no redistribution license has been selected. No third-party artwork is included.


Project naming and decisions: [Docs/DECISIONS_2026-11-02.md](Docs/DECISIONS_2026-11-02.md).
