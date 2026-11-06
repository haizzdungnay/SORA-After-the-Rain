# SORA Street Exterior — VS01 tooling

This folder contains the **public, original-code side** of SORA's first generated 3D commercial-street vertical slice. The **actual derived GLB, reusable source kit GLBs and full local-build Python script** were delivered to the owner in a private ZIP in the 2026-11-06 conversation, not in this public repository. The original AFX GLB was owner-supplied and is not redistributed here.

## What exists

- [VS01 implemented design and QA report](../../Docs/StreetExterior/Reports/2026-11-06-VS01-IMPLEMENTATION.md)
- [VS01 3-store layout (JSON)](../../Docs/StreetExterior/VS01_LAYOUT.json)
- `extract_afx_component_groups.py`: real node-based AFX group extraction with pivot normalization; individual/semantic mesh splitting still requires review.
- `verify_vslice_artifact.py`: run structural checks against a privately generated SORA VS01 GLB and JSON catalog.
- [Unity Editor staging script](../Unity/SoraStreetVSliceSceneBuilder.cs): can create private art-only scene after converting imported GLB to FBX; **not tested in Unity**.

### Private asset workflows

1. Keep `japanese_street_at_night.glb` in `LocalReferenceAssets/AFX_JapaneseStreet/`.
2. Run `python Tools/StreetExterior/extract_afx_component_groups.py --source LocalReferenceAssets/AFX_JapaneseStreet/japanese_street_at_night.glb --output LocalReferenceAssets/AFX_Kits --groups A G VENDING POLE`.
3. Unpack the **private VS01 deliverable ZIP** obtained from the user conversation; its `builder/build_sora_street_vslice.py` is the tested *full* visual-slice generator (the public repo does not duplicate its full source until the packaging workflow is audited).
4. Open the resulting `SORA_Street_VSlice_VS01.glb` in Blender or glTF-compatible viewer; see `previews/` for actual GLB renders. In the private package, use `builder/convert_glb_to_fbx_blender.py` to convert to FBX and textures.
5. Copy the FBX/Textures into `Assets/SORA/Environment/StreetExterior/PrivateGenerated/` (gitignored), and copy `Tools/Unity/SoraStreetVSliceSceneBuilder.cs` into `Assets/SORA/Scripts/Editor/` on the local workstation. Let Unity generate metadata; use the Editor menu `SORA > Street Exterior > VS01 - Build staging scene`.
6. Run `python Tools/StreetExterior/verify_vslice_artifact.py LocalReferenceAssets/SORA_VS01` for geometry checks; **also** test import, materials, cameras, collisions and gameplay in Unity before merging any runtime changes.

**No current workflow uploads raw or modified AFX assets to GitHub.** Local reference, derivatives and local staging scene are excluded for distribution-rights reasons. Owner-approved experimentation is not equivalent to a grant to redistribute model data.

**Current quality stage:** early visual prototype. Source group mesh names are not a catalog of independently validated doors, windows, roofs or shopfronts. One needs to inspect/edit those parts in Blender, add missing outer faces and implement tested URP night/rain lighting before production use. The original game remains unchanged.
