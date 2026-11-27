using System.Collections.Generic;
using UnityEngine;

namespace SinsOfTheSpire
{
    /// <summary>Procedural primitives (cones, petals) and small object-building helpers.</summary>
    public static class MeshKit
    {
        private static readonly Dictionary<string, Mesh> Cache = new();

        /// <summary>Truncated cone whose pivot sits at the base centre, growing along +Y.</summary>
        public static Mesh Cone(float bottomRadius, float topRadius, float height, int segments = 14)
        {
            string key = $"cone:{bottomRadius:0.000}:{topRadius:0.000}:{height:0.000}:{segments}";
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = t * Mathf.PI * 2f;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                verts.Add(new Vector3(cos * bottomRadius, 0f, sin * bottomRadius));
                uvs.Add(new Vector2(t, 0f));
                verts.Add(new Vector3(cos * topRadius, height, sin * topRadius));
                uvs.Add(new Vector2(t, 1f));
            }

            const int ringStride = 2;
            for (int i = 0; i < segments; i++)
            {
                int b0 = i * ringStride;
                int t0 = b0 + 1;
                int b1 = b0 + ringStride;
                int t1 = b1 + 1;
                tris.Add(b0); tris.Add(t0); tris.Add(t1);
                tris.Add(b0); tris.Add(t1); tris.Add(b1);
            }

            AddCap(verts, uvs, tris, topRadius, height, segments, true);
            AddCap(verts, uvs, tris, bottomRadius, 0f, segments, false);

            var mesh = new Mesh { name = "Cone" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Cache[key] = mesh;
            return mesh;
        }

        private static void AddCap(List<Vector3> verts, List<Vector2> uvs, List<int> tris, float radius, float y, int segments, bool up)
        {
            if (radius <= 0.0001f) return;

            int centre = verts.Count;
            verts.Add(new Vector3(0f, y, 0f));
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int i = 0; i <= segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                verts.Add(new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius));
                uvs.Add(new Vector2(Mathf.Cos(angle) * 0.5f + 0.5f, Mathf.Sin(angle) * 0.5f + 0.5f));
            }

            for (int i = 0; i < segments; i++)
            {
                if (up)
                {
                    tris.Add(centre);
                    tris.Add(centre + 1 + i + 1);
                    tris.Add(centre + 1 + i);
                }
                else
                {
                    tris.Add(centre);
                    tris.Add(centre + 1 + i);
                    tris.Add(centre + 1 + i + 1);
                }
            }
        }

        // --- object helpers ---------------------------------------------------

        public static GameObject Part(Transform parent, PrimitiveType type, string partName, Vector3 localPos,
            Vector3 localScale, Material material, Vector3 euler = default)
        {
            var go = new GameObject(partName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = localScale;
            go.AddComponent<MeshFilter>().sharedMesh = PrimitiveMesh(type);
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        private static readonly Dictionary<PrimitiveType, Mesh> PrimitiveMeshes = new();

        private static Mesh PrimitiveMesh(PrimitiveType type)
        {
            if (PrimitiveMeshes.TryGetValue(type, out var mesh) && mesh != null) return mesh;

            var temp = GameObject.CreatePrimitive(type);
            mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            if (Application.isPlaying) Object.Destroy(temp);
            else Object.DestroyImmediate(temp);
            PrimitiveMeshes[type] = mesh;
            return mesh;
        }

        public static GameObject MeshPart(Transform parent, Mesh mesh, string partName, Vector3 localPos,
            Vector3 localScale, Material material, Vector3 euler = default)
        {
            var go = new GameObject(partName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = localScale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        public static Transform Pivot(Transform parent, string pivotName, Vector3 localPos)
        {
            var go = new GameObject(pivotName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go.transform;
        }
    }
}
