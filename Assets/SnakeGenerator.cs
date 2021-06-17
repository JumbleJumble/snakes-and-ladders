using System.Collections.Generic;
using UnityEngine;

namespace SnakesAndLadders
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class SnakeGenerator : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private bool dirty;

        public int numSections = 16;
        public float sectionLength = 0.1f;

        [Range(3, 48)]
        public int numSides = 3;

        [Min(0)]
        public float bodyRadius = 0.5f;

        [Range(1, 36)]
        public int tailSections = 10;

        [Min(0)]
        public float tailLength = 0.2f;

        [Range(0, 5f)]
        public float tailCurvePower = 0.8f;

        [Min(1)]
        public float tailCurveBias = 2;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        private void OnEnable()
        {
            dirty = true;
        }

        private void GenerateSnake()
        {
            var (tail, _) = CreateTail(Vector3.zero);
            var (vtc, nrm, tri) = tail;
            var msh = new Mesh { name = "snake mesh", vertices = vtc, normals = nrm, triangles = tri };
            meshFilter.mesh = msh;
            return;

            //
            // var mesh = new Mesh() { name = "snake mesh" };
            // var vertices = new List<Vector3>();
            // var normals = new List<Vector3>();
            // var triangles = new List<int>();
            // vertices.Add(Vector3.zero);
            // normals.Add(Vector3.back);
            // for (int section = 0; section <= numSections; section++)
            // {
            //     var centre = Vector3.forward * (sectionLength * section + tailLength);
            //     if (section > 0)
            //     {
            //         for (int side = 0; side < numSides; side++)
            //         {
            //             var q = Quaternion.AngleAxis(side * 360f / numSides, Vector3.forward);
            //             var vertex = centre + q * Vector3.up * bodyRadius;
            //             vertices.Add(vertex);
            //             var normal = -(centre - vertex).normalized / 3f;
            //             normals.Add(normal);
            //
            //             int sideBase = (section - 1) * numSides + 1;
            //             int vertexIdx = vertices.Count - 1;
            //             if (section == 1)
            //             {
            //                 AddTriangle(triangles, vertexIdx, 0, vertexIdx % numSides + 1);
            //             }
            //             else
            //             {
            //                 int lowerSideBase = (section - 2) * numSides + 1;
            //                 var quad = new[]
            //                 {
            //                     sideBase + side, sideBase + (side + 1) % numSides,
            //                     lowerSideBase + (side + 1) % numSides, lowerSideBase + side
            //                 };
            //
            //                 AddTriangle(triangles, quad[0], quad[2], quad[1]);
            //                 AddTriangle(triangles, quad[0], quad[3], quad[2]);
            //             }
            //         }
            //     }
            // }
            //
            // Assert.AreEqual(vertices.Count, normals.Count);
            // mesh.vertices = vertices.ToArray();
            // mesh.normals = normals.ToArray();
            // mesh.triangles = triangles.ToArray();
            // meshFilter.mesh = mesh;
        }

        private static void AddTriangle(List<int> triangles, int point1, int point2, int point3)
        {
            triangles.Add(point1);
            triangles.Add(point2);
            triangles.Add(point3);
        }

        // SQRT(1-((1-A2)^$E$1)^2)
        private float TailCurveFunc(float tailPos) =>
            Mathf.Sqrt(1 - Mathf.Pow(Mathf.Pow(1 - tailPos, tailCurvePower), 2));

        private (MeshSection tailMesh, Vector3 continuationPoint) CreateTail(Vector3 startPoint)
        {
            var tailCore = startPoint + Vector3.forward * tailLength;

            // first point is the very center of the end of the tail
            var tail = new MeshSection();
            tail.AddVertex(startPoint, Vector3.back);

            var capRadius = TailCurveFunc(Mathf.Pow(0.0001f / tailSections, tailCurveBias)) * bodyRadius;
            var tailCap = GetVerticesAroundCenter(startPoint, capRadius, false);
            for (var i = 0; i < tailCap.Length; i++)
            {
                tail.AddVertex(tailCap[i], tailCap[i] - tailCore);
                tail.AddTriangle(i + 1, 0, (i + 1) % numSides + 1);
            }

            bool twist = true;
            for (var s = 0; s < tailSections; s++)
            {
                float secPos = (float) (s + 1) / tailSections;
                var currentPos = Mathf.Pow(secPos, tailCurveBias);

                var v = tail.NextVertexIndex;
                int ThisRow(int n) => v + n % numSides;
                int PrevRow(int n) => v - numSides + (n + numSides) % numSides;

                var tailPoints = GetVerticesAtPos(currentPos, twist, startPoint);
                for (var i = 0; i < tailPoints.Length; i++)
                {
                    var adjacentVertex = tailPoints[(i - 1 + numSides) % numSides];
                    var normal = Vector3.Cross(tailPoints[i], adjacentVertex);
                    tail.AddVertex(tailPoints[i], normal);

                    var a = ThisRow(i);
                    var b = ThisRow(i + 1);
                    var t = PrevRow(i - (twist ? 0 : 1));
                    var u = PrevRow(i + (twist ? 1 : 0));
                    tail.AddTriangle(a, t, u);
                    tail.AddTriangle(a, u, b);
                }

                twist = !twist;
            }

            return (tail, tailCore);
        }

        private Vector3[] GetVerticesAtPos(float sectionPos, bool twist, Vector3 startPoint)
        {
            var posRadius = TailCurveFunc(sectionPos) * bodyRadius;
            var center = startPoint + sectionPos * tailLength * Vector3.forward;
            Vector3[] result = GetVerticesAroundCenter(center, posRadius, twist);
            return result;
        }

        private Vector3[] GetVerticesAroundCenter(Vector3 center, float radius, bool twist)
        {
            var first = Vector3.up * radius;
            var segmentAngle = 360f / numSides;
            var angleOffset = twist ? segmentAngle / 2f : 0;
            var result = new Vector3[numSides];
            for (int i = 0; i < numSides; i++)
            {
                var angle = i * segmentAngle + angleOffset;
                var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                result[i] = center + rotation * first;
            }

            return result;
        }

        private void OnValidate()
        {
            dirty = true;
        }

        // Update is called once per frame
        private void Update()
        {
            if (dirty)
            {
                GenerateSnake();
            }
        }
    }
}
