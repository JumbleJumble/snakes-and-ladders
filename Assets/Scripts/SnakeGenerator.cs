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

        [Min(0)]
        public float totalLength = 1;

        [Range(1, 48)]
        public int sectionResolution = 16;

        [Range(3, 48)]
        public int numSides = 24;

        [Min(0)]
        public float bodyRadius = 0.5f;

        [Range(1, 36)]
        public int tailSections = 10;

        [Min(0)]
        public float tailLength = 0.2f;

        [Range(0, 5f)]
        public float tailThicknessExponent = 0.8f;

        [Min(1)]
        public float tailRadialsBias = 2;

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
            var (tail, tailEnd) = CreateTail(Vector3.zero);
            var (body, _) = CreateBody(tailEnd);
            var (vtc, nrm, tri, uvs) = tail.Append(body);
            var msh = new Mesh { name = "snake mesh", vertices = vtc, normals = nrm, triangles = tri, uv = uvs};
            meshFilter.mesh = msh;
        }

        private float TailCurveFunc(float tailPos) =>
            Mathf.Sqrt(1 - Mathf.Pow(1 - tailPos, tailThicknessExponent * 2));

        private Vector2 GetUVForPosAndAngle(float pos, float angle)
        {
            return new Vector2(
                1 -(angle + 180 * Mathf.Sign(180 - angle)) / 360f,
                pos
            );
        }

        private (MeshSection tailMesh, Vector3 continuationPoint) CreateTail(Vector3 startPoint)
        {
            // first point is the very center of the end of the tail
            var tail = new MeshSection();
            tail.AddVertex(startPoint, Vector3.back, new Vector2(0.5f, 0));

            var tailCap = GetVerticesAroundCenter(startPoint, 0.0001f, false);
            for (var i = 0; i < tailCap.Length; i++)
            {
                var (vertex, angle) = tailCap[i];
                tail.AddVertex(vertex, Vector3.back, GetUVForPosAndAngle(0, angle));
                tail.AddTriangle(i + 1, 0, (i + 1) % numSides + 1);
            }

            bool twist = true;
            for (var s = 0; s < tailSections; s++)
            {
                var endOfTail = tailLength / totalLength;
                float secPos = (float) (s + 1) / tailSections;
                var currentPos = Mathf.Pow(secPos, tailRadialsBias);
                var uvPos = currentPos * endOfTail;

                var v = tail.NextVertexIndex;
                int ThisRow(int n) => v + n % numSides;
                int PrevRow(int n) => v - numSides + (n + numSides) % numSides;

                var tailPoints = GetTailVerticesAtPos(currentPos, twist, startPoint);

                for (var i = 0; i < tailPoints.Length; i++)
                {
                    var a = ThisRow(i);
                    var b = ThisRow(i + 1);
                    var t = PrevRow(i - (twist ? 0 : 1));
                    var u = PrevRow(i + (twist ? 1 : 0));

                    var (vertex, angle) = tailPoints[i];
                    var side1 = vertex - tail.Vertices[t];
                    var side2 = vertex - tail.Vertices[u];
                    var normal = Vector3.Cross(side1, side2);
                    
                    tail.AddVertex(vertex, normal, GetUVForPosAndAngle(uvPos, angle));
                    tail.AddTriangle(a, t, u);
                    tail.AddTriangle(a, u, b);
                }

                twist = !twist;
            }

            var tailCore = startPoint + Vector3.forward * tailLength;
            return (tail, tailCore);
        }

        private (MeshSection bodyMesh, Vector3 continuationPoint) CreateBody(Vector3 startPoint)
        {
            var sectionLength = 1f / sectionResolution;
            var bodyLength = totalLength - tailLength;
            var numSections = bodyLength / sectionLength;
            var body = new MeshSection();
            var extraSection = numSections - Mathf.Floor(numSections) > 0.01;

            Vector3 spine = startPoint;
            bool twist = tailSections % 2 == 0;
            for (int s = 1; s < numSections; s++)
            {
                spine = startPoint + s * sectionLength * Vector3.forward;
                var overallPos = (tailLength + s * sectionLength) / totalLength;
                
                var vertices = GetVerticesAroundCenter(spine, bodyRadius, twist);
                var v = body.NextVertexIndex;
                int ThisRow(int n) => v + n % numSides;
                int PrevRow(int n) => v - numSides + (n + numSides) % numSides;
                for (var i = 0; i < vertices.Length; i++)
                {
                    var (vertex, angle) = vertices[i];
                    var normal = vertex - spine;

                    if (s != 0)
                    {
                        body.AddVertex(vertex, normal, GetUVForPosAndAngle(overallPos, angle));
                    }

                    var a = ThisRow(i);
                    var b = ThisRow(i + 1);
                    var t = PrevRow(i - (twist ? 0 : 1));
                    var u = PrevRow(i + (twist ? 1 : 0));
                    body.AddTriangle(a, t, u);
                    body.AddTriangle(a, u, b);
                }

                twist = !twist;
            }

            return (body, spine);
        }

        private (Vector3 vertex, float angle)[] GetTailVerticesAtPos(float sectionPos, bool twist, Vector3 startPoint)
        {
            var posRadius = TailCurveFunc(sectionPos) * bodyRadius;
            var center = startPoint + sectionPos * tailLength * Vector3.forward;
            return GetVerticesAroundCenter(center, posRadius, twist);
        }

        private (Vector3 vertex, float angle)[] GetVerticesAroundCenter(Vector3 center, float radius, bool twist)
        {
            var segmentAngle = 360f / numSides;
            var angleOffset = twist ? segmentAngle / 2f : 0;
            var result = new (Vector3 vertex, float angle)[numSides];
            for (int i = 0; i < numSides; i++)
            {
                var angle = i * segmentAngle + angleOffset;
                result[i].vertex = GetVertexAtAngle(center, angle, radius);
                result[i].angle = angle;
            }

            return result;
        }

        private Vector3 GetVertexAtAngle(Vector3 center, float angle, float radius)
        {
            var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            return center + rotation * (Vector3.up * radius);
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
