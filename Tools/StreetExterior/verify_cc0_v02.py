#!/usr/bin/env python3
"""Validate CC0 source files imported in ArtSource/ThirdParty/CC0 (not Unity).
Run: python Tools/StreetExterior/verify_cc0_v02.py ArtSource/ThirdParty/CC0
"""
import hashlib
import json
import struct
import sys
from pathlib import Path


def verify(root):
    root = Path(root)
    data = json.loads((root / "manifest.json").read_text(encoding="utf-8"))
    files = data["files"]
    if len(files) != 5:
        raise AssertionError("Exactly 2 GLBs and 3 PBR images expected")
    seen = set()
    for item in files:
        relative = Path(item["path"])
        if relative.is_absolute() or ".." in relative.parts or relative.as_posix() in seen:
            raise AssertionError("Unsafe or duplicate source path")
        seen.add(relative.as_posix())
        path = root / relative
        payload = path.read_bytes()
        if len(payload) != item["bytes"]:
            raise AssertionError("Size mismatch: " + str(path))
        if hashlib.sha256(payload).hexdigest() != item["sha256"]:
            raise AssertionError("SHA-256 mismatch: " + str(path))
        if path.suffix == ".glb":
            if payload[:4] != b"glTF" or struct.unpack_from("<II", payload, 4)[1] != len(payload):
                raise AssertionError("Malformed GLB: " + str(path))
        else:
            from PIL import Image
            with Image.open(path) as im:
                if im.size != (2048, 2048):
                    raise AssertionError("PBR maps must be 2K: " + str(path))
                im.verify()
        print("PASS", relative, len(payload), "bytes")
    assert len([p for p in seen if p.endswith(".glb")]) == 2
    assert len([p for p in seen if p.endswith((".jpg", ".png"))]) == 3
    print("PASS: CC0 binaries checked; Unity/URP/Play Mode NOT tested.")


if __name__ == "__main__":
    if len(sys.argv) != 2:
        raise SystemExit("Usage: verify_cc0_v02.py ArtSource/ThirdParty/CC0")
    verify(sys.argv[1])
