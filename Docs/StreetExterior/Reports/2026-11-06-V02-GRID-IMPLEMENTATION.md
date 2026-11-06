# SORA — Neighborhood V02: real 3D grid prototype (2026-11-06)

## Delivered privately to the project owner

**Artifact:** `SORA_Neighborhood_V02_Private_2026-11-06.zip`, downloadable in the originating ChatGPT conversation; contains `SORA_Neighborhood_V02_Grid.glb` (~95.17 MB), full Python generator, VS01 base generator dependency, nominal layout, candidate storefront CSV, preview renders, and optional Blender/Unity staging helpers. **Original 112 MB AFX source is NOT included**; owner already has the legitimate original. **Do not place AFX-derived binary assets in this public repository.**

## Actual work performed

- Built a **new** 126 × 126 m nominal square neighborhood, not a copied AFX alley or simple expansion of the old primitive 168 × 143 district.
- 3 streets oriented East–West + 3 North–South, centered at world coordinates `-45, 0, 45`; **9 real crossings** with 216 white zebra-marking meshes, temporary tactile tiles, road centerlines, 9 m street surfaces and connected walkways.
- Four separate plot blocks: NW, NE, SW, SE, with sidewalks/curbs and small internal courtyards.
- **16 candidate shop/exterior IDs**, 4 per block; SORA MART at NE-01 `(17,0,6.75)`. Each block contains 4 uniquely assigned exterior models; the building positions and source mappings are in [V02_GRID_LAYOUT.json](../V02_GRID_LAYOUT.json).
- Repositioned/scaled/rotated the owner-supplied source AFX groups **A-J individually**, augmented with original SORA-made exteriors for the missing store types and 16 distinct storefront signs using Japanese typography. Reused source vending and pole props at new locations.
- Created a genuine GLB asset from geometry and textures, **1,161 geometry instances**. GLB was reloaded and checked with an offline script; project construction script assertions passed.
- Produced previews: 4 exact-geometry clay views of full district (not final textured render), 2 textured VTK crop views of SORA MART and neighboring shops, plus a labeled top-down plot map.
- **No Unity gameplay code was changed**; this is a separate asset and private Unity staging workflow for later import/review.

## Known blockers / art quality

- All 16 shop IDs are **visual prototypes**, not 16 finished exteriors. Visible side/back walls of some AFX source groups remain sparse; 4 courtyards still lack believable urban infill. The owner should review the square-grid organization before a larger asset production pass.
- Road geometry currently uses authored preview surfaces; real asphalt PBR, wet decals, rain exclusion under eaves, puddle reflection probes and nighttime URP lighting remain unimplemented.
- Imported FBX, shader/material mapping, UV normal directions, mesh colliders, accessible storefront/camera walk paths, NavMesh, build and FPS have **not** been validated in Unity.
- Full VTK render with every high-resolution embedded AFX texture exceeded this execution environment's memory; renderer produced full-district geometry-only views and a separate region-limited textured view instead. The **GLB itself retains source textures**.

## Test results

| Gate | Actual result |
|---|---|
| 3×3 road topology and 9 junctions | PASS structural |
| 4 blocks with separated land/sidewalk/curb | PASS structural |
| Unique IDs for 16 proposed buildings including SORA MART | PASS structural |
| Original source group instances + props | PASS geometry |
| GLB save / read / source code compile | PASS |
| VTK cropped source-texture viewport | PASS preview only |
| Full city render with PBR lighting | NOT RUN |
| Blender converter / FBX export | NOT RUN |
| Unity compile / Play Mode / gameplay regression | NOT RUN |
| Windows build and hardware benchmark | NOT RUN |
| Every store EX-01..12 completion | NOT PASS |

## Remaining work packages proposed

- V02-01: Owner review topology, square-grid road hierarchy, site of SORA MART, 16 candidate facade inventory.
- V02-02: Import CC0 street kit and complementary Japanese shops; improve road intersections, signs, side facades, roofs and interiors-visible-through-glass.
- V02-03: Blender export and Unity staging (visual only, no duplicate world), calibrate scale, colliders and camera.
- V02-04: Rain night PBR materials, reflection, illumination, pedestrian-level art QA of all accessible surfaces.
- V02-05: Extend to additional parcels after V02 art/interaction gates pass; protect existing POS, switch and save schema.

## Source and distribution handling

Original AFX scene, its derivatives and texture bytes are only in owner-held local files; public GitHub contains metadata and original scripts/docs. Before official release, re-evaluate asset redistribution and embedding constraints, and keep attribution/provenance register current. No third-party model is pushed into this PR.
