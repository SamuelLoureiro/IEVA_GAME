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
            var mesh = new Mesh { name = "Pyramid" };

            var vertices = new Vector3[]
            {
                // Base
                new(-0.5f, 0, -0.5f),
                new(0.5f, 0, -0.5f),
                new(0.5f, 0, 0.5f),
                new(-0.5f, 0, 0.5f),
                // Apex
                new(0, 1f, 0)
            };

            var triangles = new int[]
            {
                // Base (two triangles)
                0, 2, 1,
                0, 3, 2,
                // Front
                0, 1, 4,
                // Right
                1, 2, 4,
                // Back
                2, 3, 4,
                // Left
                3, 0, 4
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            const string path = "Assets/_Project/Meshes/Pyramid.asset";
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();

            Debug.Log($"[PyramidMeshGenerator] Pyramid mesh generated at {path}");
        }
    }
}
#endif
