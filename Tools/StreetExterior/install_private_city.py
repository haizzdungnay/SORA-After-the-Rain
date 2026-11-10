#!/usr/bin/env python3
"""Install the inspected owner-held V03/V05 pair locally, without running ZIP code.

python Tools/StreetExterior/install_private_city.py [--verify]
Private assets and their source ZIPs must remain outside public Git commits.
"""
import argparse
from collections import Counter
import hashlib
import json
import math
from pathlib import Path, PurePosixPath
import struct
import zipfile


ROOT = Path(__file__).resolve().parents[2]
SOURCES = ROOT / "3d model enviroment"
TARGET = ROOT / "Assets/Resources/SoraCity/Private"
ARCHIVES = (
    ("SORA_Unity_Private_AFX_Install_2026-11-09.zip",
     "1007fc97502f2efe10a758b51a507b969b83141b2b24160e08c88deaf0a4f649",
     "Assets/Resources/SoraCity/Private/V03_Integrated_NoMart.glb",
     "V03_Integrated_NoMart.glb",
     "100d73d6c7ddfd88520b4bcec60b66d363ee22e2daf0f2ccfa2b9b6f27c4154d"),
    ("SORA_Neighborhood_V05_Kenney_Private_2026-11-06.zip",
     "8ee959e08ad65b589c183f4e2b6aade483e0916c0b1d49546859b375607f6e7a",
     "maps/SORA_V05_Kenney_9Block_Overlay.glb",
     "V05_Kenney_9Block_Overlay.glb",
     "c4800a5f47af6bb4e2740b31ea1417b0644298e3b7fb60b3f6b0e6fb415f3f97"),
)


def payloads():
    files = {}
    for archive, archive_sha, member, target, model_sha in ARCHIVES:
        data = (SOURCES / archive).read_bytes()
        if hashlib.sha256(data).hexdigest() != archive_sha:
            raise ValueError("Unreviewed archive checksum: " + archive)
        with zipfile.ZipFile(SOURCES / archive) as source:
            for entry in source.infolist():
                name = entry.filename.replace("\\", "/")
                if PurePosixPath(name).is_absolute() or ".." in PurePosixPath(name).parts or ":" in name:
                    raise ValueError("Unsafe ZIP path: " + name)
            if source.testzip() is not None:
                raise ValueError("ZIP CRC failure: " + archive)
            model = source.read(member)
            if hashlib.sha256(model).hexdigest() != model_sha:
                raise ValueError("Model checksum mismatch: " + member)
            if struct.unpack_from("<4sII", model) != (b"glTF", 2, len(model)):
                raise ValueError("Invalid GLB header: " + member)
            files[target] = model
            if target.startswith("V05"):
                boxes = json.loads(source.read("data/V05_COLLIDERS.json"))["colliders"]
                groups = {"BUILDING": "BUILDING", "VEHICLE": "VEHICLE",
                          "HARD_COLLIDER": "HARD_BOUNDARY", "ROAD_OBSTACLE": "ROAD_BLOCK"}
                items = []
                for box in boxes:
                    center, size = box["center"], box["size"]
                    if (len(center) != 3 or len(size) != 3 or
                            not all(math.isfinite(v) for v in center + size) or min(size) <= 0):
                        raise ValueError("Invalid collision box: " + box["id"])
                    items.append(dict(id=box["id"], group=groups[box["group"]],
                                      **dict(zip(("cx", "cy", "cz", "sx", "sy", "sz"), center + size))))
                if (Counter(b["group"] for b in items) !=
                        Counter(BUILDING=60, VEHICLE=42, HARD_BOUNDARY=4, ROAD_BLOCK=16) or
                        len({b["id"] for b in items}) != 122):
                    raise ValueError("Incomplete V05 collision manifest")
                files["V05_COLLIDERS.json"] = (json.dumps({"items": items}, indent=2) + "\n").encode()
    return files


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--verify", action="store_true", help="Check installed bytes without writing")
    args = parser.parse_args()
    files = payloads()
    # Check every destination before writing; never overwrite another local art revision.
    for name, data in files.items():
        destination = TARGET / name
        if destination.exists() and destination.read_bytes() != data:
            raise ValueError("Different local asset; preserve and review first: " + name)
        if args.verify and not destination.is_file():
            raise ValueError("Not installed: " + name)
    if not args.verify:
        TARGET.mkdir(parents=True, exist_ok=True)
        for name, data in files.items():
            destination = TARGET / name
            if not destination.exists():
                with destination.open("xb") as output:
                    output.write(data)
    for name, data in files.items():
        print("PASS", name, len(data), hashlib.sha256(data).hexdigest())


if __name__ == "__main__":
    main()
