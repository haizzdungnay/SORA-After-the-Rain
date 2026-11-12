# Saltline weather and puddles - 2026-11-12

SORA now reuses the owner's Saltline surface detail and environment audio for a
shared clear/drizzle/rain/heavy-rain cycle. Rain intensity changes gradually;
ground becomes wet and puddles fill, then recede after rain stops. F8 opens the
weather panel, selects a mode and toggles automatic cycling. Existing schema-1
saves accept an optional weather snapshot; legacy saves remain supported.
An explicit `hasWeatherSnapshot` flag distinguishes missing legacy data from
the empty inline object that Unity JsonUtility creates for an absent field.

## Implementation

- `SoraWeather` controls the existing rain ParticleSystem, three environment
  audio sources, shelter crossfade, thunder and modest night fog/post settings.
  Existing roof collisions and active-twin rain following remain in place.
- `SoraWetSurfaces` fits a reusable transparent mesh to real, flat outdoor
  ground near the active twin, checks tile corners against roofs/height changes
  and adds no collision geometry. At most 529 tiles/1,058 triangles; only 16
  candidates are tested per frame during an area rebuild. World-space patterns
  remain stable across rebuilds.
- The URP puddle shader blends irregular water masks and animated rain ripples
  using Saltline detail/normal PNGs. Native 128-pixel cubemap probes supply local
  environment reflection. Probe captures omit the water layer and are triggered
  on demand, with a 15-second minimum movement refresh interval.
- Rain collision splashes are capped at four emitted particles per frame near
  the active twin and 128 live particles. Ground/water/metal footstep variants
  use traveled distance; puddle queries sample the same source noise and reject
  sheltered ground. One existing listener remains on the active camera.
- The original street, store materials/UVs, characters, POS and aggregate stock
  remain under their existing owners. World masks/colliders from Saltline's
  harbor are not imported. No new package or combat system is added.
- The public V03 PBR roads previously had no mesh colliders and used the safety
  floor underneath. Their real road surfaces now receive ground colliders, so
  rain/puddle queries meet visible geometry. CharacterController hits are ignored
  by the nonallocating ground query, preventing square dry holes under people.

## Source and branches

Selected source PNG/WAV bytes, hashes, source-document hash and shader/function
reference hashes: [asset manifest](../SALTLINE_WEATHER_ASSETS.json). The owner
confirmed authorship and authorized extraction and integration in chat. The
asset register records owner-authored source; this does not invent a separate
project-wide public license. PNGs and WAVs use Git LFS.

Feature branch `feature/saltline-weather` is stacked on the store resources in
PR #33, rather than the older `develop`. No PR is automatically merged. Weather
files are also integrated into the owner's current character-animation checkout;
pre-existing store/character changes and generated scene bytes are protected.

## Verification

| Check | Actual result |
|---|---|
| Unity import/C# compile | PASS, Unity 6000.3.7f1 / URP 17.3.0; Unity generated texture/audio metadata, materials, Volume and renderer post-process reference |
| Feature EditMode | 30/30 PASS, `TestResults/weather-editmode-release.xml`, `Logs/weather-editmode-release.log` |
| Owner EditMode | 24/30 PASS; all 8 weather cases pass, six pre-existing character cases remain; `Artifacts/SaltlineWeather/owner-editmode.xml` |
| Public-city Play Mode | PASS, `Logs/weather-public-playmode-final.log`, `Artifacts/SaltlineWeather/report.json`, `privateCity=false` |
| Owner/private-city Play Mode | PASS, `Logs/weather-owner-playmode-final2.log`, same report path in owner checkout, `privateCity=true` |
| Gameplay per Play Mode run | 8 movement routes, 100 switches, safe draft cancellation, exactly one sale, delivery/restock, weather/store save-load, legacy save accepted and malformed weather rejected |
| Shelter per Play Mode run | 9 roof/canopy/outdoor rain probes; interior query stays dry and indoor audio crossfade reaches shelter |
| Puddles | 92 ground tiles in public capture, 223 private; 59 wet points in each tested front-street sampling grid; after-rain puddles remain while rain reaches zero |
| Source asset integrity | All 17 selected PNG/WAV bytes match the committed source manifest hashes |
| Public Windows x64 | Build and actual standalone startup PASS, `Logs/weather-public-build.log`, `Logs/weather-player.log` in feature worktree; Direct3D 11, public city and weather initialized |
| Owner Windows x64 | Build and actual standalone startup PASS, `Logs/weather-owner-build.log`, `Logs/weather-player.log` in owner checkout; Direct3D 11, `private AFX=True`, weather initialized |
| Preservation | 690 protected file hashes compared; existing character binaries/prefabs/materials, metadata GUIDs, scene and project settings restored/preserved. Only intended weather additions to attributes/register/readme/changelog and public-road collider recognition differ |
| Final static/LFS checks | PASS; feature: 399 unique GUIDs / 1,187 tracked files; owner: 404 / 880 tracked files. New selected binaries use LFS, staged source hashes and metadata sidecars checked; whitespace check passes |

Both test-owned players responded and logged completed store/city/weather
startup, with no checked runtime exceptions, shader errors or mesh-creation
failures. Test players are stopped afterward. This proves startup, not a manual
standalone walkthrough or audio/art acceptance. The owner build is
`Builds/Windows/SORA_After_the_Rain.exe` with its accompanying data directory.
No licensed remote CI workflow was dispatched.

Owner checkout baseline before weather: 22 EditMode cases, 16 passed and six
existing character cases failed. Those cases expect eight locomotion actions,
but their current imported prefabs contain only Idle/Walk/Run. Weather does not
replace the character assets or change those assertions.

The source test initially exposed Unity's absent-inline-object serialization
behavior, now protected by `hasWeatherSnapshot`. A regression also samples a
real public PBR road beneath a CharacterController, covering actual road support
and the dry-hole defect. This is not only a string/source-marker check.

The local JSON includes raw `UnityStats` counters. Those reflect Editor/UI
rendering state around manual camera capture and are not treated as representative
world draw calls, triangle totals or a target-PC FPS benchmark. The surface mesh
budget is explicitly bounded in code; full-scene CPU/GPU profiling is pending.

Public Editor retains its existing `UnityEditor.Search.SearchDatabase` startup
exception. Owner Editor retains missing Animator-parameter warnings associated
with the separate avatar work. Neither is introduced by weather; the checked
standalone startup logs contain no such exceptions or missing-parameter warnings.

## Genuine Unity captures

Five direct 1600 x 900 Play Mode camera renders from the public-city configuration.
Identical front camera for dry/rain/after-rain; close-up and interior are separate
review angles. No compositing or AI artwork. Owner-private geometry stays in
ignored local artifacts.

![Dry front](../../Media/Changelog/sora-weather-dry-front.png)

![Rain front](../../Media/Changelog/sora-weather-rain-front.png)

![Puddles and rain ripples](../../Media/Changelog/sora-weather-puddle-closeup.png)

![Sheltered store interior](../../Media/Changelog/sora-weather-sheltered-interior.png)

![Puddles remain after rain stops](../../Media/Changelog/sora-weather-after-rain.png)

## Limits

- Cubemap environment reflection does not reproduce planar, screen-space or
  ray-traced moving-character reflections. Full volumetric fog is outside this
  pass. Depth/opaque textures are not enabled merely to support these effects.
- Puddles model visual accumulation on flat outdoor roads/sidewalks, not water
  volume, runoff, terrain flooding or wet-cloth simulation. Roofed patches stay
  dry; partial corner tiles are conservatively omitted.
- The four-stage cycle is a bounded visual cycle. It is not a time-of-day,
  forecast or shift scheduler. Wetness/puddles dry more slowly than the rain
  transition; selecting Clear intentionally does not immediately erase water.
- Footstep contacts are distance-based until the avatar controllers expose
  consistent animation events. Manual keyboard/audio/art acceptance and target
  hardware profiling remain separate from scripted checks and captures.
