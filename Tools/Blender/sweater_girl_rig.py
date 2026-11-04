"""Unity-Humanoid rig, region skin weights and scripted in-place loops for build_sweater_girl.py.

Character faces -Y, her left is +X. Poses are authored as (axis, degrees) in armature space and converted to each
bone's rest frame; left-side values mirror to the right (X axis kept, Y/Z flipped). Pitch about +X: positive tips a
downward bone backward (+Y) and an upward bone forward (-Y).
"""
import math

import bpy
import numpy as np
from mathutils import Quaternion, Vector

FPS = 30
FOOT_Z = (0.09, 0.14)  # legs/boots: below -> Foot, above -> LowerLeg (boot shaft follows the shin)
TOE_Y = (0.07, 0.11)   # boot-local -y: Foot -> Toes
X, Y, Z = (1, 0, 0), (0, 1, 0), (0, 0, 1)
SIDES = (('Left', 1), ('Right', -1))
# thigh (forward bias, amp), knee (base, amp, phase shift), ankle compensation, upper arm (bias, amp),
# elbow (bend, extra bend while forward, yaw), torso lean, pelvis twist; degrees
WALK = dict(thigh=(2, 24), knee=(5, 45, 0.8), ankle=0.75, arm=(0, 14), elbow=(-12, -10, 0), lean=2, twist=5)
RUN = dict(thigh=(10, 34), knee=(20, 80, 0.9), ankle=0.6, arm=(0, 30), elbow=(-75, -10, -25), lean=8, twist=7)
CLIPS = (('Idle', 96, None), ('Walk', 32, WALK), ('Run', 20, RUN))  # frames per loop at FPS


def smooth(v, a, b):
    t = np.clip((v - a) / (b - a), 0, 1)
    return t * t * (3 - 2 * t)


# ---------------------------------------------------------------- skeleton
def bone_table(arm_a, arm_b, boot_yaw):
    d = arm_b - arm_a
    hand = d / np.linalg.norm(d) + (0.3, 0, 0)
    hand /= np.linalg.norm(hand)
    c, s = math.cos(boot_yaw), math.sin(boot_yaw)

    def boot(y, z):  # boot-local (0, y, z) -> +X side, matching build_boots
        return 0.066 - s * y, c * y, z

    knee, ankle, toe = (0.052, -0.004, 0.45), (0.066, 0, 0.09), boot(-0.10, 0.03)
    table = [('Hips', None, (0, 0, .78), (0, 0, .90)), ('Spine', 'Hips', (0, 0, .90), (0, 0, 1.05)),
             ('Chest', 'Spine', (0, 0, 1.05), (0, 0, 1.20)), ('Neck', 'Chest', (0, 0, 1.20), (0, 0, 1.30)),
             ('Head', 'Neck', (0, 0, 1.30), (0, 0, 1.54))]
    limbs = [('Shoulder', 'Chest', (.03, 0, 1.15), arm_a), ('UpperArm', 'Shoulder', arm_a, arm_a + d / 2),
             ('LowerArm', 'UpperArm', arm_a + d / 2, arm_b), ('Hand', 'LowerArm', arm_b, arm_b + hand * .09),
             ('UpperLeg', 'Hips', (.075, 0, .75), knee), ('LowerLeg', 'UpperLeg', knee, ankle),
             ('Foot', 'LowerLeg', ankle, toe), ('Toes', 'Foot', toe, boot(-.155, .03))]
    for side, sgn in SIDES:
        m = np.array([sgn, 1, 1])
        for name, parent, head, tail in limbs:
            table.append((side + name, parent if parent in ('Chest', 'Hips') else side + parent,
                          np.asarray(head, float) * m, np.asarray(tail, float) * m))
    return table


def build_armature(root, table):
    arm = bpy.data.objects.new('Armature', bpy.data.armatures.new('SG_Rig'))
    bpy.context.scene.collection.objects.link(arm)
    arm.parent = root
    bpy.context.view_layer.objects.active = arm
    bpy.ops.object.mode_set(mode='EDIT')
    eb = arm.data.edit_bones
    for name, parent, head, tail in table:
        b = eb.new(name)
        b.head, b.tail = Vector(tuple(map(float, head))), Vector(tuple(map(float, tail)))
        b.parent = eb[parent] if parent else None
    bpy.ops.object.mode_set(mode='OBJECT')
    return arm


# ---------------------------------------------------------------- skinning
def region_weights(name, v, tag, arm_a, arm_b, boot_yaw):
    """{bone: per-vertex weight} for one mesh; object space == armature space."""
    x, y, z = v.T
    w = {}

    def put(bone, val):
        w[bone] = w.get(bone, 0.0) + np.asarray(val, float)

    def both(bone, val):  # split by body side
        left = x >= 0
        put('Left' + bone, val * left)
        put('Right' + bone, val * ~left)

    if name == 'Face':  # mesh names must differ from Humanoid bone names
        put('Head', np.ones(len(v)))
    elif name == 'Hair':
        h = smooth(z, 1.2, 1.3)
        put('Head', h)
        put('Chest', 1 - h)
    elif name == 'Accessories':
        leg = z < 0.7  # ribbon + bow on her left stocking
        put('Head', ~leg)
        put('LeftUpperLeg', leg)
    elif name == 'Skirt':
        a = 0.35 * smooth(z, 0.72, 0.64)  # hem follows the thighs a little; blended across the front seam
        lw = smooth(x, -0.05, 0.05)
        put('Hips', 1 - a)
        put('LeftUpperLeg', a * lw)
        put('RightUpperLeg', a * (1 - lw))
    elif name == 'Boots':
        ly = -math.sin(boot_yaw) * (np.abs(x) - 0.066) + math.cos(boot_yaw) * y  # undo toe-out yaw
        foot = 1 - smooth(z, *FOOT_Z)
        toe = smooth(-ly, *TOE_Y)
        both('LowerLeg', 1 - foot)
        both('Foot', foot * (1 - toe))
        both('Toes', foot * toe)
    elif name == 'Body':
        neck = z > 1.1
        hand = (np.abs(x) > 0.25) & ~neck
        legs = ~(neck | hand)
        h, k, f = smooth(z, 1.27, 1.31), smooth(z, 0.42, 0.50), smooth(z, *FOOT_Z)
        put('Neck', neck * (1 - h))
        put('Head', neck * h)
        both('Hand', hand)
        both('UpperLeg', legs * k)
        both('LowerLeg', legs * (1 - k) * f)
        both('Foot', legs * (1 - f))
    elif name == 'Sweater':
        torso, sleeve = tag == 0, tag == 1
        s, c, nk = smooth(z, 0.86, 0.96), smooth(z, 1.0, 1.08), smooth(z, 1.17, 1.21)
        put('Hips', torso * (1 - s))
        put('Spine', torso * s * (1 - c))
        put('Chest', torso * s * c * (1 - nk))
        put('Neck', torso * s * c * nk)
        d = arm_b - arm_a
        t = (np.column_stack([np.abs(x), y, z]) - arm_a) @ d / (d @ d)  # 0 shoulder .. 1 wrist
        sh, el = smooth(t, 0.0, 0.15), smooth(t, 0.42, 0.58)
        both('Shoulder', sleeve * (1 - sh))
        both('UpperArm', sleeve * sh * (1 - el))
        both('LowerArm', sleeve * sh * el)
    else:
        raise ValueError(f'no skin rule for {name}')
    return w


def skin(arm, root, tags, arm_a, arm_b, boot_yaw):
    for ob in root.children:
        if ob.type != 'MESH':
            continue
        n = len(ob.data.vertices)
        co = np.empty(n * 3)
        ob.data.vertices.foreach_get('co', co)
        w = region_weights(ob.name, co.reshape(-1, 3), tags.get(ob.name, np.zeros(n)), arm_a, arm_b, boot_yaw)
        w = {bone: np.broadcast_to(val, n) for bone, val in w.items()}
        assert np.allclose(sum(w.values()), 1, atol=1e-6), ob.name
        for bone, val in w.items():
            q = np.round(val * 64) / 64  # quantised: one vertex_groups.add per value, not per vertex
            vg = ob.vertex_groups.new(name=bone)
            for k in np.unique(q[q > 0]):
                vg.add(np.flatnonzero(q == k).tolist(), float(k), 'REPLACE')
        ob.modifiers.new('Armature', 'ARMATURE').object = arm
        ob.parent = arm  # identity transforms, so nothing moves


def build(root, tags, arm_a, arm_b, boot_yaw):
    arm = build_armature(root, bone_table(arm_a, arm_b, boot_yaw))
    skin(arm, root, tags, arm_a, arm_b, boot_yaw)
    return arm


# ---------------------------------------------------------------- animation
def rotation(bone, rots):
    to_local = bone.matrix_local.to_3x3().inverted()
    q = Quaternion()
    for axis, deg in rots:
        q = Quaternion(to_local @ Vector(axis), math.radians(deg)) @ q
    return q


def mirrored(rots, sgn):
    return [((ax, ay * sgn, az * sgn), deg) for (ax, ay, az), deg in rots]


def idle(ph):
    b = math.sin(ph)  # breath
    pose = {'Spine': [(X, 0.4 * b)], 'Chest': [(X, -1.0 * b)], 'Neck': [(X, 0.4 * b)],
            'Head': [(Y, 1.5 * math.sin(ph + 1.0)), (X, 0.8 * math.sin(2 * ph))]}
    for side, sgn in SIDES:
        pose[side + 'Shoulder'] = mirrored([(Y, -0.6 * (1 + b))], sgn)
        pose[side + 'UpperArm'] = [(X, 1.5 * math.sin(ph + 0.5))]
        pose[side + 'LowerArm'] = [(X, -8 - 1.5 * b)]
    return pose, 0.0


def gait(ph, g, legs):
    """One frame of an in-place walk/run; ph = left-leg phase, legs = (thigh, shin) length."""
    pose, reach = {}, []
    for side, sgn in SIDES:
        p = ph if sgn > 0 else ph + math.pi
        th = -(g['thigh'][0] + g['thigh'][1] * math.sin(p))
        kn = g['knee'][0] + g['knee'][1] * max(0.0, math.cos(p + g['knee'][2])) ** 1.5
        bend, extra, yaw = g['elbow']
        pose[side + 'UpperLeg'] = [(X, th)]
        pose[side + 'LowerLeg'] = [(X, kn)]
        pose[side + 'Foot'] = [(X, -g['ankle'] * (th + kn))]  # keep the sole near flat
        pose[side + 'UpperArm'] = [(X, g['arm'][0] + g['arm'][1] * math.sin(p))]  # swings against its own leg
        pose[side + 'LowerArm'] = mirrored([(X, bend + extra * max(0.0, -math.sin(p))), (Z, yaw)], sgn)
        reach.append(legs[0] * math.cos(math.radians(th)) + legs[1] * math.cos(math.radians(th + kn)))
    tw = g['twist'] * math.sin(ph)
    pose.update(Hips=[(Z, -tw)], Spine=[(X, g['lean']), (Z, 0.6 * tw)], Chest=[(Z, 0.9 * tw)],
                Neck=[(Z, -0.25 * tw)], Head=[(X, -0.6 * g['lean']), (Z, -0.25 * tw)])
    return pose, max(reach) - sum(legs)  # drop the hips until the lower foot meets the ground


def make_actions(arm):
    bpy.context.scene.render.fps = FPS
    bones = arm.pose.bones
    legs = (arm.data.bones['LeftUpperLeg'].length, arm.data.bones['LeftLowerLeg'].length)
    up = arm.data.bones['Hips'].matrix_local.to_3x3().inverted() @ Vector(Z)
    ad = arm.animation_data_create()
    for name, n, spec in CLIPS:
        ad.action = bpy.data.actions.new(name)
        ad.action.use_fake_user = True
        for f in range(n + 1):  # last frame repeats the first: seamless loop
            ph = 2 * math.pi * f / n
            pose, dz = gait(ph, spec, legs) if spec else idle(ph)
            for pb in bones:
                pb.rotation_quaternion = rotation(pb.bone, pose.get(pb.name, ()))
                pb.keyframe_insert('rotation_quaternion', frame=f + 1, group=pb.name)
            bones['Hips'].location = up * dz
            bones['Hips'].keyframe_insert('location', frame=f + 1, group='Hips')
    ad.action = None
    for pb in bones:
        pb.rotation_quaternion, pb.location = Quaternion(), Vector()
