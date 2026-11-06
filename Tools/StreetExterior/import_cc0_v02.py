#!/usr/bin/env python3
"""Download and verify owner-reviewed CC0 street source assets for SORA V02.

The official publisher files must match SHA256 from the owner's uploaded
references. Keep them under ArtSource (NOT directly in Unity Assets).
Usage:
  python Tools/StreetExterior/import_cc0_v02.py \
      --output-dir ArtSource/ThirdParty/CC0
Dependencies: Pillow, numpy, opencv-python-headless with OpenEXR support.
"""
import argparse
import hashlib
import io
import json
import os
import struct
import urllib.request
from pathlib import Path

os.environ.setdefault("OPENCV_IO_ENABLE_OPENEXR", "1")
import cv2
import numpy as np
from PIL import Image

SOURCE = {
    "izakaya": (
        "https://cdn.3dassets.dev/assets/36462/v1/model.glb",
        "693cebbe5a8cf9e30710c707adfb1daf76140b28a14f289cdca9ebd879e0748f"),
    "city": (
        "https://cdn.3dassets.dev/assets/30098/v1/model.glb",
        "3169feade33f5df2e67080c96a4c939b427fec558f4e42f60a438386697d0f70"),
    "diff": (
        "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/4k/asphalt_01/asphalt_01_diff_4k.jpg",
        "802216690827192e5418ce194622f671f08c476ea48856cadb88804f7d83e42f"),
    "normal": (
        "https://dl.polyhaven.org/file/ph-assets/Textures/exr/4k/asphalt_01/asphalt_01_nor_gl_4k.exr",
        "00b613f0b91639a78234d523cab724f662b24501177895228114d801a2321cbb"),
    "rough": (
        "https://dl.polyhaven.org/file/ph-assets/Textures/exr/4k/asphalt_01/asphalt_01_rough_4k.exr",
        "1ec5fb05470c90f9355b71d3326e0f8c8ef0b3789124ecfb3bd07d6ab3c20378"),
}
PAGES = {
    "izakaya": "https://3dassets.dev/packs/izakaya-alley-and-ramen-shop",
    "city": "https://3dassets.dev/packs/japanese-school-and-city-street",
    "asphalt": "https://polyhaven.com/a/asphalt_01",
}
SOURCE_ZIP_SHA256 = "a77bbd983524e29b1e684c6c79f8da47d512debd8ff867540d708e7d1832a239"


def digest(payload):
    return hashlib.sha256(payload).hexdigest()


def load_verified(key, local_path=None):
    url, expected = SOURCE[key]
    if local_path is not None:
        payload = Path(local_path).read_bytes()
    else:
        request = urllib.request.Request(url, headers={"User-Agent": "SORA-CC0-Asset-Intake/1.0"})
        with urllib.request.urlopen(request, timeout=180) as response:
            payload = response.read()
    if digest(payload) != expected:
        raise RuntimeError("Hash mismatch for %s (upstream file changed), refusing import" % key)
    print("VERIFIED:", key, len(payload), "bytes")
    return payload


def inspect_glb(payload):
    if payload[:4] != b"glTF" or len(payload) < 20:
        raise ValueError("Not a GLB")
    version, total = struct.unpack_from("<II", payload, 4)
    n, kind = struct.unpack_from("<I4s", payload, 12)
    if version != 2 or total != len(payload) or kind != b"JSON":
        raise ValueError("Invalid GLB v2 header")
    document = json.loads(payload[20:20+n])
    if document.get("asset", {}).get("generator") != "3dassets.dev ingest":
        raise ValueError("Unexpected GLB generator")
    return {
        "nodes": len(document.get("nodes", [])),
        "meshes": len(document.get("meshes", [])),
        "materials": len(document.get("materials", [])),
        "extensions": document.get("extensionsUsed", []),
    }


def jpg_2k(payload):
    with Image.open(io.BytesIO(payload)) as source:
        if source.size != (4096, 4096):
            raise ValueError("Expected 4K asphalt diffuse JPG")
        image = source.convert("RGB").resize((2048, 2048), Image.Resampling.LANCZOS)
        output = io.BytesIO()
        image.save(output, "JPEG", quality=91, optimize=True, subsampling=0)
        return output.getvalue()


def exr_2k(payload):
    image = cv2.imdecode(np.frombuffer(payload, dtype=np.uint8), cv2.IMREAD_UNCHANGED)
    if image is None or image.dtype != np.float32 or image.shape[:2] != (4096, 4096):
        raise ValueError("Expected 4096x4096 float EXR; OpenEXR codec may be unavailable")
    image = cv2.resize(image, (2048, 2048), interpolation=cv2.INTER_AREA)
    image = (np.clip(image, 0, 1) * 255 + 0.5).astype(np.uint8)
    okay, encoded = cv2.imencode(".png", image)
    if not okay:
        raise ValueError("Cannot encode optimized PBR texture")
    return encoded.tobytes()


def run(root):
    root = Path(root)
    outputs = [
        ("izakaya", "IzakayaAlley/izakaya_starter_scene.glb"),
        ("city", "JapaneseSchoolCity/school_city_starter_scene.glb"),
    ]
    files = []
    details = {}
    for name, relative in outputs:
        data = load_verified(name)
        details[name] = inspect_glb(data)
        path = root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
        files.append({"path": relative, "sha256": digest(data), "bytes": len(data)})
    for name, relative, transform in [
        ("diff", "Asphalt01/asphalt_diffuse_2k.jpg", jpg_2k),
        ("normal", "Asphalt01/asphalt_normal_gl_2k.png", exr_2k),
        ("rough", "Asphalt01/asphalt_roughness_2k.png", exr_2k),
    ]:
        data = transform(load_verified(name))
        path = root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
        files.append({"path": relative, "sha256": digest(data), "bytes": len(data)})
    record = {
        "source_rights": "CC0 1.0 Universal; see publisher source pages",
        "reference_uploaded_zip_sha256": SOURCE_ZIP_SHA256,
        "publisher_pages": PAGES,
        "official_downloads": {key: {"url": url, "sha256": sha} for key,(url,sha) in SOURCE.items()},
        "glb_structure": details,
        "files": files,
        "conversion": "Asphalt 4K diffuse resized to 2K JPG, normal_GL/roughness EXRs downsampled and quantized to 8-bit PNG",
        "unity_status": "NOT IMPORTED; no .meta/GUID, material, FBX or Play Mode validation",
        "import_caution": "GLB uses EXT_texture_webp and KHR_mesh_quantization. Import via verified glTFast or Blender and check URP materials.",
    }
    (root / "manifest.json").write_text(json.dumps(record, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("PASS: five checked CC0 asset files prepared; Unity NOT RUN")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--output-dir", required=True, type=Path)
    run(parser.parse_args().output_dir)
