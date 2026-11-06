# SORA V03 — Public CC0 3D overlay

**This directory contains a REAL generated model, not only a plan:**

- [SORA_V03_CC0_Overlay.glb](SORA_V03_CC0_Overlay.glb) — 20.78 MB, binary stored via Git LFS.
- [manifest.json](manifest.json) — SHA-256, geometry assertions and source provenance.

This GLB includes **23 shopfront/upper-floor module groups**, props for all **24 total SORA parcels**, **8 newly authored complete building shells**, courtyards and **15 PBR asphalt road sections**. SORA MART's authored geometry and the old 16 V02 shop masses are *not* duplicated. The overlay therefore **must align with the owner-held V02 base at world origin (0,0,0)**.

## How to obtain both asset layers

```bash
git lfs install
git lfs pull
# Rebuild public overlay from registered CC0 inputs:
python -m pip install 'numpy>=1.26,<3' 'trimesh>=4.5,<5' 'Pillow>=10,<13'
python Tools/StreetExterior/build_v03_cc0_overlay.py --root . --output ArtSource/Generated/V03_CC0
```

The full assembled local `SORA_Neighborhood_V03_Integrated.glb` (116 MB, 2,592 objects) is privately delivered to the owner in a separate ZIP, because it includes AFX-derived material. Do **not** copy that file to public GitHub without authorization from AFX author.

## Unity handoff

- Download/install Blender and use the conversion script from the owner's private V03 package on **both** `SORA_Neighborhood_V02_Grid.glb` and `SORA_V03_CC0_Overlay.glb`, obtaining matching FBXs and textures.
- Copy to `Assets/SORA/Environment/StreetExterior/PrivateGenerated/`.
- Copy `Tools/Unity/SoraNeighborhoodV03SceneBuilder.cs` into the Unity Editor scripts folder and choose `SORA > Street Exterior > V03 - Create art staging scene`.
- This creates **art-only staging**. No Unity compilation, URP import test, actual rainfall or gameplay regression has been executed. The prototype scene and save state stay untouched.
- Inspect every front, side, rear and roof, including camera near-collision, before a playable-map claim. Do not swap active scene just because geometry exports.

See [integration and QA report](../../../Docs/StreetExterior/Reports/2026-11-06-V03-CC0-INTEGRATION.md).
