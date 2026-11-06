#!/usr/bin/env python3
"""Validate owner-held private SORA V02 GLB. Requires trimesh and numpy.

Usage:
  python Tools/StreetExterior/verify_neighborhood_v02.py /path/to/V02/output

This only checks structural geometry. It does NOT compile/test Unity.
"""
import json
import sys
from pathlib import Path
import trimesh


def main(root):
    root = Path(root)
    manifest = json.loads((root / "V02_LAYOUT_MANIFEST.json").read_text(encoding="utf-8"))
    if len(manifest.get("intersections", [])) != 9:
        raise AssertionError("Expected nine real road junctions")
    shops = manifest.get("shops", [])
    if len(shops) != 16 or len({x["id"] for x in shops}) != 16:
        raise AssertionError("Expected sixteen unique storefronts")
    if len(manifest.get("blocks", {})) != 4:
        raise AssertionError("Expected four district blocks")
    model = root / "SORA_Neighborhood_V02_Grid.glb"
    scene = trimesh.load(model, force="scene", process=False)
    names = scene.graph.nodes_geometry
    checks = {
        "real_3d_meshes": len(names) >= 1000,
        "mart_model_exists": any(n.startswith("NE-01/SORA_MART/") for n in names),
        "afx_reuse_exists": sum("/AFX_KIT/" in n for n in names) > 100,
        "nine_crossings_four_approaches": sum(n.startswith("CROSSING/") for n in names) == 216,
        "east_west_and_north_south_roads": any(n.startswith("ROAD/EW_") for n in names)
                                             and any(n.startswith("ROAD/NS_") for n in names),
    }
    for name, ok in checks.items():
        print(("PASS" if ok else "FAIL") + ": " + name)
    if not all(checks.values()):
        raise SystemExit(1)
    print("GLB mesh instances", len(names))
    print("NOT RUN: Unity shaders, compile, gameplay, collisions, rain or Windows build.")


if __name__ == "__main__":
    if len(sys.argv) != 2:
        raise SystemExit("usage: verify_neighborhood_v02.py <private V02 output folder>")
    main(sys.argv[1])
