"""Check the shopping list against the local downloads and recorded provenance.

python Tools/StreetExterior/verify_sora_mart_downloads.py [--intake PATH]
"""
import argparse
import csv
import hashlib
import json
from pathlib import Path
import re
import struct
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--intake", type=Path, default=ROOT / "LocalReferenceAssets/SoraMartIncoming")
args = parser.parse_args()
intake = args.intake.resolve(strict=True)
snapshot = json.loads((ROOT / "Docs/StreetExterior/SORA_MART_DOWNLOADS.json").read_text(encoding="utf-8"))
local = json.loads((intake / "intake_manifest.json").read_text(encoding="utf-8"))
assert snapshot["items"] == local["items"], "Local intake differs from the published evidence"
shopping = (ROOT / "Docs/SORA_MART_MODEL_SHOPPING_LIST.md").read_text(encoding="utf-8")
rows = dict(re.findall(r"^\| (SM-[A-J]\d{2}) \| (.+)$", shopping, re.M))
assert len(rows) == len(re.findall(r"^\| SM-[A-J]\d{2} \|", shopping, re.M)) == 151
coverage = {key: [] for key in rows}
paths, hashes = set(), set()
existing = set()
for source in snapshot["existing_sources"]:
    path = (ROOT / source["path"]).resolve()
    assert path.is_relative_to(ROOT)
    payload = path.read_bytes()
    assert hashlib.sha256(payload).hexdigest() == source["sha256"]
    length = struct.unpack_from("<I", payload, 12)[0]
    nodes = {node.get("name") for node in json.loads(payload[20:20 + length])["nodes"]}
    for item_id, names in source["candidate_nodes"].items():
        assert item_id in rows and set(names) <= nodes
        existing.add(item_id)
for source in snapshot["items"]:
    assert source["license"] in {"CC0 1.0", "CC-BY 3.0"}
    evidence = (intake / source["folder"] / "SOURCE_PAGE.html").resolve()
    license_path = (intake / source["license_file"]).resolve()
    assert evidence.is_relative_to(intake) and evidence.is_file()
    assert license_path.is_relative_to(intake) and license_path.stat().st_size > 0
    if source["license"] == "CC-BY 3.0":
        assert source["creator"] in (evidence.parent / "ATTRIBUTION.txt").read_text(encoding="utf-8")
    for record in source["files"]:
        path = (intake / record["path"]).resolve()
        assert path.is_relative_to(intake), record["path"]
        payload = path.read_bytes()
        assert len(payload) == record["bytes"], record["path"]
        assert hashlib.sha256(payload).hexdigest() == record["sha256"], record["path"]
        if path.suffix != ".glb":
            continue
        assert record["path"] not in paths and record["sha256"] not in hashes, "Duplicate model export"
        paths.add(record["path"])
        hashes.add(record["sha256"])
        magic, version, size, length, kind = struct.unpack_from("<4sIIII", payload)
        assert magic == b"glTF" and version == 2 and size == len(payload)
        assert kind == 0x4E4F534A and length <= size - 20
        document = json.loads(payload[20:20 + length])
        assert document.get("meshes"), record["path"]
        for preview in ("preview_path", "rear_preview_path"):
            if preview in record:
                image = (intake / record[preview]).resolve()
                assert image.is_relative_to(intake) and image.is_file()
        for key in ("buffers", "images"):
            for resource in document.get(key, []):
                uri = resource.get("uri", "")
                if uri and not uri.startswith("data:"):
                    dependency = (path.parent / unquote(uri)).resolve()
                    assert dependency.is_relative_to(intake) and dependency.is_file(), (record["path"], uri)
        for item_id in record.get("candidate_ids", source["candidate_ids"]):
            assert item_id in coverage, item_id
            coverage[item_id].append(record["path"])
for item_id, files in coverage.items():
    marker = (f"[x] Tải {len(files)} ứng viên" if files else
              "[x] Có nguồn sẵn" if item_id in existing else "[ ] Chưa tải")
    assert marker in rows[item_id], (item_id, marker)
with (ROOT / "Docs/StreetExterior/SORA_MART_DOWNLOADS.csv").open(encoding="utf-8-sig", newline="") as handle:
    table = list(csv.DictReader(handle))
assert len(table) == len(paths)
assert {row["sha256"] for row in table} == hashes
assert {row["local_path"].removeprefix("LocalReferenceAssets/SoraMartIncoming/") for row in table} == paths
selected = json.loads((ROOT / "ArtSource/ThirdParty/SoraMart/manifest.json").read_text(encoding="utf-8"))
placed = json.loads((ROOT / "Docs/StreetExterior/SORA_MART_ASSETS.json").read_text(encoding="utf-8"))["model_placements"]
integrated = sum(source["sha256"] in hashes and placed.get(source["key"], 0) > 0 for source in selected["sources"])
assert len(paths) == snapshot["model_count"]
assert len(snapshot["items"]) == snapshot["distinct_source_count"]
assert integrated == snapshot["unity_integrated_models"]
assert sum(bool(files) for files in coverage.values()) == snapshot["download_candidate_items"]
assert len(existing) == snapshot["existing_candidate_items"]
assert 151 - len(existing | {key for key, files in coverage.items() if files}) == snapshot["missing_candidate_items"]
assert snapshot["accepted_shopping_list_items"] == 0, "Downloads do not establish full art acceptance"
print(f"PASS: {len(paths)} distinct GLBs, {len(coverage)} checklist rows, source/texture/license/hash checks.")
print(f"Candidates for {snapshot['download_candidate_items']}/151 requirements; {integrated} previously placed sources.")
print("New intake: Unity import, final art acceptance and gameplay NOT VERIFIED.")
