"""Build SORA MART from licensed source models and original architecture.

Run Blender 5.2: blender -b --factory-startup --python-exit-code 1 --python Tools/Blender/build_sora_mart.py
Coordinates in helpers are Unity world metres; glTFast mirrors glTF X on import.
"""
import hashlib
import json
import math
from collections import Counter
from pathlib import Path

import bpy
from mathutils import Matrix, Vector

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "Assets/Resources/SoraStore/SoraMart.glb"
SOURCE = ROOT / "ArtSource/ThirdParty/CC0/IzakayaAlley/izakaya_starter_scene.glb"
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)


def point(p):
    return (-p[0], -p[2], p[1])


def material(name, rgb, roughness=.45, emission=0, alpha=1):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*rgb, alpha)
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Alpha"].default_value = alpha
    bsdf.inputs["Emission Color"].default_value = (*rgb, 1)
    bsdf.inputs["Emission Strength"].default_value = emission
    mat.diffuse_color = (*rgb, alpha)
    if alpha < 1:
        mat.surface_render_method = "DITHERED"
    return mat


ivory = material("SORA | warm porcelain", (.77, .78, .70))
frame = material("SORA | graphite aluminium", (.075, .105, .11), .25)
oak = material("SORA | honey oak", (.42, .235, .105), .5)
teal = material("SORA | jade brand", (.035, .33, .29), .3)
orange = material("SORA | tangerine accent", (.95, .28, .055), .4)
white = material("SORA | paper white", (.92, .94, .88))
ink = material("SORA | printed ink", (.045, .075, .07))
light = material("SORA | warm LED", (1, .88, .64), .3, 2)
ceiling = material("SORA | acoustic ceiling", (.36, .37, .34), .85)
sign = material("SORA | illuminated lettering", (.86, 1, .93), .3, 1.1)
glass = material("SORA | clear storefront glass", (.64, .84, .85), .08, alpha=.13)
cooler_glass = material("SORA | refrigerator glass", (.62, .81, .82), .12, alpha=.18)
rubber = material("SORA | entrance mat", (.065, .08, .08), .95)
steel = material("SORA | brushed steel", (.32, .38, .38), .27)


def box(name, at, size, mat, bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=point(at))
    obj = bpy.context.object
    obj.name = name
    obj.scale = (size[0], size[2], size[1])
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(mat)
    if bevel:
        modifier = obj.modifiers.new("Manufactured edges", "BEVEL")
        modifier.width = bevel
        modifier.segments = 2
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    return obj


def text(body, at, height, mat=white, facing="front", font=None):
    curve = bpy.data.curves.new("SORA original lettering", "FONT")
    curve.body = body
    curve.size = height
    curve.align_x = "CENTER"
    curve.align_y = "CENTER"
    curve.extrude = .0005
    curve.resolution_u = 4
    if font is not None:
        curve.font = font
    obj = bpy.data.objects.new("LABEL | " + body, curve)
    bpy.context.collection.objects.link(obj)
    obj.location = point(at)
    # Text local +Z faces Unity -Z; local +X maps to Unity +X.
    obj.rotation_euler = (math.pi / 2, 0, math.pi)
    if facing == "back":
        obj.rotation_euler.z = 0
    elif facing == "left":
        obj.rotation_euler.z = math.pi / 2
    curve.materials.append(mat)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    for other in bpy.context.selected_objects:
        if other != obj:
            other.select_set(False)
    bpy.ops.object.convert(target="MESH")
    return obj


model_source = ROOT / "ArtSource/ThirdParty/SoraMart"
model_manifest = json.loads((model_source / "manifest.json").read_text(encoding="utf-8"))
models = {}
placements = Counter()
food_material = None
constant_materials = {}
japanese_font = bpy.data.fonts.load(str(model_source / "Signage/ZenKakuGothicNew-Regular.ttf"))


def share_material(mat):
    nodes = list(mat.node_tree.nodes) if mat.use_nodes else []
    if not nodes:
        return mat
    # Compare shader graphs and actual image paths, including UV and alpha settings.
    settings = []
    for node in sorted(nodes, key=lambda item: item.name):
        inputs = tuple((socket.identifier, tuple(socket.default_value) if hasattr(socket.default_value, "__iter__")
                        and not isinstance(socket.default_value, str) else socket.default_value)
                       for socket in node.inputs if hasattr(socket, "default_value"))
        image = (bpy.path.abspath(node.image.filepath), node.image.colorspace_settings.name,
                 node.interpolation, node.extension) if node.type == "TEX_IMAGE" and node.image else None
        options = tuple((key, getattr(node, key)) for key in ("operation", "blend_type", "uv_map", "vector_type", "space")
                        if hasattr(node, key))
        settings.append((node.name, node.type, inputs, image, options))
    links = tuple(sorted((link.from_node.name, link.from_socket.identifier, link.to_node.name, link.to_socket.identifier)
                         for link in mat.node_tree.links))
    key = (tuple(settings), links, mat.use_backface_culling, mat.surface_render_method, tuple(mat.diffuse_color))
    return constant_materials.setdefault(key, mat)


for record in model_manifest["textures"] + model_manifest["sources"] + model_manifest.get("dependencies", []):
    path = (model_source / record["path"]).resolve()
    assert path.is_relative_to(model_source.resolve()), "Source path escapes model folder"
    assert hashlib.sha256(path.read_bytes()).hexdigest() == record["sha256"], "Model source checksum changed: " + str(path)
for record in model_manifest["sources"]:
    assert record["license"] in {"CC0 1.0", "CC-BY 3.0", "CC-BY 4.0"}, "Unreviewed model license"
    before = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=str(model_source / record["path"]))
    objects = set(bpy.data.objects) - before
    meshes = []
    for obj in objects:
        if obj.type == "MESH":
            # Replace the cabinet's stock blocks with the separately downloaded food meshes.
            if record["key"] == "hot_food" and all(mat.name.split(".")[0] == "stock" for mat in obj.data.materials):
                continue
            mesh = obj.data.copy()
            mesh.transform(obj.matrix_world)
            # These reviewed Kenney exports share identical material settings and palette.
            if record["creator"] == "Kenney":
                assert len(mesh.materials) == 1
                if food_material is None:
                    food_material = mesh.materials[0]
                mesh.materials[0] = food_material
            else:
                for index, mat in enumerate(mesh.materials):
                    mesh.materials[index] = share_material(mat)
            meshes.append(mesh)
    assert meshes, "Source contains no meshes: " + record["key"]
    points = [vertex.co for mesh in meshes for vertex in mesh.vertices]
    low = Vector(tuple(min(p[i] for p in points) for i in range(3)))
    high = Vector(tuple(max(p[i] for p in points) for i in range(3)))
    assert min(high - low) > 0, "Degenerate model bounds"
    models[record["key"]] = (meshes, low, high)
    for obj in objects:
        bpy.data.objects.remove(obj, do_unlink=True)


def place_model(key, at, width=None, size=None, yaw=0):
    meshes, low, high = models[key]
    extent = high - low
    scale = width / extent.x if width else min(size[0] / extent.x, size[1] / extent.z, size[2] / extent.y)
    origin = Vector(((low.x + high.x) / 2, (low.y + high.y) / 2, low.z))
    transform = (Matrix.Translation(point(at)) @ Matrix.Rotation(math.pi + math.radians(yaw), 4, "Z")
                 @ Matrix.Scale(scale, 4) @ Matrix.Translation(-origin))
    for mesh in meshes:
        obj = bpy.data.objects.new("IMPORTED | " + key + " | " + mesh.name, mesh)
        bpy.context.collection.objects.link(obj)
        obj.matrix_world = transform
        record = next(source for source in model_manifest["sources"] if source["key"] == key)
        obj["sora_source"] = (key if record["path"].startswith("Fixtures/") else
                              "packaged_goods" if record.get("source_pack") else "goods")
    placements[key] += 1
    return scale


# Select actual existing source groups, retaining their imported materials/UVs.
source_manifest = json.loads((ROOT / "ArtSource/ThirdParty/CC0/manifest.json").read_text(encoding="utf-8"))
assert hashlib.sha256(SOURCE.read_bytes()).hexdigest() == source_manifest["official_downloads"]["izakaya"]["sha256"], "CC0 source checksum changed"
before = set(bpy.data.objects)
bpy.ops.import_scene.gltf(filepath=str(SOURCE))
imported = set(bpy.data.objects) - before
# glTFast's installed importer has no WebP decoder; retain UVs but embed PNGs.
texture_folder = ROOT / "Artifacts/SoraMart/source-textures"
texture_folder.mkdir(parents=True, exist_ok=True)
replacements = {}
for index, image in enumerate(list(bpy.data.images)):
    if image.file_format != "WEBP":
        continue
    png = image.copy()
    png.filepath_raw = str(texture_folder / ("cc0-palette-" + str(index) + ".png"))
    png.file_format = "PNG"
    png.save()
    replacements[image] = bpy.data.images.load(png.filepath_raw, check_existing=False)
    replacements[image].pack()
for mat in bpy.data.materials:
    if mat.node_tree:
        for node in mat.node_tree.nodes:
            if node.type == "TEX_IMAGE" and node.image in replacements:
                node.image = replacements[node.image]
templates = {}
groups = ["095-drinks-vending-machine", "105-potted-plant-in-can", "117-air-conditioning-outdoor-unit",
          "087-beer-crate-stack", "100-alley-waste-bins"]
for name in groups:
    root = next(o for o in imported if o.name == name)
    templates[name] = [o for o in root.children_recursive if o.type == "MESH"]


def reuse(group, at, width, yaw=0):
    objects = templates[group]
    corners = [obj.matrix_world @ Vector(corner) for obj in objects for corner in obj.bound_box]
    low = Vector(tuple(min(p[i] for p in corners) for i in range(3)))
    high = Vector(tuple(max(p[i] for p in corners) for i in range(3)))
    origin = Vector(((low.x + high.x) / 2, (low.y + high.y) / 2, low.z))
    scale = width / (high.x - low.x)
    for source in objects:
        mesh = source.data.copy()
        for vertex in mesh.vertices:
            vertex.co = (source.matrix_world @ vertex.co - origin) * scale
        obj = bpy.data.objects.new("CC0 | " + group + " | " + source.name, mesh)
        bpy.context.collection.objects.link(obj)
        obj.location = point(at)
        obj.rotation_euler.z = math.pi + math.radians(yaw)


# Complete envelope, roof and visible side/rear treatment.
box("MART | tiled floor", (0, -.035, 0), (13.98, .05, 10.9), ivory)
grout = material("SORA | floor grout", (.47, .51, .48), .8)
for x in range(-6, 7):
    box("Floor grout", (x, -.007, 0), (.012, .004, 10.8), grout)
for z in range(-5, 6):
    box("Floor grout", (0, -.007, z), (13.8, .004, .012), grout)
for x in [-7, 7]:
    box("MART | side wall", (x, 2.2, .1), (.36, 4.4, 10.7), ivory)
    box("MART | side plinth", (x * 1.015, .22, .1), (.12, .44, 10.7), frame)
    for z in [-4, -.5, 3.8]:
        box("MART | side pilaster", (x * 1.018, 2.12, z), (.16, 4.15, .18), white)
    box("MART | side jade band", (x * 1.018, 3.65, .1), (.16, .18, 10.7), teal)
box("MART | rear wall", (0, 2.2, 5.45), (14, 4.4, .35), ivory)
box("MART | rear service door", (5, 1.14, 5.64), (1.15, 2.28, .08), frame, .025)
box("MART | roof", (0, 4.4, .1), (14.45, .20, 11.1), ceiling)
box("MART | canopy", (0, 3.83, -5.95), (14.5, .20, 1.95), frame, .025)
box("MART | fascia", (0, 4.34, -5.9), (14.5, .82, .24), teal, .035)
box("MART | orange signature line", (0, 3.96, -6.04), (14.45, .075, .025), orange)
text("SORA MART", (0, 4.35, -6.065), .69, sign)
text("24H", (5.95, 4.35, -6.065), .34, sign)
text("FRESH FOOD  /  DAILY GOODS", (-5.25, 4.35, -6.065), .14, sign)
for x in [-6.8, -4.65, -2.5, 2.5, 4.65, 6.8]:
    box("MART | glazing mullion", (x, 1.85, -5.3), (.085, 3.65, .17), frame)
for x in [-4.65, 4.65]:
    box("MART | clear front window", (x, 1.9, -5.3), (4.22, 3.35, .025), glass)
    box("MART | window sill", (x, .16, -5.3), (4.25, .25, .24), frame)
    box("MART | window safety stripe", (x, 1.1, -5.325), (4.2, .07, .012), teal)
for x in [-1.88, 1.88]:
    box("MART | parked sliding glass door", (x, 1.77, -5.26), (1.2, 3.4, .025), glass)
    for side in [-.6, .6]:
        box("MART | door stile", (x + side, 1.77, -5.28), (.055, 3.4, .075), frame)
    box("MART | door safety stripe", (x, 1.1, -5.30), (1.16, .07, .012), orange)
box("MART | entrance mat", (0, .005, -5.25), (2.4, .025, 1.4), rubber)
text("WELCOME", (0, 3.58, -5.43), .18, white)
# R1: a low, bright acoustic ceiling with a visible fluorescent grid.
interior_ceiling = material("SORA | interior acoustic panels", (.88, .89, .84), .9, .08)
box("MART | interior ceiling", (0, 3.12, .1), (13.8, .06, 10.7), interior_ceiling)
for x in [-6, -4, -2, 0, 2, 4, 6]:
    box("MART | ceiling grid", (x, 3.082, .1), (.016, .014, 10.6), ivory)
for z in [-4, -2, 0, 2, 4]:
    box("MART | ceiling grid", (0, 3.082, z), (13.7, .014, .016), ivory)
for x in [-5.6, -3.35, -1.1, 1.15, 3.4, 5.65]:
    box("MART | ceiling light housing", (x, 3.06, 0), (.35, .06, 8.6), white)
    box("MART | ceiling LED diffuser", (x, 3.02, 0), (.26, .025, 8.4), light)


text("\u30bd\u30e9\u30de\u30fc\u30c8", (0, 3.98, -6.065), .15, sign, font=japanese_font)

# Categorize the actual downloaded SKUs; do not recolor one box into many products.
retail = {key: [] for key in ["care", "pantry", "breakfast", "sweets", "noodles", "bakery", "drinks"]}
for source in model_manifest["sources"]:
    if source.get("source_pack") not in {"FoodCollection", "PSXFood"}:
        continue
    key = source["key"]
    name = source["source_node"].lower()
    if source["category"] == "HouseholdAndCare":
        category = "care"
    elif any(word in name for word in ("coffee", "tea_", "cereal")):
        category = "breakfast"
    elif source["category"] == "Drinks":
        category = "drinks"
    elif "noodles" in name:
        category = "noodles"
    elif any(word in name for word in ("pizza", "burgerbox")):
        category = "bakery"
    elif any(word in name for word in ("choco", "cand", "chewing", "cookies", "snack")):
        category = "sweets"
    else:
        category = "pantry"
    retail[category].append(key)

# R1/R6: low central racks; essentials near the front, food toward the rear.
categories = ["DAILY CARE", "PANTRY", "BREAKFAST", "SWEETS", "INSTANT NOODLES", "BREAD / TO GO"]
jp_categories = ["\u65e5\u7528\u54c1", "\u98df\u54c1\u30fb\u8abf\u5473\u6599", "\u671d\u98df\u30fb\u30b3\u30fc\u30d2\u30fc", "\u304a\u83d3\u5b50", "\u30ab\u30c3\u30d7\u9eba", "\u30d1\u30f3\u30fb\u8efd\u98df"]
assortments = [retail[key] for key in ["care", "pantry", "breakfast", "sweets", "noodles", "bakery"]]
for row, x in enumerate([-2.6, 2.5]):
    for column, z in enumerate([-1.1, 1.0, 3.1]):
        category = categories[row * 3 + column]
        assortment = assortments[row * 3 + column]
        scale = place_model("gondola", (x, 0, z), width=1.45)
        for side in [-1, 1]:
            for level, deck in enumerate([.164, .446, .836, 1.226]):
                y = deck * scale
                box("MART | shelf price rail", (x, y, z + side * .337), (1.36, .045, .02), teal)
                for item in range(6):
                    xx = x - .55 + item * .22
                    for depth in [.12, .265]:
                        place_model(assortment[(item + level * 6 + (24 if side > 0 else 0)) % len(assortment)],
                                    (xx, y + .004, z + side * depth), size=(.17, .24, .135), yaw=0 if side < 0 else 180)
                    box("MART | price ticket", (xx, y, z + side * .35), (.15, .035, .006), white)
                    text(str([198, 168, 298, 128, 178, 248][row * 3 + column]),
                         (xx, y, z + side * .355), .023, ink, facing="back" if side > 0 else "front")
        box("MART | category header", (x, 1.7, z), (1.45, .25, .14), teal, .018)
        text(jp_categories[row * 3 + column], (x, 1.73, z - .077), .145, font=japanese_font)
        text(jp_categories[row * 3 + column], (x, 1.73, z + .077), .145, facing="back", font=japanese_font)
        text(category, (x, 1.62, z - .077), .065)
        text(category, (x, 1.62, z + .077), .065, facing="back")
        end_x = x + (.95 if row == 0 else -.95)
        place_model("endcap", (end_x, 0, z), width=.66, yaw=90 if row == 0 else -90)
        for deck in [.466, .816, 1.166]:
            for item in range(3):
                place_model(assortment[item % len(assortment)],
                            (end_x + (.08 if row == 0 else -.08), deck + .005, z - .18 + item * .18),
                            size=(.14, .24, .14), yaw=90 if row == 0 else -90)

# Existing POS location; visuals never register a second checkout.
for x in [-5.5, -4.2]:
    place_model("checkout", (x, 0, -2.5), width=1.3)
place_model("register_console", (-4.85, .995, -2.54), size=(.57, .48, .47), yaw=180)
place_model("pos", (-5.55, .995, -2.60), width=.34)
place_model("cash_tray", (-4.83, 1.0, -2.86), size=(.27, .08, .18))
place_model("card_reader", (-4.03, 1.0, -2.76), size=(.24, .28, .25))
place_model("receipt_printer", (-4.32, 1.0, -2.4), size=(.26, .24, .24), yaw=180)
place_model("scanner", (-5.35, 1.0, -2.58), size=(.16, .12, .30))
hot_scale = place_model("hot_food", (-6.25, 0, -3.6), size=(.86, 1.65, .63))
for deck, key in [(1.026, "corn-dog"), (1.286, "sausage")]:
    for x in [-6.45, -6.25, -6.05]:
        place_model(key, (x, deck * hot_scale + .003, -3.72), size=(.14, .15, .18))
text("CHECKOUT", (-4.85, 2.75, -2.5), .25, sign)
box("MART | checkout hanging board", (-4.85, 2.75, -2.44), (2.3, .45, .08), teal)
text("\u304a\u4f1a\u8a08", (-4.85, 2.93, -2.5), .15, sign, font=japanese_font)

# Right-wall chilled drinks; handles/glass have depth and actual stocked shelves.
for z in [-3.1, -1.6, -.1, 1.4]:
    box("MART | cooler back", (6.74, 1.45, z), (.07, 2.9, 1.45), frame)
    for dz in [-.7, .7]:
        box("MART | cooler side", (6.35, 1.45, z + dz), (.85, 2.9, .05), frame)
    for y in [.08, 2.82]:
        box("MART | cooler cap", (6.35, y, z), (.85, .16, 1.45), frame)
    box("MART | cooler lit backing", (6.72, 1.40, z), (.02, 2.5, 1.3), white)
    box("MART | cooler clear door", (5.90, 1.45, z), (.025, 2.55, 1.30), cooler_glass)
    for dz in [-.66, .66]:
        box("MART | cooler vertical frame", (5.87, 1.45, z + dz), (.07, 2.65, .05), steel)
    box("MART | cooler handle", (5.82, 1.45, z + .48), (.08, .65, .035), steel, .012)
    for level in range(4):
        y = .28 + level * .58
        box("MART | cooler shelf", (6.28, y, z), (.7, .035, 1.28), ivory)
        for item in range(7):
            if level == 0 and item < 2:
                if item == 0:
                    place_model("fc_bottles_pack_01", (6.12, y + .018, z - .45), size=(.30, .35, .28), yaw=-90)
                continue
            drinks = [key for key in retail["drinks"] if key != "fc_bottles_pack_01"]
            place_model(drinks[(item + level * 7) % len(drinks)],
                        (6.12, y + .018, z - .52 + item * .17), size=(.14, .40, .14), yaw=-90)
    box("MART | cooler header", (5.88, 2.79, z), (.04, .20, 1.28), teal)
    text("COLD DRINKS", (5.84, 2.79, z), .11, sign, facing="left")

# R2: shallow chilled-food bays along the rear wall, facing the sales floor.
wrapping = material("SORA | clear food packaging", (.90, .96, .98), .24, alpha=.11)
fresh_names = {"rice-ball": "\u304a\u306b\u304e\u308a", "sandwich": "\u30b5\u30f3\u30c9", "styrofoam-dinner": "\u304a\u5f01\u5f53", "maki-salmon": "\u304a\u5bff\u53f8"}
for x in [-1.5, 1.25]:
    chilled_scale = place_model("chilled_food", (x, 0, 4.87), width=1.35)
    for level, deck in enumerate([.20, .47, .97, 1.47]):
        y = (deck - models["chilled_food"][1].z) * chilled_scale
        for item in range(5):
            key = ("rice-ball", "sandwich", "styrofoam-dinner", "yoghurt", "pudding",
                   "butter", "egg_carton", "maki-salmon")[(item + level * 2) % 8]
            xx = x - .48 + item * .24
            scale = place_model(key, (xx, y + .006, 4.57), size=(.20, .19, .24))
            if key in fresh_names:
                extent = models[key][2] - models[key][1]
                width, height, depth = extent.x * scale + .008, extent.z * scale + .012, extent.y * scale + .008
                if key in {"rice-ball", "sandwich"}:
                    # R2/R5: a clear triangular sleeve around the downloaded food.
                    verts = [(sx * width / 2, sy * depth / 2, 0) for sy in [-1, 1] for sx in [-1, 1]]
                    verts += [(0, -depth / 2, height), (0, depth / 2, height)]
                    mesh = bpy.data.meshes.new("SORA original triangular food sleeve")
                    mesh.from_pydata(verts, [], [(1, 4, 0), (5, 3, 2), (4, 5, 2, 0), (3, 5, 4, 1), (2, 3, 1, 0)])
                    mesh.materials.append(wrapping)
                    sleeve = bpy.data.objects.new("SORA | food wrapper", mesh)
                    bpy.context.collection.objects.link(sleeve)
                    sleeve.location = point((xx, y + .005, 4.57))
                else:
                    box("SORA | transparent meal lid", (xx, y + .006 + height / 2, 4.57), (width, height, depth), wrapping, .008)
                label_y = y + min(.06, height * .35)
                box("SORA | fresh food label", (xx, label_y, 4.57 - depth / 2 - .006), (min(.09, width * .65), .045, .002), white)
                text(fresh_names[key], (xx, label_y + .008, 4.57 - depth / 2 - .008), .011, ink, font=japanese_font)
                text("SORA  298", (xx, label_y - .011, 4.57 - depth / 2 - .008), .009, teal)
        box("MART | chilled shelf price rail", (x, y - .018, 4.41), (1.24, .04, .015), teal)
    text("\u304a\u306b\u304e\u308a\u30fb\u304a\u5f01\u5f53", (x, 2.06, 4.40), .14, sign, font=japanese_font)

# R6: coffee and warming beside the staffed counter; bakery on the same side.
box("MART | coffee counter", (-6.25, .55, 1.6), (1.05, 1.1, 2.6), oak, .025)
box("MART | coffee counter top", (-6.25, 1.13, 1.6), (1.12, .08, 2.7), ivory, .02)
place_model("coffee_brewer", (-6.3, 1.18, .85), size=(.65, .75, .65), yaw=90)
place_model("microwave", (-6.25, 1.18, 1.80), size=(.64, .42, .66), yaw=90)
for z in [2.35, 2.55, 2.75]:
    place_model("cup-coffee", (-6.2, 1.18, z), size=(.16, .16, .16))
bakery_scale = place_model("bakery_rack", (-6.43, 0, 3.94), width=1.20, yaw=90)
for deck in [.25, .70, 1.15, 1.60]:
    y = (deck - models["bakery_rack"][1].z) * bakery_scale + .005
    for item in range(4):
        place_model(("sliced_bread", "croissant", "muffin")[item % 3],
                    (-6.34, y, 3.49 + item * .27), size=(.20, .22, .20), yaw=90)

# R6: magazines, basket stack and umbrellas sit by the window, clear of the doorway.
magazine_scale = place_model("magazine_rack", (3.35, 0, -4.52), width=.92, yaw=180)
for deck in [.47, .77, 1.07, 1.37]:
    y = (deck - models["magazine_rack"][1].z) * magazine_scale + .005
    for x in [3.12, 3.55]:
        place_model("magazine", (x, y, -4.40), size=(.30, .20, .24), yaw=180)
place_model("basket_stack", (1.90, 0, -4.6), width=.58)
place_model("umbrella_stand", (-1.90, 0, -4.6), width=.38)
for x in [-1.98, -1.88]:
    place_model("umbrella", (x, .20, -4.6), size=(.08, .78, .08))
reuse("087-beer-crate-stack", (5, 0, 3.15), 1.05)
place_model("stock_shelf", (5, 0, 4.3), width=1.435)
place_model("back_fixture", (-4.85, 0, 5.05), width=1.42)
for y in [.59, 1.135, 1.415]:
    for item in range(5):
        place_model(["can", "honey", "peanut-butter"][item % 3], (-5.35 + item * .25, y + .005, 4.97), size=(.16, .22, .16))
box("MART | staff partition", (4.85, 1.9, 4.85), (3.6, 3.8, .25), ivory)
text("STAFF / STOCK", (4.85, 2.6, 4.70), .20, teal)
box("MART | stock terminal", (6.3, 1.3, 1.6), (.24, .62, .8), frame)
place_model("wall_clock", (-6.73, 1.40, 3.8), width=.42, yaw=90)
place_model("extinguisher", (-6.63, 0, -.50), width=.34, yaw=90)
place_model("delivery_boxes", (5.0, .70, 4.23), size=(.85, .50, .46))
place_model("stock_carton", (5.55, .70, 4.25), size=(.42, .38, .38))
place_model("clipboard", (6.15, 1.62, 1.55), size=(.20, .12, .30))

# Existing imported street furniture complements the store rather than duplicating the city.
reuse("095-drinks-vending-machine", (-6.15, 0, -6.05), 1.02)
reuse("105-potted-plant-in-can", (6.5, 0, -6.2), .52)
reuse("100-alley-waste-bins", (5.15, 0, -6.25), .95)
reuse("117-air-conditioning-outdoor-unit", (5, 4.52, 2.6), 1.4)

for obj in imported:
    bpy.data.objects.remove(obj, do_unlink=True)

# Merge static opaque parts per material; retain glass separately for correct sorting.
by_material = {}
for obj in list(bpy.context.scene.objects):
    if obj.type == "MESH" and len(obj.data.materials) == 1:
        mat = obj.data.materials[0]
        if mat.diffuse_color[3] >= .999:
            by_material.setdefault((obj.get("sora_source", ""), mat), []).append(obj)
for (group, mat), objects in by_material.items():
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    if len(objects) > 1:
        bpy.ops.object.join()
    objects[0].name = ("IMPORTED | " + group + " | " if group else "SORA MART | ") + mat.name

OUTPUT.parent.mkdir(parents=True, exist_ok=True)
assert set(placements) <= set(models)
assert all(placements[source["key"]] > 0 for source in model_manifest["sources"] if source.get("source_pack")), "Selected library resource was not placed"
bpy.ops.export_scene.gltf(filepath=str(OUTPUT), export_format="GLB", export_yup=True,
                          export_cameras=False, export_lights=False, export_extras=True)
manifest = {
    "asset": str(OUTPUT.relative_to(ROOT)).replace("\\", "/"),
    "sha256": hashlib.sha256(OUTPUT.read_bytes()).hexdigest(),
    "source": str(SOURCE.relative_to(ROOT)).replace("\\", "/"),
    "source_sha256": hashlib.sha256(SOURCE.read_bytes()).hexdigest(),
    "source_groups": groups,
    "rights": "CC0, CC-BY 3.0 and CC-BY 4.0 source models plus original SORA architecture; OFL 1.1 signage font; no AFX geometry; retain ArtSource/ThirdParty/SoraMart/ATTRIBUTION.md",
    "model_source_manifest": "ArtSource/ThirdParty/SoraMart/manifest.json",
    "model_source_manifest_sha256": hashlib.sha256((model_source / "manifest.json").read_bytes()).hexdigest(),
    "model_placements": dict(placements),
    "available_model_sources": len(models),
    "active_model_sources": len(placements),
    "packaged_product_sources": sum(1 for source in model_manifest["sources"] if source.get("source_pack") in {"FoodCollection", "PSXFood"} and placements[source["key"]]),
    "reference_layout": "Docs/StreetExterior/SORA_MART_KONBINI_REFERENCE.md (R1-R6; adapted to existing gameplay anchors)",
    "coordinates": "Unity metres after glTFast import; identity transform; entrance (0,0,-5.25)",
    "interaction_anchors": {"POS": [-4.85, 1.3, -2.54], "restock": [-2.6, 1.65, -.32], "delivery": [5, .45, 3.15]},
    "limits": "Decorative aggregate stock; doors parked open; source materials retained; Japanese packaging/labels and final art acceptance pending",
}
(ROOT / "Docs/StreetExterior/SORA_MART_ASSETS.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
print("SORA MART exported:", OUTPUT, OUTPUT.stat().st_size, "bytes")
