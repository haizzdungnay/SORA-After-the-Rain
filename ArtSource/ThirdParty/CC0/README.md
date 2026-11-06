# SORA V02 — CC0 source assets (owner-supplied and verified)

**Status: Uploaded to this branch using Git LFS.** These are **source art archives**, not yet Unity-imported prefabs. All five source/optimized files are registered in [manifest.json](manifest.json), including SHA-256 and upstream URLs.

| Asset | Git LFS path | Source / rights |
|---|---|---|
| 3DAssets.dev Izakaya Alley and Ramen Shop | `IzakayaAlley/izakaya_starter_scene.glb` | [Publisher](https://3dassets.dev/packs/izakaya-alley-and-ramen-shop), CC0 |
| 3DAssets.dev Japanese School and City Street | `JapaneseSchoolCity/school_city_starter_scene.glb` | [Publisher](https://3dassets.dev/packs/japanese-school-and-city-street), CC0 |
| Poly Haven Asphalt 01 | `Asphalt01/asphalt_diffuse_2k.jpg`, `Asphalt01/asphalt_normal_gl_2k.png`, `Asphalt01/asphalt_roughness_2k.png` | [Publisher](https://polyhaven.com/a/asphalt_01), CC0 |

The two starter GLBs are exact byte-for-byte matches of the owner-provided files (verified SHA-256). They **must be decomposed into reusable storefront/prop modules**, not placed unchanged as a copied Japanese neighborhood. They currently contain `KHR_mesh_quantization`, `EXT_texture_webp` and in the school file `KHR_materials_transmission`; check importer support before accepting Unity visual QA.

The owner supplied Poly Haven's **original 4K Blend ZIP** (84.49 MB compressed). To keep the game assets lean, only **three optimized 2K map outputs** were committed to LFS. The original ZIP SHA-256 is recorded in the manifest; the ZIP itself is intentionally not in GitHub. Import normal maps as **normal**, roughness maps as **data** (linear), not as diffuse/sRGB. In URP Lit, smoothness is derived from inverted roughness and must be tuned for rain/wet surfaces.

## Reproduction and download

```bash
git lfs install
git lfs pull
python -m pip install Pillow numpy opencv-python-headless
python Tools/StreetExterior/verify_cc0_v02.py ArtSource/ThirdParty/CC0
```

Run `python Tools/StreetExterior/import_cc0_v02.py --output-dir ArtSource/ThirdParty/CC0` to fetch the exact verified files again from the official CDN (the importer aborts if upstream SHA changes). A one-time, owner-scoped GitHub Actions workflow performed the import to this feature branch.

To use in Unity, copy/import only required parts into `Assets/SORA/Environment/StreetExterior/` using Unity/Blender; let Unity generate `.meta` files. Do not replace the existing `SORA_Prototype.unity`, modify the dual-character/POS/save systems, or report Play Mode as passed without actual Editor testing.

**No AFX-derived mesh or texture is stored in this CC0 directory**. AFX rights differ and its private source remains separate.
