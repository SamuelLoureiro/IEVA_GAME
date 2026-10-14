#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VARG.Editor
{
    public static class PyramidMeshGenerator
    {
        [MenuItem("VARG/Generate Pyramid Mesh")]
        public static void GeneratePyramid()
        {
            var top = new Vector3(0f, 0.5f, 0f);
            var b0 = new Vector3(-0.5f, -0.5f, -0.5f); // Front-Left
            var b1 = new Vector3( 0.5f, -0.5f, -0.5f); // Front-Right
            var b2 = new Vector3( 0.5f, -0.5f,  0.5f); // Back-Right
            var b3 = new Vector3(-0.5f, -0.5f,  0.5f); // Back-Left

            // Flat-shaded: each face has distinct vertices so normals are sharp and outward-facing
            var vertices = new Vector3[]
            {
                // Front
                b0, top, b1,
                // Right
                b1, top, b2,
                // Back
                b2, top, b3,
                // Left
                b3, top, b0,
                // Base (two triangles)
                b0, b1, b2,
                b0, b2, b3
            };

            var triangles = new int[]
            {
                0, 1, 2,     // Front
                3, 4, 5,     // Right
                6, 7, 8,     // Back
                9, 10, 11,   // Left
                12, 13, 14,  // Base 1
                15, 16, 17   // Base 2
            };

            var uvs = new Vector2[]
            {
                // Front
                new(0f, 0f), new(0.5f, 1f), new(1f, 0f),
                // Right
                new(0f, 0f), new(0.5f, 1f), new(1f, 0f),
                // Back
                new(0f, 0f), new(0.5f, 1f), new(1f, 0f),
                // Left
                new(0f, 0f), new(0.5f, 1f), new(1f, 0f),
                // Base
                new(0f, 0f), new(1f, 0f), new(1f, 1f),
                new(0f, 0f), new(1f, 1f), new(0f, 1f)
            };

            const string path = "Assets/_Project/Meshes/Pyramid.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                existing.Clear();
                existing.vertices = vertices;
                existing.triangles = triangles;
                existing.uv = uvs;
                existing.RecalculateNormals();
                existing.RecalculateTangents();
                existing.RecalculateBounds();
                EditorUtility.SetDirty(existing);
            }
            else
            {
                var mesh = new Mesh
                {
                    name = "Pyramid",
                    vertices = vertices,
                    triangles = triangles,
                    uv = uvs
                };
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
                mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh, path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[PyramidMeshGenerator] Pyramid mesh generated with corrected outward normals at {path}");
        }
    }
}
#endif
