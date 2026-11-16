# Character motion polish - 12/11/2026

## Goal and visual direction

Complete the existing six-model Humanoid locomotion, preserving the twins,
CharacterController, store transactions, camera orbit and weather integration.
Use Honkai: Star Rail as a reference for relaxed stance, readable silhouettes
and idle variety; use Genshin Impact as a reference for walk/jog/sprint cadence,
responsive direction changes and grounded jump/landing flow. These are visual
references, not claims to reproduce their animation data or production quality.
All new clips are authored in the project's Blender pose generator.

Chosen approach: retain native Animator + CharacterController, extend the shared
controller and add a visual motion component for idle timing, terrain foot IK and
animation-event footsteps. A package/controller replacement adds migration cost;
editing only the controller would leave the idle, contact and input defects.

## Behavior

- WASD jogs; Ctrl toggles walking; hold Shift to sprint; Space jumps.
- Q/R pivot left/right while stationary; RMB continues to orbit only the camera.
- Acceleration/braking use actual collision-constrained speed for animation.
- Jump has a short input buffer/coyote window, rejects airborne double jumps,
  interrupts grounded idle/turn/landing, and follows physical ascent/descent.
- Idle breathing remains a loop. Look-around and relaxed stretch one-shots play
  after inactivity, return to neutral, and cancel promptly on movement/jump/menus.
- Rebuild the shared controller in Unity without changing its GUID. Update the
  prototype Akiko fallback too; retain source rigs, scale, materials and meshes.
- Fit feet to nearby walkable colliders with native Humanoid IK, constrain stance
  drift, scale gait cadence for each avatar, and trigger footsteps/splashes from
  clip contacts. Keep distance-based sound only for unanimated blockouts.

## Scope and limits

No combat, stamina, climbing, swimming, facial animation, cloth or character art
replacement. Existing ten interaction clips remain source resources; POS money
and stock changes stay in gameplay code. Uneven-ground IK is a bounded visual
correction, not a replacement for navigation or CharacterController collision.

## Execution and acceptance

1. Preserve dirty checkout; author two idle actions; export source FBX without
   rendering or changing geometry; configure takes/events with Unity's importer.
2. Fix controller priorities, implement movement/idle/foot contact, update controls
   and fallback, then run existing and targeted Unity regression checks.
3. In actual Play Mode exercise all six models: idle cancel, walk/jog/sprint,
   stop, wall, pivot, jump from idle/turn/land, ledge, repeated jump input, model
   swap and twin switch. Check feet on flat/ramp/step surfaces and wet contacts.
4. Run store/weather/save regressions, capture real Unity frames, build and launch
   Windows player. Report measured checks separately from manual art/performance
   acceptance. Preserve unrelated owner files and all pre-existing asset GUIDs.
5. Publish an isolated feature PR against the applicable upstream baseline. Include
   source and generated assets, real screenshots, exact checks and remaining
   limitations. Never merge automatically or dispatch licensed CI.

## Progress

- Baseline recorded: current shared controller has three clips; six character
  tests fail. Source FBX has eighteen actions. Detailed audit remains available
  locally in `Artifacts/AnimationVerification/REPORT.md`.
- Implementation complete: 21 source clips, 11 gameplay clips, shared controller,
  six-model and fallback integration, movement/idle/ground-contact fixes.
- Native validation: 43/43 EditMode in owner and public feature; 139 scripted
  motion checks in both; public/private weather/store/save regressions and Windows
  builds/startup pass. Pose math 64/64. Evidence and art/performance limits:
  `Docs/CHARACTER_MOTION_REPORT_2026-11-12.md`.
