# SORA V05 — Kenney Car Kit 3.1 integration report

**Date:** 2026-11-06 (Asia/Ho_Chi_Minh). **Objective:** The owner provided `kenney_car-kit.zip` to replace 42 procedurally generated placeholder vehicles in the nine-block V04 map and asked that source be added to GitHub. Result: **DONE FOR 3D MODEL; UNITY QA PENDING**.

## User upload verified

- `kenney_car-kit.zip`: **4,814,237 bytes**, **268 ZIP entries**, no damaged members, SHA256 `fac7dacac5c7874348cf19729af3ef205f3d366493edaf0a827d93f4fdf3d0c4`.
- `License.txt` states **Kenney Car Kit (3.1), CC0**, personal/commercial use allowed, optional attribution.
- ZIP contains 50 separate `Models/GLB format/*.glb` (vehicle variants and wheels/parts), their corresponding FBX/OBJ and shared `Models/GLB format/Textures/colormap.png`.

## Actual private model assembled and delivered

A privately packaged owner artifact `SORA_Neighborhood_V05_Kenney_Private_2026-11-06.zip` includes the owner-held V03 GLB (with AFX material), a new full V05 replacement for V04 with real Kenney models, Blender conversion script, both scene/material-preview images, exact source ZIP/CC0 license, collision and model manifest and rebuild script.

- **42 individual parked cars**, eight chosen Kenney variants: hatchback-sports, sedan-sports, sedan, taxi, truck, delivery, SUV, van.
- **Removed 756 placeholder car mesh instances** from the V04 owner-private model; added **225 Kenney mesh instances**, resulting in **3,241 mesh instances**.
- Original scale of the Kenney sedan source was ~1.5 × 1.3 × 2.55 units. Vehicles were non-uniformly scaled into type-aware target sizes (kei/sedan/van/delivery) and grounded at the same 42 coordinates, so they do not float or require moving any building.
- **Color fix:** Kenney individual GLBs refer to a shared **external palette texture**. Embedded the ZIP's `colormap.png` in the final exported GLB; real VTK rendering verified colored cars instead of blank gray.
- **PRIVATE V05 GLB:** 21,356,436 bytes, SHA256 `c4800a5f47af6bb4e2740b31ea1417b0644298e3b7fb60b3f6b0e6fb415f3f97`. Real GLB exported and reloaded.
- Saved all car collision AABBs and **kept the original 4 closed map-boundary walls + 16 road-end barriers**; no gameplay code changed.

## Public source and GLB on GitHub via Git LFS — confirmed

The publisher's direct Kenney ZIP endpoint redirected GitHub's unattended runner to HTML; the checksum guard correctly rejected that response. **OpenGameArt** hosts the same Car Kit 3.1 ZIP uploaded by **Kenney**, CC0. The runner accepted this alternate download **only after its SHA-256 matched the owner upload exactly**.

[Successful GitHub Actions run #42198697484](https://github.com/haizzdungnay/SORA-After-the-Rain/actions/runs/42198697484) completed the import, model replacement, GLB reload, metadata and LFS push.

- `ArtSource/ThirdParty/CC0/KenneyCarKit/kenney_car-kit.zip` — **4,814,237 bytes**, LFS OID `fac7dacac5c7874348cf19729af3ef205f3d366493edaf0a827d93f4fdf3d0c4`.
- `ArtSource/Generated/V05_CC0/SORA_V05_Kenney_9Block_Overlay.glb` — **20,653,268 bytes**, **2,198 mesh instances**, LFS OID `805fa2e82e5243361db1a7ab8c897462a235e0da7d3f171622eff042ef13f603`.
- Public CC0 V05 covers 42 individual cars in **six** model variations (its base V04 contained slightly different vehicle categories from the private edition). No AFX-derived asset bytes published.
- `ArtSource/Generated/V05_CC0/manifest.json` — exact 42 vehicle IDs/mesh replacements; `colliders.json` — **122 proxies** (60 buildings, 42 cars, 4 continuous boundary walls, 16 road-end obstacles). The GitHub LFS pointer contents were retrieved and checked against the manifest.

## Design constraints — do not violate

- **Use V03 base + V05 full overlay at the same origin**. Do NOT stack V04 and V05: V05 is a **replacement V04 layer** (contains both all the previous buildings/roads and the Kenney vehicles). Stacking both produces duplicate buildings and collisions.
- Existing SORA MART, the 9 blocks, 84 exterior buildings and 16 junctions are retained; the car pack only replaces road dressing.
- Only CC0/SORA-authored source and resulting geometry are public; original/modified AFX remains in owner-private V03.
- `Tools/Unity/SoraNeighborhoodV05Staging.cs` creates an **art-only staging scene**, importing local V03 + V05 FBX plus updated physical proxy colliders without invoking `SoraDemoCore` or writing to `SORA_Prototype.unity`.

## Verification completed vs pending

| Scope | Actual status |
|---|---|
| ZIP CRC + Kenney CC0 license + exact owner SHA-256 | **PASS** |
| 42 vehicle placements and removed old mesh nodes | **PASS**, geometry and manifest |
| Shared colormap attached and VTK preview shows actual colors | **PASS**, offscreen GLB-based render |
| Private and public rebuilt GLB export/reload | **PASS** |
| Public Git LFS source archive and output SHA | **PASS**, verified GitHub responses |
| Boundary and 16 road-end barriers metadata continuity | **PASS**, structural |
| Unity Editor/URP material import, `.meta` & GUIDs | **NOT RUN** |
| CharacterController/Tab switch/collision against all boundaries | **NOT RUN** |
| POS, inventory, save/load, camera/NPC gameplay regression | **NOT RUN** |
| Night rain, wet surfaces, commercial art polish, FPS | **NOT RUN** |

**Next:** Import V03 + V05 in a Unity Editor environment, verify shader colors/scale, stage 42 vehicle colliders and physically try traversing each boundary with **both** twins. This remains separate from gameplay bootstrap until regression passes.
