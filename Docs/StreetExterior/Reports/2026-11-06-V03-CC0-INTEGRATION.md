# SORA Neighborhood V03 — Asset integration & map production report

**Date:** 2026-11-06. **Base:** V02 grid, `feature/neighborhood-v02-grid-blockout`. **Task:** integrate owner-supplied CC0 Japanese meshes and Asphalt 01 into the SORA original neighborhood (not copy either starter GLB as a scene).

## Delivered REAL model data

| Aspect | Verified result |
|---|---|
| Target neighborhood | 126 × 126 m nominal, 3×3 street grid |
| Intersections / blocks | 9 crossings / 4 city blocks |
| Exteriors | 24 positions total (16 V02 + 8 newly authored infill shells) |
| Shop kitbashed CC0 facades | 23 positions (SORA MART keeps its existing authored exterior) |
| Street props | 24 shop-adjacent prop groups, including bicycles, lanterns, plants, A-frame signs and some vending machines |
| Small plazas | 4 courtyards with trees, bike racks and flower beds |
| Road surfaces | 15 Asphalt 01 tiled PBR sections; 2K diffuse/normal/roughness converted to glTF metallic-roughness channels |
| **Private fully assembled GLB** | `SORA_Neighborhood_V03_Integrated.glb`, 116,090,804 bytes, **2,592 mesh instances**, 419,187 instance triangles, geometry AABB X/Z ±63 m; GLB SHA-256 `516d190636ff15c9003f168a94f521af54fc91a60dbb12ca78e181ea83f7d874` |
| **Public CC0-only overlay GLB** | `ArtSource/Generated/V03_CC0/SORA_V03_CC0_Overlay.glb`, 20,782,520 bytes, **1,361 mesh instances**, Git LFS SHA-256 `9a753ddecfd14bd1da8796f9e36083054dddab89be888e7ef764da751e399733` |

The private assembled GLB includes owner-held AFX-derived V02 geometry, original SORA-made buildings and new CC0 pieces. It is delivered separately to the owner as a ZIP, not pushed to the public repo. **The public overlay contains NO AFX models** and aligns at the exact origin with the private V02 GLB/FBX. Combining the two creates the upgraded street environment without importing source packs as intact scenes.

### Files on the GitHub branch

- `ArtSource/Generated/V03_CC0/SORA_V03_CC0_Overlay.glb` (actual Git LFS model binary)
- `ArtSource/Generated/V03_CC0/manifest.json` (asset SHA, QA and source metadata)
- `Tools/StreetExterior/build_v03_cc0_overlay.py` (fully reproducible standalone overlay generator using only repository CC0 files)
- `Tools/Unity/SoraNeighborhoodV03SceneBuilder.cs` (Editor-only separate staging scene for V02 base + V03 overlay; moon, preview rain, camera, anchors and temporary colliders)
- `Docs/StreetExterior/Reports/2026-11-06-V03-CC0-INTEGRATION.md` (this report)
- The optional local private ZIP includes the **tested complete V02+AFX+CC0** V03 GLB, its Python generator, labeled plan, real geometry renders and manifest.

## Critical fix — GLB source quantization

Both 3DAssets.dev starter scene GLBs encode POSITION as signed 16-bit integer with `normalized:true` (`KHR_mesh_quantization`). Without applying normalized conversion, a generic trimesh import produces unrealistic extents (up to hundreds of kilometers). Both integration pipelines check the glTF accessor type and normalize vertex positions, then use each source node's world transform to extract named components. The source layout was NOT copied into SORA.

## Tests actually executed

- **PASS (local):** original user-provided files parsed; mesh normalization; modular kit placement; extra building shell creation; full V03 GLB export and reload; 2,592 instances and 419,187 triangles; 24 distinct exterior IDs; 23 CC0 facades; 24 prop zones; 8 infill shells; 4 courtyards; 15 asphalt material sections; genuine clay and cropped textured GLB renderer captures.
- **PASS (remote):** [GitHub Actions V03 CC0 build](https://github.com/haizzdungnay/SORA-After-the-Rain/actions/runs/42189003506). Public overlay model generated on Linux from CC0 source in Git LFS, GLB reopened, SHA/membership checks passed; **20.78 MB LFS binary committed**.
- **NOT RUN:** Blender FBX conversion on this machine, Unity Editor / URP import/material fixes, actual Unity C# compilation, Play Mode, weather dry zones, collision/camera checks, two-character/POS/save-load regression, Windows x64 build and FPS profiling.
- **NOT FINAL ART PASS:** some source façades and roof/side/bank geometry remain less detailed than the desired commercial visual bar; VTK preview lighting renders some dark glass/sign panels. A VTK preview is NOT a Unity quality guarantee. Rain PBR smoothness tuning and pedestrian-level QA for each of 24 buildings remain needed.

## Next phase to actually play this neighborhood

1. In a Unity workstation clone the V02/V03 feature branches with `git lfs pull`. Open the owner-provided private V02 or private V03 package **outside the public repository**.
2. Use Blender to convert `SORA_Neighborhood_V02_Grid.glb` and the public `SORA_V03_CC0_Overlay.glb` to FBX while preserving materials and texture paths, then import both FBXs to `Assets/SORA/Environment/StreetExterior/PrivateGenerated/`. The original private assembled V03 GLB is an alternative single model for visual inspection, not required if combining the two aligned components.
3. Copy `Tools/Unity/SoraNeighborhoodV03SceneBuilder.cs` to `Assets/SORA/Scripts/Editor/`, then run **SORA > Street Exterior > V03 - Create art staging scene**. This creates `SORA_Neighborhood_V03_Staging.unity` and DOES NOT alter legacy gameplay or spawn a duplicate POS.
4. Fix any URP shader/texture conversion issues (color vs normal vs roughness, transparency and sign emissive); create authentic character-scale Unity screenshots and QA for every storefront/side.
5. Only then implement a reviewed **single-world** visual bootstrap that keeps saved character coordinates, SORA MART interaction/POS, camera, twin switching, inventory and quest systems consistent; do not merge staging directly into a playable scene without regression.

## IP and provenance

- [3DAssets.dev Izakaya Alley](https://3dassets.dev/packs/izakaya-alley-and-ramen-shop), CC0.
- [3DAssets.dev Japanese School & City Street](https://3dassets.dev/packs/japanese-school-and-city-street), CC0.
- [Poly Haven Asphalt 01](https://polyhaven.com/a/asphalt_01), CC0.
- Original owner-held AFX is **not** part of the public overlay, because creator supplementary terms restrict raw and modified source redistribution; official release requires rights review.
- `Docs/ASSET_REGISTER.csv` contains provenance, as does `ArtSource/ThirdParty/CC0/manifest.json`.

**Honest acceptance:** asset integration and a first real 24-exterior geometry map are implemented; final Unity playable-map and per-facade quality acceptance remain open.
