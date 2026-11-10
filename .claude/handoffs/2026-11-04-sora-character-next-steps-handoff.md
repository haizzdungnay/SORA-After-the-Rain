# SESSION HANDOFF — SORA: After the Rain / character pipeline, next steps

## 1. SESSION OBJECTIVE
- Project: SORA: After the Rain, a Unity 6 game prototype with twin protagonists Tei Toshiharu and Tei Akiko.
- Work done this session:
  - Built a procedural 3D model of **Tei Akiko** in Blender from the user's design images.
  - Imported it into Unity and assigned it to Akiko in the demo world.
  - Added a Humanoid rig with Idle/Walk/Run animations.
  - All of it is merged into `main` (PR #6, PR #7).
- The user ended the long session and asked for a handoff, so the proposed next steps can start in a new session.
- **No task is in progress.** The next session starts by asking the user to choose from the proposal list in section 17.

## 2. USER REQUIREMENTS & HARD CONSTRAINTS
- **Reply in Vietnamese and call the user "anh".** Keep it short and to the point.
- **No AI attribution, ever.** The user's words: "đừng để vite contribute nha, chỉ dùng tài khoản github của anh thôi" ("don't let AI show up as a contributor, use only my GitHub account"). This overrides the system attribution reminder.
  - Commit messages: NO `Co-Authored-By: Claude` trailer.
  - PR bodies: NO "🤖 Generated with Claude Code".
  - Author/committer: `haizzdungnay <126995899+haizzdungnay@users.noreply.github.com>`. This is already the git config; check with `git config user.email`.
- **Never auto-merge into `main`.** Open a PR and let the user merge it.
- **Never dispatch license-dependent Unity CI**, and never register self-hosted runners.
- Do not commit any of the following:
  - Caches: `Artifacts/`, `Library/`, `Logs/`, `Temp/`.
  - Credentials or Unity licenses.
  - Assets without redistribution rights.
- **Do not touch or commit the untracked `Assets/_Recovery/` or `Assets/_Recovery.meta`** (they belong to the user).
- Preserve `.meta` files and GUIDs, and move every asset together with its `.meta`.
- Record every new asset in `Docs/ASSET_REGISTER.csv` (columns follow the existing AKIKO-00x rows).
- Configure Git LFS before adding binaries. It already tracks `*.fbx` and `Assets/SORA/Characters/**/*.png`. `Docs/Media/*.png` are plain git.
- Toolchain: Unity `6000.3.7f1`, URP `17.3.0`, Windows PC first. Add a dependency only when it is truly needed, and explain why.
- C# assemblies:
  - Runtime code goes in `Sora.Demo`.
  - Editor automation goes in `Sora.Demo.Editor` (namespace `Sora.AfterTheRain.Demo.Editor`).
  - EditMode tests go in `Sora.Demo.EditModeTests`.
- Game rules that must not break:
  - Money, inventory and quests are shared and survive character switching.
  - A POS sale must never be charged twice.
  - Each twin keeps separate relationships.
  - Both twins are 18. The GDD is canonical: `Docs/GAME_DESIGN_DOCUMENT_v0.4.md`.
- Branches: `feature/<scope>` or `fix/<issue>`. AGENTS.md says to base them on `develop`, but in practice this session branched from `main` and opened PRs against `main`, and the user merged them.
- Every PR must state:
  - Objective, changed files, architecture/logic.
  - Tests actually run, tests pending, risks and limits.
  - **Real Unity screenshots** for visual changes.
- Run `python Tools/validate_project.py` before committing. It does not compile C#; real Unity logs are required for that.

## 3. WORKING METHOD / USER-DEFINED WORKFLOW

**USER-DEFINED WORKFLOW**
- The user gives a short request ("Làm đi" = "go ahead", "Import vào Unity" = "import it into Unity", "commit…"). The agent does the whole thing, then reports back.
- Commit and push only when the user asks. "Commit" from this user means commit, push and open a PR.
- When the agent proposes work, the user picks it.

**SESSION-ESTABLISHED WORKFLOW** (worked well for Akiko; reuse it for Toshiharu)
1. Read the related code and design (GDD, existing scripts) first.
2. Write a procedural Blender script in `Tools/Blender/` that builds the model, rig and keyframes, and exports an FBX to `Artifacts/Character/`.
3. Render Blender previews and compare them against the design.
4. Copy the FBX into `Assets/SORA/Characters/<Name>/`.
5. Run a Unity batch import through an Editor importer, which:
   - builds URP Lit materials,
   - sets up the Humanoid rig,
   - builds the AnimatorController,
   - saves a prefab into `Assets/SORA/Characters/Resources/`.
6. Run the EditMode tests (`Assets/SORA/Tests/Editor/CharacterModelTests.cs`).
7. Run the in-game capture (Play Mode batch) to produce real screenshots, and check the images visually.
8. Run `validate_project.py`.
9. Commit (no AI line), push, and open a PR with the body in `Artifacts/pr_body.md` following AGENTS.md.
10. Report to the user in Vietnamese.

**WORKFLOW INVARIANTS**
- Make each step pass (import exit 0, tests pass, capture looks right) before moving to the next.
- When something breaks, find the root cause before fixing it, and add a test that guards against the bug. Example: the stride test exists because the walk captures once showed closed legs.
- Do not refactor outside the task. Keep the existing prototype and only replace a part when there is a concrete reason.
- Keep diffs as small as possible and do not add packages.

## 4. CURRENT WORKFLOW POSITION
```
Akiko pipeline:    model → import → rig/anim → test → capture → PR → merge   (ALL DONE, PR #6 + #7 merged)
Proposed next:     [user picks one]  1 Play test  | 2 Toshiharu model | 3 hair/skirt spring | 4 interaction anims | 5 Windows build
Currently working: none, waiting for the user to choose
```

## 5. WORKSPACE / ENVIRONMENT
- Working directory: `E:\TuanDuong\lap trinh kiem com\SORA-After-the-Rain`.
- Repo: https://github.com/haizzdungnay/SORA-After-the-Rain
- Local branch: `feature/tei-akiko-animation`, already merged. The next session must run `git checkout main && git pull`, then create a new branch.
- `origin/main` = `083a29b Merge pull request #7`.
- OS and shell: Windows 11; the Claude Code shell is Git Bash.
- Blender 5.2:
  ```
  "C:/Program Files/Blender Foundation/Blender 5.2/blender.exe" -b --factory-startup --python Tools/Blender/build_sweater_girl.py -- Artifacts/Character
  ```
- Unity:
  ```
  U="C:/Program Files/Unity/Hub/Editor/6000.3.7f1/Editor/Unity.exe"
  ```
  The license is already active on this machine. **The Editor must be closed** before running batch mode on the project.
- Other tools: `gh` CLI (logged in as haizzdungnay) and Python 3 (for `validate_project.py`).
- `Docs/` and `docs/` are the same folder (Windows is case-insensitive).

## 6. CURRENT VERIFIED STATE

**VERIFIED**
- PR #6 and PR #7 are MERGED into `main`, CI 3 SUCCESS / 2 SKIPPED. No PRs are open (`gh pr list`).
- `git status` shows only `?? Assets/_Recovery/` and `?? Assets/_Recovery.meta` (do not touch).
- On the last run of commit `6ba4db4`:
  - `validate_project.py` PASS.
  - Unity import exit 0.
  - EditMode 6/6 pass.
  - In-game capture exit 0. The `front` / `side` / `walk` / `run` images show Akiko facing the right way, legs not mirrored, and a clear stride.

**UNVERIFIED**
- Interactive Play Mode (WASD, Shift sprint, Tab to switch characters) has never been tested by hand with the new model.
- No Windows x64 build has ever been made.
- Unity CI (license-dependent) was not dispatched.
- PR #7 Auto-fix: the tool refused to enable it, and the user has not confirmed turning it on. It no longer matters because the PR is merged.

## 7. COMPLETED WORK
1. **Akiko model** (PR #6):
   - `Tools/Blender/build_sweater_girl.py` builds a procedural mesh: sweater, skirt, stockings, boots, white hair, face texture.
   - Imported into Unity, with URP materials in `Assets/SORA/Characters/SweaterGirl/Materials/`.
   - Prefab `Assets/SORA/Characters/Resources/TeiAkiko.prefab`. `SoraDemoWorld` loads it through `Resources.Load(SoraDemoWorld.AkikoModelResource)`.
2. **Akiko animation** (PR #7):
   - `Tools/Blender/sweater_girl_rig.py`:
     - 21-bone Humanoid skeleton with Unity Humanoid bone names.
     - Region-based skin weights.
     - Scripted in-place loops: Idle (96 frames), Walk (32), Run (20) at 30 fps.
   - `SoraSweaterGirlImporter`: Humanoid with an explicit bone map, plus `TeiAkiko.controller` with a 1D blend tree on `Speed`.
   - `SoraDemoCharacter` drives `Speed`.
   - Tests: avatar, facing +Z, clips loop, legs not mirrored, stride > 0.35 m.
   - Media: `Docs/Media/tei-akiko-{ingame,walk,run}.png`.

## 8. WORK IN PROGRESS
None. Everything is merged.

## 9. IMPORTANT DECISIONS
- **Procedural Blender scripts instead of hand modelling or downloaded assets.** Agent's choice, accepted by the user. Reasons: reproducible, reviewable in git, and no asset licensing problems.
- **Humanoid rig with explicit bone names** (`Hips`, `Spine`, `Chest`, `LeftUpperLeg`…), so the Unity bone map is 1:1. Reason: auto-mapping skips Chest.
- **Animation built in Blender with code** (sin-based gait), not mocap. Accepted: the result is basic, and hair and skirt stay rigid.
- **Blend tree thresholds in m/s** match `SoraDemoCharacter`:

  | Speed (m/s) | Clip | timeScale |
  |---|---|---|
  | 0 | Idle | — |
  | 1.25 (AI walk) | Walk | 1.25/1.16 |
  | 3.05 (player walk) | Run | 3.05/2.42 |
  | 5.0 (Shift) | Run | 1.6 |

  Accepted: sprint has slight foot slide.
- `applyRootMotion = false`: `SoraDemoCharacter` moves the CharacterController.
- No new packages added.

## 10. RELEVANT FILES & ARTIFACTS
- `AGENTS.md` — agent rules for the project. Read it first.
- `Docs/GAME_DESIGN_DOCUMENT_v0.4.md` — canon for the twins (appearance, age).
- `Docs/ROADMAP_GITHUB.md` — Gates 1–3:
  - Gate 2 needs a Windows build and interaction.
  - Gate 3 needs real models to replace the primitives, plus an animation pass.
- `Docs/ASSET_REGISTER.csv` — rows AKIKO-001..003 are the template for new assets.
- `Tools/Blender/build_sweater_girl.py` — Akiko model builder; template for Toshiharu.
- `Tools/Blender/sweater_girl_rig.py` — reusable rig, skin and animation module. Its `region_weights()` uses Akiko's mesh names, so a new model needs new rules.
- `Assets/SORA/Scripts/Editor/SoraSweaterGirlImporter.cs`:
  - `Import()` (menu "SORA/5. Import Tei Akiko Model") and `BuildController()`.
  - `CapturePreview()` and `CaptureInGame()`/`ResumeCapture()` for Play Mode screenshots.
- `Assets/SORA/Scripts/SoraDemoWorld.cs`:
  - Spawns both twins. `SpawnToshiharu()` at around line 409 still builds a primitive blockout. Comment at line 402: "18-year-old male twin, blue shirt, dark denim jeans, sneakers, short spiky hair".
  - Akiko's model loads through `AkikoModelResource`.
- `Assets/SORA/Scripts/SoraDemoCharacter.cs` — movement (AI 1.25, walk 3.05, Shift 5.0 m/s) and sets Animator `Speed`.
- `Assets/SORA/Tests/Editor/CharacterModelTests.cs` — model and animation tests; copy them for Toshiharu.
- `Assets/SORA/Characters/SweaterGirl/` — `sweater_girl.fbx` (LFS), `TeiAkiko.controller`, `Materials/`, `Textures/`.
- `Artifacts/` (gitignored):
  - `Character/` holds Blender previews and Unity captures.
  - `pr_body.md` is a PR body template.

## 11. CODE CHANGES
All merged; see the commits:
- `63c6219` — model.
- `6ba4db4` — animation.

## 12. COMMANDS RUN
```bash
# Blender build
"C:/Program Files/Blender Foundation/Blender 5.2/blender.exe" -b --factory-startup --python Tools/Blender/build_sweater_girl.py -- Artifacts/Character
cp Artifacts/Character/sweater_girl.fbx Assets/SORA/Characters/SweaterGirl/

U="C:/Program Files/Unity/Hub/Editor/6000.3.7f1/Editor/Unity.exe"

# Import (Editor must be closed)
"$U" -batchmode -nographics -quit -projectPath "$(pwd -W)" -logFile Artifacts/import.log -executeMethod Sora.AfterTheRain.Demo.Editor.SoraSweaterGirlImporter.Import

# EditMode tests (no -quit)
"$U" -batchmode -nographics -projectPath "$(pwd -W)" -runTests -testPlatform EditMode -testResults "$(pwd -W)/Artifacts/editmode.xml" -logFile Artifacts/tests.log

# In-game capture (no -quit, no -nographics; exits itself, 180 s timeout)
"$U" -batchmode -projectPath "$(pwd -W)" -logFile Artifacts/capture.log -executeMethod Sora.AfterTheRain.Demo.Editor.SoraSweaterGirlImporter.CaptureInGame
# Output: Artifacts/Character/unity_ingame_{front,side,walk,run}.png

python Tools/validate_project.py

# PR
gh pr create --base main --head feature/<scope> --title "..." --body-file Artifacts/pr_body.md
```

## 13. TEST / VALIDATION STATUS
- EditMode 6/6 PASS at `6ba4db4`.
- validate PASS. GitHub CI on PR #7: 3 SUCCESS, 2 SKIPPED (license-dependent Unity jobs, skipped by design).
- Not tested yet:
  - Interactive Play Mode.
  - Tab character switching with the new model.
  - Windows build.
  - Play Mode tests.

## 14. ERRORS / BLOCKERS
- No active blocker.
- Known limits:
  - Hair and skirt are rigid.
  - Boots can clip into the hair during Run.
  - Slight foot slide at Shift sprint.
  - Toshiharu is still a primitive blockout.
- `mcp__ccd_pr__set_monitor` refuses to enable Auto-fix for PRs the app is not sure this session opened. The user has to toggle it in the PR bar.

## 15. FAILED ATTEMPTS — DO NOT REPEAT

| Symptom | Wrong approach (DO NOT REPEAT) | Correct approach |
|---|---|---|
| Height test fails because skinned bounds are padded to cover every animation frame | Asserting on `SkinnedMeshRenderer.bounds` | `BakeMesh` the vertices |
| Chest drops out of the Humanoid auto-map | Auto-mapping | Explicit `HumanBone` map from `HumanTrait.BoneName` |
| Model turned 180° | `bakeAxisConversion = true` | Keep it `false` |
| Legs mirrored (left foot crosses to +X) | Reimporting while the old `humanDescription` is kept | Reset `importer.humanDescription = new HumanDescription()` before the first reimport (already in `Import()`) |
| Outline mesh grew spikes | Outline with even offset | Dropped even offset |
| Name clash between a mesh and a bone | Mesh named `Head` | Rename the mesh (e.g. `Face`). Mesh names must differ from Humanoid bone names. |
| Batch-capture skinned mesh keeps its rest pose (legs closed) | Calling `cam.Render()` / `animator.Update()` alone | Set `SkinnedMeshRenderer.forceMatrixRecalculationPerRender = true` |

## 16. IMPORTANT CONVERSATION CONTEXT
- The user is a solo developer building a portfolio. He wants the twins to look like real anime characters, not primitives.
- He cares a lot about authorship: the GitHub history must show only his account.
- The agent's latest proposal list, in recommended order (the user has not chosen yet):
  1. **Wrap up PR #7 by testing Play Mode by hand.** WASD, Shift, Tab. The user does this himself; the PR is already merged, so this is a smoke test.
  2. **Model for Tei Toshiharu (recommended).**
     - Reuse the Akiko pipeline.
     - The user was asked to send Toshiharu's design images; none received yet.
     - Without images, use the GDD and the `SoraDemoWorld.cs:402` comment: blue shirt, dark jeans, sneakers, short spiky hair, male, 18.
  3. **Secondary motion for hair and skirt.** A small spring-bone C# script, no package. Fixes the boots clipping into the hair.
  4. **Interaction animations.** POS scanning, NPC talk, turning in place (Gate 2).
  5. **First Windows x64 build** (Gate 2).

## 17. NEXT ACTIONS
1. `git checkout main && git pull`. Confirm `main` is at `083a29b` or later and `git status` shows only `_Recovery`.
2. Ask the user (in Vietnamese, short) which item from section 16 to start. If he picks 2, ask for Toshiharu's design images.
3. If he picks **Toshiharu**, follow the section-3 workflow in order:
   1. Create branch `feature/tei-toshiharu-model` from `main`.
   2. Read the GDD and `SpawnToshiharu()`.
   3. Write `Tools/Blender/build_<name>.py` and reuse `sweater_girl_rig.py`. Generalise only what is truly needed, e.g. make `region_weights` accept new mesh names. Run Blender, check the previews.
   4. Write an importer, either a new one or a shared one if that is truly simpler, plus a controller and a prefab in `Resources/`. Wire it into `SoraDemoWorld` the same way as `AkikoModelResource`.
   5. Copy the tests for Toshiharu. Use the height range from his CharacterController.
   6. Run the import, then tests, then capture. Check the images.
   7. Run validate, then update `ASSET_REGISTER.csv` and `Docs/Media`.
   8. Report to the user. Commit and open a PR only when asked, with no AI attribution.
4. If he picks **spring bone**:
   1. Branch `feature/akiko-secondary-motion`.
   2. Add bones for the hair and skirt in the rig script.
   3. Write a runtime component in `Sora.Demo` and add a test.
   4. Capture Run to check the boots no longer clip into the hair.
5. If he picks the **Windows build**:
   1. Use `Tools/Build-SORA.ps1` (read it first).
   2. Report the real log.
   3. Do not dispatch CI.

## 18. DEFINITION OF DONE (for any next task)
- Blender build runs cleanly, if a model was touched.
- Unity import exit 0, EditMode tests all pass.
- In-game capture exit 0 and the images look right.
- `validate_project.py` PASS.
- No changes in `_Recovery`.
- `ASSET_REGISTER.csv` updated.
- PR opened per AGENTS.md with real screenshots, no AI attribution, not auto-merged.

# INSTRUCTION FOR NEXT CLAUDE CODE SESSION

Treat this as a **continuation of the same working session**, not a new task.

1. Read the whole handoff before acting.
2. Check the workspace, git status and the referenced artifacts and files to confirm the real state.
3. If the handoff and the filesystem disagree, trust the real filesystem/code/git state, but find the cause of the difference before changing anything.
4. Do not re-analyse the project from scratch if the information is already in the handoff.
5. Do not ask the user to re-explain requirements already written in the handoff.
6. Keep the previous session's working method.
7. Follow the USER-DEFINED WORKFLOW and WORKFLOW INVARIANTS exactly.
8. If the user asked for the order `A → B → C`, continue in the order `A → B → C`. Do not reorder, skip, merge or replace steps, or do steps before their turn — unless there is a hard technical reason or the user gives a new request.
9. Check CURRENT WORKFLOW POSITION to find which step you are at, and continue from exactly there.
10. Do not redo steps that are already done and verified unless there is evidence they are wrong.
11. Do not refactor or redesign finished parts that are not directly related to the current task.
12. Do not reverse decisions the user has settled.
13. Do not repeat anything in FAILED ATTEMPTS — DO NOT REPEAT.
14. Continue directly from NEXT ACTIONS.
15. After each important step, run the specified validation procedure before moving to the next step.
16. If the workflow requires finishing and verifying each step before continuing, follow that rule.
17. Keep the same way of reporting progress, organising work and interacting with the user, if the user set it in the previous session.
18. Change the workflow only when the user asks directly, or when the current workflow technically cannot continue. In the second case, state the problem clearly before applying an alternative.
