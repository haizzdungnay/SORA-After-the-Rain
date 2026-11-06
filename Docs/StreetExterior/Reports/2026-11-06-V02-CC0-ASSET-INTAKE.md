# SORA V02 — Report: owner-supplied CC0 asset intake

**Date:** 2026-11-06 (Asia/Ho_Chi_Minh). **Feature branch:** `feature/neighborhood-v02-grid-blockout`. **Status: SOURCE ASSETS PUSHED, UNITY IMPORT NOT RUN.**

## Received files and verified identity

| Local upload | Verified official product | SHA-256 of received original | Size |
|---|---|---|---:|
| `model.glb` | [Izakaya Alley and Ramen Shop](https://3dassets.dev/packs/izakaya-alley-and-ramen-shop) — starter scene | `693cebbe5a8cf9e30710c707adfb1daf76140b28a14f289cdca9ebd879e0748f` | 1,727,580 B |
| `model (1).glb` | [Japanese School and City Street](https://3dassets.dev/packs/japanese-school-and-city-street) — starter scene | `3169feade33f5df2e67080c96a4c939b427fec558f4e42f60a438386697d0f70` | 2,337,352 B |
| `asphalt_01_4k.blend.zip` | [Poly Haven Asphalt 01](https://polyhaven.com/a/asphalt_01) — Blend bundle + 4K maps | `a77bbd983524e29b1e684c6c79f8da47d512debd8ff867540d708e7d1832a239` | 84,494,529 B |

All three publishers label the content **CC0 1.0 Universal**. The owner supplied the actual source bytes. These resources are separate from the AFX asset with additional license restrictions.

## GitHub upload DONE via Git LFS

The [asset-import workflow](../../../.github/workflows/import-cc0-v02.yml) ran successfully on GitHub Actions: [run #42186358685](https://github.com/haizzdungnay/SORA-After-the-Rain/actions/runs/42186358685). It downloaded both GLBs and the 4K source image channels from official publishers, **checked the original SHA-256 against the owner upload**, created optimized 2K maps, ran a structural validator and committed **all five files with Git LFS**.

Actual repository paths:
- `ArtSource/ThirdParty/CC0/IzakayaAlley/izakaya_starter_scene.glb`
- `ArtSource/ThirdParty/CC0/JapaneseSchoolCity/school_city_starter_scene.glb`
- `ArtSource/ThirdParty/CC0/Asphalt01/asphalt_diffuse_2k.jpg`
- `ArtSource/ThirdParty/CC0/Asphalt01/asphalt_normal_gl_2k.png`
- `ArtSource/ThirdParty/CC0/Asphalt01/asphalt_roughness_2k.png`

`ArtSource/ThirdParty/CC0/manifest.json` records each final size and SHA-256. GitHub API checks confirmed that the resulting committed GLBs and asphalt JPEG are LFS pointer files with matching SHA-256 OIDs. The full **original 4K Blender ZIP is not pushed** to avoid unnecessary storage; its checksum remains here and in manifest.

## What was verified / not verified

- **PASS:** ZIP integrity, Blender + 4K EXR/JPG membership, GLB 2.0 headers, texture source hashes, glTF structural metadata, local 2K processing, five output files, local ZIP delivery, remote import action success, Git LFS pointer verification.
- **Source structure:** Izakaya starter GLB — **614 nodes, 274 meshes, 11 materials**. Japanese School/City starter GLB — **361 nodes, 165 meshes, 12 materials**.
- **Importer caveat:** both source scenes use `KHR_mesh_quantization` and `EXT_texture_webp`; school/city also uses `KHR_materials_transmission`. Check support in chosen Unity importer.
- **NOT RUN:** Unity import into `Assets/SORA`, per-shop prefab splitting, material conversion/normal/roughness setup, URP Play Mode, collision, visual QA, dual-character/POS regression, rain simulation or Windows build.
- **Missing:** Kenney City Kit Roads was not part of the three uploaded files; it remains optional for the next streets/intersections milestone.

## Next exact steps

1. Extract individually reusable source components into a named kit catalog (not the starter scenes as-is); inspect pivots, UV, material slots and exposed backs.
2. Select V02 shop IDs in `V02_GRID_LAYOUT.json` and assign compatible components. Avoid replacing the AFX neighborhood with either uploaded *starter scene*.
3. Import selected art into Unity **after** smoke-testing the importer on a separate staging scene; let Unity create its GUID/`.meta`, do not overwrite current gameplay bootstrap.
4. Apply Asphalt 01 via URP material setup (normal map non-sRGB, roughness linear and inverted/scaled as smoothness). Night/wet reflections must be authored and tested separately.
5. For a new feature PR, render genuine Unity walk-around captures of every selected facade and perform gameplay regression before marking it DONE.

**Source of truth:** [Asset register](../../ASSET_REGISTER.csv), [CC0 README](../../../ArtSource/ThirdParty/CC0/README.md), [manifest](../../../ArtSource/ThirdParty/CC0/manifest.json).
