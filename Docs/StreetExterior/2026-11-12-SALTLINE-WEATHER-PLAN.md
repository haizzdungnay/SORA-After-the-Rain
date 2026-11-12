# Saltline Weather Implementation Plan

> Execution: inline in this session; user approved planning, implementation and integration. Skills: writing-plans, using-git-worktrees, verification-before-completion. One advisory plan review; no approval pause or automatic merge.

**Goal:** integrate changing rain, accumulating/drying puddles, ripples/splashes and sheltered rain audio into the existing playable SORA world.

**Architecture:** keep the current rain ParticleSystem and its roof collisions/follower. One shared weather component drives intensity, surface wetness, puddle fill, lighting and two rain loops. A separate surface component fits bounded, reusable puddle patches to outdoor ground near the active twin; URP shader uses an environment probe and procedural rain ripples. Gameplay remains owned by SoraDemoCore.

**Stack:** Unity 6000.3.7f1, URP 17.3.0, existing ParticleSystem/Physics/AudioSource/ReflectionProbe; no new package. Owner-authored Saltline textures/audio, hashes and source attribution recorded. Original Saltline world masks are not copied onto SORA streets.

## Scope decisions

- Start with environment probes and a local wet-surface shader; full planar reflection and volumetric raymarch remain outside this pass unless actual captures show the probe inadequate.
- Wetness and pooled puddles follow the existing playable city/fallback. No new map, character rig, quests, POS or inventory system.
- Automatic clear/drizzle/rain/heavy-rain transitions, manual F8 cycle for local review, smooth intensity change, slow puddle drying after rain stops.
- Outdoor-only puddles/splash: real ground raycasts and overhead shelter checks, no new physics blockers. No particles/loops duplicated on twin switches.
- Optional weather snapshot in schema-1 saves, absent in old saves; validate finite values/ranges before accepting. Preserve old save files during automated verification.
- Implement and publish from feature/saltline-weather, stacked on the already-open retail-store PR #33. Also copy only the reviewed weather change into the owner's current checkout, preserving unrelated files/GUIDs. Do not merge either PR.

## Tasks

### 1. Baseline and meaningful regression checks

- [x] Record clean feature baseline; run offline validator and baseline EditMode tests.
- [x] Add Assets/SORA/Tests/Editor/WeatherTests.cs: bounded accumulation/drying, transitions, invalid input/snapshot, old-save compatibility, sheltered surface rejection, no duplicate weather/POS after switching. Existing NUnit/Unity Test Framework only.
- [x] Run new checks before implementation where possible and retain actual failing evidence.

### 2. Weather and wet surfaces

- [x] Create Assets/SORA/Scripts/SoraWeather.cs: state/snapshot validation, modes, smooth rain, persistent wetness/puddles, audio/shelter, small F8 review control, owned-resource cleanup.
- [x] Create Assets/SORA/Scripts/SoraWetSurfaces.cs and Assets/SORA/Resources/Weather/Puddle.shader: transparent overlay preserves source art, bounded ground-fitted patches, ripples, environment reflection and local splash. Reuse existing camera smoothing and rain collider configuration.
- [x] Modify SoraDemoWorld.cs only at the bootstrap integration; modify SoraDemoCore.cs only for optional save snapshot capture/restore.
- [x] Use original Saltline detail/noise and normal maps and rain/wind/thunder WAVs selected for weather. Importer config, material/texture metadata and reflection settings generated through Unity Editor automation in Assets/SORA/Scripts/Editor/SoraWeatherSetup.cs.
- [x] Record exact selected file hashes and ownership in Docs/StreetExterior/SALTLINE_WEATHER_ASSETS.json and append one asset register entry. Track binaries with existing/new scoped Git LFS rules before staging.

### 3. Real Unity verification and captures

- [x] Add Assets/SORA/Scripts/Editor/SoraWeatherReview.cs for unattended real Play Mode checks/captures: wet/dry same view, puddle close-up, heavy rain, sheltered interior; state transitions, no indoor puddles, roof probes and bounded objects.
- [x] Run Unity import/C# compile, EditMode, public-city Play Mode and owner's current/private-city Play Mode. Inspect actual rendered screenshots and fix visual defects.
- [x] Verify 100 switches, draft cancellation, one sale, delivery/restock and save/load including weather; restore pre-test user save bytes.
- [x] Build Windows x64 and launch actual player, check startup/shader/runtime errors and weather initialization. Capture draw-call/triangle/memory counters as available; report startup/render evidence separately from manual QA/FPS.

### 4. Integrate and deliver

- [x] Apply only weather files/patches to owner checkout; preserve pre-existing store/character changes and binary/scene hashes.
- [x] Update CHANGELOG.md and Docs/StreetExterior/Reports/2026-11-12-SALTLINE-WEATHER.md with genuine screenshots, exact executed checks and limitations.
- [ ] Run offline validator and diff/LFS checks, inspect changes, commit weather scope, push branch and open stacked PR with required objective/files/architecture/tests/pending/risks. Never merge automatically.

## Acceptance

Rain changes smoothly; dry weather leaves puddles that recede. Reflections/ripples appear on irregular puddles on actual road geometry. Store interior/roof remain dry; no new colliders block walks. Twin switches keep one weather instance/listener and shared store state. Old saves load; invalid weather data cannot replace valid store state. Source textures/materials and .meta GUIDs survive. Unity screenshots and actual test/build logs substantiate completion; remaining manual art/performance checks are explicit.

## Progress evidence

- Offline feature baseline: PASS, 369 GUIDs / 1,121 tracked files. Owner baseline: 16/22 passed; six pre-existing avatar clip failures recorded separately.
- First new feature test run exposed JsonUtility fabricating absent inline objects. Fixed with explicit hasWeatherSnapshot; final release run 30/30 PASS, including real public-road/CharacterController regression.
- First owner Play Mode check exposed a test-fixture overlap between the two parked twins. Corrected fixture placement before the final run.
- Native Unity import/C# compile and material/Volume generation completed. Final public/private Play Mode passed; 8 routes, 100 switches, 9 rain probes and weather/POS/save checks each. Public/private Windows builds and actual startup passed. Owner EditMode: 24/30, only the six baseline avatar failures remain.
- All 17 selected source hashes match. 690 protected original-checkout files checked; unrelated character work, scene, settings and GUIDs preserved. Genuine public-city screenshots accompany the implementation report.
