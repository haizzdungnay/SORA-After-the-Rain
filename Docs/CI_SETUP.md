# CI and local Unity acceptance

## Automated GitHub-hosted Runner CI (GameCI)

Workflow file: `.github/workflows/sora-gameci-build.yml`  
Runs on GitHub-hosted `ubuntu-latest` runners using official GameCI actions (`game-ci/unity-test-runner@v4` and `game-ci/unity-builder@v4`) and Docker image `unityci/editor:ubuntu-6000.3.7f1-windows-mono-3.2.2`.

### One-step Owner Setup (Unity Personal License)

To enable automated testing and Windows builds without running a local self-hosted runner:
1. Open **Unity Hub** on your computer and log in with your Unity ID.
2. In **Unity Hub > Preferences > Licenses**, click **Add** > **Get a free personal license**.
3. Copy the content of the generated license file:
   - Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
   - macOS: `/Library/Application Support/Unity/Unity_lic.ulf`
   - Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`
4. In this GitHub repository, go to **Settings > Secrets and variables > Actions > New repository secret**:
   - Name: `UNITY_LICENSE`
   - Secret: Paste the complete XML content of `Unity_lic.ulf`
   - (Optional if needed by account: add `UNITY_EMAIL` and `UNITY_PASSWORD`)

Once configured:
- Every push to `main` and `develop` triggers EditMode tests and StandaloneWindows64 build.
- You can trigger builds manually under **Actions > SORA GameCI Automated Unity Test and Windows Build > Run workflow**.
- Download the resulting Windows executable from **Artifacts > SORA-Windows-x64** and test reports from **SORA-TestResults**.

## Comparison: GitHub-hosted (GameCI) vs Windows Self-hosted Runner

| Feature | GameCI (GitHub-hosted) | Windows Self-hosted Runner |
|---|---|---|
| Runner Host | Cloud `ubuntu-latest` (Zero machine maintenance) | Dedicated Windows PC running 24/7 runner daemon |
| Unity Version | `unityci/editor:ubuntu-6000.3.7f1-windows-mono` | Unity 6000.3.7f1 installed via Unity Hub |
| Build Output | StandaloneWindows64 (Mono runtime) `.exe` | StandaloneWindows64 `.exe` |
| License | `UNITY_LICENSE` stored safely in GitHub Secrets | Activated locally via Unity Hub |
| Security | Ephemeral cloud container per build | Runner executes on local machine (security risk on public repos) |
| Cost | Free GitHub Actions minutes | Local hardware & electricity |

## Without Unity

`python Tools/validate_project.py` runs on pushes and pull requests using a short GitHub-hosted Linux job. It checks required files, package pins, JSON, GUID uniqueness, metadata and common repository hygiene issues. Secret signatures are a limited heuristic, not a full audit. It does not compile C#.

## Local Windows machine

1. Clone `https://github.com/haizzdungnay/SORA-After-the-Rain.git` and install Git LFS before adding art.
2. Use Unity Hub to install **6000.3.7f1**, activate an appropriate license and open the project. Wait for package resolution. Commit the real `Packages/packages-lock.json` after Unity creates it; no guessed dependency lock is supplied.
3. Run **SORA > 0. Configure Prototype Project**, then **SORA > 1. Generate Demo Scene**. Commit the resulting URP assets, scene, `.meta` files and ProjectSettings after review. Never regenerate existing GUIDs. Generated URP settings and setup automation have not yet been verified in Editor.
4. Close the Editor GUI before batch tasks. Run `./Tools/Test-SORA.ps1`, then `./Tools/Build-SORA.ps1 -SmokeOnly`, then `./Tools/Build-SORA.ps1` in PowerShell. Add `-UnityExe` when the Editor is installed elsewhere.
5. Test Runner XML is `TestResults/editmode.xml`; logs are in `Logs/`; standalone is `Builds/Windows/SORA_After_the_Rain.exe` with its accompanying data. Test Runner also works via Window > General > Test Runner > EditMode.
6. Run the actual Play Mode checklist in `Docs/TEST_CHECKLIST.md`. EditMode tests do not verify movement, shader appearance, rain or user interactions.

## Optional Self-hosted GitHub Unity workflow

The self-hosted workflow (`.github/workflows/sora-local-runner-build.yml`) is **manual only**, restricted to `main`, and skips unless repository variable `SORA_ENABLE_UNITY_CI=true` and the dispatch checkbox is confirmed.

For a PUBLIC repository, use an isolated/disposable Windows runner labelled `self-hosted`, `windows`, `x64`, `unity`, with PowerShell 7, Git LFS, the pinned Editor and a valid already-activated license. Do not attach a personal workstation carrying private credentials; do not execute untrusted PRs on it. Only reviewed trusted main source may run.

Dispatch `editmode` to run tests plus import/scene generation, or `windows-build` to also build. Successful Windows jobs upload the complete `Builds/Windows/` directory; test XML and logs upload even on failure. Never commit license files or use another person's activation.

Five EditMode tests cover draft cancellation, one paid sale across 100 swaps, empty-stock POS, duplicate deliveries/restock conservation, and per-character relation isolation. They invoke the existing private prototype methods through reflection to avoid changing runtime interfaces during import. Refactor these seams with the later modular architecture.
