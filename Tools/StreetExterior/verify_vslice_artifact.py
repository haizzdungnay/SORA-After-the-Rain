#!/usr/bin/env python3
"""Verify the local/private SORA VS01 geometry artifact (never uploads assets).

Usage:
  python Tools/StreetExterior/verify_vslice_artifact.py LocalReferenceAssets/SORA_VS01

Requires: pip install trimesh numpy
This is geometry/provenance QA, NOT Unity compilation/render/gameplay validation.
"""
import argparse
import json
from pathlib import Path

import numpy as np
import trimesh


def verify(root):
    root = Path(root)
    model = root / "SORA_Street_VSlice_VS01.glb"
    catalog_file = root / "KIT_CATALOG_SOURCE.json"
    manifest_file = root / "SCENE_MANIFEST.json"
    if not all(p.is_file() for p in (model, catalog_file, manifest_file)):
        raise FileNotFoundError("Missing VS01 GLB, KIT_CATALOG_SOURCE.json, or SCENE_MANIFEST.json")
    catalog = json.loads(catalog_file.read_text(encoding="utf-8"))
    manifest = json.loads(manifest_file.read_text(encoding="utf-8"))
    scene = trimesh.load(model, force="scene", process=False)
    names = set(scene.graph.nodes_geometry)
    required = {
        "149_source_candidates": len(catalog) == 149,
        "reloaded_real_meshes": len(scene.graph.nodes_geometry) >= 200,
        "three_distinct_frontages": all(any(n.startswith(prefix) for n in names)
                                       for prefix in ("SORA_MART/", "MIDORI_RAMEN_AFX/", "HOSHI_BOOKS_AFX/")),
        "road_48m_present": any("asphalt_48m" in n for n in names),
        "AFX_source_groups_transformed": len(manifest.get("source_components_used", [])) == 2,
        "full_road_width": float(np.diff(scene.bounds[:, 0])[0]) >= 48.0 - 1e-3,
        "source_tracked": len(manifest.get("source_sha256", "")) == 64,
        "no_release_rights_claim": any("local" in str(item.get("rights","")).lower()
                                       for item in catalog),
    }
    for key, ok in required.items():
        print(("PASS" if ok else "FAIL") + ": " + key)
    print("scene_mesh_nodes", len(names), "bounds", np.asarray(scene.bounds).round(3).tolist())
    print("NOT VERIFIED: Unity FBX/URP materials, collider gameplay, rain, Play Mode or Windows build.")
    if not all(required.values()):
        raise SystemExit(1)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("private_output", type=Path)
    verify(parser.parse_args().private_output)
