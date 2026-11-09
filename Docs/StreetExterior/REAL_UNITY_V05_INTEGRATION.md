# SORA — REAL Unity city integration V03 + V05 + V06 (2026-11-09)

> **This replaces the previous documentation/staging-only approach.**
> The source code in `Assets/SORA/Scripts/SoraDemoWorld.cs` now invokes `SoraCityRuntimeWorld.TryBuild(world, game)` in the **existing playable scene bootstrap**. When the imported GLB assets are present, neither `SetupStreet()` nor `SoraNeighborhoodDistrict.Build()` runs. The existing store POS, stock/inventory, two protagonists, dialogue, camera, rain and schema-1 saves remain active.
>
> **Actual status:** Unity-ready source/binary asset integration **committed**, CI file integrity PASS. **Unity compilation, imported prefab results, Play Mode physics and Windows build NOT TESTED** in this working environment. Do not call this a validated playable build until an actual Unity Editor log and player capture exist.

## 1. What is actually inside Unity `Assets/`

The three **real GLB binaries** are copied by [GitHub Actions](../../.github/workflows/install-real-city-unity-assets.yml) from Git LFS ArtSource, compared byte-for-byte to source manifests, and stored at:

| Unity asset path | Content | Real source size |
|---|---|---:|
| `Assets/Resources/SoraCity/Models/V03_CC0_Overlay.glb` | 23 CC0 Japanese shop façades/upper floors, props and PBR asphalt | 20,782,520 bytes |
| `Assets/Resources/SoraCity/Models/V05_Kenney_9Block_Overlay.glb` | 5 added blocks, 60 building shells, **42 Kenney Car Kit cars**, 16 crossings and perimeter structures | 20,653,268 bytes |
| `Assets/Resources/SoraCity/Models/V06_Facade_Details.glb` | **3,301 authored facade-detail geometry nodes** for 60 newer storefronts: awnings, doors, windows, balconies and utilities (additive only) | 567,224 bytes |
| `Assets/Resources/SoraCity/V02_GRID_LAYOUT.json` | Original 16 lot positions and dimensions used to close the public-mode 15 shop shells | TextAsset |
| `Assets/Resources/SoraCity/V05_COLLIDERS.json` | **122** actual 3D collider proxies for 60 new buildings, 42 cars, 4 perimeter walls and 16 terminal barricades | TextAsset |

Every file above is in `Assets/`, **not just** `ArtSource/`, `Tools/`, a separate staging scene, or a handoff ZIP. GLBs are managed with **Git LFS** and stable `.meta` GUIDs. A new direct `com.unity.cloud.gltfast@6.20.0` dependency imports `.glb` into native Unity GameObjects. The actual prefab import outcome must be inspected in Unity.

The **older V04 overlay is not loaded** in the playable scene. V05 **replaces** V04 (otherwise vehicles/streets would duplicate). V03 + V05 have a common origin.

## 2. Actual gameplay hook

```text
SORA_Prototype.unity (unchanged scene file)
  -> SoraDemoCore.Start()
     -> SoraDemoWorld.Build(this)
        -> SetupEnvironment() + SetupShop() [existing POS and store interior]
        -> SoraCityRuntimeWorld.TryBuild(world, game) [NEW - actual GLB art + colliders]
           -> public V03 + public V05 + V06 details + 15 closing shells
              OR (when installed) private AFX V03 + public V05 + V06 details
           -> BoxCollider wall, cars, new buildings, floor
           -> SoraCityBoundaryGuard for both twins
        -> SetupTwins() [original]
        -> SetupCustomers(), SetupCamera(), SetupRain() [original]
        -> SetupStreet() + SoraNeighborhoodDistrict.Build(world)
           ONLY WHEN THE IMPORTED ASSETS ARE MISSING OR INVALID
```

V06 is purely **additive** and must never replace V05 or create new colliders; the loader instantiates it once at the same world origin. The full 3D runtime world is V03 (private AFX if supplied, else public CC0) **plus V05 replacement and V06 details**.

The GLB coordinate `(17,0,6.75)` is moved by **(-17,0,-12)** so the authored SORA MART storefront matches the existing gameplay entrance at approximately `(0,0,-5.25)`. There is no rewrite of character positions in the save file.

The runtime Hierarchy must contain:
```text
SORA - Playable World (real city assets when available)
  |- (existing store/POS/twins/camera/rain objects)
  |- SORA_CITY_V05_REAL_ASSETS - 9 blocks - 42 Kenney vehicles
       |- V03 CC0 real shopfronts ... [or V03 AFX owner private]
       |- V05 real Kenney Car Kit ...
       |- V06 REAL facade details ...
       |- V02 15 building shells ... [CC0-only mode]
       |- REAL CITY COLLISIONS ...
```

## 3. Import the owner-held AFX architecture **for real**, without violating source rights

The project owner has a separate ZIP in this conversation:

`SORA_Unity_Private_AFX_Install_2026-11-09.zip`

It contains the full owner-supplied AFX-derived **V03 city GLB**, renamed to
`Assets/Resources/SoraCity/Private/V03_Integrated_NoMart.glb`.

This is an **actual GLB**, not a text reference. It retains **2,463 imported mesh nodes**, preserving the AFX architecture and CC0 city art while hiding 129 mesh nodes of the duplicated V03 SORA MART shell, to leave the original interactive POS store accessible.

To install:
1. Clone/checkout this feature branch; `git lfs pull` for all three public Unity GLBs.
2. Extract the private ZIP **directly into the local Unity project's root**, preserving the exact `Assets/Resources/SoraCity/Private/V03_Integrated_NoMart.glb` path.
3. Open Unity Hub using **Unity 6000.3.7f1** and allow `glTFast` / all GLBs to finish import.
4. Open the existing `Assets/SORA/Scenes/SORA_Prototype.unity` and press **Play**.
5. Observe the Console line `SORA CITY: REAL V03 + V05 + V06 GLB meshes instantiated...` and `private AFX=True`. Verify the hierarchy listed above.
6. **Do not place V04 staging scenes or old V04 overlay into this scene.**

Because of additional AFX distribution restrictions, `Assets/Resources/SoraCity/Private/` is **gitignored** and not part of the public repo. The released game will need a separate license review before any AFX-derived geometry can be distributed.

If the private GLB is not installed, the gameplay still loads the **public CC0 V03 + V05 + V06 asset models** from Unity's `Assets/Resources/`. The first 15 exterior bodies use simple authored side/rear/roof underlays, because the restricted AFX source is missing. They are an honest quality fallback, not finished Japanese exteriors.

## 4. Real Unity acceptance checklist (NOT yet run)

- [ ] Open Unity project on **6000.3.7f1 / URP 17.3.0**. Package Manager resolves glTFast 6.20.0.
- [ ] Inspect all three GLB assets in Unity Project/Inspector; check materials, mesh count and any glTF extension warnings.
- [ ] Run **`SoraCityRealAssetTests`** in Test Runner EditMode; assert imported V03/V05/V06 mesh renderers and 122 collider entries.
- [ ] Play `SORA_Prototype.unity`. In Hierarchy confirm `SORA_CITY_V05_REAL_ASSETS`. Check there is no `SORA Neighborhood v0.5 - Connected Blocks` object.
- [ ] Walk out of SORA MART and inspect CC0/AFX buildings and Kenney cars. Check building/road/actor scale and storefront alignment.
- [ ] Try **both twins (Tab)** at all 16 end-of-road barriers and 4 perimeter corners. Ensure no fall into void. Verify collision proxy scale against glTF imported geometry.
- [ ] Press E on POS and inventory, test delivery/restocking, F5 save and F9 load. Confirm old locations and interactions are stable.
- [ ] Check rain, material roughness, transparent shop glass, glTF shader variants, nighttime lighting and FPS.
- [ ] Capture true Unity Editor/Play Mode screenshots and attach logs; only then mark Unity gameplay and visual quality PASS.

### Reproduction / static verification

```bash
git lfs install
git lfs pull
python Tools/validate_project.py
python Tools/StreetExterior/verify_unity_city_install.py .
```

Static validation tests the committed assets, hashes and gameplay source hook. It **cannot replace the Unity Editor tests**.

**Source provenance:** [V03 real model](../../ArtSource/Generated/V03_CC0/README.md), [V05 real Kenney model](../../ArtSource/Generated/V05_CC0/README.md), [asset register](../ASSET_REGISTER.csv). No AFX raw or derived bytes are committed publicly.
