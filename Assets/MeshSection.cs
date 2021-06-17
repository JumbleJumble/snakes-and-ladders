using System.Collections.Generic;
using UnityEngine;

namespace SnakesAndLadders
{
    public class MeshSection
    {
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Vector3> normals = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();

        /// <summary>
        /// Adds a vertex to the section with its normal.
        /// </summary>
        /// <returns>The index of the just-added vertex.</returns>
        public int AddVertex(Vector3 vertex, Vector3 normal)
        {
            vertices.Add(vertex);
            normals.Add(normal);
            return vertices.Count - 1;
        }

        public void AddTriangle(int a, int b, int c)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        public int NextVertexIndex => vertices.Count;

        public void Deconstruct(out Vector3[] outVertices, out Vector3[] outNormals, out int[] outTriangles)
        {
            outVertices = vertices.ToArray();
            outNormals = normals.ToArray();
            outTriangles = triangles.ToArray();
        }
    }
}
