"""Procedural stylized anime character built from the sweater-girl design sheet (trial).

Headless:
  blender -b --factory-startup --python Tools/Blender/build_sweater_girl.py -- <out_dir>
Writes <out_dir>/sweater_girl.{fbx,glb,blend}, face/hair textures and toon preview sheets.
Character faces -Y (Blender front view), 1 unit = 1 m, ~1.55 m tall.
Unity Humanoid bone names; skin weights come from body regions; Idle/Walk/Run are scripted in-place loops.

ponytail: rigid hair/skirt (no spring bones or cloth), no IK, blendshapes or LODs; boots can poke through the
long back hair while running. Add secondary motion when gameplay needs close-ups.
"""
import math
import os
import sys

import bmesh
import bpy
import numpy as np
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import sweater_girl_rig as rig  # noqa: E402  (Blender runs this file as a script)

OUT = os.path.abspath(sys.argv[sys.argv.index('--') + 1] if '--' in sys.argv else 'Artifacts/Character')
RNG = np.random.default_rng(7)
X, Y, Z = np.eye(3)
HC = np.array([0.0, 0.0, 1.405])     # head ellipsoid centre
HR = np.array([0.095, 0.105, 0.13])  # head radii
FACE_X0, FACE_Z1, FACE_SPAN, FACE_N = -0.1, 1.47, 0.2, 1024  # face texture covers x[-.1,.1] z[1.27,1.47]
ARM_A, ARM_B = np.array([0.14, 0.0, 1.11]), np.array([0.39, 0.0, 0.75])  # sleeve axis (+X side)
# long-hair envelope by height: half width, back depth, centre y
EZ = [0.25, 0.45, 0.62, 0.79, 0.95, 1.12, 1.24, 1.35, 1.45, 1.52]
EW = [0.34, 0.37, 0.38, 0.35, 0.30, 0.225, 0.165, 0.122, 0.102, 0.062]
ED = [0.07, 0.07, 0.07, 0.07, 0.05, 0.04, 0.055, 0.11, 0.112, 0.07]
EY = [0.15, 0.15, 0.15, 0.15, 0.14, 0.13, 0.09, 0.015, 0.0, 0.0]
# leg spline (+X leg): z, x, y, radius; short anime legs under a long sweater (sheet: hem 0.71, knee 0.45)
LEG = [(0.04, .066, .0, .030), (0.10, .064, .0, .028), (0.16, .062, .002, .030), (0.23, .060, .006, .036),
       (0.31, .057, .010, .044), (0.37, .054, .006, .042), (0.44, .052, -.004, .039), (0.51, .058, -.004, .050),
       (0.59, .064, -.002, .058), (0.67, .070, .0, .066), (0.75, .075, .0, .072)]
BAND_Z = (0.585, 0.600)  # stocking top band
BOOT_YAW = math.radians(8)  # boots toe out
ROOT = ARM = None
OUTLINED = []
MAT_INFO = {}
TAGS = {}  # object name -> per-vertex part tag (Sweater: 1 = sleeve)


# ---------------------------------------------------------------- math helpers
def unit(v):
    return v / np.linalg.norm(v)


def unit_rows(a):
    return a / (np.linalg.norm(a, axis=1, keepdims=True) + 1e-12)


def hexrgb(h):
    return np.array([int(h[i:i + 2], 16) / 255 for i in (1, 3, 5)])


def lin(h):
    c = hexrgb(h)
    return tuple(np.where(c > 0.04045, ((c + 0.055) / 1.055) ** 2.4, c / 12.92))


def frame(x, hint=Z):
    x = unit(x)
    y = unit(np.cross(hint, x))
    return np.column_stack([x, y, np.cross(x, y)])


def on_head(az, el, k=1.0):
    """Point on the (inflated) head ellipsoid; az 0 = front (-Y), +90 = character's left (+X)."""
    a, e = math.radians(az), math.radians(el)
    return HC + HR * k * np.array([math.cos(e) * math.sin(a), -math.cos(e) * math.cos(a), math.sin(e)])


def catmull(c, n):
    c = np.asarray(c, float)
    c = np.vstack([2 * c[0] - c[1], c, 2 * c[-1] - c[-2]])
    m = len(c) - 3
    t = np.linspace(0, m, n)
    i = np.minimum(t.astype(int), m - 1)
    u = (t - i)[:, None]
    p0, p1, p2, p3 = c[i], c[i + 1], c[i + 2], c[i + 3]
    return 0.5 * (2 * p1 + (p2 - p0) * u + (2 * p0 - 5 * p1 + 4 * p2 - p3) * u ** 2 + (3 * p1 - p0 - 3 * p2 + p3) * u ** 3)


def leg_at(z):
    c = catmull(LEG, 160)
    return tuple(np.interp(z, c[:, 0], c[:, k]) for k in (1, 2, 3))


# ---------------------------------------------------------------- geometry primitives
def sphere(nu=16, nv=10):
    th = np.linspace(0, np.pi, nv + 1)[1:-1]
    ph = np.linspace(0, 2 * np.pi, nu, endpoint=False)
    t, p = np.meshgrid(th, ph, indexing='ij')
    ring = np.stack([np.sin(t) * np.cos(p), np.sin(t) * np.sin(p), np.cos(t)], -1).reshape(-1, 3)
    v = np.vstack([[0, 0, 1], ring, [0, 0, -1]])
    f = [(0, 1 + j, 1 + (j + 1) % nu) for j in range(nu)]
    for i in range(nv - 2):
        a, b = 1 + i * nu, 1 + (i + 1) * nu
        f += [(a + j, b + j, b + (j + 1) % nu, a + (j + 1) % nu) for j in range(nu)]
    last, bot = 1 + (nv - 2) * nu, len(v) - 1
    f += [(last + j, last + (j + 1) % nu, bot) for j in range(nu)]
    return v, f


def ellipsoid(c, radii, rot=np.eye(3), nu=16, nv=10, shape=None):
    v, f = sphere(nu, nv)
    if shape:
        v = shape(v)
    return (v * radii) @ np.asarray(rot).T + c, f


def box(c, axes, half):
    corners = np.array([[a, b, d] for a in (-1, 1) for b in (-1, 1) for d in (-1, 1)], float)
    v = np.asarray(c) + (corners * half) @ np.asarray(axes)
    return v, [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)]


def loft(rings, n, p=2.0, cap=''):
    """rings: (centre, u, v, rx, ry, offset_fn|None). Superellipse exponent p (2 = ellipse)."""
    t = np.linspace(0, 2 * np.pi, n, endpoint=False)
    e = 2.0 / p
    cu = np.sign(np.cos(t)) * np.abs(np.cos(t)) ** e
    cv = np.sign(np.sin(t)) * np.abs(np.sin(t)) ** e
    v = []
    for c, u, w, rx, ry, off in rings:
        k = 1 + off(t) / ((rx + ry) / 2) if off else 1
        v.append(np.asarray(c, float) + np.outer(cu * rx * k, u) + np.outer(cv * ry * k, w))
    v = np.concatenate(v)
    f = [(i * n + j, i * n + (j + 1) % n, (i + 1) * n + (j + 1) % n, (i + 1) * n + j)
         for i in range(len(rings) - 1) for j in range(n)]
    for which, ring in (('start', 0), ('end', len(rings) - 1)):
        if which in cap:
            ci = len(v)
            v = np.vstack([v, rings[ring][0]])
            f += [(ring * n + j, ring * n + (j + 1) % n, ci) for j in range(n)]
    return v, f


def hring(z, rx, ry, cx=0.0, cy=0.0, off=None):
    return (cx, cy, z), X, Y, rx, ry, off


def rib(amp, count):
    return lambda t: amp * np.cos(count * t)


def pleat(amp, count):
    return lambda t: amp * ((t * count / (2 * np.pi)) % 1 - 0.5)


def strand(ctrl, width, n=24, sides=8, flat=0.28, outward=Z, root_w=0.6, taper_len=0.35, tip_w=0.0):
    """Lens-section hair/ribbon strand along a Catmull-Rom path; width axis is perpendicular to outward."""
    p = catmull(ctrl, n)
    t = unit_rows(np.gradient(p, axis=0))
    o = outward(p) if callable(outward) else np.tile(np.asarray(outward, float), (n, 1))
    b = unit_rows(np.cross(t, o))
    nn = np.cross(b, t)
    s = np.linspace(0, 1, n)
    w = width * (root_w + (1 - root_w) * np.clip(s / 0.2, 0, 1)) \
        * (tip_w + (1 - tip_w) * np.clip((1 - s) / taper_len, 0, 1) ** 0.8)
    phi = np.linspace(0, 2 * np.pi, sides, endpoint=False)
    rings = n - 1 if tip_w == 0 else n
    v = [p[0]]
    for i in range(rings):
        v.extend(p[i] + np.outer(np.cos(phi) * w[i] / 2, b[i]) + np.outer(np.sin(phi) * flat * w[i] / 2, nn[i]))
    v.append(p[-1])
    f = [(0, 1 + j, 1 + (j + 1) % sides) for j in range(sides)]
    for i in range(rings - 1):
        a, c = 1 + i * sides, 1 + (i + 1) * sides
        f += [(a + j, a + (j + 1) % sides, c + (j + 1) % sides, c + j) for j in range(sides)]
    last, tip = 1 + (rings - 1) * sides, len(v) - 1
    f += [(last + j, last + (j + 1) % sides, tip) for j in range(sides)]
    return np.array(v), f


class Parts:
    """Accumulates several primitives into one mesh object."""

    def __init__(self):
        self.v, self.f, self.m, self.t, self.n = [], [], [], [], 0

    def add(self, v, f, mat=0, tag=0):
        self.v.append(np.asarray(v, float))
        self.f += [tuple(i + self.n for i in face) for face in f]
        self.m += [mat] * len(f) if np.isscalar(mat) else list(mat)
        self.t += [tag] * len(v)
        self.n += len(v)

    def build(self, name, mats, uv=None, **kw):
        v = np.concatenate(self.v)
        TAGS[name] = np.array(self.t)
        return make_obj(name, v, self.f, mats, self.m, uv(v) if uv else None, **kw)


def make_obj(name, v, f, mats, mat_idx=None, uv=None, smooth=True, outline=0.003):
    me = bpy.data.meshes.new(name)
    me.from_pydata(np.asarray(v, float).tolist(), [], [tuple(int(i) for i in face) for face in f])
    bm = bmesh.new()
    bm.from_mesh(me)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    bm.to_mesh(me)
    bm.free()
    for m in mats:
        me.materials.append(m)
    if mat_idx is not None:
        me.polygons.foreach_set('material_index', np.asarray(mat_idx, np.int32))
    me.polygons.foreach_set('use_smooth', [smooth] * len(me.polygons))
    if uv is not None:
        vi = np.empty(len(me.loops), np.int32)
        me.loops.foreach_get('vertex_index', vi)
        me.uv_layers.new(name='UVMap').data.foreach_set('uv', np.asarray(uv, np.float32)[vi].ravel())
    me.update()
    ob = bpy.data.objects.new(name, me)
    bpy.context.scene.collection.objects.link(ob)
    ob.parent = ROOT
    OUTLINED.append((ob, outline))
    return ob


# ---------------------------------------------------------------- textures
def paint_face():
    n, px = FACE_N, FACE_SPAN / FACE_N
    img = np.ones((n, n, 4))
    img[..., :3] = hexrgb('#FCE9E0')
    c = (np.arange(n) + 0.5) * px
    gx, gz = np.meshgrid(FACE_X0 + c, FACE_Z1 - c)

    def put(mask, color):
        m = np.clip(mask, 0, 1)[..., None]
        img[..., :3] = img[..., :3] * (1 - m) + np.asarray(color) * m

    def aa(d):
        return np.clip(0.5 - d / px, 0, 1)

    def ell(cx, cz, a, b):
        return (np.hypot((gx - cx) / a, (gz - cz) / b) - 1) * min(a, b)

    def stroke(pts, hw, color, alpha=1.0):
        pts = np.asarray(pts, float)
        hw = np.broadcast_to(np.asarray(hw, float), len(pts)) / px
        col, row = (pts[:, 0] - FACE_X0) / px, (FACE_Z1 - pts[:, 1]) / px
        pad = hw.max() + 2
        c0, c1 = int(max(col.min() - pad, 0)), int(min(col.max() + pad, n))
        r0, r1 = int(max(row.min() - pad, 0)), int(min(row.max() + pad, n))
        yy, xx = np.mgrid[r0:r1, c0:c1] + 0.5
        d = np.full(xx.shape, 1e9)
        for k in range(len(pts) - 1):
            ax, ay, ex, ey = col[k], row[k], col[k + 1] - col[k], row[k + 1] - row[k]
            t = np.clip(((xx - ax) * ex + (yy - ay) * ey) / max(ex * ex + ey * ey, 1e-9), 0, 1)
            d = np.minimum(d, np.hypot(xx - ax - t * ex, yy - ay - t * ey) - (hw[k] + (hw[k + 1] - hw[k]) * t))
        m = (np.clip(0.5 - d, 0, 1) * alpha)[..., None]
        img[r0:r1, c0:c1, :3] = img[r0:r1, c0:c1, :3] * (1 - m) + np.asarray(color) * m

    for s in (-1, 1):  # blush + hatching
        bx, bz = s * 0.052, 1.326
        put(0.55 * np.exp(-((gx - bx) / 0.017) ** 2 - ((gz - bz) / 0.0075) ** 2), hexrgb('#F4A7AE'))
        for k in range(3):
            x0 = bx - 0.008 + k * 0.007
            stroke([(x0 - 0.002, bz - 0.003), (x0 + 0.002, bz + 0.003)], 0.00035, hexrgb('#E77F8E'), 0.6)

    a_eye = 0.026
    lid_top = lambda lx: 0.004 + 0.019 * np.clip(1 - (lx / a_eye) ** 2, 0, 1) ** 0.45 + 0.002 * lx / a_eye
    lid_bot = lambda lx: -0.019 * np.clip(1 - (lx / a_eye) ** 2, 0, 1) ** 0.75
    iris_cols = [hexrgb('#FA9EC6'), hexrgb('#D8407F'), hexrgb('#6B1238')]
    for s in (-1, 1):
        ex, ez = s * 0.046, 1.352
        lx, lz = (gx - ex) * s, gz - ez
        top = lid_top(lx)
        sclera = aa(np.maximum(np.maximum(lz - top, lid_bot(lx) - lz), np.abs(lx) - a_eye))
        put(sclera, (1, 0.985, 0.985))
        put(sclera * np.clip(1 - (top - lz) / 0.007, 0, 1) * 0.55, hexrgb('#CFC3DD'))
        icx, icz = ex + s * 0.0015, ez - 0.002
        iris = aa(ell(icx, icz, 0.0175, 0.022)) * sclera
        t = np.clip((gz - (icz - 0.022)) / 0.044, 0, 1)[..., None]
        grad = np.where(t < 0.5, iris_cols[0] + (iris_cols[1] - iris_cols[0]) * t * 2,
                        iris_cols[1] + (iris_cols[2] - iris_cols[1]) * (t - 0.5) * 2)
        img[..., :3] = img[..., :3] * (1 - iris[..., None]) + grad * iris[..., None]
        ring = np.clip((np.hypot((gx - icx) / 0.0175, (gz - icz) / 0.022) - 0.8) / 0.2, 0, 1)
        put(iris * ring * 0.7, hexrgb('#5A0F33'))
        put(aa(ell(icx, icz - 0.009, 0.012, 0.008)) * iris * 0.45, hexrgb('#FFC2DA'))
        put(aa(ell(icx, icz + 0.002, 0.0065, 0.0095)) * sclera * 0.9, hexrgb('#4A0A26'))
        for ox, oz, r, a in ((-0.0065, 0.009, 0.0045, 0.95), (0.006, -0.010, 0.002, 0.9), (0.001, 0.014, 0.0012, 0.8)):
            put(aa(ell(icx + ox, icz + oz, r, r)) * sclera * a, (1, 1, 1))
        lxs = np.linspace(-a_eye * 0.95, a_eye, 30)
        w = 0.0007 + 0.0018 * ((lxs / a_eye + 1) / 2) ** 1.3
        pts = [(ex + s * x, ez + z + ww * 0.5) for x, z, ww in zip(lxs, lid_top(lxs), w)]
        end = lid_top(a_eye)
        pts += [(ex + s * (a_eye + 0.003), ez + end + 0.0004), (ex + s * (a_eye + 0.0065), ez + end - 0.0022)]
        stroke(pts, list(w) + [0.0016, 0.0006], hexrgb('#3A1C28'))
        lx2 = np.linspace(0.004, 0.022, 10)
        stroke([(ex + s * x, ez + z - 0.0006) for x, z in zip(lx2, lid_bot(lx2))], 0.0004, hexrgb('#8A4A5A'), 0.8)
        lx3 = np.linspace(-0.012, 0.02, 10)
        stroke([(ex + s * x, ez + z + 0.0045) for x, z in zip(lx3, lid_top(lx3))], 0.0003, hexrgb('#C98F92'), 0.6)
        lx4 = np.linspace(-0.02, 0.021, 12)
        stroke([(ex + s * x, ez + 0.041 + 0.004 * (1 - ((x - 0.002) / 0.022) ** 2)) for x in lx4],
               np.linspace(0.0009, 0.0004, 12), hexrgb('#9E94A6'))
    xs = np.linspace(-0.008, 0.008, 9)
    stroke([(x, 1.2995 + 0.0022 * (x / 0.008) ** 2) for x in xs], 0.00065, hexrgb('#A65A60'))
    stroke([(0.0006, 1.3215), (-0.0002, 1.3195)], 0.0005, hexrgb('#E6A79A'), 0.8)
    return img


def paint_hair_gradient():
    v = np.linspace(1, 0, 256)  # row 0 = top of hair
    keys = [(0.0, '#E3A9CB'), (0.1, '#DAA6C8'), (0.3, '#BAAACB'), (0.46, '#C6C3CD'), (1.0, '#CCC9D1')]
    cols = np.array([hexrgb(h) for _, h in keys])
    rgb = np.stack([np.interp(v, [k for k, _ in keys], cols[:, i]) for i in range(3)], -1)
    img = np.ones((256, 4, 4))
    img[..., :3] = rgb[:, None, :]
    return img


def hair_uv(v):
    return np.column_stack([np.full(len(v), 0.5), np.clip((v[:, 2] - 0.25) / 1.35, 0, 1)])


def save_image(name, arr):
    h, w = arr.shape[:2]
    tmp = bpy.data.images.new(name + '_tmp', w, h, alpha=True)
    tmp.pixels.foreach_set(np.ascontiguousarray(arr[::-1], np.float32).ravel())
    path = os.path.join(OUT, name + '.png')
    tmp.filepath_raw, tmp.file_format = path, 'PNG'
    tmp.save()
    bpy.data.images.remove(tmp)
    return bpy.data.images.load(path)


# ---------------------------------------------------------------- materials
def material(name, base, tint, thr=0.22):
    m = bpy.data.materials.new(name)
    try:
        m.use_nodes = True
    except AttributeError:
        pass
    nt = m.node_tree
    nt.nodes.clear()
    out = nt.nodes.new('ShaderNodeOutputMaterial')
    bsdf = nt.nodes.new('ShaderNodeBsdfPrincipled')
    bsdf.inputs['Roughness'].default_value = 0.85
    if isinstance(base, str):
        bsdf.inputs['Base Color'].default_value = (*lin(base), 1)
    else:
        tex = nt.nodes.new('ShaderNodeTexImage')
        tex.image, tex.extension = base, 'EXTEND'
        nt.links.new(tex.outputs['Color'], bsdf.inputs['Base Color'])
    nt.links.new(bsdf.outputs['BSDF'], out.inputs['Surface'])
    MAT_INFO[m.name] = (base, tint, thr)
    return m


def make_materials(face_img, hair_img):
    return {
        'face': material('SG_Face', face_img, '#F6D3CE', 0.06),
        'skin': material('SG_Skin', '#FCE9E0', '#F2CBC6', 0.15),
        'sweater': material('SG_Sweater', '#F4ECDA', '#DCD2D8'),
        'skirt': material('SG_Skirt', '#2C3448', '#9DA0C0'),
        'stripe': material('SG_SkirtStripe', '#47B3B8', '#A0B8C8'),
        'stocking': material('SG_Stocking', '#34343E', '#9A9AB0'),
        'band': material('SG_StockingBand', '#7F8FB2', '#B8B8D0'),
        'boot': material('SG_Boot', '#2A2A30', '#9090A8'),
        'sole': material('SG_Sole', '#1B1B1F', '#A0A0B0'),
        'lace': material('SG_Lace', '#5C5C68', '#A0A0B0'),
        'hair': material('SG_Hair', hair_img, '#C8BCD8'),
        'black': material('SG_BlackBand', '#24232A', '#A0A0B0'),
        'ribbon': material('SG_Ribbon', '#A9BEE3', '#B9BCD9'),
        'clip': material('SG_Clip', '#B5C8E8', '#A8B4D0'),
    }


# ---------------------------------------------------------------- body parts
def build_head(mats):
    v, f = sphere(48, 32)
    t = v[:, 2]
    s = np.sqrt(np.clip(1 - t ** 2, 1e-6, None))
    a = np.clip((-t - 0.3) / 0.7, 0, 1)
    k = np.where(t < -0.3, np.minimum(1, (0.954 - 0.82 * a ** 1.3) / s), 1.0)  # anime V jaw
    v[:, 0] *= k
    v[:, 1] *= np.where(v[:, 1] < 0, 0.92, k)
    v = v * HR + HC
    u = (v[:, 0] - FACE_X0) / FACE_SPAN
    w = np.where(v[:, 1] > 0, 1.0, (v[:, 2] - (FACE_Z1 - FACE_SPAN)) / FACE_SPAN)  # back of head -> plain skin row
    make_obj('Face', v, f, [mats['face']], uv=np.column_stack([u, w]), outline=0.0025)


def arm_frame():
    d = unit(ARM_B - ARM_A)
    return d, np.array([-d[2], 0, d[0]])


def sleeve():
    d, u = arm_frame()
    length = np.linalg.norm(ARM_B - ARM_A)
    cuff = rib(0.0018, 36)
    prof = [(0.0, .060, None), (0.12, .066, None), (0.35, .064, None), (0.6, .070, None), (0.78, .074, None),
            (0.85, .068, None), (0.875, .048, cuff), (0.9, .047, cuff), (1.0, .044, cuff), (0.985, .034, cuff)]
    return loft([(ARM_A + d * length * s, u, Y, r, r * 1.05, off) for s, r, off in prof], 120, cap='start end')


def hand():
    d, _ = arm_frame()
    h = unit(d + np.array([0.3, 0, 0]))
    a = np.array([-h[2], 0, h[0]])
    pc = ARM_B + d * 0.012 + h * 0.03
    parts = [ellipsoid(pc, (0.034, 0.028, 0.011), frame(h, Y), 16, 10)]
    for off, ang, ln, r in ((-0.016, -10, 0.050, 0.0062), (-0.0055, -3, 0.056, 0.0062),
                            (0.0055, 4, 0.053, 0.006), (0.016, 11, 0.043, 0.0055)):
        g = math.radians(ang)
        fd = h * math.cos(g) + a * math.sin(g)
        parts.append(ellipsoid(pc + h * 0.024 + a * off + fd * ln / 2, (ln / 2, r, r), frame(fd, Y), 10, 8))
    td = unit(h * 0.55 - a * 0.8 - Y * 0.2)
    parts.append(ellipsoid(pc - a * 0.022 + td * 0.02, (0.021, 0.0072, 0.0072), frame(td, Y), 10, 8))
    return parts


def leg():
    c = catmull(LEG, 160)
    zq = np.unique(np.concatenate([np.linspace(0.04, 0.75, 40), BAND_Z]))
    x, y, r = (np.interp(zq, c[:, 0], c[:, k]) for k in (1, 2, 3))
    v, f = loft([hring(z, rr, rr * 1.04, xx, yy) for z, rr, xx, yy in zip(zq, r, x, y)], 32)
    zc = (zq[:-1] + zq[1:]) / 2
    return v, f, np.repeat(np.where(zc > BAND_Z[1], 0, np.where(zc > BAND_Z[0], 2, 1)), 32)


def build_body(mats):
    p = Parts()
    p.add(*loft([hring(z, .034, .034, cy=.012) for z in (1.17, 1.25, 1.33)], 24))
    for s in (1, -1):
        for v, f in hand():
            p.add(v * (s, 1, 1), f)
        v, f, m = leg()
        p.add(v * (s, 1, 1), f, m)
    p.build('Body', [mats['skin'], mats['stocking'], mats['band']], outline=0.0025)


def build_sweater(mats):
    p = Parts()
    hem = rib(0.0022, 56)
    rows = [(0.71, .168, .122, 0, hem), (0.70, .182, .136, 0, hem), (0.725, .184, .137, 0, hem),  # hem clears skirt pleats
            (0.75, .184, .135, 0, hem), (0.77, .187, .131, 0, None), (0.9, .187, .125, 0, None),
            (1.0, .188, .124, 0, None), (1.06, .19, .125, -.004, None), (1.10, .19, .12, 0, None),
            (1.13, .182, .113, 0, None), (1.155, .165, .104, 0, None), (1.175, .14, .094, 0, None),
            (1.192, .112, .082, 0, None), (1.205, .09, .072, 0, None), (1.215, .072, .063, 0, None)]
    p.add(*loft([hring(z, rx, ry, cy=cy, off=o) for z, rx, ry, cy, o in rows], 168, p=2.6))
    col = rib(0.0022, 44)  # rolled turtleneck: bulging fold, rounded lip
    p.add(*loft([hring(z, r, r * 0.95, off=col) for z, r in
                 ((1.17, .088), (1.19, .097), (1.215, .101), (1.24, .097), (1.256, .087), (1.262, .074),
                  (1.256, .063), (1.24, .056))], 132))
    for s in (1, -1):
        v, f = sleeve()
        p.add(v * (s, 1, 1), f, tag=1)
    p.build('Sweater', [mats['sweater']], outline=0.003)


def build_skirt(mats):
    rows = [(0.775, .150, .110, 0), (0.74, .158, .116, .004), (0.70, .168, .124, .008), (0.665, .176, .130, .011),
            (0.652, .179, .132, .012), (0.645, .180, .133, .012), (0.633, .182, .135, .012), (0.637, .171, .125, .010)]
    v, f = loft([hring(z, rx, ry, off=pleat(a, 20) if a else None) for z, rx, ry, a in rows], 120)
    ring_mat = [0, 0, 0, 0, 1, 0, 0]  # 0.652-0.645 band = teal stripe
    make_obj('Skirt', v, f, [mats['skirt'], mats['stripe']], np.repeat(ring_mat, 120), smooth=False, outline=0.0025)


def boot():
    shaft = [hring(z, rx, ry, 0, cy) for z, rx, ry, cy in
             ((0.05, .045, .05, .015), (0.10, .044, .048, .012), (0.15, .046, .048, .008),
              (0.185, .049, .05, .006), (0.193, .047, .048, .006), (0.188, .036, .038, .006))]
    foot = [((0, y, cz), X, Z, rx, rz, None) for y, cz, rx, rz in
            ((0.055, .085, .030, .05), (0.035, .085, .042, .05), (0.0, .08, .046, .045), (-0.04, .065, .047, .04),
             (-0.08, .05, .046, .03), (-0.115, .042, .042, .024), (-0.14, .04, .034, .02), (-0.155, .04, .02, .012))]
    parts = [(*loft(shaft, 32), 0), (*loft(foot, 32, p=2.3, cap='start end'), 0),
             (*box((0, -0.069, 0.011), np.eye(3), (0.049, 0.09, 0.011)), 1),
             (*box((0, 0.03, 0.02), np.eye(3), (0.037, 0.031, 0.02)), 1)]
    for z in (0.115, 0.135, 0.155, 0.175):  # crossed laces on the shaft front
        y = np.interp(z, (0.10, 0.15, 0.185), (-0.036, -0.040, -0.044))
        for g in (0.45, -0.45):
            axes = [(math.cos(g), 0, math.sin(g)), (0, 1, 0), (-math.sin(g), 0, math.cos(g))]
            parts.append((*box((0, y, z), axes, (0.013, 0.0016, 0.0022)), 2))
    return parts


def build_boots(mats):
    p = Parts()
    yaw = BOOT_YAW
    rot = np.array([[math.cos(yaw), -math.sin(yaw), 0], [math.sin(yaw), math.cos(yaw), 0], [0, 0, 1]])
    for s in (1, -1):
        for v, f, m in boot():
            p.add((v @ rot.T + (0.066, 0, 0)) * (s, 1, 1), f, m)
    p.build('Boots', [mats['boot'], mats['sole'], mats['lace']], outline=0.0025)


# ---------------------------------------------------------------- hair
def radial(q):
    return unit_rows(q - HC)


def horizontal(q):
    return unit_rows(q * (1, 1, 0))


def hang_out(q):
    o = q.copy()
    o[:, 1] -= np.interp(q[:, 2], EZ, EY)
    o[:, 2] = 0
    return unit_rows(o)


def back_out(q):
    k = np.clip((q[:, 2] - 1.30) / 0.1, 0, 1)[:, None]  # radial over the skull, envelope-outward below
    return unit_rows(k * radial(q) + (1 - k) * hang_out(q))


def back_ctrl(theta, tip_z, off, flare):
    """Root at the crown, hug the inflated skull, then fall along the long-hair envelope."""
    shell = [on_head(180 - theta, el, k + off * 3) for el, k in ((78, 1.10), (55, 1.13), (30, 1.16), (8, 1.18))]
    zs = np.linspace(1.30, tip_z, 9)
    th = np.radians(theta + 2.5 * np.sin(zs * 7 + theta))
    w, d, y0 = np.interp(zs, EZ, EW) + off, np.interp(zs, EZ, ED) + off, np.interp(zs, EZ, EY)
    s = (1.30 - zs) / (1.30 - tip_z)
    x = w * np.sin(th) * (1 + flare * np.clip((s - 0.6) / 0.4, 0, 1) ** 2)
    return np.vstack([shell, np.column_stack([x, y0 + d * np.cos(th), zs])])


def braid(p):
    ctrl = np.array([on_head(70, 28, 1.09), on_head(80, 0, 1.12), on_head(80, -32, 1.15),
                     (0.102, -0.075, 1.25), (0.088, -0.118, 1.17), (0.068, -0.142, 1.10)])
    p.add(*strand(ctrl[:4], 0.03, n=18, outward=radial, root_w=0.7, taper_len=0.3, tip_w=0.55))
    path = catmull(ctrl[2:], 60)
    cum = np.concatenate([[0], np.cumsum(np.linalg.norm(np.diff(path, axis=0), axis=1))])
    tan = unit_rows(np.gradient(path, axis=0))
    lobes = 9
    for i, d in enumerate(np.linspace(0, cum[-1], lobes)):
        c = np.array([np.interp(d, cum, path[:, k]) for k in range(3)])
        t = unit(np.array([np.interp(d, cum, tan[:, k]) for k in range(3)]))
        b = unit(np.cross(t, unit(c * (1, 1, 0))))
        side, sc = (1 if i % 2 else -1), 1 - 0.18 * i / lobes
        g = 0.5 * side
        ax, bx = t * math.cos(g) + b * math.sin(g), -t * math.sin(g) + b * math.cos(g)
        p.add(*ellipsoid(c + b * side * 0.0045 * sc, (0.02 * sc, 0.012 * sc, 0.0095 * sc),
                         np.column_stack([ax, bx, np.cross(ax, bx)]), 12, 8))
    end, t = path[-1], tan[-1]
    o = unit(end * (1, 1, 0))
    b = unit(np.cross(t, o))
    p.add(*ellipsoid(end, (0.006, 0.0085, 0.0075), np.column_stack([t, b, np.cross(t, b)]), 12, 8), mat=1)
    for k in (-1, 0, 1):
        p.add(*strand([end, end + t * 0.025 + b * k * 0.004, end + t * 0.055 + b * k * 0.011], 0.016, n=12,
                      outward=o, root_w=0.8, taper_len=0.6))


def build_hair(mats):
    p = Parts()
    v, f = sphere(48, 28)
    keep = []
    for face in f:
        c = unit(v[list(face)].mean(0))
        if not ((c[1] < -0.3 and c[2] < 0.45) or (c[2] < -0.2 and c[1] < 0.3) or c[2] < -0.8):
            keep.append(face)
    used = sorted({i for face in keep for i in face})
    remap = {o: i for i, o in enumerate(used)}
    p.add(v[used] * HR * 1.08 + HC, [tuple(remap[i] for i in face) for face in keep])

    step = 216 / 30
    layers = ((np.linspace(-108, 108, 31), 0.006, 0.062, 0.0), (np.linspace(-108 + step / 2, 108 - step / 2, 30), -0.002, 0.058, 0.0),
              (np.linspace(-100, 100, 15), -0.01, 0.1, 0.06))  # last = wide filler hiding gaps
    for layer, (thetas, off, width, short) in enumerate(layers):
        for th in thetas:
            e = abs(th) / 108
            tip = 0.27 + 0.12 * e ** 2 + RNG.uniform(-0.02, 0.06) + 0.03 * layer + short
            p.add(*strand(back_ctrl(th, tip, off, 0.14 if e > 0.8 else 0.03), width, n=36, outward=back_out,
                          root_w=0.5, taper_len=0.3))

    bangs = [(0, -20, .032), (11, -9, .034), (-11, -9, .034), (22, -17, .034), (-22, -17, .034), (33, -6, .032),
             (-33, -6, .032), (44, -15, .032), (-44, -15, .032), (55, -30, .03), (-55, -30, .03), (64, -48, .028),
             (-64, -48, .028), (6, -3, .02), (-6, -3, .02), (28, -2, .02), (-28, -2, .02), (50, -6, .02), (-50, -6, .02)]
    for az, el, w in bangs:
        els = np.linspace(82, el, 7)
        azs = az * np.array([0.25, 0.5, 0.75, 0.9, 1, 1, 1]) - np.sign(az) * np.array([0, 0, 0, 0, 0, 1.5, 4])
        ks = np.linspace(1.09, 1.11 + 0.03 * (abs(az) > 58), 7)
        p.add(*strand([on_head(a, e, k) for a, e, k in zip(azs, els, ks)], w, outward=radial, root_w=0.5, taper_len=0.45))

    side_locks = [[on_head(-70, 30, 1.09), on_head(-80, 0, 1.11), on_head(-82, -30, 1.13),
                   (-0.11, -0.06, 1.25), (-0.115, -0.095, 1.18), (-0.112, -0.112, 1.14)],
                  [on_head(-84, 28, 1.09), on_head(-90, -5, 1.12), (-0.118, -0.03, 1.30),
                   (-0.13, -0.07, 1.22), (-0.135, -0.1, 1.17)]]
    for ctrl, w in zip(side_locks, (0.03, 0.032)):
        p.add(*strand(ctrl, w, n=28, outward=horizontal, root_w=0.6))
    braid(p)

    q = unit(np.array([0.45, -0.9, 0]))  # ahoge curls forward / to her left
    root = np.array([0, -0.005, 1.538])
    pts = [root + Z * a + q * b for a, b in ((0, 0), (0.035, 0.004), (0.065, 0.02), (0.075, 0.045), (0.06, 0.062), (0.045, 0.058))]
    p.add(*strand(pts, 0.014, flat=0.45, outward=unit(np.cross(Z, q)), root_w=1.0, taper_len=0.5))
    p.build('Hair', [mats['hair'], mats['black']], uv=hair_uv, outline=0.0018)


# ---------------------------------------------------------------- accessories
def bow(p, c, e1, e2, e3, size=1.0, tail=1.0, mat=0):
    length, thick, height = 0.021 * size, 0.0085 * size, 0.019 * size
    for side in (-1, 1):
        g = math.radians(22)
        ax = e1 * side * math.cos(g) + e3 * math.sin(g)
        up = e3 * math.cos(g) - e1 * side * math.sin(g)
        shape = lambda v: v * np.column_stack([np.ones(len(v)), np.ones(len(v)), 0.55 + 0.45 * (v[:, 0] + 1) / 2])
        p.add(*ellipsoid(c + ax * length * 0.9 + e3 * 0.002 * size, (length, thick, height),
                         np.column_stack([ax, e2, up]), 16, 10, shape), mat)
        p.add(*strand([c, c + e1 * side * 0.007 * size - e3 * 0.016 * size * tail,
                       c + e1 * side * 0.013 * size - e3 * 0.04 * size * tail],
                      0.012 * size, n=12, flat=0.25, outward=e2, root_w=0.8, tip_w=0.85), mat)
    p.add(*ellipsoid(c + e2 * 0.002 * size, (0.0075 * size, 0.006 * size, 0.009 * size),
                     np.column_stack([e1, e2, e3]), 12, 8), mat)


def build_accessories(mats):
    p = Parts()
    nrm = unit(np.array([0, 0.861, 0.509]))
    up = np.cross(X, nrm)
    pts = []
    for phi in np.radians(np.linspace(-95, 95, 21)):
        d = math.cos(phi) * up + math.sin(phi) * X
        pts.append(HC + d * 1.2 / math.sqrt(((d / HR) ** 2).sum()))
    p.add(*strand(pts, 0.022, n=40, flat=0.32, outward=radial, root_w=1.0, tip_w=1.0), 0)  # headband

    for s in (1, -1):  # hair bows
        e2 = unit(np.array([s, -0.3, 0.08]))
        e3 = unit(Z - Z.dot(e2) * e2)
        bow(p, np.array([s * 0.122, 0.035, 1.465]), np.cross(e2, e3), e2, e3, size=1.8, mat=1)

    p0 = on_head(52, 20, 1.15)  # two bar clips on her left bangs
    e2 = unit(p0 - HC)
    e1 = on_head(53, 20, 1.15) - on_head(51, 20, 1.15)
    e1 = unit(e1 - e1.dot(e2) * e2)
    e3 = np.cross(e2, e1)
    e3 = e3 if e3[2] > 0 else -e3
    g = math.radians(35)
    dr = e1 * math.cos(g) + e3 * math.sin(g)
    perp = unit(np.cross(e2, dr))
    for off in (-0.0045, 0.0045):
        c = p0 + perp * off
        p.add(*box(c, [dr, perp, e2], (0.024, 0.003, 0.0018)), 2)
        p.add(*box(c + dr * 0.021, [dr, perp, e2], (0.005, 0.005, 0.0024)), 2)

    zm = sum(BAND_Z) / 2
    x, y, r = leg_at(zm)  # ribbon around her left stocking top + bow
    p.add(*loft([hring(z, r + dr_, (r + dr_) * 1.04, x, y)
                 for z, dr_ in ((BAND_Z[1] + .001, .0025), (zm, .0038), (BAND_Z[0] - .001, .0025))], 32), 1)
    e2 = unit(np.array([0.5, -0.87, 0]))
    bow(p, np.array([x, y, zm]) + e2 * (r + 0.008), np.cross(e2, Z), e2, Z, size=0.55, tail=2.4, mat=1)
    p.build('Accessories', [mats['black'], mats['ribbon'], mats['clip']], outline=0.0012)


# ---------------------------------------------------------------- export / preview
def check_meshes():
    tris = 0
    for ob in (o for o in ROOT.children_recursive if o.type == 'MESH'):
        co = np.empty(len(ob.data.vertices) * 3)
        ob.data.vertices.foreach_get('co', co)
        assert np.isfinite(co).all(), ob.name
        ob.data.calc_loop_triangles()
        tris += len(ob.data.loop_triangles)
        print(f'{ob.name:12s} verts={len(ob.data.vertices):6d} tris={len(ob.data.loop_triangles):6d}')
    assert tris < 200_000, tris
    print('total tris', tris)


def export():
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, 'sweater_girl.fbx'), object_types={'EMPTY', 'MESH', 'ARMATURE'},
                             apply_scale_options='FBX_SCALE_UNITS', mesh_smooth_type='FACE', path_mode='COPY',
                             embed_textures=True, add_leaf_bones=False, bake_anim=True, bake_anim_use_all_actions=True,
                             bake_anim_use_nla_strips=False, bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0)
    bpy.ops.export_scene.gltf(filepath=os.path.join(OUT, 'sweater_girl.glb'), export_format='GLB')


def sock(node, ident, out=False):
    return next(s for s in (node.outputs if out else node.inputs) if s.identifier == ident)


def toonify():
    """Preview-only cel shading: 2-tone diffuse ramp; exported files keep plain Principled colours."""
    for name, (base, tint, thr) in MAT_INFO.items():
        nt = bpy.data.materials[name].node_tree
        nodes, links = nt.nodes, nt.links
        out = next(n for n in nodes if n.type == 'OUTPUT_MATERIAL')
        tex = next((n for n in nodes if n.type == 'TEX_IMAGE'), None)
        s2r = nodes.new('ShaderNodeShaderToRGB')
        links.new(nodes.new('ShaderNodeBsdfDiffuse').outputs[0], s2r.inputs[0])
        ramp = nodes.new('ShaderNodeValToRGB')
        cr = ramp.color_ramp
        cr.interpolation = 'CONSTANT'
        cr.elements[0].color, cr.elements[1].position, cr.elements[1].color = (0, 0, 0, 1), thr, (1, 1, 1, 1)
        links.new(s2r.outputs[0], ramp.inputs[0])
        shade, mix = nodes.new('ShaderNodeMix'), nodes.new('ShaderNodeMix')
        shade.data_type, shade.blend_type, mix.data_type = 'RGBA', 'MULTIPLY', 'RGBA'
        sock(shade, 'Factor_Float').default_value = 1.0
        sock(shade, 'B_Color').default_value = (*lin(tint), 1)
        for s in (sock(shade, 'A_Color'), sock(mix, 'B_Color')):
            if tex:
                links.new(tex.outputs['Color'], s)
            else:
                s.default_value = (*lin(base), 1)
        links.new(ramp.outputs['Color'], sock(mix, 'Factor_Float'))
        links.new(sock(shade, 'Result_Color', True), sock(mix, 'A_Color'))
        emi = nodes.new('ShaderNodeEmission')
        links.new(sock(mix, 'Result_Color', True), emi.inputs['Color'])
        links.new(emi.outputs['Emission'], out.inputs['Surface'])


def add_outlines():
    m = bpy.data.materials.new('SG_Outline')
    try:
        m.use_nodes = True
    except AttributeError:
        pass
    nt = m.node_tree
    nt.nodes.clear()
    emi = nt.nodes.new('ShaderNodeEmission')
    emi.inputs['Color'].default_value = (*lin('#3B3242'), 1)
    nt.links.new(emi.outputs[0], nt.nodes.new('ShaderNodeOutputMaterial').inputs['Surface'])
    m.use_backface_culling = True
    for ob, th in OUTLINED:
        k = len(ob.data.materials)
        for _ in range(k):
            ob.data.materials.append(m)
        mod = ob.modifiers.new('Outline', 'SOLIDIFY')
        mod.thickness, mod.offset, mod.use_flip_normals, mod.material_offset = th, 1.0, True, k  # even offset spikes at bent joints


def setup_render():
    sc = bpy.context.scene
    sc.render.engine = 'BLENDER_EEVEE'
    sc.eevee.taa_render_samples = 16
    sc.view_settings.view_transform = 'Standard'
    sc.render.image_settings.file_format = 'PNG'
    world = bpy.data.worlds.new('Studio')
    sc.world = world
    try:
        world.use_nodes = True
    except AttributeError:
        pass
    nodes, links = world.node_tree.nodes, world.node_tree.links
    nodes.clear()
    dark, bg, mix = nodes.new('ShaderNodeBackground'), nodes.new('ShaderNodeBackground'), nodes.new('ShaderNodeMixShader')
    dark.inputs['Strength'].default_value = 0.0
    bg.inputs['Color'].default_value = (*lin('#E6E4EA'), 1)
    links.new(nodes.new('ShaderNodeLightPath').outputs['Is Camera Ray'], mix.inputs[0])
    links.new(dark.outputs[0], mix.inputs[1])
    links.new(bg.outputs[0], mix.inputs[2])
    links.new(mix.outputs[0], nodes.new('ShaderNodeOutputWorld').inputs['Surface'])
    sun = bpy.data.lights.new('Sun', 'SUN')
    sun.energy, sun.use_shadow = 3.0, False
    sun_ob = bpy.data.objects.new('Sun', sun)
    sun_ob.rotation_euler = Vector((0.45, 0.7, -0.55)).normalized().to_track_quat('-Z', 'Y').to_euler()
    cam = bpy.data.cameras.new('Cam')
    cam.type, cam.clip_end = 'ORTHO', 20
    cam_ob = bpy.data.objects.new('Cam', cam)
    cam_ob.rotation_euler = (math.radians(90), 0, 0)
    for ob in (sun_ob, cam_ob):
        sc.collection.objects.link(ob)
    sc.camera = cam_ob
    return sc, cam_ob


def render_sheet(sc, cam_ob, name, views, ortho, cam_z, res):
    cam_ob.data.ortho_scale = ortho
    cam_ob.location = (0, -6, cam_z)
    sc.render.resolution_x, sc.render.resolution_y = res
    tiles = []
    for view, rot, *pose in views:  # optional pose: (action, frame)
        if pose:
            ARM.animation_data.action = bpy.data.actions[pose[0][0]]
            bpy.context.scene.frame_set(pose[0][1])
        ROOT.rotation_euler = (0, 0, math.radians(rot))
        path = os.path.join(OUT, f'render_{view}.png')
        sc.render.filepath = path
        bpy.ops.render.render(write_still=True)
        img = bpy.data.images.load(path)
        px = np.empty(res[0] * res[1] * 4, np.float32)
        img.pixels.foreach_get(px)
        tiles.append(px.reshape(res[1], res[0], 4))
    ROOT.rotation_euler = (0, 0, 0)
    sheet = np.concatenate(tiles, axis=1)
    out = bpy.data.images.new(name, sheet.shape[1], sheet.shape[0], alpha=True)
    out.pixels.foreach_set(sheet.ravel())
    out.filepath_raw, out.file_format = os.path.join(OUT, name + '.png'), 'PNG'
    out.save()


def main():
    global ROOT, ARM
    os.makedirs(OUT, exist_ok=True)
    for ob in list(bpy.data.objects):
        bpy.data.objects.remove(ob)
    ROOT = bpy.data.objects.new('SweaterGirl', None)
    bpy.context.scene.collection.objects.link(ROOT)
    mats = make_materials(save_image('sg_face', paint_face()), save_image('sg_hair_gradient', paint_hair_gradient()))
    for build in (build_head, build_body, build_sweater, build_skirt, build_boots, build_hair, build_accessories):
        build(mats)
    ARM = rig.build(ROOT, TAGS, ARM_A, ARM_B, BOOT_YAW)
    rig.make_actions(ARM)
    check_meshes()
    export()
    toonify()
    add_outlines()
    sc, cam_ob = setup_render()
    render_sheet(sc, cam_ob, 'sheet_body', [('front', 0), ('left', -90), ('right', 90), ('back', 180)], 1.78, 0.82, (700, 1100))
    render_sheet(sc, cam_ob, 'sheet_head', [('head_front', 0), ('head_left', -90), ('head_34', -35), ('head_back', 180)], 0.46, 1.40, (700, 700))
    render_sheet(sc, cam_ob, 'sheet_motion', [(f'{a}_{f}', -90, (a, f)) for a, f in
                                              (('Walk', 1), ('Walk', 9), ('Run', 1), ('Run', 6))] +
                 [('run_front', 0, ('Run', 6))], 1.78, 0.82, (700, 1100))
    ARM.animation_data.action = None
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, 'sweater_girl.blend'))


main()
