#!/usr/bin/env python3
"""SORA V06 source tests: these do not constitute Unity acceptance."""
import importlib.util
import json
import tempfile
import unittest
from pathlib import Path
import trimesh

spec=importlib.util.spec_from_file_location("v06",Path(__file__).with_name("build_v06_facade_detail.py"))
v06=importlib.util.module_from_spec(spec)
spec.loader.exec_module(v06)

class TestV06(unittest.TestCase):
    def test_orientation(self):
        for yaw in (0,90,180,-90):
            d=v06.DetailPass()
            d.local({"id":"A","front_xz":[10,20],"yaw":yaw},"sign",[0,2,-1],[1,1,.1],"test",(1,2,3))
            tf=d.scene.graph["V06/A/sign"][0]
            import math
            self.assertAlmostEqual(tf[0,3],10-math.sin(math.radians(yaw)),places=5)
            self.assertAlmostEqual(tf[2,3],20-math.cos(math.radians(yaw)),places=5)

    def test_actual_v04_layout_and_v05_contract(self):
        root=Path(__file__).resolve().parents[2]
        layout,v05=v06.read_inputs(root/"Docs/StreetExterior/V04_DENSE_GRID_LAYOUT.json",root/"ArtSource/Generated/V05_CC0/manifest.json")
        self.assertEqual(len(layout["building_catalog"]),60)
        self.assertEqual(len(layout["blocks"]),9)
        self.assertEqual(v05["kenney_car_count"],42)

    def test_export(self):
        root=Path(__file__).resolve().parents[2]
        with tempfile.TemporaryDirectory() as tmp:
            m=v06.build(root/"Docs/StreetExterior/V04_DENSE_GRID_LAYOUT.json",root/"ArtSource/Generated/V05_CC0/manifest.json",tmp)
            self.assertEqual(m["new_v04_v05_facades_detailed"],60)
            self.assertEqual(m["new_colliders"],0)
            self.assertEqual(len(m["facades"]),60)
            self.assertGreater(m["geometry_nodes"],1000)
            scene=trimesh.load(Path(tmp)/"SORA_V06_9Block_Facade_Details.glb",force="scene",process=False)
            self.assertEqual(len(scene.graph.nodes_geometry),m["geometry_nodes"])

if __name__=="__main__":
    unittest.main()
