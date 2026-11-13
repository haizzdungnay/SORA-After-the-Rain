"""Unity-Humanoid rig, region skin weights and scripted in-place clips for build_sweater_girl.py.

Character faces -Y, her left is +X. Poses are authored as (axis, degrees) in armature space and converted to each
bone's rest frame; left-side values mirror to the right (X axis kept, Y/Z flipped). Pitch about +X: positive tips a
downward bone backward (+Y) and an upward bone forward (-Y).
"""
import math

import bpy
import numpy as np
from mathutils import Quaternion, Vector

FPS = 30
TAU = 2 * math.pi
FOOT_Z = (0.09, 0.14)  # legs/boots: below -> Foot, above -> LowerLeg (boot shaft follows the shin)
TOE_Y = (0.07, 0.11)   # boot-local -y: Foot -> Toes
X, Y, Z = (1, 0, 0), (0, 1, 0), (0, 0, 1)
SIDES = (('Left', 1), ('Right', -1))
# thigh (forward bias, amp), knee (base, amp, phase shift), ankle compensation, upper arm (bias, amp),
# elbow (bend, extra bend while forward, yaw), torso lean, pelvis twist; degrees
WALK = dict(thigh=(2, 24), knee=(5, 45, 0.8), ankle=0.75, arm=(0, 14), elbow=(-12, -10, 0), lean=2, twist=5)
JOG = dict(thigh=(6, 29), knee=(12, 62, 0.9), ankle=0.7, arm=(0, 21), elbow=(-42, -10, -8), lean=4, twist=6)
RUN = dict(thigh=(10, 34), knee=(20, 80, 0.9), ankle=0.6, arm=(0, 30), elbow=(-75, -10, -25), lean=8, twist=7)


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


def flat_pose(frame):  # every authored number of one frame, for the loop-closure check
    pose, dz = frame
    return [deg for rots in pose.values() for _, deg in rots] + [dz]


def idle(u, legs):
    ph = TAU * u
    b = math.sin(ph)  # breath
    pose = {'Spine': [(X, 0.4 * b)], 'Chest': [(X, -1.0 * b)], 'Neck': [(X, 0.4 * b)],
            'Head': [(Y, 1.5 * math.sin(ph + 1.0)), (X, 0.8 * math.sin(2 * ph))]}
    for side, sgn in SIDES:
        pose[side + 'Shoulder'] = mirrored([(Y, -0.6 * (1 + b))], sgn)
        pose[side + 'UpperArm'] = mirrored([(Y, 22), (X, 1.5 * math.sin(ph + 0.5))], sgn)
        pose[side + 'LowerArm'] = [(X, -8 - 1.5 * b)]
    return pose, 0.0


def blend(a, b, t):
    """Ease between two authored frames; an axis missing from one side counts as 0 degrees there."""
    (pa, za), (pb, zb) = a, b
    s = float(smooth(t, 0, 1))
    pose = {}
    for name in set(pa) | set(pb):
        ra = dict(pa.get(name, ()))
        rb = dict(pb.get(name, ()))
        pose[name] = [(ax, ra.get(ax, 0.0) * (1 - s) + rb.get(ax, 0.0) * s)
                      for ax in dict.fromkeys(list(ra) + list(rb))]
    return pose, za * (1 - s) + zb * s


def idle_look_around(u, legs):
    """A quiet glance each way, returning to the neutral breathing pose."""
    neutral = idle(0, legs)
    pose = dict(neutral[0])
    look = 28 * math.sin(TAU * u)
    pose.update(Head=[(Z, look), (X, -3)], Neck=[(Z, look * .2)],
                Chest=[(Z, look * .12), (X, -1)])
    fade = float(smooth(u, 0, .18) * (1 - smooth(u, .82, 1)))
    return blend(neutral, (pose, 0.0), fade)


def idle_stretch(u, legs):
    """A relaxed shoulder/neck stretch; feet and root stay at their neutral marks."""
    neutral = idle(0, legs)
    pose = dict(neutral[0])
    for side, sgn in SIDES:
        pose[side + 'Shoulder'] = mirrored([(Y, -10)], sgn)
        pose[side + 'UpperArm'] = mirrored([(Y, -52), (X, 12)], sgn)
        pose[side + 'LowerArm'] = mirrored([(X, -66)], sgn)
    pose.update(Spine=[(X, -4)], Chest=[(X, -3)], Head=[(X, -5), (Y, 8 * math.sin(TAU * u))])
    fade = float(smooth(u, 0, .28) * (1 - smooth(u, .7, 1)))
    return blend(neutral, (pose, 0.0), fade)


def stance(legs, thigh, knee, arm, elbow, lean, ground=1.0):
    """Symmetric pose in degrees; ground=0 leaves the hips at rest height (feet off the floor)."""
    pose = {'Spine': [(X, lean)], 'Head': [(X, -0.6 * lean)]}
    for side, sgn in SIDES:
        pose[side + 'UpperLeg'] = [(X, thigh)]
        pose[side + 'LowerLeg'] = [(X, knee)]
        pose[side + 'Foot'] = [(X, -0.75 * (thigh + knee))]
        pose[side + 'UpperArm'] = mirrored([(Y, 18), (X, arm)], sgn)
        pose[side + 'LowerArm'] = [(X, elbow)]
    reach = legs[0] * math.cos(math.radians(thigh)) + legs[1] * math.cos(math.radians(thigh + knee))
    return pose, ground * (reach - sum(legs))


def jump(u, legs):
    """Gather on both feet, then drive off; one-shot. The CharacterController supplies the rise."""
    crouch = stance(legs, -36, 72, 18, -34, 14)
    launch = stance(legs, -6, 4, -52, -12, -5, ground=0)
    return blend(idle(0, legs), crouch, u / .4) if u < .4 else blend(crouch, launch, (u - .4) / .6)


def fall(u, legs):
    """Airborne loop: legs trail, arms out to balance."""
    s = math.sin(TAU * u)
    pose, dz = stance(legs, -14 + 5 * s, 30 + 9 * s, -40 - 6 * s, -26, -7, ground=0)
    pose['Head'] = [(X, 5 + 2 * s)]
    return pose, dz


def land(u, legs):
    """Absorb the impact, then settle back into the idle stance; one-shot."""
    touch = stance(legs, -10, 16, -24, -20, -3)
    absorb = stance(legs, -28, 56, 12, -30, 13)
    return blend(touch, absorb, u / .3) if u < .3 else blend(absorb, idle(0, legs), (u - .3) / .7)


def turn(u, legs, sgn):
    """One-shot in-place pivot; sgn +1 steps to her left. Root yaw stays with the controller."""
    lift = math.sin(math.pi * u)
    tw, lead = sgn * 15 * lift, ('Left' if sgn > 0 else 'Right')
    th, kn = -5 - 24 * lift, 7 + 38 * lift
    pose, dz = stance(legs, -5, 7, 5, -16, 2)
    pose[lead + 'UpperLeg'] = [(X, th)]
    pose[lead + 'LowerLeg'] = [(X, kn)]
    pose[lead + 'Foot'] = [(X, -0.75 * (th + kn))]
    pose.update(Hips=[(Z, -tw)], Spine=[(X, 2), (Z, 0.7 * tw)], Chest=[(Z, 0.9 * tw)],
                Head=[(Y, sgn * 11 * lift), (Z, -0.4 * tw)])
    return pose, dz


def seated(legs, breath=0.0):
    """Perched on a chair: thighs level and forward, shins vertical, feet flat, hands on the thighs.

    Thigh and knee cancel, so the shin hangs vertical and the sole stays flat at Foot = 0. The hip drop
    falls out of the leg geometry in stance(): no seat height is written down here, and the chair prop has
    to match the pose rather than the other way round (~0.49 m hip height on this rig).
    """
    pose, dz = stance(legs, -88, 88, -6, -26, 4)
    pose['Spine'] = [(X, 4 + 0.5 * breath)]
    pose['Chest'] = [(X, -1.2 * breath)]
    pose['Head'] = [(X, -3 + 0.4 * breath)]
    for side, sgn in SIDES:
        pose[side + 'UpperArm'] = [(X, -4 - 1.5 * breath)]
        pose[side + 'LowerArm'] = mirrored([(X, -46), (Z, -10)], sgn)
    return pose, dz


def sit_idle(u, legs):
    """Seated breathing loop. The chair is scenery, so the root never moves here."""
    return seated(legs, math.sin(TAU * u))


def sit_down(u, legs):
    """Hips back over the seat, then settle onto it; one-shot ending on the seated pose."""
    perch = stance(legs, -34, 62, 14, -30, 10)  # weight back, hands ready to take it
    return blend(idle(0, legs), perch, u / .45) if u < .45 else blend(perch, seated(legs), (u - .45) / .55)


def stand_up(u, legs):
    """Push off the seat and straighten; one-shot, the way out of sit_idle."""
    rise = stance(legs, -30, 54, -8, -34, 18)
    return blend(seated(legs), rise, u / .5) if u < .5 else blend(rise, idle(0, legs), (u - .5) / .5)


def pick_up(u, legs):
    """Squat, close her right hand on something at her feet, then rise holding it; one-shot."""
    low, low_dz = stance(legs, -58, 92, -28, -44, 26)
    low['Head'] = [(X, 18)]  # looking at what she is taking
    low['RightUpperArm'] = [(X, -44)]
    low['RightLowerArm'] = mirrored([(X, -56), (Z, 12)], -1)
    carry, carry_dz = stance(legs, -8, 12, -20, -62, 6)
    carry['RightLowerArm'] = mirrored([(X, -84), (Z, 14)], -1)  # held in at the waist
    return (blend(idle(0, legs), (low, low_dz), u / .42) if u < .42
            else blend((low, low_dz), (carry, carry_dz), (u - .42) / .58))


def open_door(u, legs):
    """Right hand onto the handle, push it away, lean through the gap; one-shot."""
    touch, touch_dz = stance(legs, -6, 10, 4, -20, 6)
    touch['RightUpperArm'] = [(X, -62)]
    touch['RightLowerArm'] = mirrored([(X, -34), (Z, -14)], -1)
    push, push_dz = stance(legs, -16, 22, 8, -16, 14)  # her weight goes into the door
    push['RightUpperArm'] = [(X, -78)]
    push['RightLowerArm'] = mirrored([(X, -8), (Z, -6)], -1)
    push['Head'] = [(X, -2), (Y, -8)]
    return (blend(idle(0, legs), (touch, touch_dz), u / .35) if u < .35
            else blend((touch, touch_dz), (push, push_dz), (u - .35) / .65))


def pos_scan(u, legs):
    """Sweep an item across the counter scanner with her right hand; one-shot, one item per play."""
    over, over_dz = stance(legs, -4, 8, 2, -22, 7)
    over['RightUpperArm'] = [(X, -34)]
    over['RightLowerArm'] = mirrored([(X, -62), (Z, -18)], -1)  # forearm across her body, over the glass
    over['Head'] = [(X, 10), (Y, -6)]
    past, past_dz = stance(legs, -4, 8, 2, -22, 5)
    past['RightUpperArm'] = [(X, -28)]
    past['RightLowerArm'] = mirrored([(X, -58), (Z, 16)], -1)  # carried on out to the bagging side
    past['Head'] = [(X, 8), (Y, 8)]
    return (blend(idle(0, legs), (over, over_dz), u / .4) if u < .4
            else blend((over, over_dz), (past, past_dz), (u - .4) / .6))


def hand_over(u, legs):
    """Both hands out to give or take goods across the counter, then back in; one-shot.

    ponytail: one clip serves giving and receiving, because the pose is the same gesture either way;
    split it once the hands need to close on a specific prop.
    """
    offer, offer_dz = stance(legs, -4, 8, -40, -58, 6)
    offer['Head'] = [(X, 6)]
    for side, sgn in SIDES:
        offer[side + 'Shoulder'] = mirrored([(Y, -4)], sgn)
        offer[side + 'LowerArm'] = mirrored([(X, -62), (Z, -14 * sgn)], sgn)  # hands meet in front of her
    return (blend(idle(0, legs), (offer, offer_dz), u / .45) if u < .45
            else blend((offer, offer_dz), idle(0, legs), (u - .45) / .55))


def wave(u, legs, side='Right'):
    """Raised hand swinging twice; upper body only, so the Action layer mask reads it over a walk.

    The arm rests pointing out and down, so the raise is about Y (her forward axis), not the pitch axis
    the other clips use. dz stays 0: a masked gesture must never move the root.
    """
    sgn = 1 if side == 'Left' else -1
    pose, _ = idle(u, legs)
    swing = math.sin(2 * TAU * u) * float(smooth(min(u, 1 - u), 0, .22))  # fades in and out at both ends
    pose[side + 'Shoulder'] = mirrored([(Y, -16)], sgn)
    pose[side + 'UpperArm'] = mirrored([(Y, -104), (X, -12)], sgn)
    pose[side + 'LowerArm'] = mirrored([(Y, -22 + 26 * swing), (X, -18)], sgn)
    pose['Head'] = [(X, -4), (Y, sgn * -7)]
    pose['Chest'] = [(X, -2)]
    return pose, 0.0


def nod(u, legs):
    """Two agreeing dips of the head; upper body only. Never tips back, so the dip stays non-negative."""
    pose, _ = idle(u, legs)
    dip = (1 - math.cos(2 * TAU * u)) / 2
    pose['Head'] = [(X, 3 + 15 * dip)]
    pose['Neck'] = [(X, 4 * dip)]
    pose['Chest'] = [(X, 1.5 * dip)]
    return pose, 0.0


def shake(u, legs):
    """Head turning side to side twice; upper body only. Symmetric, so the raw sine is right here."""
    pose, _ = idle(u, legs)
    turn_ = math.sin(2 * TAU * u) * float(smooth(min(u, 1 - u), 0, .25))
    pose['Head'] = [(Y, 21 * turn_), (X, -2)]
    pose['Neck'] = [(Y, 6 * turn_)]
    return pose, 0.0


def gait(u, g, legs):
    """One frame of an in-place walk/run; u = normalized time, legs = (thigh, shin) length."""
    ph = TAU * u
    pose, reach = {}, []
    for side, sgn in SIDES:
        p = ph if sgn > 0 else ph + math.pi
        th = -(g['thigh'][0] + g['thigh'][1] * math.sin(p))
        kn = g['knee'][0] + g['knee'][1] * max(0.0, math.cos(p + g['knee'][2])) ** 1.5
        bend, extra, yaw = g['elbow']
        pose[side + 'UpperLeg'] = [(X, th)]
        pose[side + 'LowerLeg'] = [(X, kn)]
        pose[side + 'Foot'] = [(X, -g['ankle'] * (th + kn))]  # keep the sole near flat
        pose[side + 'UpperArm'] = mirrored([(Y, 18), (X, g['arm'][0] + g['arm'][1] * math.sin(p))], sgn)
        pose[side + 'LowerArm'] = mirrored([(X, bend + extra * max(0.0, -math.sin(p))), (Z, yaw)], sgn)
        reach.append(legs[0] * math.cos(math.radians(th)) + legs[1] * math.cos(math.radians(th + kn)))
    tw = g['twist'] * math.sin(ph)
    pose.update(Hips=[(Z, -tw)], Spine=[(X, g['lean']), (Z, 0.6 * tw)], Chest=[(Z, 0.9 * tw)],
                Neck=[(Z, -0.25 * tw)], Head=[(X, -0.6 * g['lean']), (Z, -0.25 * tw)])
    return pose, max(reach) - sum(legs)  # drop the hips until the lower foot meets the ground


# name, frames at FPS, looping, pose generator of (u, legs)
# ponytail: walk/run starts and stops are Animator transition blends, not clips; author planted stops with a real gait pass.
CLIPS = (('Idle', 96, True, idle), ('Walk', 32, True, lambda u, legs: gait(u, WALK, legs)),
         ('Jog', 26, True, lambda u, legs: gait(u, JOG, legs)),
         ('Run', 20, True, lambda u, legs: gait(u, RUN, legs)), ('Fall', 24, True, fall),
         ('Jump', 14, False, jump), ('Land', 18, False, land),
         ('TurnLeft', 16, False, lambda u, legs: turn(u, legs, 1)),
         ('TurnRight', 16, False, lambda u, legs: turn(u, legs, -1)),
         ('IdleLookAround', 108, False, idle_look_around), ('IdleStretch', 114, False, idle_stretch),
         # Life-sim interactions. Each full-body one-shot opens on the neutral idle pose so the Animator can
         # blend in from the locomotion layer; it ends wherever the action leaves her (seated, holding the
         # item, leaning on the door), and the transition out of that is the Animator's to blend.
         ('SitDown', 24, False, sit_down), ('SitIdle', 90, True, sit_idle), ('StandUp', 22, False, stand_up),
         ('PickUp', 32, False, pick_up), ('OpenDoor', 26, False, open_door),
         ('PosScan', 20, False, pos_scan), ('HandOver', 28, False, hand_over),
         # Conversation gestures: upper body only, root held still, for the masked Action layer.
         ('Wave', 36, False, wave), ('Nod', 24, False, nod), ('Shake', 26, False, shake))


def make_actions(arm):
    bpy.context.scene.render.fps = FPS
    bones = arm.pose.bones
    legs = (arm.data.bones['LeftUpperLeg'].length, arm.data.bones['LeftLowerLeg'].length)
    up = arm.data.bones['Hips'].matrix_local.to_3x3().inverted() @ Vector(Z)
    ad = arm.animation_data_create()
    for name, n, loop, frame_at in CLIPS:
        assert not loop or np.allclose(flat_pose(frame_at(0.0, legs)), flat_pose(frame_at(1.0, legs))), name
        span = n if loop else n - 1  # a looping clip keys one extra frame that repeats u = 0
        ad.action = bpy.data.actions.new(name)
        ad.action.use_fake_user = True
        for f in range(span + 1):
            pose, dz = frame_at(f / span, legs)
            for pb in bones:
                pb.rotation_quaternion = rotation(pb.bone, pose.get(pb.name, ()))
                pb.keyframe_insert('rotation_quaternion', frame=f + 1, group=pb.name)
            bones['Hips'].location = up * dz
            bones['Hips'].keyframe_insert('location', frame=f + 1, group='Hips')
    ad.action = None
    for pb in bones:
        pb.rotation_quaternion, pb.location = Quaternion(), Vector()
