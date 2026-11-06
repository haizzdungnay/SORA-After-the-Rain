#!/usr/bin/env python3
"""Extract named source-node groups from owner-held AFX GLB into pivot-normalized local GLBs.

This is a real mesh extraction utility, NOT a semantic-storefront classifier or
permission to redistribute extracted geometry. Output stays local/private.
Dependencies: pip install 'trimesh>=4.5,<5' 'numpy>=1.24'
Example:
  python Tools/StreetExterior/extract_afx_component_groups.py \
    --source LocalReferenceAssets/AFX_JapaneseStreet/japanese_street_at_night.glb \
    --output LocalReferenceAssets/AFX_Kits --groups A G VENDING POLE
"""
import argparse
import json
import re
from collections import defaultdict
from pathlib import Path

import numpy as np
import trimesh


def index_scene(scene):
    parent = {child: p for p, child, metadata in scene.graph.to_edgelist()}
    groups = defaultdict(list)
    for node in scene.graph.nodes_geometry:
        parent_name = parent.get(node, "")
        if parent_name.startswith(("Build_", "Built_")):
            name = parent_name.split("_")[1][0]
        elif parent_name.startswith("ven_"):
            name = "VENDING"
        elif parent_name.startswith("pole_"):
            name = "POLE"
        else:
            name = "MISC"
        groups[name].append(node)
    return parent, groups


def export_group(source, nodes, parent, dst, group):
    corners = []
    rows = []
    for node in sorted(nodes):
        matrix, geometry_name = source.graph[node]
        mesh = source.geometry[geometry_name]
        points = trimesh.transform_points(trimesh.bounds.corners(mesh.bounds), matrix)
        corners.append(points)
        rows.append((node, parent.get(node, ""), matrix, geometry_name, mesh))
    if not rows:
        raise ValueError("No source nodes selected for " + group)
    bounds = np.vstack(corners)
    minimum = bounds.min(axis=0)
    maximum = bounds.max(axis=0)
    center = (minimum + maximum) / 2
    # Ground pivot: X/Z center, Y at mesh minimum. Retain original size.
    pivot = np.array([center[0], minimum[1], center[2]])
    normalize = np.eye(4)
    normalize[:3, 3] = -pivot

    output = trimesh.Scene()
    catalog = []
    for node, source_name, matrix, geom_name, mesh in rows:
        name = group + "/" + source_name
        output.add_geometry(mesh.copy(), node_name=name, geom_name=name,
                            transform=normalize @ matrix)
        catalog.append({
            "source_node": source_name, "mesh_node": node, "mesh_name": geom_name,
            "triangles": len(mesh.faces), "vertices": len(mesh.vertices),
            "pivot_normalization_xyz": pivot.round(5).tolist(),
            "candidate_only": True, "material_uv_review_required": True,
        })
    out = dst / ("AFX_KIT_" + group + ".glb")
    out.write_bytes(output.export(file_type="glb"))
    return {"source_group": group, "file": out.name, "mesh_nodes": len(catalog),
            "triangles": sum(x["triangles"] for x in catalog),
            "source_bounds": [minimum.round(5).tolist(), maximum.round(5).tolist()],
            "assets": catalog, "private_derivative": True}


def run(source_path, output_dir, selected):
    if not source_path.is_file():
        raise FileNotFoundError(source_path)
    output_dir.mkdir(parents=True, exist_ok=True)
    original = trimesh.load(source_path, force="scene", process=False)
    parent, groups = index_scene(original)
    catalog = []
    for group in selected:
        if group not in groups:
            raise ValueError("Group " + group + " not found; available: " + str(sorted(groups)))
        data = export_group(original, groups[group], parent, output_dir, group)
        catalog.append(data)
        print("EXTRACTED", group, data["mesh_nodes"], "nodes", data["triangles"], "triangles")
    (output_dir / "AFX_KIT_GROUP_CATALOG.json").write_text(
        json.dumps(catalog, indent=2, ensure_ascii=False), encoding="utf-8")
    print("Source nodes extracted:", sum(x["mesh_nodes"] for x in catalog))
    print("All exported GLBs remain PRIVATE; material, pivot and face QA must follow.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--groups", nargs="+", default=["A", "G"])
    args = parser.parse_args()
    run(args.source.resolve(), args.output.resolve(), args.groups)
