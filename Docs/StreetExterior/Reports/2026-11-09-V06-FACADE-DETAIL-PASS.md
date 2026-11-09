# SORA V06 — 9-block facade detail pass (2026-11-09)

## Goal and precise status
This is an **incremental, additive facade-art pass** built on V05. It does not replace the V05 nine-block map. It does not modify the playable prototype yet.

* Scope: 60 of the existing 84 building exteriors (the five expansion blocks introduced in V04/V05), leaving the 24 earlier V03 facades unchanged for later QA.
* Additions: 3D shop doors/frames/thresholds, illuminated fascia and projecting signs, awnings/brackets, glass-front display props, upper-storey mullions/sills and optional balcony railings, side services/vents/drains, rear windows and rooftop HVAC/water tanks.
* Eight deterministic shop color/style variants. This is an **intermediate pass**, not a claim that every storefront is unique enough or photorealistic. Japanese typography and decals are still pending licensed art.
* No change to the 42 Kenney cars, four perimeter walls, 16 road ends, 122 V05 collision proxies, POS, inventory, twins or saved state. New decorations are non-colliding until walkthrough QA.

## Layer contract
Place **V03 owner-private base + V05 complete replacement overlay + V06 additive details** at world position (0,0,0), rotation 0, scale (1,1,1).

Do not stack V04 with V05: V05 replaces the complete V04 extension and already includes the 42 Kenney vehicles. Do not treat V06 as a full standalone map.

## Deliverables
* `Tools/StreetExterior/build_v06_facade_detail.py` — deterministic GLB generator, based on 60 existing facade IDs in `Docs/StreetExterior/V04_DENSE_GRID_LAYOUT.json` and checked against the existing V05 manifest.
* `Tools/StreetExterior/test_v06_facade_detail.py` — orientation, layout-contract and export/reload tests.
* `.github/workflows/build-v06-facade-detail.yml` — source regression, model generation, SHA/manifest verification, LFS publishing to feature branch.
* `ArtSource/Generated/V06_CC0/SORA_V06_9Block_Facade_Details.glb` and `manifest.json` — generated artifacts from CI after successful run.
* `Assets/SORA/Scripts/Editor/SoraV06FacadeStaging.cs` — art-only Unity preview scene creation, keeping `SORA_Prototype.unity` untouched.

## Tests and limits
The generator was executed against a synthetic but structurally valid fixture: 60 facades generated 4,868 detail nodes and a 867,556-byte GLB, then passed three local offline tests including GLB reload. This is not real Unity evidence. GitHub CI must rerun against the checked-in V04 layout and V05 manifest. **Unity FBX import, URP shader mapping, real camera/character collision and Windows profiling remain NOT RUN.**

## Unity visual inspection
1. Checkout feature branch and `git lfs pull`; wait for V06 CI to publish output GLB.
2. Convert V06 GLB to FBX using `Tools/StreetExterior/convert_glb_to_fbx_blender.py`, output `Assets/SORA/Environment/StreetExterior/PublicGenerated/SORA_V06_9Block_Facade_Details.fbx`. Inspect its textures/materials. Do the same with V05 in `PublicGenerated` if needed.
3. Keep owner-private V03 FBX in `Assets/SORA/Environment/StreetExterior/PrivateGenerated/SORA_Neighborhood_V03_Integrated.fbx`. Do not publish AFX models/derivatives.
4. Run Unity menu `SORA > Street Exterior > V06 - Build detailed art QA scene`. The output scene is `Assets/SORA/Scenes/SORA_Neighborhood_V06_ArtQA.unity`, intentionally **without** `SoraDemoCore`.
5. Inspect every store front, side and rear at pedestrian/camera height. Check floating geometry, occlusion, glass, material/normal/roughness, signage, gaps, mesh orientation, collider intersections, and night contrast.
6. Record real EX-01…12 storefront QA and Unity screenshots. Only after coordinate verification should a single-world gameplay adapter replace the primitive background, with regression on both characters, camera, POS, saves and Windows FPS.

## Next gate
V06.1: inspect/repair alignment and choose two adjacent facades for complete authored art + rainy-night lighting. V06.2: replace the old procedural outdoor geometry in the playable scene with a single V05+V06 world and pass regression, retaining fallback until acceptance.
