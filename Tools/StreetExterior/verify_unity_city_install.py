#!/usr/bin/env python3
"""Prove that Unity Assets/ contains genuine V03+V05 GLB bytes and live source hook.

Offline integrity verification ONLY. It cannot attest Unity compilation,
glTFast importer output, Play Mode rendering, or any gameplay behaviour.
Usage:
  python Tools/StreetExterior/verify_unity_city_install.py .
"""
import hashlib
import json
from pathlib import Path
import struct
import sys


def sha(data):
    return hashlib.sha256(data).hexdigest()


def check(root):
    root = Path(root)
    sources = root / "ArtSource/Generated"
    target = root / "Assets/Resources/SoraCity"
    v03 = json.loads((sources / "V03_CC0/manifest.json").read_text(encoding="utf-8"))
    v05 = json.loads((sources / "V05_CC0/manifest.json").read_text(encoding="utf-8"))
    v06 = json.loads((sources / "V06_CC0/manifest.json").read_text(encoding="utf-8"))
    files = [
        ("Models/V03_CC0_Overlay.glb", v03["files"][0]["sha256"], 20782520),
        ("Models/V05_Kenney_9Block_Overlay.glb", v05["glb_sha256"], 20653268),
        ("Models/V06_Facade_Details.glb", v06["glb_sha256"], v06["bytes"]),
    ]
    for relative, expected_hash, expected_len in files:
        path = target / relative
        data = path.read_bytes()
        assert data[:4] == b"glTF", (path, "not a GLB binary (did you git lfs pull?)")
        assert struct.unpack_from("<I", data, 8)[0] == len(data), "Invalid GLB length"
        assert len(data) == expected_len and sha(data) == expected_hash, "Checksum mismatch " + relative
        assert path.with_name(path.name + ".meta").is_file(), "Missing stable Unity .meta GUID"
        print("PASS actual Unity asset:", relative, "bytes:", len(data), "SHA256:", expected_hash)
    colliders = json.loads((target / "V05_COLLIDERS.json").read_text(encoding="utf-8"))["items"]
    assert len(colliders) == 122, "Expected 60 building + 42 car + 4 wall + 16 road end proxies"
    for group, expected in (("BUILDING", 60), ("VEHICLE", 42),
                            ("HARD_BOUNDARY", 4), ("ROAD_BLOCK", 16)):
        assert sum(c["group"] == group for c in colliders) == expected, group
    layout = json.loads((target / "V02_GRID_LAYOUT.json").read_text(encoding="utf-8"))
    assert len(layout["shops"]) == 16

    package = json.loads((root / "Packages/manifest.json").read_text(encoding="utf-8"))
    assert package["dependencies"]["com.unity.cloud.gltfast"] == "6.20.0"
    world_code = (root / "Assets/SORA/Scripts/SoraDemoWorld.cs").read_text(encoding="utf-8")
    assert "SoraCityRuntimeWorld.TryBuild(world, game)" in world_code
    assert "if (!cityLoaded) SetupStreet()" in world_code
    assert "if (!cityLoaded) SoraNeighborhoodDistrict.Build(world)" in world_code
    loader = (root / "Assets/SORA/Scripts/SoraCityRuntimeWorld.cs").read_text(encoding="utf-8")
    for signal in ["Resources.Load<GameObject>(PublicV05Path)",
                   "Resources.Load<GameObject>(PrivateV03Path)",
                   "Resources.Load<GameObject>(PublicV06Path)",
                   "UnityEngine.Object.Instantiate(v05Asset, city, false)",
                   "BuildCollisionProxies(city, proxyData)"]:
        assert signal in loader, "Missing actual GameObject runtime integration: " + signal
    assert not (target / "Private/V03_Integrated_NoMart.glb").exists(), (
        "Private AFX derivative accidentally present; never include it in a public CI upload")
    print("PASS: 3 real GLBs in Assets/; 122 collider proxies; Build() live gameplay hook; no private AFX")
    print("NOT RUN: Unity Editor compilation/import/Play Mode/POS/twin/camera/Windows build")


if __name__ == "__main__":
    check(Path(sys.argv[1] if len(sys.argv) > 1 else ".").resolve())
