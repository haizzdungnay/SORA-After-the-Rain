# Development and AI collaboration

## Source of truth

GitHub repository is canonical. Avoid divergent copies uploaded between chats. Begin each session by fetching the latest `main`, relevant feature branches and open PRs. Base design decisions on `Docs/GAME_DESIGN_DOCUMENT_v0.4.md` unless a later, reviewed update supersedes it.

## Branches & reviews

- `main` = reviewed snapshot, no speculative direct rewrites.
- `feature/character-switch`, `feature/store-pos`, `feature/visual-night-rain`, `feature/npc-social`, etc.
- `fix/<problem>` = corrective changes.
- Use a PR explaining change, affected files, validation evidence, visual before/after if relevant and risks.
- Do not label Unity builds PASS until Editor actually compiles; do not label screenshots real unless produced by a running Unity build.

## Unity asset rules

Commit scenes, prefabs, material assets and their `.meta` sidecars. Avoid committing generated `Library`, `Temp`, `Obj`, `Builds`, `Logs`, `UserSettings`, IDE files.

Large binary assets should be tracked with Git LFS BEFORE adding them:

```powershell
git lfs install
git lfs track "*.fbx" "*.blend" "*.psd" "*.tga" "*.exr" "*.wav" "*.mp4"
git add .gitattributes
git commit -m "chore: track art binaries with git lfs"
```

If using PNG/JPG for textures or previews, evaluate their sizes and optionally track them in LFS too. Ensure Git LFS storage/bandwidth limits are understood. Never copy assets or screenshots from another commercial game unless appropriately licensed. Track asset origin and license in `Docs/ASSET_REGISTER.csv`.

## Rules for coding assistants

1. Read `README.md`, GDD and the affected scripts before modifying systems.
2. Prefer minimal changes preserving interface contracts and shared world state.
3. Do not write duplicate POS transactions or reset shared world state when characters swap.
4. Preserve 18+ ages at start and sibling relationship. Romance with customers/NPCs only; **no incest narrative**.
5. Never claim tests have passed without running them and including actual output or logs.
6. Avoid installing packages or downloading model files unless licensing and compatibility are confirmed.
7. Keep secrets, tokens, Unity `.ulf`, account IDs and private local paths out of commits.
8. Verify Unity-generated scene/prefab `.meta` stability; use Unity for asset edits when possible.

## Model pipeline

- Store original editable Blender assets alongside export FBX only when rights and repository storage permit.
- Rigging, animation controllers, avatar replacements and shader variants should be separate PRs.
- Verify model licensing for commercial distribution and avoid copying third-party game assets.
- Each 3D visual PR includes Unity screenshot(s) from Play Mode, draw calls/triangles where available, and PC target test details.

## Example new AI work session

> Review the latest GitHub `main` of this SORA project. Implement a minimal `feature/character-switch` PR. Keep the existing gameplay systems and Unity pin unchanged. Supply modified files, tests actually run, Unity-specific tests still required, and acceptance criteria. Do not report compile PASS unless Unity Editor was used.
