# Character weather motion implementation plan - 2026-11-13

User approved the video-derived direction and implementation in this chat.

**Goal:** add context-aware umbrella handling and clearer weight transfer without replacing existing locomotion, physics, foot IK, weather or store state.

**Architecture:** keep CharacterController authoritative. Extend the shared Animator with interruptible start/left-stop/right-stop clips and a right-arm-only umbrella layer. Each animated character samples its own overhead shelter and the existing weather state; it carries the folded prop indoors and opens it outside in rain. Original Unity-authored prop geometry and existing FBX-authored poses require no package or downloaded animation.

**Stack:** Unity 6000.3.7f1, URP 17.3.0, Blender 5.2, existing NUnit EditMode tests.

## Acceptance and decisions

- Rain threshold hysteresis and a short shelter debounce prevent door-edge flapping. Model changes, disabled visuals, save/load and airborne motion reset safely; no duplicate props or colliders.
- Feet retain the base gait/IK while the masked right arm raises/holds/lowers the umbrella. Opening takes about 0.5 seconds, matching the reference direction rather than copying source animation.
- Movement and physics-approved jumps interrupt start/stop/idle. Stop chooses a matching lead foot. Steering adds bounded torso lean; existing footsteps and puddle events remain authoritative.
- Subtle secondary offsets apply only to dedicated non-Humanoid hair/garment bones found in each model. Unsupported rigs remain intact; this is not cloth simulation or a rig rebuild.
- User MP4 is visual reference only. No binary extraction or third-party asset downloads.
- Reuse the clean feature/character-motion-polish worktree and update open PR #36. Copy only reviewed changes into the dirty owner checkout, preserving all other files and existing GUIDs. No merge or licensed CI dispatch.

## Task 1 - Baseline and executable regressions

Files: Assets/SORA/Tests/Editor/CharacterMotionTests.cs; CharacterModelTests.cs; Tools/Blender/test_sweater_girl_rig.py.

- [x] Record baseline EditMode and static-validator results; inspect rig bone names and source pose geometry.
- [x] Add regressions for masked arm isolation, opening/closing/shelter hysteresis, locomotion interruption, prop lifecycle and stop endpoints.
- [x] Run targeted checks before and after the changes; preserve evidence under Artifacts/UmbrellaMotion/.

## Task 2 - Authored clips and original prop

Files: Tools/Blender/sweater_girl_rig.py; existing sweater_girl.fbx/meta; Assets/SORA/Scripts/Editor/SoraUmbrellaBuilder.cs; SoraAnimationControllerBuilder.cs; Unity-generated resource assets.

- [x] Author MoveStart, StopLeft/Right, UmbrellaCarry/Raise/Hold/Lower/Adjust. Export takes into the existing FBX without changing meshes/rest rig.
- [x] Generate an original eight-panel umbrella with a native opening blend shape, shaft and handle; save materials/mesh/prefab with Unity metadata.
- [x] Rebuild the shared controller in place: base actions remain interruptible; the umbrella mask includes only right arm/fingers.
- [x] Verify loop flags, geometry and controller bindings across every playable model.

## Task 3 - Runtime integration

Files: Assets/SORA/Scripts/SoraCharacterUmbrella.cs; SoraCharacterMotion.cs; SoraDemoCharacter.cs.

- [x] Drive umbrella from existing weather plus per-character roof probes. Keep the folded prop in hand when sheltered; debounce door crossings and blend layer weight.
- [x] Match visual opening with the arm state, fit the grip with native hand IK, and bound prop sway.
- [x] Add phase-selected stops, start interruption, steering lean and optional bounded hair/garment motion. Restore offsets on disable/reset.
- [x] Keep movement speeds, economy, saves and model selection contracts intact.

## Task 4 - Actual Unity validation and delivery

Files: existing SoraCharacterMotionReview.cs; new/extended tests; Docs/CHARACTER_WEATHER_MOTION_REPORT_2026-11-13.md; README.md; CHANGELOG.md; Docs/ASSET_REGISTER.csv; Docs/TEST_CHECKLIST.md; actual Unity media.

- [x] Run Unity EditMode and scripted Play Mode across six models, both roof contexts, interrupted transitions, jumping, repeated model switches, contact events and store state.
- [x] Capture genuine Unity images/video. Inspect grip, canopy/head clearance and stop/turn poses; fix findings and rerun affected checks.
- [x] Run python Tools/validate_project.py. Build Windows x64 and start the actual player; report manual/performance limits explicitly.
- [x] Transfer reviewed files into the owner checkout, check preservation and GUIDs, validate its build, then push the feature and update PR #36 with evidence. Never auto-merge.

## Review findings applied

Independent read-only review identified four integration risks: keep steering yaw separate from stationary Turn; distinguish blocked menus from movement intent; avoid forcing a closing stop foot down merely because root speed is zero; enable RightHandIK and IK pass on the arm layer. Roof probes now include ordinary non-default layers and ignore character colliders. Same-ID save/load resets transient umbrella state through ResetMotion. Actual-runtime checks cover these boundaries.
