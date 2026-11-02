# CI and local Unity acceptance

## Without Unity

`python Tools/validate_project.py` runs on pushes and pull requests using a short GitHub-hosted Linux job. It checks required files, package pins, JSON, GUID uniqueness, metadata and common repository hygiene issues. Secret signatures are a limited heuristic, not a full audit. It does not compile C#.

## Local Windows machine

1. Clone `https://github.com/haizzdungnay/SORA-After-the-Rain.git` and install Git LFS before adding art.
2. Use Unity Hub to install **6000.3.7f1**, activate an appropriate license and open the project. Wait for package resolution. Commit the real `Packages/packages-lock.json` after Unity creates it; no guessed dependency lock is supplied.
3. Run **SORA > 0. Configure Prototype Project**, then **SORA > 1. Generate Demo Scene**. Commit the resulting URP assets, scene, `.meta` files and ProjectSettings after review. Never regenerate existing GUIDs. Generated URP settings and setup automation have not yet been verified in Editor.
4. Close the Editor GUI before batch tasks. Run `./Tools/Test-SORA.ps1`, then `./Tools/Build-SORA.ps1 -SmokeOnly`, then `./Tools/Build-SORA.ps1` in PowerShell. Add `-UnityExe` when the Editor is installed elsewhere.
5. Test Runner XML is `TestResults/editmode.xml`; logs are in `Logs/`; standalone is `Builds/Windows/SORA_After_the_Rain.exe` with its accompanying data. Test Runner also works via Window > General > Test Runner > EditMode.
6. Run the actual Play Mode checklist in `Docs/TEST_CHECKLIST.md`. EditMode tests do not verify movement, shader appearance, rain or user interactions.

## Optional GitHub Unity workflow

The licensed workflow is **manual only**, restricted to `main`, and skips unless repository variable `SORA_ENABLE_UNITY_CI=true` and the dispatch checkbox is confirmed. No runner or Unity license was configured or used during import. Leave the variable unset until the owner deliberately enables it.

For a PUBLIC repository, use an isolated/disposable Windows runner labelled `self-hosted`, `windows`, `x64`, `unity`, with PowerShell 7, Git LFS, the pinned Editor and a valid already-activated license. Do not attach a personal workstation carrying private credentials; do not execute untrusted PRs on it. Only reviewed trusted main source may run. Do not change this workflow to `pull_request_target`.

Dispatch `editmode` to run tests plus import/scene generation, or `windows-build` to also build. Successful Windows jobs upload the complete `Builds/Windows/` directory; test XML and logs upload even on failure. Never commit license files or use another person's activation. The import did not dispatch this workflow.

Five EditMode tests cover draft cancellation, one paid sale across 100 swaps, empty-stock POS, duplicate deliveries/restock conservation, and per-character relation isolation. They invoke the existing private prototype methods through reflection to avoid changing runtime interfaces during import. Refactor these seams with the later modular architecture.
