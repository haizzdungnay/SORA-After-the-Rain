# Character umbrella and weight transfer - 2026-11-13

## Result

Six playable models and the procedural Akiko fallback retain the existing locomotion/physics and add context-aware umbrella handling, first steps, lead-foot stops, steering lean and idle weight shift. The owner approved implementation after analysis of the supplied video. All added motion/prop geometry is locally authored; the video is reference only. No Unity package was added.

## Behavior and files

- `SoraDemoCharacter` owns collision-constrained displacement, acceleration, gravity, buffered/coyote jump and separate movement/steering/menu signals. Valid jumps interrupt start, stop, pivot and idle actions.
- `SoraCharacterMotion` drives the base Animator, contact events, terrain/stance/pelvis IK and bounded visual balance/secondary offsets. Stops choose the last contact foot; their closing foot remains free after root braking. Source starts/stops also emit contact events.
- `SoraCharacterUmbrella` samples shared rain and each twin's own overhead cover. Rain thresholds have hysteresis and roof changes have a 0.22-second dwell. Native hand IK fits the wrist; a 0.53-second raise/lower drives the canopy blend shape. Folded umbrellas remain carried indoors/when clear. Disabled visuals and save/load reset transient state and layer weights.
- Right-arm and finger-only AvatarMasks preserve base gait, left arm and both legs. Five existing avatars map their existing right-finger chains; Rover and procedural Akiko retain their simpler rigs. Controller/source/prefab/avatar GUIDs remain stable.
- The FBX contains 29 authored takes: 19 used gameplay clips plus ten retained interaction resources. A Unity-native finger muscle clip makes 20 controller clips. Original umbrella mesh/material/prefab is generated through Unity, with open-canopy normal deltas.
- Dedicated secondary roots: Chisa 7, Citlali 9, Denia 28, Lynae 24; Rover/Zhongli have none selected. Chisa's anonymous roots were identified from actual Blender hair-mesh skin weights. Offsets are bounded; source meshes/rest rig are preserved.

Authoring: `Tools/Blender/sweater_girl_rig.py`, `export_character_motion.py`, `SoraAnimationControllerBuilder`, `SoraUmbrellaBuilder` and the existing playable importer. Runtime remains in `Sora.Demo`, authoring/review in `Sora.Demo.Editor`, tests in `Sora.Demo.EditModeTests`.

## Actual validation

| Check | Evidence/result |
|---|---|
| Unity 6000.3.7f1 / URP 17.3.0 EditMode | 63/63 pass in public worktree and owner checkout before concurrent weather follow-up; zero skipped. `editmode-release.xml`, owner `owner-editmode.xml` |
| Public native Play Mode | 285 checks pass, zero runtime errors, all six models. `PlayMode/report.json`, `playmode-contact.log` |
| Owner native Play Mode | 285 checks pass, zero runtime errors, all six models. `PlayMode/report.json`, `owner-playmode-supported.log` |
| Native scenarios | Open/fold/raise/carry, ordinary and non-default roof layers, interrupt/re-enable/reset, closing-foot motion, jump priority, real puddle effect, spatial audio and 100 POS-safe switches |
| Python pose mathematics | 83/83 pass; `python -m pytest Tools/Blender/test_sweater_girl_rig.py -q -p no:cacheprovider` |
| Blender source geometry | Seven meshes retain exactly matching coordinates, vertex counts, material slots and skin-group names; `geometry-preservation.json` |
| Public GUID preservation | All 403 existing GUIDs preserved; `guid-preservation.json` |
| Windows x64 builds | Public and owner builds pass; `public-windows.log`, `owner-windows.log` |
| Actual standalone startup | Both Direct3D 11 players initialize city/weather; `public-player.log`, `owner-player.log`. Test players stopped afterward |

Evidence paths are under `Artifacts/UmbrellaMotion/` in their respective checkouts. Final validation after the concurrent weather edits and diagnostic/review changes is appended below. Offline `python Tools/validate_project.py` checks structure/GUIDs, not compilation or rendering.

The owner checkout received only reviewed animation files. Eleven concurrent weather/store source/asset changes were detected separately and preserved; they are not part of this animation PR. The feature worktree remains based on the weather/store dependency branch and updates PR #36 without merging automatically or dispatching licensed CI.

## Visual evidence

![Umbrella motion](Media/Changelog/sora-umbrella-motion.png)

![Six models](Media/Changelog/sora-umbrella-six-models.png)

[Actual Unity demo](Media/Changelog/sora-umbrella-motion-demo.mp4): 289 native camera frames, 960 x 720, 15 fps, 19.27 seconds. Fixed 1/30-second simulation; no performance claim. The public media shows the review platform, without private city geometry.

Maximum sampled wrist-target error across 30 open/folded/walk/stop/jump samples: 0.044 mm. This is a Humanoid wrist proxy, not per-finger contact certification. Standing foot proxies still use the baseline ramp/step checks; neither measurement certifies every shoe or cloth vertex.

## Limits and reproduction

- Manual keyboard/art/audio walkthrough and target-PC profiling remain pending. Shared start/stop style is not cinematic phase-perfect foot locking; moving platforms and full cloth/finger collisions remain follow-ups.
- Umbrella geometry is visual and collider-free; per-character rain-particle occlusion is not added. Secondary offsets are bounded visual response, not cloth/wind simulation.
- The owner map can report physical ungrounding at a collider seam while visual coyote grounding remains true. Airborne splash rejection stays intact. The final fixture places an actual closing foot in an existing irregular puddle with verified physical support; attempted event counts alone do not prove an effect.
- Editor startup logs retain pre-existing Search-index, entitlement lookup and TLS messages before the review listener starts. Runtime acceptance reports record zero runtime errors. A stale generated Bee cache was backed up and regenerated after an interrupted compilation; source assets were preserved.

Run the pinned Editor with `-batchmode -runTests -testPlatform EditMode -testResults <absolute.xml> -logFile <absolute.log>`; omit `-quit` for Test Runner. Native review: `-batchmode -soraMotionReview` (optional `-soraMotionVideo`), exits itself. Existing scene Windows entry point: `Sora.AfterTheRain.Demo.Editor.SoraCharacterMotionReview.BuildWindows`. Source motion export/import uses the baseline report's commands; **SORA > Characters > 5. Upgrade Motion Library** rebuilds assets in place.

## Final owner integration checks

After concurrent weather edits: **64/64 EditMode pass, zero skipped** (`owner-editmode-final.xml`), **285 native Play Mode checks pass** (`owner-playmode-supported.log`) and the final Windows x64 build passes (`owner-windows-final.log`). The native contact fixture explicitly records `contactGrounded=True` and `water=True`, with actual splash/audio output.

Preservation audit: **1,177 unrelated baseline files remain byte-identical**, **403 recorded baseline metadata GUIDs preserved**, and the eleven separately detected weather/store changes remain intact (`preservation.json`). Original owner versions of transferred files are backed up under `Artifacts/UmbrellaMotion/pre-transfer/`.
