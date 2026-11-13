"""Replace authored takes in the existing FBX; keep its meshes, skin and rest rig.

Run with Blender: --background --python Tools/Blender/export_character_motion.py
"""
import json
import sys
from pathlib import Path

import bpy

sys.path.insert(0, str(Path(__file__).resolve().parent))
import sweater_girl_rig as rig

root = Path(__file__).resolve().parents[2]
source = root / 'Assets/SORA/Characters/SweaterGirl/sweater_girl.fbx'
output = root / 'Artifacts/CharacterMotion'
output.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(source))
arm, = [ob for ob in bpy.context.scene.objects if ob.type == 'ARMATURE']
for action in list(bpy.data.actions):
    bpy.data.actions.remove(action)
arm.animation_data_clear()
for bone in arm.pose.bones:
    bone.rotation_mode = 'QUATERNION'
    bone.rotation_quaternion = (1, 0, 0, 0)
    bone.location = (0, 0, 0)
rig.make_actions(arm)
facts = {'clips': [a.name for a in bpy.data.actions], 'bones': len(arm.data.bones),
         'meshes': {ob.name: len(ob.data.vertices) for ob in bpy.context.scene.objects if ob.type == 'MESH'}}
bpy.ops.export_scene.fbx(filepath=str(output / 'sweater_girl.fbx'),
    object_types={'EMPTY', 'MESH', 'ARMATURE'}, apply_scale_options='FBX_SCALE_UNITS',
    mesh_smooth_type='FACE', path_mode='COPY', embed_textures=True, add_leaf_bones=False,
    bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
    bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0)
(output / 'export.json').write_text(json.dumps(facts, indent=2), encoding='utf-8')
print('SORA_MOTION_EXPORT:', len(facts['clips']), 'clips;', facts['bones'], 'bones')
