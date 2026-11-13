#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sora.AfterTheRain.Demo.Editor
{
    /// <summary>Original eight-panel umbrella. Unity owns mesh, material and prefab serialization.</summary>
    public static class SoraUmbrellaBuilder
    {
        const string Folder = "Assets/SORA/Resources/Weather";

        public static void Build()
        {
            var cloth = Material("Umbrella canopy", new Color(.57f, .78f, .84f, .58f), true);
            var metal = Material("Umbrella frame", new Color(.64f, .69f, .73f), false);
            var grip = Material("Umbrella grip", new Color(.13f, .19f, .22f), false);
            var vertices = new List<Vector3>();
            var opened = new List<Vector3>();
            var triangles = new List<int>();
            var ribs = new List<int>();
            const int segments = 32, rings = 5;
            for (int ring = 0; ring < rings; ring++)
                for (int segment = 0; segment <= segments; segment++)
                {
                    float u = ring / (float)(rings - 1), angle = segment * Mathf.PI * 2 / segments;
                    var radial = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                    vertices.Add(radial * (.008f + .028f * u) + Vector3.up * (.84f - .72f * u));
                    opened.Add(radial * (.008f + .69f * u) + Vector3.up * (.84f - .2f * u * u));
                    if (ring == 0 || segment == 0) continue;
                    int i = vertices.Count - 1, stride = segments + 1;
                    triangles.AddRange(new[] { i - stride - 1, i - stride, i, i - stride - 1, i, i - 1 });
                }
            // Ribs deform with the same native blend shape as the fabric, so no runtime bone scaffold is needed.
            for (int rib = 0; rib < 8; rib++)
                for (int ring = 0; ring < rings; ring++)
                {
                    int center = ring * (segments + 1) + rib * 4;
                    Vector3 tangent = Vector3.Cross(Vector3.up, opened[center]).normalized * .003f;
                    int first = vertices.Count;
                    vertices.Add(vertices[center] - tangent); vertices.Add(vertices[center] + tangent);
                    opened.Add(opened[center] - tangent + Vector3.up * .002f);
                    opened.Add(opened[center] + tangent + Vector3.up * .002f);
                    if (ring > 0) ribs.AddRange(new[] { first - 2, first, first + 1, first - 2, first + 1, first - 1 });
                }
            var generated = new Mesh { name = "SORA umbrella canopy" };
            generated.SetVertices(vertices);
            generated.subMeshCount = 2;
            generated.SetTriangles(triangles, 0); generated.SetTriangles(ribs, 1);
            generated.RecalculateNormals(); generated.RecalculateBounds();
            var deltas = new Vector3[vertices.Count];
            for (int i = 0; i < deltas.Length; i++) deltas[i] = opened[i] - vertices[i];
            var closedNormals = generated.normals;
            var openMesh = Object.Instantiate(generated);
            openMesh.SetVertices(opened); openMesh.RecalculateNormals();
            var normalDeltas = openMesh.normals;
            for (int i = 0; i < normalDeltas.Length; i++) normalDeltas[i] -= closedNormals[i];
            Object.DestroyImmediate(openMesh);
            generated.AddBlendShapeFrame("Open", 100, deltas, normalDeltas, new Vector3[deltas.Length]);
            string meshPath = Folder + "/Umbrella canopy.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null) { mesh = generated; AssetDatabase.CreateAsset(mesh, meshPath); }
            else { EditorUtility.CopySerialized(generated, mesh); Object.DestroyImmediate(generated); }

            var root = new GameObject("SORA Umbrella");
            try
            {
                var canopy = new GameObject("Canopy"); canopy.transform.SetParent(root.transform, false);
                var skin = canopy.AddComponent<SkinnedMeshRenderer>();
                skin.sharedMesh = mesh;
                skin.sharedMaterials = new[] { cloth, metal };
                skin.localBounds = new Bounds(new Vector3(0, .48f, 0), new Vector3(1.5f, .9f, 1.5f));
                skin.updateWhenOffscreen = true;
                skin.shadowCastingMode = ShadowCastingMode.Off;
                Cylinder("Shaft", root.transform, new Vector3(0, .42f, 0), new Vector3(.009f, .42f, .009f), metal);
                Cylinder("Handle", root.transform, new Vector3(0, -.04f, 0), new Vector3(.028f, .07f, .028f), grip);
                Cylinder("Tip", root.transform, new Vector3(0, .865f, 0), new Vector3(.015f, .035f, .015f), metal);
                PrefabUtility.SaveAsPrefabAsset(root, Folder + "/SoraUmbrella.prefab");
            }
            finally { Object.DestroyImmediate(root); }
            AssetDatabase.SaveAssets();
        }

        static Material Material(string name, Color color, bool transparent)
        {
            string path = Folder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", .65f);
            material.SetFloat("_Cull", (float)CullMode.Off);
            if (transparent)
            {
                material.SetFloat("_Surface", 1); material.SetFloat("_ZWrite", 0);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        static void Cylinder(string name, Transform parent, Vector3 at, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            part.name = name; part.transform.SetParent(parent, false);
            part.transform.localPosition = at; part.transform.localScale = scale;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
#endif
