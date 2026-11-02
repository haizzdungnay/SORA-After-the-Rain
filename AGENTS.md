# SORA coding agent instructions

Read `README.md`, `Docs/GAME_DESIGN_DOCUMENT_v0.4.md`, `Docs/TECHNICAL_NOTES.md`, `Docs/CONTRIBUTING_AND_AI.md` and `Docs/ROADMAP_GITHUB.md` before changes. GitHub is canonical: fetch current refs and review open PRs first.

- Preserve the existing prototype; avoid replacing working systems without a concrete reason.
- Use `feature/<scope>` for features and `fix/<issue>` for fixes, normally based on `develop`. Open a PR; never automatically merge into `main`.
- Keep Unity `6000.3.7f1`, URP `17.3.0`, Windows PC first. Add dependencies only when needed and explain them.
- Preserve asset `.meta` files and GUIDs; move each asset with its sidecar. Let Unity generate scene/material/pipeline YAML, then commit it with metadata.
- Keep C# modular. Runtime scripts belong to `Sora.Demo`; Editor automation to `Sora.Demo.Editor`; EditMode tests to `Sora.Demo.EditModeTests`.
- Shared money, inventory and quest progress survive character switching. Draft POS sales cancel safely; one sale must never charge twice. Relations remain separate per twin.
- The twins are Tei Toshiharu (丁俊陽) and Tei Akiko (明子), both 18 at gameplay start. Preserve the canonical GDD; open details remain proposals.
- Do not commit caches, credentials, Unity licenses or assets without redistribution rights. Record third-party asset provenance in `Docs/ASSET_REGISTER.csv`; configure Git LFS before adding binaries.
- Run `python Tools/validate_project.py`. This checks structure and common mistakes, **not C# compilation**. Unity compile, EditMode, Play Mode and Windows build results require actual Editor logs.
- Every PR must state objective, changed files, architecture/logic, tests actually run, tests pending, risks and limits. Include genuine Unity screenshots for visual changes.
- Unity CI is manual and opt-in. Never dispatch license-dependent jobs or register runners without the owner's configuration. Never run untrusted PR code on a personal self-hosted runner.
