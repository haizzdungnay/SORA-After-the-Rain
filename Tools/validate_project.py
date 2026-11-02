#!/usr/bin/env python3
"""Offline structure/GUID/hygiene checks. Does NOT compile C# or run Unity."""
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent

def require(condition, message):
    if not condition:
        raise SystemExit("FAIL: " + message)

required = [
    "README.md", "AGENTS.md", "Docs/GAME_DESIGN_DOCUMENT_v0.4.md",
    "Docs/TECHNICAL_NOTES.md", "Packages/manifest.json",
    "ProjectSettings/ProjectVersion.txt", "ProjectSettings/InputManager.asset",
    "ProjectSettings/EditorSettings.asset", "Tools/Setup-SORA.ps1",
    "Tools/Build-SORA.ps1", "Tools/Test-SORA.ps1",
    "Assets/SORA/Scripts/SoraDemoCore.cs", "Assets/SORA/Scripts/SoraDemoWorld.cs",
    "Assets/SORA/Scripts/SoraDemoCharacter.cs",
    "Assets/SORA/Scripts/Editor/SoraDemoSceneGenerator.cs",
    "Assets/SORA/Scripts/Editor/SoraDevAutomation.cs",
    "Assets/SORA/Scripts/Editor/SoraProjectSetup.cs",
    "Assets/SORA/Tests/Editor/StoreIntegrityTests.cs",
    ".github/pull_request_template.md",
    ".github/workflows/sora-static-check.yml",
    ".github/workflows/sora-local-runner-build.yml",
]
for rel in required:
    require((ROOT / rel).is_file(), "missing " + rel)
manifest = json.loads((ROOT / "Packages/manifest.json").read_text())
for package, version in {"com.unity.render-pipelines.universal": "17.3.0", "com.unity.test-framework": "1.6.0"}.items():
    require(manifest["dependencies"].get(package) == version, "unexpected package pin: " + package)
for module in ["imgui", "inputlegacy", "textrendering"]:
    require(manifest["dependencies"].get("com.unity.modules." + module) == "1.0.0", "missing runtime module: " + module)
require("m_EditorVersion: 6000.3.7f1" in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(), "Unity version changed")
for path in (ROOT / "Assets").rglob("*.cs"):
    require("namespace Sora.AfterTheRain.Demo" in path.read_text(), "unexpected namespace: " + str(path.relative_to(ROOT)))
for path in (ROOT / "Assets").rglob("*.asmdef"):
    json.loads(path.read_text())
# Regression guard: Unity ProjectSettings are not regular AssetDatabase assets.
setup_source = (ROOT / "Assets/SORA/Scripts/Editor/SoraProjectSetup.cs").read_text()
require("GraphicsSettings.GetGraphicsSettings()" in setup_source,
        "Unity GraphicsSettings accessor missing from Editor bootstrap")
require('AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")' not in setup_source,
        "do not load GraphicsSettings.asset through AssetDatabase")
guids = {}
for path in (ROOT / "Assets").rglob("*"):
    if path.suffix != ".meta":
        require(Path(str(path) + ".meta").is_file(), "missing .meta: " + str(path.relative_to(ROOT)))
        continue
    require(Path(str(path)[:-5]).exists(), "orphan .meta: " + str(path.relative_to(ROOT)))
    match = re.search(r"^guid: ([0-9a-f]{32})$", path.read_text(), re.M)
    require(match is not None, "invalid GUID: " + str(path.relative_to(ROOT)))
    require(match[1] not in guids, "duplicate GUID: " + str(path.relative_to(ROOT)))
    guids[match[1]] = path
# Inspect tracked content, so a developer's local Unity cache does not fail validation.
if (ROOT / ".git").exists():
    names = subprocess.check_output(["git", "ls-files", "-z"], cwd=ROOT).decode().split("\0")
    paths = [ROOT / name for name in names if name]
else:
    paths = [p for p in ROOT.rglob("*") if p.is_file()]
for path in paths:
    rel = path.relative_to(ROOT)
    require(rel.parts[0].lower() not in {"library", "temp", "obj", "logs", "build", "builds", "usersettings", "testresults"}, "generated directory tracked: " + str(rel))
    require(not path.name.startswith(".env") or path.name == ".env.example", "environment file tracked: " + str(rel))
    require(path.suffix.lower() not in {".ulf", ".alf", ".pem", ".key", ".pfx", ".p12", ".keystore"}, "credential/license file tracked: " + str(rel))
    require(path.stat().st_size < 50 * 1024 * 1024, "large file needs asset/LFS review: " + str(rel))
    body = path.read_bytes().decode("utf-8", errors="ignore")
    require(not re.search(r"(?:gh[pousr]_[A-Za-z0-9]{30,}|github_pat_[A-Za-z0-9_]{40,}|AKIA[A-Z0-9]{16}|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----)", body), "possible secret in " + str(rel))
print(f"PASS: offline structure, JSON, {len(guids)} unique asset GUIDs, and {len(paths)} tracked files checked.")
print("NOT RUN: Unity package resolution, C# compile, EditMode/Play Mode tests, rendering, Windows build.")
