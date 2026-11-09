#!/usr/bin/env python3
"""Build SORA-authored facade-detail ADDITIVE GLB for V05.

Load V03 base + V05 replacement + V06 detail at the same origin and scale.
No private AFX meshes are read or republished. This improves 60 V04/V05
facades, not the 24 earlier V03 facades.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import math
from collections import Counter
from pathlib import Path

import numpy as np
import trimesh
from trimesh.visual.material import PBRMaterial
from trimesh.visual.texture import TextureVisuals

PALETTES = [
    ((159, 76, 61), (243, 181, 106), "RAMEN"),
    ((57, 104, 111), (137, 222, 215), "PHARMACY"),
    ((120, 87, 115), (244, 168, 207), "BOOKS"),
    ((105, 105, 92), (231, 200, 124), "BAKERY"),
    ((67, 82, 131), (112, 168, 236), "LAUNDRY"),
    ((107, 70, 58), (243, 150, 89), "IZAKAYA"),
    ((69, 118, 90), (164, 219, 148), "FLOWERS"),
    ((84, 74, 109), (197, 171, 246), "STUDIO"),
]


def transform(position, dimensions, yaw):
    angle = math.radians(float(yaw))
    c, s = math.cos(angle), math.sin(angle)
    x, y, z = (float(v) for v in position)
    sx, sy, sz = (float(v) for v in dimensions)
    return np.array([[c*sx, 0, s*sz, x],
                     [0, sy, 0, y],
                     [-s*sx, 0, c*sz, z],
                     [0, 0, 0, 1]], dtype=float)


class DetailPass:
    def __init__(self):
        self.scene = trimesh.Scene()
        self.materials = {}
        self.counts = Counter()
        self.facade_counts = {}
        self.decorations = 0

    def material(self, key, rgb, rough=.65, emit=False):
        if key not in self.materials:
            self.materials[key] = PBRMaterial(
                name="SORA_V06_" + key,
                baseColorFactor=[*rgb, 255],
                roughnessFactor=rough,
                metallicFactor=.12 if key.startswith("metal") else 0,
                emissiveFactor=[v / 255.0 * .42 for v in rgb] if emit else None,
            )
        return self.materials[key]

    def box(self, building_id, label, center, size, mat_key, rgb, yaw=0, rough=.65, emit=False):
        if min(size) <= .001:
            raise ValueError("Invalid box dimensions for " + building_id + "/" + label)
        mat = self.material(mat_key, rgb, rough, emit)
        geo_key = "BOX/" + mat_key
        if geo_key not in self.scene.geometry:
            mesh = trimesh.creation.box(extents=[1, 1, 1])
            mesh.visual = TextureVisuals(material=mat)
            self.scene.geometry[geo_key] = mesh
        node = "V06/" + building_id + "/" + label
        if node in self.scene.graph.nodes:
            raise ValueError("Duplicate V06 node: " + node)
        self.scene.graph.update(frame_to=node, frame_from=self.scene.graph.base_frame,
                                matrix=transform(center, size, yaw), geometry=geo_key)
        self.counts[building_id] += 1
        self.decorations += 1

    def local(self, building, label, xyz, dimensions, key, rgb, rough=.65, emit=False):
        # Front midpoint is local origin; -Z faces the street for all yaw values.
        yaw = float(building["yaw"])
        angle = math.radians(yaw)
        xx, yy, zz = xyz
        fx, fz = building["front_xz"]
        center = [fx + xx*math.cos(angle) + zz*math.sin(angle), yy,
                  fz - xx*math.sin(angle) + zz*math.cos(angle)]
        self.box(building["id"], label, center, dimensions, key, rgb, yaw, rough, emit)

    def facade(self, b, i):
        key = b["id"]
        w, depth, h = (float(b[n]) for n in ("width", "depth", "height"))
        wall, accent, category = PALETTES[i % len(PALETTES)]
        warm, metal, concrete = (254, 222, 158), (54, 62, 71), (172, 169, 152)
        frame, glass, trim = (47, 55, 62), (80, 121, 135), (206, 194, 167)

        def put(label, center, size, mat, rgb, rough=.65, emit=False):
            self.local(b, label, center, size, mat, rgb, rough, emit)

        for side in (-1, 1):
            put("shop/pilaster"+str(side), (side*(w/2-.31), 1.80, -.36),
                (.24, 3.6, .32), "metal-dark", frame, .34)
            put("shop/awning-bracket"+str(side), (side*(w/2-.7), 3.25, -.58),
                (.10, .48, .72), "metal-dark", metal)
        put("shop/door-upper", (-w*.245, 2.25, -.37), (1.68, .12, .10), "metal-dark", frame)
        put("shop/door-left", (-w*.245-.79, 1.16, -.37), (.10, 2.17, .10), "metal-dark", frame)
        put("shop/door-right", (-w*.245+.79, 1.16, -.37), (.10, 2.17, .10), "metal-dark", frame)
        put("shop/door-threshold", (-w*.245, .07, -.40), (1.72, .09, .32), "metal-dark", metal)
        put("shop/handle", (-w*.245+.58, 1.10, -.49), (.045, .43, .07), "metal-light", trim)
        put("shop/awning-edge", (0, 3.31, -.93), (w-.25, .12, .14),
            "accent-"+str(i%8), accent, .44)
        put("shop/fascia-underline", (0, 3.61, -.43), (w-.38, .06, .09), "metal-light", trim)
        put("shop/lit-strip", (0, 3.14, -.48), (w-.85, .045, .06),
            "warm-lamp", warm, .34, True)
        put("shop/blade-support", (w*.37, 4.72, -.70), (.10, .10, .68), "metal-dark", metal)
        put("shop/blade-sign", (w*.37, 4.63, -1.00), (.15, 1.35, .65),
            "accent-"+str(i%8), accent, .40, True)
        put("shop/blade-border", (w*.37, 4.63, -1.38), (.18, 1.44, .06), "metal-dark", frame)
        put("shop/display-plinth", (w*.17, .39, -.27), (w*.31, .40, .25),
            "wall-"+str(i%8), wall)
        for k in range(3):
            x = w*.055 + k*w*.095
            put("shop/display-item"+str(k), (x, .81, -.38), (.28, .49+.08*(k%2), .22),
                "accent-"+str(i%8), accent)

        floors = max(0, int((h-.48)/3.05)-1)
        bays = 2 if w < 9 else 3
        for floor in range(floors):
            y = 4.95 + floor*3.05
            if y+1.00 > h-.3:
                continue
            for col in range(bays):
                x = ((col+.5)/bays-.5)*(w-1.7)
                for side in (-1, 1):
                    put(f"upper/{floor}/{col}/jamb{side}", (x+side*.64, y, -.30),
                        (.08, 1.44, .12), "metal-dark", frame)
                put(f"upper/{floor}/{col}/lintel", (x, y+.73, -.30),
                    (1.38, .08, .12), "metal-dark", frame)
                put(f"upper/{floor}/{col}/sill", (x, y-.76, -.39),
                    (1.48, .11, .32), "metal-light", concrete)
                put(f"upper/{floor}/{col}/mullion", (x, y, -.35),
                    (.055, 1.35, .07), "metal-light", trim)
                if col == (i+floor) % bays:
                    put(f"upper/{floor}/{col}/curtain", (x-.30, y, -.26),
                        (.42, 1.15, .05), "curtain", (198, 186, 164), .88)
            if (i+floor) % 3 != 0:
                put(f"upper/{floor}/balcony-deck", (0, y-.91, -.70),
                    (w-.95, .13, .95), "metal-light", concrete)
                rails = max(3, int(w/1.2))
                for rail in range(rails):
                    xx = -(w-1.25)/2 + rail*(w-1.25)/(rails-1)
                    put(f"upper/{floor}/rail{rail}", (xx, y-.35, -1.08),
                        (.05, 1.0, .06), "metal-dark", frame)
                put(f"upper/{floor}/rail-top", (0, y+.14, -1.08),
                    (w-1.12, .07, .07), "metal-dark", frame)

        for side in (-1, 1):
            put("service/side-drain"+str(side), (side*(w/2+.055), h*.51, depth*.42),
                (.10, max(1,h-.70), .12), "metal-dark", metal)
            put("service/side-ac"+str(side), (side*(w/2+.12), 3.22, depth*.58),
                (.40, .65, .86), "metal-light", concrete)
            for level in range(1, max(1, int(h//3))):
                yy = level*2.94+1.20
                if yy+0.6 > h-.25:
                    continue
                put(f"service/side-vent{side}-{level}",
                    (side*(w/2+.14), yy, depth*.76), (.15, .65, .91),
                    "metal-dark", metal)
        for xx in (-w*.27, w*.27):
            put("rear/window-sill"+str(xx), (xx, h*.61-.71, depth+.13),
                (1.5, .10, .35), "metal-light", concrete)
            put("rear/window-frame"+str(xx), (xx, h*.61+.05, depth+.13),
                (1.35, 1.50, .08), "metal-dark", frame)
            put("rear/window-glass"+str(xx), (xx, h*.61+.05, depth+.19),
                (1.14, 1.24, .035), "glass", glass, .20)
        put("roof/hatch", (w*.22, h+.35, depth*.73), (1.2, .58, .95),
            "metal-dark", metal)
        if i % 4 == 0:
            put("roof/water-tank", (-w*.25, h+.58, depth*.65), (1.25, 1.14, 1.20),
                "metal-light", concrete)
        else:
            put("roof/hvac", (-w*.25, h+.30, depth*.65), (1.30, .58, 1.10),
                "metal-light", concrete)
        self.facade_counts[key] = {"detail_nodes": self.counts[key], "type":category,
                                   "block": b.get("block", ""), "source": "SORA-authored"}


def read_inputs(layout_path, manifest_path):
    layout = json.loads(Path(layout_path).read_text(encoding="utf-8"))
    manifest = json.loads(Path(manifest_path).read_text(encoding="utf-8"))
    if manifest.get("version") != "V05" or manifest.get("kenney_car_count") != 42:
        raise ValueError("V06 requires an existing verified V05 (42 Kenney vehicles)")
    blocks = layout.get("blocks", [])
    buildings = layout.get("building_catalog", [])
    if len(blocks) != 9 or len(buildings) != 60:
        raise ValueError("The reviewed nine-block/60-facade layout is mandatory")
    ids = [b["id"] for b in buildings]
    if len(ids) != len(set(ids)):
        raise ValueError("Duplicate building ID")
    for b in buildings:
        if not (6.0 <= b["width"] <= 25 and 6.0 <= b["depth"] <= 20
                and 6.0 <= b["height"] <= 28):
            raise ValueError("Invalid building size: " + b["id"])
        if b["yaw"] not in (0, 90, 180, -90):
            raise ValueError("Unexpected building yaw: " + b["id"])
        if not all(math.isfinite(float(v)) and -63 <= float(v) <= 108 for v in b["front_xz"]):
            raise ValueError("Out-of-bounds facade: " + b["id"])
    return layout, manifest


def build(layout_path, manifest_path, output):
    layout, prior = read_inputs(layout_path, manifest_path)
    result = DetailPass()
    for i, facade in enumerate(layout["building_catalog"]):
        result.facade(facade, i)
    count = len(result.scene.graph.nodes_geometry)
    if count != result.decorations or len(result.facade_counts) != 60:
        raise AssertionError("Detail scene incomplete")
    output = Path(output)
    output.mkdir(parents=True, exist_ok=True)
    glb = output / "SORA_V06_9Block_Facade_Details.glb"
    data = result.scene.export(file_type="glb")
    glb.write_bytes(data)
    reloaded = trimesh.load(glb, force="scene", process=False)
    if len(reloaded.graph.nodes_geometry) != count:
        raise AssertionError("GLB reimport dropped detail objects")
    document = {
        "version":"V06", "layer":"ADDITIVE DETAIL ONLY", "target":"V03 base + V05 replacement overlay",
        "target_v05_sha256": prior["glb_sha256"], "blocks_unchanged":9,
        "new_v04_v05_facades_detailed":60, "retained_v03_facades_not_modified":24,
        "vehicle_count_unchanged":42, "new_colliders":0,
        "geometry_nodes":count, "bytes":len(data),
        "glb_sha256":hashlib.sha256(data).hexdigest(),
        "facades":result.facade_counts,
        "source_license":"SORA-authored geometry; third-party source not included",
        "unity_editor":"NOT RUN", "unity_playmode":"NOT RUN",
        "quality_gate":"PRELIMINARY ART; EX-01..12 PENDING",
        "important":"V06 is an additive layer at origin (0,0,0) and scale 1. Do not remove V05. Never stack V04 and V05."
    }
    (output / "manifest.json").write_text(json.dumps(document, ensure_ascii=False, indent=2)+"\n",encoding="utf-8")
    print("V06 PASS:", count, "detail mesh nodes across 60 facades; additive GLB bytes:",len(data), flush=True)
    return document


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--layout", type=Path, required=True)
    parser.add_argument("--v05-manifest", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    build(args.layout, args.v05_manifest, args.output)
