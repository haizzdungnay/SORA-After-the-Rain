# Storefront lighting and heavy-rain wet road — 2026-11-13

The owner selected heavy rain around SORA MART as the immediate visual target.
This pass replaces the near-opaque, sharply masked dark puddle appearance with
a softer continuous wet film, lights the store and its approach, and adds subtle
animated surface flow and staggered rain ripples. Intermediate and after-rain art
acceptance remain later work.

## Changes

- The wet-road shader now compiles URP additional-light variants. Previously a
  local store lamp did not illuminate this overlay at all. Retain original road
  textures and markings: film opacity is `0.24 * wetness + 0.16 * pool`, instead
  of interpolating from `0.075 * wetness` to `0.9`. Broaden the pool edge around
  the existing footstep-query midpoint; retain the query, shelter and ground fit.
- Reduce coarse surface-normal distortion, add a fine directional animated normal
  and stagger ripple positions/lifetimes. These are visual effects, not water
  depth, drainage or a fluid simulation.
- Give the six ceiling fixtures warm point-light fill below the ceiling and add
  one bounded exterior spill spotlight. Balance front light, blue night fill,
  exposure, contrast, saturation and bloom. Existing store geometry, packaging,
  glass, colliders and gameplay anchors remain intact.
- Enable existing URP probe blending/box projection and eight additional lights
  per object; increase the existing local cubemap to 256. This still uses cubemap
  reflections, without a new planar-reflection camera or package.
- Start newly created runtime weather in fixed HeavyRain with a wet road; F8
  manual modes and restored saved weather retain their existing behavior. Use
  finer, denser rain and FXAA. No weather enum or save-schema change.
- Extend the existing review entry point with unobstructed front/angled captures
  and optional `-soraRainMovie`: 90 deterministic-timestep frames at 15 fps.

## Files and integration

Runtime changes stay in `Sora.Demo`: `SoraMartVisuals`, `SoraWeather`,
`SoraWetSurfaces`, and the existing environment-light setup in `SoraDemoWorld`
and `SoraCityRuntimeWorld`. `Puddle.shader` owns wet-film shading. Editor asset
setup/review stay in `Sora.Demo.Editor`; the GPU regression uses the existing
EditMode test assembly. Unity writes the Volume and pipeline asset changes;
metadata/GUIDs and source models remain intact.

Branch `feature/heavy-rain-storefront` is based on
`feature/character-motion-polish` at `11cfd59`, preserving the step-contact work
in PR #36. The older develop lacks these integrated store/weather/character
resources. An isolated ignored worktree protects the owner's dirty checkout.
Only this visual change is intended for integration; unrelated character work
and source models must not be overwritten.

No downloaded asset, dependency or new third-party source is added. The existing
owner-authored Saltline textures/audio and licensed store resources retain their
recorded provenance. Private AFX model copies and private-city captures stay local.
The captures below must come from the verified public-city configuration.

## Verification

| Check | Result and evidence |
|---|---|
| Unity | 6000.3.7f1, URP 17.3.0, Direct3D 11, RTX 3050 Laptop GPU |
| GPU regression before fix | RED: `WetRoadRespondsToLocalStoreLight` gives the same wet-film brightness with lamp off/on; `TestResults/heavy-rain-red.xml` |
| GPU regression after fix | GREEN in both full GPU runs; final report `TestResults/heavy-rain-green.xml` |
| Initial full EditMode | 43/44 pass. The single old assertion required Spot lights; updated it to verify floor reach while retaining below-ceiling, glass, collider and shelter checks |
| Final full EditMode | **44/44 PASS**, GPU enabled, `TestResults/heavy-rain-green.xml`, `Logs/heavy-rain-green.log` |
| Owner/private-city Play Mode | PASS, `Logs/heavy-rain-final-private.log`, `Artifacts/HeavyRainPrivate/report.json`: eight routes, 100 switches, one sale, safe draft cancellation, stock/save-load, old/malformed-save checks, nine roof probes, 223 ground tiles |
| Private moving capture | 90 actual Unity frames, six seconds at 15 fps; local `Artifacts/HeavyRainPrivate/heavy-rain-preview.mp4` |
| Public-city Play Mode | PASS, `Logs/heavy-rain-public.log`, `Artifacts/HeavyRainPublic/report.json`: same gameplay/save/shelter checks; 92 ground tiles, `privateCity=false` |
| Offline validator | PASS: final staged feature tree 405 unique GUIDs / 1,206 tracked files; owner integration check 414 GUIDs / 880 tracked files. Counts differ because owner has additional local work and feature validation temporarily excludes private models |
| Windows x64 build/startup | **PASS**, public-city build `Builds/Windows/SORA_After_the_Rain.exe`, `Logs/heavy-rain-public-build.log`; actual responding player, Direct3D 11 and completed city/store/weather startup in `Logs/heavy-rain-public-player.log`; no checked runtime/shader errors |

The pre-existing internal UnityEditor.Search.SearchDatabase startup exception
also appears in baseline Editor logs. It is separate from runtime game/shader
acceptance. The raw UnityStats counters recorded by the review are Editor/UI
observations, not representative world draw calls, triangle totals or FPS.

## Genuine public Unity captures

Direct 1600 x 900 Play Mode camera renders, public city, fixed HeavyRain. No AI
art or compositing. Private reference geometry stays in ignored local artifacts.

![Heavy rain and storefront lighting](../../Media/Changelog/sora-heavy-rain-front.png)

![Wet road and store light](../../Media/Changelog/sora-heavy-rain-road.png)

![Adjacent street angle](../../Media/Changelog/sora-heavy-rain-street.png)

The owner's visual patch was applied using checked diff contexts. The owner's
separate shelter-raycast fix is retained, as are character resources, store GLB,
scene metadata and ongoing character/umbrella work. The owner's open Editor was
not closed to run batch checks; the 44-case result above belongs to the isolated
feature worktree. The existing owner Windows executable is not replaced by the
public test build.

An additional batch attempt against the owner project ended before Editor
initialization (`Logs/heavy-rain-owner-integrated.log`). It provides no owner
compile/Play Mode result. The successful private-city captures above use the
same store/private models in the isolated worktree, while preserving the owner's
separate shelter logic during patch integration. The public test player is
stopped after startup checks.

## Limits

- This is a first lighting/wet-film pass. It does not claim a frame-identical
  reproduction of the reference video or final owner art acceptance.
- Cubemaps do not reproduce correctly positioned live planar reflections of
  moving twins. Full dynamic character reflections, volumetric rain/fog and
  authored surface drainage are outside this change.
- The fixed review movie proves animated rendering. It is not a manual player
  walkthrough or a target-PC frame-time benchmark. Performance acceptance and
  moving-camera/manual audio review remain separate.
- New-world HeavyRain is intentional for this milestone. A save that already
  contains another valid weather snapshot continues to restore that snapshot.
