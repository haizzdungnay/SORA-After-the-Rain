#!/usr/bin/env python3
"""Inspect a legitimately obtained AFX street model using Blender, without uploading it.

Run (from the repository root):
  blender -b -Y --python Tools/Blender/audit_street_reference.py -- \
    --input LocalReferenceAssets/AFX_JapaneseStreet/scene.gltf \
    --output LocalReferenceAssets/AFX_JapaneseStreet/audit --render

The output is evidence for a HUMAN frontage audit; a mesh is not a shop.
Blender 4.x/5.x required for FBX, glTF/GLB, OBJ or .blend inputs.
"""
import argparse
import csv
import hashlib
import json
import sys
from pathlib import Path


def parse_args(argv):
    items = argv[argv.index("--") + 1:] if "--" in argv else argv[1:]
    parser = argparse.ArgumentParser(description="Local-only Blender AFX geometry and texture audit")
    parser.add_argument("--input", required=True, help="Local source .glb/.gltf/.fbx/.obj/.blend")
    parser.add_argument("--output", required=True, help="Output directory (keep out of the public repo)")
    parser.add_argument("--render", action="store_true", help="Render top and four oblique inspection images")
    return parser.parse_args(items)


def sha256(path):
    digest = hashlib.sha256()
    with path.open("rb") as source:
        for block in iter(lambda: source.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def load_source(bpy, source):
    suffix = source.suffix.lower()
    if suffix == ".blend":
        bpy.ops.wm.open_mainfile(filepath=str(source))
        return
    bpy.ops.wm.read_factory_settings(use_empty=True)
    if suffix in (".glb", ".gltf"):
        bpy.ops.import_scene.gltf(filepath=str(source))
    elif suffix == ".fbx":
        bpy.ops.import_scene.fbx(filepath=str(source))
    elif suffix == ".obj":
        bpy.ops.wm.obj_import(filepath=str(source))
    else:
        raise ValueError("Unsupported file type: " + suffix)


def bounds_for(obj, Vector):
    corners = [obj.matrix_world @ Vector(point) for point in obj.bound_box]
    return (
        [min(c[k] for c in corners) for k in range(3)],
        [max(c[k] for c in corners) for k in range(3)],
    )


def render_views(bpy, Vector, limits, outdir):
    mn, mx = limits
    center = Vector([(a + b) / 2 for a, b in zip(mn, mx)])
    span = max(max(b - a for a, b in zip(mn, mx)), 1.0)
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.display.shading.color_type = "MATERIAL"
    scene.render.resolution_x = 1280
    scene.render.resolution_y = 720
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    camera_data = bpy.data.cameras.new("SORA_Audit_Camera")
    camera_data.type = "ORTHO"
    camera_data.ortho_scale = span * 2.0
    camera = bpy.data.objects.new("SORA_Audit_Camera", camera_data)
    scene.collection.objects.link(camera)
    scene.camera = camera
    views = {
        "top": (0, 0, 1),
        "north_oblique": (0, 1, 0.65),
        "south_oblique": (0, -1, 0.65),
        "east_oblique": (1, 0, 0.65),
        "west_oblique": (-1, 0, 0.65),
    }
    rendered = []
    for name, direction in views.items():
        eye = center + Vector(direction).normalized() * span * 2.5
        camera.location = eye
        camera.rotation_euler = (center - eye).to_track_quat("-Z", "Y").to_euler()
        path = outdir / ("overview_" + name + ".png")
        scene.render.filepath = str(path)
        bpy.ops.render.render(write_still=True)
        rendered.append(path.name)
    return rendered


def main():
    args = parse_args(sys.argv)
    source = Path(args.input).expanduser().resolve()
    outdir = Path(args.output).expanduser().resolve()
    if not source.is_file():
        raise SystemExit("Missing source file: " + str(source))
    if source.suffix.lower() not in {".glb", ".gltf", ".fbx", ".obj", ".blend"}:
        raise SystemExit("Input must be .glb/.gltf/.fbx/.obj/.blend")
    if outdir == source.parent and args.render:
        print("Note: reports and renders will share the source folder.")
    try:
        import bpy
        from mathutils import Vector
    except ImportError:
        raise SystemExit("Run with Blender's Python: blender -b -Y --python ...")
    outdir.mkdir(parents=True, exist_ok=True)
    load_source(bpy, source)

    objects = []
    min_total = [float("inf")] * 3
    max_total = [float("-inf")] * 3
    for obj in sorted(bpy.data.objects, key=lambda x: x.name):
        if obj.type != "MESH":
            continue
        mesh = obj.data
        mn, mx = bounds_for(obj, Vector)
        for i in range(3):
            min_total[i] = min(min_total[i], mn[i])
            max_total[i] = max(max_total[i], mx[i])
        objects.append({
            "object_path": "/".join(reversed([o.name for o in _ancestors(obj)])),
            "name": obj.name,
            "vertices": len(mesh.vertices),
            "triangles_base_estimate": sum(max(0, len(p.vertices) - 2) for p in mesh.polygons),
            "material_slots": "|".join(slot.material.name if slot.material else "" for slot in obj.material_slots),
            "uv_layers": len(mesh.uv_layers),
            "min_x": round(mn[0], 4), "min_y": round(mn[1], 4), "min_z": round(mn[2], 4),
            "max_x": round(mx[0], 4), "max_y": round(mx[1], 4), "max_z": round(mx[2], 4),
        })

    images = []
    for img in sorted(bpy.data.images, key=lambda x: x.name):
        raw_path = img.filepath or ""
        unpacked_path = bpy.path.abspath(raw_path) if raw_path else ""
        packed = bool(img.packed_file) or bool(getattr(img, "packed_files", []))
        found = bool(unpacked_path and Path(unpacked_path).is_file())
        images.append({
            "name": img.name, "source": img.source, "packed": packed,
            "file_found": found, "width": img.size[0], "height": img.size[1],
            "filepath": raw_path,
        })

    result = {
        "source_name": source.name, "source_sha256": sha256(source),
        "format": source.suffix.lower(), "mesh_count": len(objects),
        "vertices_base": sum(x["vertices"] for x in objects),
        "triangles_base_estimate": sum(x["triangles_base_estimate"] for x in objects),
        "material_count": len(bpy.data.materials), "image_count": len(images),
        "missing_unpacked_images": [x["name"] for x in images if x["source"] == "FILE" and not x["packed"] and not x["file_found"]],
        "world_bounds_blender_units": [min_total, max_total] if objects else None,
        "dimensions_are_not_verified_meters": True,
        "do_not_infer_number_of_stores_from_meshes": True,
        "objects": objects, "images": images, "renders": [],
    }
    if args.render and objects:
        result["renders"] = render_views(bpy, Vector, (min_total, max_total), outdir)
    (outdir / "scene_audit.json").write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    with (outdir / "mesh_inventory.csv").open("w", encoding="utf-8", newline="") as file:
        writer = csv.DictWriter(file, fieldnames=[
            "object_path", "name", "vertices", "triangles_base_estimate", "material_slots", "uv_layers",
            "min_x", "min_y", "min_z", "max_x", "max_y", "max_z"])
        writer.writeheader()
        writer.writerows(objects)
    print("G0 audit complete:", len(objects), "mesh objects,", len(images), "images.")
    print("Evidence:", str(outdir))
    print("NOTE: human review of storefronts, scale, hidden surfaces and rights is still required.")


def _ancestors(obj):
    chain = []
    while obj is not None:
        chain.append(obj)
        obj = obj.parent
    return chain


if __name__ == "__main__":
    main()
