# SORA Neighborhood V04 — 9 dense blocks

This folder contains a **real, generated CC0 + SORA-authored GLB**, stored via Git LFS:

- [SORA_V04_CC0_Dense_Overlay.glb](SORA_V04_CC0_Dense_Overlay.glb) — 20,254,020 bytes, SHA256 `8f96ca8d4a6a62d4f18556d8cf525b10d9a5af448c8a8f6f1961bfe17c2ac096`.
- [colliders.json](colliders.json) — 122 staging collider boxes (60 buildings + 42 parked cars + 4 perimeter walls + 16 terminal barricades).
- [manifest.json](manifest.json) — geometric QA, independent LFS checksum, no AFX data.

This model is an **additive overlay**. Keep the owner-private V03 neighborhood at **world origin (0,0,0)** and place V04 at the same transform. Do not move or resize the original SORA MART. The new environment has 4 roads per axis, 16 junctions, 9 total blocks, 84 total building shells and 42 placeholder cars. All four exterior boundary colliders are continuous and overlap at corners.

## Rebuild from repository

```bash
git lfs install
git lfs pull
python -m pip install 'numpy>=1.26,<3' 'trimesh>=4.5,<5' 'Pillow>=10,<13'
python Tools/StreetExterior/build_v04_dense_grid.py --root . --output ArtSource/Generated/V04_CC0
```

The builder reads `Docs/StreetExterior/V04_DENSE_GRID_LAYOUT.json` and both CC0 starter GLBs from `ArtSource/ThirdParty/CC0/`; it does **not** use any AFX geometry. The owner also received a **private richer 3D V04 overlay + V03 base**, including Japanese signs and full combined clay preview in the conversation deliverable. The private edition must stay outside the public repository because V03 contains AFX-derived assets.

## Unity (still not Editor-tested)

Convert V03 and V04 from GLB to FBX/Textures with Blender. Put them under `Assets/SORA/Environment/StreetExterior/PrivateGenerated/`. Copy `Tools/Unity/SoraNeighborhoodV04Staging.cs` to `Assets/SORA/Scripts/Editor/`, then run the menu `SORA > Street Exterior > V04 - Build 9-block staging scene`. The script uses this directory's `colliders.json` to instantiate wall/road/vehicle/building collision proxies. This creates a **separate art scene only**, not a new gameplay scene or POS instance.

The `SoraNeighborhoodV04BoundaryGuard.cs` is an **optional, untested** runtime failsafe; both controlled characters must be bound after reviewed gameplay integration. Do not claim playability or collision PASS until a Unity Editor test with character movement, Tab switching, camera, POS, save/load and both map edges is recorded.

See [implementation report](../../../Docs/StreetExterior/Reports/2026-11-06-V04-NINE-BLOCK-IMPLEMENTATION.md) and [free vehicle-art request](../../../Docs/StreetExterior/V04_FREE_ASSET_REQUESTS.md).
