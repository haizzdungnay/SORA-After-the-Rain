#!/usr/bin/env python3
"""Replace V04 placeholder cars with 42 real Kenney Car Kit 3.1 GLB vehicles.

The output is a REPLACEMENT for the V04 overlay, not an additional layer.
Preserve the separately held V03 base at the same identity world transform.
Kenney source is verified against its CC0 archive SHA-256, then read in memory.
No private AFX mesh is imported by this script.

python Tools/StreetExterior/build_v05_kenney.py \
 --base-glb ArtSource/Generated/V04_CC0/SORA_V04_CC0_Dense_Overlay.glb \
 --cars-json Docs/StreetExterior/V04_DENSE_GRID_LAYOUT.json \
 --colliders ArtSource/Generated/V04_CC0/colliders.json \
 --kenney-zip ArtSource/ThirdParty/CC0/KenneyCarKit/kenney_car-kit.zip \
 --output ArtSource/Generated/V05_CC0
"""
import argparse
import hashlib
import io
import json
import math
import zipfile
from collections import Counter
from pathlib import Path

import numpy as np
import trimesh
from PIL import Image

EXPECTED_ARCHIVE_SHA = "fac7dacac5c7874348cf19729af3ef205f3d366493edaf0a827d93f4fdf3d0c4"
KINDS = {
    "kei": ((1.50, 1.65, 3.50), ("hatchback-sports", "sedan-sports", "hatchback-sports")),
    "sedan": ((1.75, 1.55, 4.40), ("sedan", "sedan-sports", "taxi", "suv")),
    "van": ((1.86, 2.15, 4.65), ("van", "suv", "van")),
    "delivery": ((1.90, 2.60, 5.20), ("delivery", "truck", "delivery")),
}


def sha(data):
    return hashlib.sha256(data).hexdigest()


def move(v):
    m = np.eye(4)
    m[:3, 3] = v
    return m


def turn_y(degrees):
    a = math.radians(degrees)
    c, s = math.cos(a), math.sin(a)
    return np.array([[c, 0, s, 0], [0, 1, 0, 0], [-s, 0, c, 0], [0, 0, 0, 1]], float)


def parse_cars(path):
    data = json.loads(Path(path).read_text(encoding="utf-8"))
    cars = []
    if "vehicles" in data:
        for c in data["vehicles"]:
            cars.append({"id": c["id"], "x": float(c["center_xz"][0]),
                         "z": float(c["center_xz"][1]), "yaw": float(c["yaw"]),
                         "kind": c["kind"]})
    elif "cars" in data:
        for c in data["cars"]:
            identifier = c["id"]
            yaw = 90 if ("PARK" in identifier or identifier.startswith(("EDGE_NORTH", "EDGE_SOUTH"))) else 0
            cars.append({"id": identifier, "x": float(c["center_xyz"][0]),
                         "z": float(c["center_xyz"][2]), "yaw": yaw, "kind": c["type"]})
    else:
        raise ValueError("Expected V04 public vehicles[] or private cars[]")
    if len(cars) != 42 or len({c["id"] for c in cars}) != 42:
        raise ValueError("Exactly 42 unique V04 parking positions are required")
    if any(c["kind"] not in KINDS or not (-63 < c["x"] < 108 and -63 < c["z"] < 108) for c in cars):
        raise ValueError("Invalid V04 parking category/position")
    return sorted(cars, key=lambda c: c["id"])


def kenney_models(path):
    payload = Path(path).read_bytes()
    if sha(payload) != EXPECTED_ARCHIVE_SHA:
        raise ValueError("Kenney ZIP checksum mismatch, refusing unreviewed source")
    with zipfile.ZipFile(io.BytesIO(payload)) as archive:
        license_text = archive.read("License.txt")
        if b"Creative Commons Zero" not in license_text or b"CC0" not in license_text:
            raise ValueError("Kenney CC0 license evidence not found")
        available = {model for _, names in KINDS.values() for model in names}
        # Kenney GLBs reference an EXTERNAL palette image. Load it from ZIP
        # and embed it in the generated GLB, or vehicles render pure white.
        palette = Image.open(io.BytesIO(archive.read("Models/GLB format/Textures/colormap.png"))).convert("RGBA")
        models = {}
        for name in sorted(available):
            data = archive.read("Models/GLB format/" + name + ".glb")
            model = trimesh.load(io.BytesIO(data), file_type="glb", force="scene", process=False)
            if model.bounds is None or not model.geometry:
                raise ValueError("Empty Kenney model: " + name)
            for geom in model.geometry.values():
                mat = getattr(geom.visual, "material", None)
                if mat is not None and getattr(mat, "baseColorTexture", None) is None:
                    mat.baseColorTexture = palette
            models[name] = model
    return models, license_text


def without_placeholder_cars(source):
    scene = trimesh.Scene()
    existing = list(source.graph.nodes_geometry)
    removed = [n for n in existing if n.startswith(("V04/CAR/", "V04/VEHICLE/"))]
    if len(removed) < 42:
        raise ValueError("Base layer does not contain the expected primitive cars")
    remove_names = set(removed)
    for node in existing:
        if node in remove_names:
            continue
        tf, key = source.graph[node]
        if key not in scene.geometry:
            scene.geometry[key] = source.geometry[key]
        scene.graph.update(frame_to=node, frame_from=scene.graph.base_frame,
                           matrix=tf, geometry=key)
    return scene, removed


def put_kenney(scene, source, variant, ident, at, yaw, dims):
    a, b = source.bounds
    dimensions = b - a
    if np.any(dimensions < 0.01):
        raise ValueError("Invalid Kenney source bounds")
    factor = np.array(dims) / dimensions
    origin = move([-(a[0] + b[0])/2, -a[1], -(a[2] + b[2])/2])
    pose = move([at[0], 0.02, at[1]]) @ turn_y(yaw) @ np.diag([*factor, 1]) @ origin
    pts = []
    for part in source.graph.nodes_geometry:
        transform, key = source.graph[part]
        combined = pose @ transform
        geometry_key = "KenneyCarKit3.1/" + variant + "/" + key
        if geometry_key not in scene.geometry:
            scene.geometry[geometry_key] = source.geometry[key]
        scene.graph.update(frame_to="V05/KENNEY_CAR/" + ident + "/" + variant + "/" + part,
                           frame_from=scene.graph.base_frame, matrix=combined,
                           geometry=geometry_key)
        pts.append(trimesh.transform_points(
            trimesh.bounds.corners(source.geometry[key].bounds), combined))
    low = np.concatenate(pts).min(axis=0)
    high = np.concatenate(pts).max(axis=0)
    exp = np.array([dims[2], dims[1], dims[0]] if int(yaw) % 180 else dims)
    if max(abs((high-low)-exp)) > .05:
        raise ValueError("Car dimensions inconsistent with vehicle footprint " + ident)
    return low, high


def collision_update(source, updates):
    data = json.loads(Path(source).read_text(encoding="utf-8"))
    if "items" in data:
        rows = data["items"]
        for row in rows:
            if row.get("group") == "VEHICLE":
                m = updates.get(row["id"])
                if m is None: raise ValueError("Vehicle collision missing " + row["id"])
                row.update({"cx": m["center"][0], "cy": m["center"][1], "cz": m["center"][2],
                            "sx": m["size"][0], "sy": m["size"][1], "sz": m["size"][2]})
        count = Counter(row["group"] for row in rows)
        if count["VEHICLE"] != 42 or count["HARD_BOUNDARY"] != 4 or count["ROAD_BLOCK"] != 16:
            raise ValueError("World boundaries unexpectedly changed")
    elif "colliders" in data:
        rows = data["colliders"]
        for row in rows:
            if row.get("group") == "VEHICLE":
                m = updates.get(row["id"])
                if m is None: raise ValueError("Vehicle collision missing " + row["id"])
                row["center"] = m["center"]
                row["size"] = m["size"]
        if sum(row["group"]=="VEHICLE" for row in rows) != 42:
            raise ValueError("Invalid private vehicle collision manifest")
    else:
        raise ValueError("Unknown collision source format")
    return data


def build(args):
    slots = parse_cars(args.cars_json)
    sources, license_text = kenney_models(args.kenney_zip)
    old = trimesh.load(args.base_glb, force="scene", process=False)
    new, removed = without_placeholder_cars(old)
    usage, updates = Counter(), {}
    for i, slot in enumerate(slots):
        dims, variants = KINDS[slot["kind"]]
        variant = variants[(i // 3 + i) % len(variants)]
        lo, hi = put_kenney(new, sources[variant], variant, slot["id"],
                            [slot["x"], slot["z"]], slot["yaw"], dims)
        updates[slot["id"]] = {
            "center": np.round((hi + lo)/2, 4).tolist(),
            "size": np.round((hi - lo) + [.12, .04, .12], 4).tolist()}
        slot.update({"kenney_glb": variant, "collider": updates[slot["id"]]})
        usage[variant] += 1

    colliders = collision_update(args.colliders, updates)
    names = list(new.graph.nodes_geometry)
    kenney_nodes = [n for n in names if n.startswith("V05/KENNEY_CAR/")]
    if len({n.split("/")[2] for n in kenney_nodes}) != 42:
        raise ValueError("Some Kenney vehicle instances missing")
    if any(n.startswith(("V04/CAR/", "V04/VEHICLE/")) for n in names):
        raise ValueError("Old placeholder vehicles were left behind")

    args.output.mkdir(parents=True, exist_ok=True)
    dest = args.output / "SORA_V05_Kenney_9Block_Overlay.glb"
    binary = new.export(file_type="glb")
    dest.write_bytes(binary)
    manifest = {
        "version": "V05", "type": "V04 REPLACEMENT overlay (not an additive car-only scene)",
        "v03_alignment": "Use the same world origin and scale as the existing V03 base",
        "original_mesh_nodes": len(old.graph.nodes_geometry),
        "removed_v04_car_nodes": len(removed),
        "kenney_car_count": 42, "kenney_mesh_nodes": len(kenney_nodes),
        "final_mesh_nodes": len(names), "vehicle_models": dict(usage),
        "official_source": "https://kenney.nl/assets/car-kit",
        "license": "CC0 1.0 Universal",
        "kenney_source_zip_sha256": EXPECTED_ARCHIVE_SHA,
        "glb_sha256": sha(binary), "glb_size": len(binary),
        "vehicles": slots, "unity_playmode": "NOT TESTED",
        "note": "Do not place both V04 and V05 overlays: the world/roads would be duplicated."
    }
    (args.output / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2)+"\n", encoding="utf-8")
    (args.output / "colliders.json").write_text(json.dumps(colliders, ensure_ascii=False, indent=2)+"\n", encoding="utf-8")
    (args.output / "License-Kenney.txt").write_bytes(license_text)

    reloaded = trimesh.load(dest, force="scene", process=False)
    checknodes = list(reloaded.graph.nodes_geometry)
    assert len(checknodes)==manifest["final_mesh_nodes"]
    assert sum(n.startswith("V05/KENNEY_CAR/") for n in checknodes)==len(kenney_nodes)
    assert not any(n.startswith(("V04/CAR/", "V04/VEHICLE/")) for n in checknodes)
    print("PASS: replaced 42 cars, used",len(usage),"Kenney vehicle variants; GLB bytes:",len(binary),flush=True)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    for name in ("base-glb", "cars-json", "colliders", "kenney-zip", "output"):
        parser.add_argument("--"+name, required=True, type=Path)
    build(parser.parse_args())
