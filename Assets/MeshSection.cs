using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SnakesAndLadders
{
    public class MeshSection
    {
        private readonly List<Vector3> normals = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        public List<Vector3> Vertices { get; } = new List<Vector3>();

        /// <summary>
        /// Adds a vertex to the section with its normal.
        /// </summary>
        /// <returns>The index of the just-added vertex.</returns>
        public int AddVertex(Vector3 vertex, Vector3 normal, Vector2 uv)
        {
            Vertices.Add(vertex);
            normals.Add(normal);
            uvs.Add(uv);
            return Vertices.Count - 1;
        }

        public void AddTriangle(int a, int b, int c)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        public int NextVertexIndex => Vertices.Count;

        public MeshSection Append(MeshSection next)
        {
            var result = new MeshSection();
            result.Vertices.AddRange(Vertices);
            result.normals.AddRange(normals);
            result.uvs.AddRange(uvs);
            result.triangles.AddRange(triangles);
            
            result.Vertices.AddRange(next.Vertices);
            result.normals.AddRange(next.normals);
            result.uvs.AddRange(next.uvs);
            result.triangles.AddRange(next.triangles.Select(i => i + Vertices.Count));

            return result;
        }

        public void Deconstruct(out Vector3[] outVertices, out Vector3[] outNormals, out int[] outTriangles, out Vector2[] outUvs)
        {
            outVertices = Vertices.ToArray();
            outNormals = normals.ToArray();
            outTriangles = triangles.ToArray();
            outUvs = uvs.ToArray();
        }
    }
}
