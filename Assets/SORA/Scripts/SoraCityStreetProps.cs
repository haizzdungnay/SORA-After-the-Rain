using System.Collections.Generic;
using UnityEngine;

namespace Sora.AfterTheRain.Demo
{
    /// <summary>
    /// Kenney City Kit Roads (CC0) street lamps, traffic lights, stop signs and trees (Suburban kit) placed on the V04 road grid.
    /// Visual only: colliders are removed so the 122-proxy collision contract is untouched.
    /// ponytail: fixed grid constants mirror V04_DENSE_GRID_LAYOUT.json; read the JSON if the grid changes.
    /// </summary>
    public static class SoraCityStreetProps
    {
        public const string LampPath = "SoraCity/Props/light-square";
        public const string SignalPath = "SoraCity/Props/traffic-light";
        public const string SignPath = "SoraCity/Props/road-sign-stop";
        public static readonly string[] TreePaths = { "SoraCity/Props/Suburban/tree-large", "SoraCity/Props/Suburban/tree-small" };
        public const float TreeScale = 8f;             // Suburban trees: ~0.6-0.8 units tall.
        public const float TreeShift = 7.5f;           // half a lamp spacing, so trees sit between lamps.
        public const float SignAlong = 9f;             // stop sign distance from intersection centre.
        public const string ConePath = "SoraCity/Props/construction-cone";
        public const float ConeEnd = 4f;               // cones sit this far inside the 5.4 m road-end barriers (map -63..108).
        public const float KenneyScale = 10f;          // Roads-pack GLBs are ~1/10 real size.
        public static readonly float[] RoadCenters = { -45f, 0f, 45f, 90f };
        public const float SidewalkOffset = 5.2f;      // road half-width 4.5 m + kerb.
        public const float LampSpacing = 15f;
        public const float MapMin = -60f, MapMax = 105f;
        // City-local (unity) spot of the interactive SORA MART; keep its frontage clear.
        public static readonly Vector2 MartXz = new Vector2(-17f, 6.75f);
        public const float MartClearRadius = 5f;

        public struct Spot { public Vector2 Xz; public Vector2 Dir; }  // Dir: direction the arm points to.

        /// <summary>Author-space (x, z) spots; Dir is the unit arm direction toward the road.</summary>
        public static List<Spot> LampSpots()
        {
            var list = new List<Spot>();
            foreach (float c in RoadCenters)
                for (float t = MapMin; t <= MapMax; t += LampSpacing)
                {
                    if (NearIntersection(t)) continue;
                    foreach (float side in new[] { -1f, 1f })
                    {
                        list.Add(new Spot { Xz = new Vector2(c + side * SidewalkOffset, t), Dir = new Vector2(-side, 0f) });
                        list.Add(new Spot { Xz = new Vector2(t, c + side * SidewalkOffset), Dir = new Vector2(0f, -side) });
                    }
                }
            return list;
        }

        public static List<Spot> TreeSpots()
        {
            var list = new List<Spot>();
            foreach (float c in RoadCenters)
                for (float t = MapMin + TreeShift; t <= MapMax; t += LampSpacing)
                {
                    if (NearIntersection(t)) continue;
                    foreach (float side in new[] { -1f, 1f })
                    {
                        list.Add(new Spot { Xz = new Vector2(c + side * SidewalkOffset, t), Dir = new Vector2(-side, 0f) });
                        list.Add(new Spot { Xz = new Vector2(t, c + side * SidewalkOffset), Dir = new Vector2(0f, -side) });
                    }
                }
            return list;
        }

        // One stop sign per intersection approach (16 intersections x 4), facing oncoming traffic.
        public static List<Spot> SignSpots()
        {
            var list = new List<Spot>();
            foreach (float cx in RoadCenters)
                foreach (float cz in RoadCenters)
                    foreach (float s in new[] { -1f, 1f })
                    {
                        list.Add(new Spot { Xz = new Vector2(cx + s * SidewalkOffset, cz + s * SignAlong), Dir = new Vector2(0f, s) });
                        list.Add(new Spot { Xz = new Vector2(cx - s * SignAlong, cz + s * SidewalkOffset), Dir = new Vector2(-s, 0f) });
                    }
            return list;
        }

        // Two cones in front of each of the 16 road-end barriers.
        public static List<Spot> ConeSpots()
        {
            var list = new List<Spot>();
            foreach (float c in RoadCenters)
                foreach (float lateral in new[] { -2f, 2f })
                    foreach (float end in new[] { -63f + ConeEnd, 108f - ConeEnd })
                    {
                        list.Add(new Spot { Xz = new Vector2(c + lateral, end), Dir = Vector2.up });
                        list.Add(new Spot { Xz = new Vector2(end, c + lateral), Dir = Vector2.right });
                    }
            return list;
        }

        public static List<Spot> SignalSpots()
        {
            var list = new List<Spot>();
            foreach (float cx in RoadCenters)
                foreach (float cz in RoadCenters)
                    foreach (float sx in new[] { -1f, 1f })
                        foreach (float sz in new[] { -1f, 1f })
                            list.Add(new Spot { Xz = new Vector2(cx + sx * SidewalkOffset, cz + sz * SidewalkOffset),
                                                Dir = new Vector2(-sx, -sz).normalized });
            return list;
        }

        // Lamps within a crossing road's width are skipped (intersections get signals instead).
        public static bool NearIntersection(float t)
        {
            foreach (float c in RoadCenters) if (Mathf.Abs(t - c) < 7f) return true;
            return false;
        }

        public static int Build(Transform city)
        {
            GameObject lamp = Resources.Load<GameObject>(LampPath);
            GameObject signal = Resources.Load<GameObject>(SignalPath);
            GameObject sign = Resources.Load<GameObject>(SignPath);
            GameObject treeA = Resources.Load<GameObject>(TreePaths[0]);
            GameObject treeB = Resources.Load<GameObject>(TreePaths[1]);
            GameObject cone = Resources.Load<GameObject>(ConePath);
            if (cone == null || lamp == null || signal == null || sign == null || treeA == null || treeB == null) { Debug.LogWarning("SORA CITY: street props missing; skipped."); return 0; }
            Transform root = new GameObject("Street props - Kenney Roads CC0 lamps and signals").transform;
            root.SetParent(city, false);
            int n = 0;
            foreach (Spot s in LampSpots()) n += Place(root, lamp, s, Vector3.back, KenneyScale);
            foreach (Spot s in SignalSpots()) n += Place(root, signal, s, Vector3.left, KenneyScale);
            foreach (Spot s in SignSpots()) n += Place(root, sign, s, Vector3.left, KenneyScale);
            foreach (Spot s in ConeSpots()) n += Place(root, cone, s, Vector3.forward, KenneyScale);
            int i = 0;
            foreach (Spot s in TreeSpots()) n += Place(root, i++ % 3 == 0 ? treeB : treeA, s, Vector3.forward, TreeScale);
            return n;
        }

        // `armLocal` = the model's local direction of the arm (lamp -z, signal -x).
        private static int Place(Transform root, GameObject prefab, Spot s, Vector3 armLocal, float scale)
        {
            var at = new Vector3(-s.Xz.x, 0f, s.Xz.y);                 // AuthorToUnity
            if (Vector2.Distance(new Vector2(at.x, at.z), MartXz) < MartClearRadius) return 0;
            var dir = new Vector3(-s.Dir.x, 0f, s.Dir.y);
            GameObject go = Object.Instantiate(prefab, root, false);
            go.transform.localPosition = at;
            go.transform.localRotation = Quaternion.FromToRotation(armLocal, dir);
            go.transform.localScale = Vector3.one * scale;
            foreach (Collider c in go.GetComponentsInChildren<Collider>(true)) Object.Destroy(c);
            return 1;
        }
    }
}
