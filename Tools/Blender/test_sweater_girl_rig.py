"""Pose-math checks for sweater_girl_rig.CLIPS that run without Blender.

The rig module imports bpy at module scope but every pose generator is pure math, so the module is
loaded here against stub bpy/mathutils. Run: pytest Tools/Blender/test_sweater_girl_rig.py

Worth having because a mistyped bone name is silent in Blender: pose.get(pb.name, ()) simply leaves
that bone at rest, and the clip exports looking almost right.
"""
from __future__ import annotations

import importlib.util
import math
import sys
import types
from pathlib import Path

import numpy as np
import pytest

HERE = Path(__file__).resolve().parent
# Leg lengths of the real rig, read off bone_table: Hips(0.75) -> knee(0.45) -> ankle(0.09).
LEGS = (0.75 - 0.45, 0.45 - 0.09)
GESTURES = ('Wave', 'Nod', 'Shake')


def _load(name: str):
    """Import a Tools/Blender module with bpy and mathutils stubbed out."""
    for stub in ('bpy', 'mathutils'):
        sys.modules.setdefault(stub, types.ModuleType(stub))
    sys.modules['mathutils'].Quaternion = sys.modules['mathutils'].Vector = object
    spec = importlib.util.spec_from_file_location(name, HERE / f'{name}.py')
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


rig = _load('sweater_girl_rig')


@pytest.fixture(scope='module')
def bone_names() -> set[str]:
    arm_a, arm_b = np.array([0.14, 0.0, 1.11]), np.array([0.39, 0.0, 0.75])  # build_sweater_girl.ARM_A/ARM_B
    return {name for name, _, _, _ in rig.bone_table(arm_a, arm_b, math.radians(8))}


def frames(frame_at, n: int, loop: bool):
    span = n if loop else n - 1
    return [frame_at(f / span, LEGS) for f in range(span + 1)]


def same_pose(a, b) -> bool:
    """Do two authored frames put the bones in the same place?

    Looser than flat_pose() on purpose, in the two ways blend() is: key order carries no meaning (blend()
    builds its dict from an unordered set), and an unnamed axis means 0 degrees, so a bone blend() carried
    through at 0 matches a pose that never mentions it.
    """
    (pa, za), (pb, zb) = a, b
    na, nb = ({name: dict(rots) for name, rots in pose.items()} for pose in (pa, pb))
    return za == pytest.approx(zb) and all(
        na.get(bone, {}).get(axis, 0.0) == pytest.approx(nb.get(bone, {}).get(axis, 0.0))
        for bone in set(na) | set(nb)
        for axis in set(na.get(bone, {})) | set(nb.get(bone, {}))
    )


@pytest.mark.parametrize('name,n,loop,frame_at', rig.CLIPS, ids=[c[0] for c in rig.CLIPS])
def test_poses_only_name_real_bones(name, n, loop, frame_at, bone_names):
    posed = {bone for pose, _ in frames(frame_at, n, loop) for bone in pose}
    assert posed <= bone_names, f'{name} poses bones the rig does not have: {sorted(posed - bone_names)}'


@pytest.mark.parametrize('name,n,loop,frame_at', rig.CLIPS, ids=[c[0] for c in rig.CLIPS])
def test_hips_never_rise_above_rest(name, n, loop, frame_at):
    """stance() only ever shortens the legs' vertical reach, so dz is a drop; a positive value floats her."""
    assert max(dz for _, dz in frames(frame_at, n, loop)) <= 1e-9, name


@pytest.mark.parametrize('name,n,loop,frame_at', [c for c in rig.CLIPS if c[2]], ids=[c[0] for c in rig.CLIPS if c[2]])
def test_looping_clips_close(name, n, loop, frame_at):
    """Same assertion make_actions() makes, checked before a 2-minute Blender run rather than during it."""
    assert np.allclose(rig.flat_pose(frame_at(0.0, LEGS)), rig.flat_pose(frame_at(1.0, LEGS))), name


@pytest.mark.parametrize('name', GESTURES)
def test_gestures_hold_the_root_still(name):
    """A masked Action-layer clip drives the upper body only: any Hips motion would fight the locomotion layer."""
    n, loop, frame_at = next((c[1], c[2], c[3]) for c in rig.CLIPS if c[0] == name)
    assert all(dz == 0.0 for _, dz in frames(frame_at, n, loop)), name
    assert not {b for pose, _ in frames(frame_at, n, loop) for b in pose} & {'Hips', 'LeftUpperLeg', 'RightUpperLeg'}


@pytest.mark.parametrize('name', GESTURES)
def test_gestures_start_and_end_neutral(name):
    """Non-looping, but they fade in and out, so the layer weight can ramp without a pop at either end."""
    frame_at = next(c[3] for c in rig.CLIPS if c[0] == name)
    assert same_pose(frame_at(0.0, LEGS), frame_at(1.0, LEGS)), name


@pytest.mark.parametrize('name', ('SitDown', 'PickUp', 'OpenDoor', 'PosScan', 'HandOver'))
def test_full_body_one_shots_open_on_idle(name):
    """So the Animator can blend into them from anywhere in the locomotion layer. They do NOT all end on
    idle: each ends where the action leaves her, and the way out is a transition blend, not keyframes."""
    frame_at = next(c[3] for c in rig.CLIPS if c[0] == name)
    assert same_pose(frame_at(0.0, LEGS), rig.idle(0.0, LEGS)), name


def test_sit_chain_meets_at_the_seated_pose():
    """SitDown -> SitIdle -> StandUp must join without a jump; the hip drop is what the chair has to match."""
    seated = rig.seated(LEGS)
    assert same_pose(rig.sit_down(1.0, LEGS), seated)
    assert same_pose(rig.stand_up(0.0, LEGS), seated)
    assert same_pose(rig.stand_up(1.0, LEGS), rig.idle(0.0, LEGS))


def test_seated_sole_stays_flat_on_the_floor():
    pose, dz = rig.seated(LEGS)
    assert pose['LeftFoot'] == [(rig.X, pytest.approx(0.0, abs=1e-9))], pose['LeftFoot']
    assert dz < -0.2, f'seated barely drops ({dz:.3f} m): she would be standing, not sitting'


@pytest.mark.parametrize('name', ('IdleLookAround', 'IdleStretch'))
def test_idle_variations_return_to_neutral_without_moving_feet(name):
    frame_at = next(c[3] for c in rig.CLIPS if c[0] == name)
    assert same_pose(frame_at(0, LEGS), rig.idle(0, LEGS))
    assert same_pose(frame_at(1, LEGS), rig.idle(0, LEGS))
    for u in np.linspace(0, 1, 33):
        pose, dz = frame_at(float(u), LEGS)
        assert dz == 0
        assert not {b for b in pose if 'Leg' in b or 'Foot' in b or 'Toes' in b}


def test_look_around_changes_heading_while_the_head_stays_upright():
    pose, _ = rig.idle_look_around(.25, LEGS)
    forward, up = np.array([0., -1., 0.]), np.array([0., 0., 1.])
    for axis, degrees in pose['Head']:
        axis = np.asarray(axis, float)
        angle = math.radians(degrees)
        def rotate(v):
            return v * math.cos(angle) + np.cross(axis, v) * math.sin(angle) + axis * np.dot(axis, v) * (1 - math.cos(angle))
        forward, up = rotate(forward), rotate(up)
    assert abs(forward[0]) > .3, 'Looking sideways must change the facing direction.'
    assert up[2] > .98, 'Looking around must not roll the head onto a shoulder.'
