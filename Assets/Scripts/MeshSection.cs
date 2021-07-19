using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SnakesAndLadders
{
    public class MeshSection
    {
        private readonly List<int> triangles = new List<int>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        
        public List<Vector3> Normals { get; } = new List<Vector3>();
        public List<Vector3> Vertices { get; } = new List<Vector3>();

        /// <summary>
        /// Adds a vertex to the section with its normal.
        /// </summary>
        /// <returns>The index of the just-added vertex.</returns>
        public int AddVertex(Vector3 vertex, Vector3 normal, Vector2 uv)
        {
            Vertices.Add(vertex);
            Normals.Add(normal);
            uvs.Add(uv);
            return Vertices.Count - 1;
        }

        public int AddVertex(Vector3 vertex, Vector2 uv)
        {
            Vertices.Add(vertex);
            uvs.Add(uv);
            return Vertices.Count - 1;
        }

        public void AddNormal(Vector3 normal)
        {
            Normals.Add(normal);
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
            result.Normals.AddRange(Normals);
            result.uvs.AddRange(uvs);
            result.triangles.AddRange(triangles);

            result.Vertices.AddRange(next.Vertices);
            result.Normals.AddRange(next.Normals);
            result.uvs.AddRange(next.uvs);
            result.triangles.AddRange(next.triangles.Select(i => i + Vertices.Count));

            return result;
        }

        public void Deconstruct(
            out Vector3[] outVertices,
            out Vector3[] outNormals,
            out int[] outTriangles,
            out Vector2[] outUvs)
        {
            outVertices = Vertices.ToArray();
            outNormals = Normals.ToArray();
            outTriangles = triangles.ToArray();
            outUvs = uvs.ToArray();
        }

        public Mesh CreateMesh(string name)
        {
            return new Mesh
            {
                name = name,
                vertices = Vertices.ToArray(),
                normals = Normals.ToArray(),
                triangles = triangles.ToArray(),
                uv = uvs.ToArray()
            };
        }
    }
}
