# SORA VS01 — First real 3D street vertical slice (2026-11-06)

**Status: IMPLEMENTED LOCALLY, UNITY NOT TESTED.** Owner requested actual modelling, not another planning-only milestone. Private downloadable artifact pack delivered in this conversation; GitHub stores source-level instructions/verification and metadata only, not AFX-derived meshes/textures.

## What was built
- A **new** 48 m straight commercial street along SORA world X-axis, 8 m asphalt road, north/south pavements, curbs, lane stripes, zebra crossing, drains, bollards, posts and road lamps. **Not the layout of the source AFX alley**.
- Original SORA MART visual geometry at x=0, z=0..11 with a 14 m frontage, roof, cladding, glass panes, framed central entrance, custom SORA MART sign, merchandising silhouettes, shelf stock, cashier-counter silhouette, ceiling lights and side-wall detail. Existing POS and warehouse gameplay are not duplicated or replaced.
- Two independently relocated, rotated and rescaled AFX **source mesh groups A and G** (16 and 14 mesh nodes) forming test neighboring stores (Midori Ramen and Hoshi Books) with original storefront signs/awnings authored for the slice.
- One AFX vending mesh and one pole mesh isolated and moved independently. Total export: **271 mesh instances**, world extents approximately x ±24 m, y -0.36..8.49 m, z -15..11.15 m.
- A 149-row source node catalog with source IDs, bounds, UV presence, candidate category and status. This is a candidate catalog, **not** a validated semantic prefab library: source nodes can still contain multiple fused details.
- New night-toned preview renders of the **assembled GLB**, not images generated from a prompt and not Unity screenshots. A UV mirroring error on all three original sign artworks was found in visual QA and corrected.

### Artifact location/rights
Owner receives a separate **private ZIP** with `SORA_Street_VSlice_VS01.glb` (~56 MB), builder Python source, preview PNGs, sign art, source-node catalog/manifest, Blender FBX conversion helper and optional Unity Editor staging script. The ZIP deliberately excludes the owner's original ~112 MB AFX GLB to avoid duplicating it. Raw/modified AFX geometry MUST NOT be committed to the public repository; creator rights still need a release-level review.

## Current validation

| Item | Result |
|---|---|
| Original AFX GLB parsed and used | PASS — owner-provided source |
| Independently transformed AFX mesh groups | PASS — groups A/G plus source props |
| Exported real textured GLB | PASS — 271 instances |
| Top-down / pedestrian / hero renders | PASS — VTK GLTF renderer, UV signage correction checked |
| Component metadata catalog | PASS — 149 candidate mesh nodes |
| Real Unity import, FBX material conversion | NOT RUN — Blender/Unity unavailable in this execution |
| URP nighttime lighting, true rain particles and reflections | NOT RUN — weather is a subsequent implementation task |
| Gameplay / character / camera / POS / save-load / Windows build | NOT RUN — the original playable prototype remains untouched |
| Every exterior side of every store | NOT PASS — source back/side faces and storefront finish need manual remodeling |

**Important:** This is an early visual slice, not a completed or commercially polished town. It is an actual 3D result, but it must be critically reviewed against SORA's required visual quality before scaling to the whole district.

## How to use / continue
1. Download the private VS01 pack from the conversation; preserve the owner AFX source GLB separately.
2. Open `SORA_Street_VSlice_VS01.glb` directly in Blender for immediate inspection. Inspect camera and source UV, especially the outer faces.
3. To rebuild locally, use the pack's `builder/build_sora_street_vslice.py` with `--source <owner GLB>` and `--output <local private folder>`; requirements numpy, trimesh and Pillow.
4. Optional: convert using `builder/convert_glb_to_fbx_blender.py` then copy the private FBX plus texture directory to `Assets/SORA/Environment/StreetExterior/PrivateGenerated/`. The private pack includes a Unity Editor staging scene helper; Unity must create its `.meta` files on the owner's machine.
5. Do not replace `SORA_Prototype.unity` or enable any new runtime bootstrap. First pass collision/camera/material/night weather QA in a **separate staging scene**, then extract production-quality individual prefabs and integrate via a single-world gameplay adapter in a later reviewed PR.

**Next scope:** `EXT-003` semantic source mesh classification and pivot/material/UV cleanup; sharpen SORA MART front/side/glass art; validate FBX and URP materials in Unity; two-storefront art quality gate. Then progress to independent district layout G1 and later segments.

## Relevant project decisions
- [Kitbash city decision](../AFX_KITBASH_CITY_DESIGN_DECISION_2026-11-06.md)
- [Phased rebuilding plan](../../STREET_EXTERIOR_REBUILD_PLAN.md)
- [Existing geometry inventory](2026-11-06-G0-AFX-GLB-INSPECTION.md)
