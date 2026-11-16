"""Check comparable Unity captures: python Tools/verify_store_visual_captures.py BEFORE AFTER."""
import argparse
import json
from pathlib import Path
import struct


def verify(before, after):
    for index in range(1, 5):
        name = f"C{index:02}"
        records = []
        for folder in (before, after):
            assert not (folder / "failure.txt").exists(), f"Failed capture: {folder}"
            data = (folder / f"{name}.png").read_bytes()
            assert data[:8] == b"\x89PNG\r\n\x1a\n", f"Invalid PNG: {folder / name}"
            assert struct.unpack_from(">II", data, 16) == (1920, 1080), f"Wrong image size: {folder / name}"
            record = json.loads((folder / f"{name}.json").read_text(encoding="utf-8"))
            assert record["graphicsApi"] != "Null", "Real graphics device required"
            assert record["models"] == ["Zhongli", "Chisa"], "Wrong baseline models"
            assert record["umbrellas"] == [False, index == 4], "Optional umbrella baseline changed"
            assert record["width"] == 1920 and record["height"] == 1080
            assert record["weather"]["automaticCycle"] is False
            records.append(record)
        for field in ("id", "unity", "gpu", "graphicsApi", "colorSpace", "position", "rotation", "fov",
                      "privateCity", "models", "characters", "umbrellas"):
            assert records[0][field] == records[1][field], f"{name}: mismatched {field}"
        for field in ("kind", "rain", "wetness", "puddles"):
            assert abs(records[0]["weather"][field] - records[1]["weather"][field]) < .001, f"{name}: different {field}"
        print(f"PASS {name}: 1920x1080, same camera, models, city and weather")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("before", type=Path)
    parser.add_argument("after", type=Path)
    args = parser.parse_args()
    verify(args.before, args.after)
