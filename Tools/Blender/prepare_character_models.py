"""Prepare already-unpacked owner models for the local Unity library.

Run with Blender --background --python-exit-code 1 --python this_file --
    --sources LocalReferenceAssets/Characters
Output stays in gitignored Assets/SORA/Characters/LocalImported.
"""
import argparse
import json
import re
import shutil
import sys
from pathlib import Path


def main():
    import bpy
    from bpy_extras.node_shader_utils import PrincipledBSDFWrapper

    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--sources", type=Path, default=Path("LocalReferenceAssets/Characters"))
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    root = Path(__file__).resolve().parents[2]
    source_root = args.sources.resolve()
    output = root / "Assets/SORA/Characters/LocalImported"
    for key in ("Chisa", "Citlali", "Denia", "Lynae", "MaleRover", "Zhongli"):
        directory = source_root / key
        candidates = [p for p in directory.rglob("*") if p.suffix.lower() in (".fbx", ".gltf", ".glb", ".usdz")]
        if len(candidates) != 1:
            raise ValueError(f"Expected one source model under {directory}; found {candidates}")
        source = candidates[0]
        dest = output / key
        textures = dest / "Textures"
        textures.mkdir(parents=True, exist_ok=True)
        # Keep supplied auxiliary maps for inspection; originals remain untouched.
        for texture in sorted(directory.rglob("*")):
            if texture.suffix.lower() in (".png", ".jpg", ".jpeg", ".bmp", ".tga"):
                target = textures / texture.name
                if not target.exists():
                    shutil.copy2(texture, target)
        for credit in directory.rglob("license.txt"):
            shutil.copy2(credit, dest / "source-license.txt")

        target_fbx = dest / (key + ".fbx")
        settings = {"id": key, "sourceFormat": source.suffix.lower(), "materials": []}
        is_fbx = source.suffix.lower() == ".fbx"
        is_ascii = is_fbx and not source.read_bytes().startswith(b"Kaydara FBX Binary")
        if is_ascii:
            # Blender rejects ASCII FBX; Unity's native importer supports it.
            body = source.read_text(encoding="utf-8")
            body = re.sub(r'(?m)^(\s*(?:FileName|Filename|RelativeFilename):\s*)"([^"\r\n]*)"',
                          lambda m: m[1] + '"Textures/' + re.split(r"[\\/]", m[2])[-1] + '"', body)
            target_fbx.write_text(body, encoding="utf-8")
        else:
            bpy.ops.wm.read_factory_settings(use_empty=True)
            if is_fbx:
                bpy.ops.import_scene.fbx(filepath=str(source))
            elif source.suffix.lower() in (".gltf", ".glb"):
                bpy.ops.import_scene.gltf(filepath=str(source))
            else:
                bpy.ops.wm.usd_import(filepath=str(source))
            bpy.ops.file.find_missing_files(directory=str(directory))
            armatures = [o for o in bpy.context.scene.objects if o.type == "ARMATURE"]
            bone_shapes = {b.custom_shape for a in armatures for b in a.pose.bones if b.custom_shape}
            meshes = [o for o in bpy.context.scene.objects if o.type == "MESH" and o not in bone_shapes]
            if not meshes:
                raise ValueError(f"No character meshes in {source}")
            materials = sorted({m for o in meshes for m in o.data.materials if m}, key=lambda m: m.name)
            gltf_materials = {}
            if source.suffix.lower() == ".gltf":
                gltf_materials = {m.get("name", ""): m for m in json.loads(source.read_text(encoding="utf-8")).get("materials", [])}
            for index, material in enumerate(materials):
                wrapper = PrincipledBSDFWrapper(material, is_readonly=True)
                image = wrapper.base_color_texture.image if wrapper.base_color_texture else None
                texture_path = ""
                if image is not None:
                    # Reading size loads packed USD/glTF images lazily.
                    if not image.size[0] or not image.has_data:
                        raise ValueError(f"Missing base-color image for {key}/{material.name}: {image.filepath}")
                    texture_path = f"Textures/Base_{index:02d}.png"
                    image.filepath_raw = str(dest / texture_path)
                    image.file_format = "PNG"
                    image.save()
                elif key == "Lynae":
                    texture_path = "Textures/" + material.name.replace("MI_", "T_", 1) + "_D.png"
                    if not (dest / texture_path).is_file():
                        raise FileNotFoundError(dest / texture_path)
                source_material = gltf_materials.get(material.name, {})
                alpha = source_material.get("alphaMode", "OPAQUE")
                if not source_material and wrapper.node_principled_bsdf and wrapper.node_principled_bsdf.inputs["Alpha"].is_linked:
                    alpha = "MASK"
                color = [*wrapper.base_color[:3], wrapper.alpha] if wrapper.node_principled_bsdf else list(material.diffuse_color)
                if is_fbx:
                    color[3] = 1.0  # FBX transparency factors are not base-color alpha; keep the texture's alpha.
                if key == "Lynae":
                    color = [1, 1, 1, 1]
                # Citlali has a duplicate face with an RGB emote atlas covering the default skin face.
                if key == "Citlali" and material.name == "material":
                    alpha, color[3] = "BLEND", 0.0
                settings["materials"].append({
                    "name": material.name,
                    "texture": texture_path,
                    "color": color,
                    "alphaMode": alpha,
                    "alphaCutoff": source_material.get("alphaCutoff", 0.35),
                })
            settings["sourceBones"] = sum(len(a.data.bones) for a in armatures)
            settings["sourceBlendShapes"] = sum(len(o.data.shape_keys.key_blocks) - 1 for o in meshes if o.data.shape_keys)
            settings["sourceActions"] = [a.name for a in bpy.data.actions]
            if is_fbx:
                shutil.copy2(source, target_fbx)
            else:
                bpy.ops.object.select_all(action="DESELECT")
                for obj in [*meshes, *armatures]:
                    obj.select_set(True)
                    parent = obj.parent
                    while parent:
                        parent.select_set(True)
                        parent = parent.parent
                bpy.context.view_layer.objects.active = meshes[0]
                bpy.ops.export_scene.fbx(filepath=str(target_fbx), use_selection=True,
                    object_types={"ARMATURE", "MESH", "EMPTY"}, add_leaf_bones=False,
                    use_mesh_modifiers=False, axis_forward="-Z", axis_up="Y", path_mode="RELATIVE",
                    bake_anim=bool(bpy.data.actions))
        (dest / "import-settings.json").write_text(json.dumps(settings, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(f"PREPARED {key}: {target_fbx.relative_to(root)}", flush=True)


if __name__ == "__main__":
    main()
