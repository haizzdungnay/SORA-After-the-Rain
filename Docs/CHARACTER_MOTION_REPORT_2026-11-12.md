# Character motion integration - 12/11/2026

## Result

The six existing playable models and the procedural Akiko fallback now share a
Unity-generated controller with eleven gameplay clips: Idle, Walk, Jog, Run,
Jump, Fall, Land, TurnLeft, TurnRight, IdleLookAround and IdleStretch.

Honkai: Star Rail informed the relaxed stance and idle variation direction;
Genshin Impact informed the walk/jog/sprint and responsive jump direction.
Cadence, poses and timings are SORA-authored choices, not measurements or
animation files extracted from those games. This is locomotion polish, not
combat, climbing, stamina, facial or cloth simulation.

## Behavior and architecture

- WASD jogs, Ctrl toggles walking, Shift sprints, Space jumps, Q/R pivot while
  stationary. RMB still orbits the camera. Speeds remain 1.25/3.05/5 m/s.
- `SoraDemoCharacter` owns acceleration/braking and collision-constrained motion.
  The jump uses 0.1 s input buffering/coyote time, consumes one press, integrates
  gravity analytically, handles ceilings and resets transient motion on load.
- `SoraCharacterMotion` drives the Animator, schedules interruptible idle actions,
  adapts cadence to Humanoid scale, fits feet to nearby colliders and applies a
  bounded visual pelvis offset where feet have different support heights.
- Jump has an Any State transition after physics approval; fall follows actual
  descent. Grounded turning, landing and idle cannot retain an obsolete launch
  trigger or block a valid jump until their clip exit time.
- Walk/Jog/Run contain two contact events. Sound and puddle splash use those
  events, with blend filtering; distance fallback remains for unanimated figures.
- The existing FBX meshes/rest rig are preserved; only authored takes are
  replaced. Importer loop settings distinguish loops and one-shots. The shared
  controller and seven prefabs are saved by Unity with their existing GUIDs.
- Store transactions remain in `SoraDemoCore`; animation events never charge POS
  or change inventory. Weather state/save schema remain compatible.

## Validation actually run

| Check | Result | Evidence |
|---|---|---|
| Owner Unity EditMode, final runtime | 43/43 pass, 0 skipped | `Artifacts/CharacterMotion/editmode-release.xml` |
| Public feature Unity EditMode | 43/43 pass, 0 skipped | same relative path in feature worktree |
| Owner scripted Play Mode motion | 139 checks pass; all six models | `Artifacts/CharacterMotion/PlayMode/report.json`; `playmode-release.log` |
| Public scripted Play Mode and demo recording | 139 checks pass; 145 captured frames | feature `Artifacts/CharacterMotion/playmode-release.log` |
| Store/weather runtime regression | Public and private worlds pass: 8 routes, 100 switches, 9 rain probes, save/load and legacy saves | `weather-regression.log` (owner), `weather-release.log` (feature) |
| Owner and public Windows x64 builds | Pass; existing scene used | `Artifacts/CharacterMotion/windows-release.log` in each checkout |
| Windows player startup | Actual Direct3D 11 startup passes in both builds; test players stopped | `Artifacts/CharacterMotion/player-release.log` in each checkout |
| Blender pose math | 64/64 pass, including upright look direction and neutral idle endings | `Tools/Blender/test_sweater_girl_rig.py` |
| Blender geometry preservation | Seven meshes unchanged within 0.1 mm, same vertex counts/material slots/skin group names | `Artifacts/CharacterMotion/geometry-preservation.json` |

Unity is **6000.3.7f1**, URP **17.3.0**; no new package. EditMode includes physical
jumps at 0.02 s and 0.1 s steps, airborne second-press rejection, immediate jump
from idle/landing/turn, idle cancellation, imported event timing, geometry,
store/save and weather checks. Offline validation is reported separately.

The motion harness runs native Animator and CharacterController in actual Play
Mode with a fixed 1/30 s simulation step. It temporarily disables the city boundary
guard only for the out-of-map review platform; shipping gameplay keeps the guard.
Puddle contact then runs on the real street. This is scripted acceptance, not a
manual keyboard walkthrough or a target-PC performance measurement.

Owner contact samples, standing after settling on review geometry:

| Surface | Samples | Foot-bone sole proxy clearance |
|---|---:|---:|
| 10-degree ramp | 12, both feet across six models | +1.61 to +1.93 mm |
| 16 cm step edge | 12, both feet across six models | -0.02 to 0.00 mm |

These samples measure foot bone height minus Unity's feet-bottom height, not
every shoe vertex or every gait phase. They do not certify all stairs, moving
platforms or precise cinematic foot locking. An initial 20 cm low-foot gap at
the step exposed the need for pelvis adjustment; the final samples use that fix.

## Visual evidence

These contact sheets compose actual Unity camera renders from the scripted
review stage. Labels/composition are added afterward; model images are not
generated artwork or claims about final in-game lighting.

![Six models](Media/Changelog/sora-motion-six-models.png)
![Locomotion](Media/Changelog/sora-motion-locomotion.png)

[Actual motion demo, 9.67 s / 15 fps](Media/Changelog/sora-motion-demo.mp4). Frames
come from the final public Play Mode run at a fixed simulation step; no audio
recording or performance claim.

## Reproduce

```powershell
python -m pytest Tools/Blender/test_sweater_girl_rig.py -q -p no:cacheprovider
python Tools/validate_project.py
```

With Unity closed, run the pinned Editor with `-batchmode -runTests -testPlatform
EditMode -testResults <absolute-output.xml> -logFile <absolute-log>`. Do not add
`-quit` for Test Runner. Motion review uses `-batchmode -soraMotionReview`; add
`-soraMotionVideo` for fifteen-frame-per-second PNG captures of Chisa's sequence.
The harness exits itself, writes structured results and leaves source scenes
unchanged. Windows build entry point:
`Sora.AfterTheRain.Demo.Editor.SoraCharacterMotionReview.BuildWindows`.

To export motion from the existing FBX without changing its geometry:
`blender --background --python Tools/Blender/export_character_motion.py`, then copy
the generated FBX from `Artifacts/CharacterMotion/` onto its existing source path
and run **SORA > Characters > 5. Upgrade Motion Library** in Unity.

## Limits and delivery

- Start/stop are controlled acceleration and blends, not dedicated planted-stop
  clips. The two twins still share the same underlying motion style.
- Stance locking is bounded and uses walkable static colliders; difficult terrain,
  clothing intersections, cloth/hair, fingers and per-avatar acting need art QA.
- Ten interaction clips remain source resources, not completed shop interactions.
- Existing startup/package warnings and any measured validation limits are
  recorded with logs; scripted frame capture is not an FPS/GPU benchmark.
- Feature branch: `feature/character-motion-polish`, based on
  `origin/feature/sora-mart-retail-finish` at `0926c17`, which includes merged weather
  PR #34. `main` did not yet include that weather commit at baseline. The motion
  PR therefore targets the dependency branch and does not merge automatically.
- Owner checkout keeps its existing branch and unrelated work: **857 unrelated
  tracked files remain byte-identical**, and **368 existing tracked GUIDs** match
  their original IDs. Known Unity material line-ending/analytics serialization
  side effects were restored. Existing scene, packages and store art are preserved.
- Raw draw-call/triangle counters in the weather review reflect Editor/UI context,
  not a valid measurement of world rendering performance.
- Delivered as [PR #36](https://github.com/haizzdungnay/SORA-After-the-Rain/pull/36),
  open against the weather/store dependency branch. Implementation commit `e397f0d`;
  no automatic merge. Owner Windows player updated in `Builds/Windows/`.
