#!/usr/bin/env python3
"""Blender 4+/5+ local-only glTF/GLB to FBX + textures conversion helper.

The input contains owner-held third-party-derived meshes: do not commit output to GitHub.
Use:
 blender -b --python Tools/StreetExterior/convert_glb_to_fbx_blender.py -- \
   --input LocalReferenceAssets/SORA_VS01/SORA_Street_VSlice_VS01.glb \
   --output LocalReferenceAssets/SORA_VS01/SORA_Street_VSlice_VS01.fbx

NOT YET TESTED under Blender/Unity; inspect material/UV/normal output manually.
"""
import argparse
from pathlib import Path
import re
import sys


def arguments():
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    return parser.parse_args(args)


def main():
    import bpy
    args = arguments()
    input_path = args.input.resolve()
    output_path = args.output.resolve()
    if not input_path.is_file():
        raise FileNotFoundError(input_path)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    textures = output_path.parent / "Textures"
    textures.mkdir(exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(input_path))
    meshes = [ob for ob in bpy.data.objects if ob.type == "MESH"]
    if not meshes:
        raise RuntimeError("Imported GLB contains no meshes; refusing empty FBX")

    for i, image in enumerate(bpy.data.images):
        if image.source != "FILE" or not image.has_data:
            continue
        filename = re.sub(r"[^A-Za-z0-9_.-]+", "_", image.name)[:64]
        output = textures / ("T_%03d_%s.png" % (i, filename))
        try:
            image.filepath_raw = str(output)
            image.file_format = "PNG"
            image.save()
        except Exception as error:
            print("WARNING: source image save failed", image.name, error)

    bpy.ops.object.select_all(action="DESELECT")
    for obj in meshes:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.export_scene.fbx(filepath=str(output_path), use_selection=True,
        axis_forward="-Z", axis_up="Y", use_mesh_modifiers=True,
        add_leaf_bones=False, path_mode="RELATIVE", bake_space_transform=False)
    print("Exported", len(meshes), "mesh objects to", str(output_path))
    print("Check Unity-import material textures and transparency; FBX is not lossless glTF.")


if __name__ == "__main__":
    main()
