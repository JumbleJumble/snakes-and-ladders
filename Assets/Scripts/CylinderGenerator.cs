using UnityEngine;

namespace SnakesAndLadders
{
    public class CylinderGenerator
    {
        private readonly Vector3 startPoint;
        private readonly float length;
        private readonly int numSides;
        private readonly int numSegments;
        private readonly float radius;

        public CylinderGenerator(Vector3 startPoint, float length, int numSides, int numSegments, float radius)
        {
            this.startPoint = startPoint;
            this.length = length;
            this.numSides = numSides;
            this.numSegments = numSegments;
            this.radius = radius;
        }

        protected virtual float GetRadius(float position) => radius;

        protected virtual float GetYRotation(float position) => 0;

        protected virtual Vector3 GetXYZForPos(float position) => startPoint + position * Vector3.forward;

        protected (float angle, Vector3 vertex)[] GetVerticesAroundCenter(
            Vector3 center,
            float yRotation,
            float segmentRadius)
        {
            var result = new (float angle, Vector3 vertex)[numSides + 1];
            float degreesPerSide = 360f / numSides;
            var axis = Quaternion.AngleAxis(yRotation, Vector3.up) * Vector3.forward;
            for (int i = 0; i <= numSides; i++)
            {
                var angle = -180 + degreesPerSide * i;
                var rotation = Quaternion.AngleAxis(angle, axis);
                result[i] = (angle, center + rotation * (Vector3.up * segmentRadius));
            }

            return result;
        }

        private Vector2 GetUVForPosAndAngle(float pos, float angle)
        {
            var u = Mathf.InverseLerp(-180, 180, angle);
            return new Vector2(u, pos);
        }

        protected virtual void StartSection(MeshSection section)
        {
            var verticesAroundCenter = GetVerticesAroundCenter(startPoint, GetYRotation(0), GetRadius(0));
            foreach (var v in verticesAroundCenter)
            {
                section.AddVertex(v.vertex, v.vertex, GetUVForPosAndAngle(0, v.angle));
            }
        }

        public MeshSection CreateSection()
        {
            var section = new MeshSection();

            // first make points around the start point
            StartSection(section);
            int vertsPerSide = numSides + 1;
            float segmentLength = length / numSegments;
            for (int i = 1; i <= numSegments; i++)
            {
                int vertStart = section.NextVertexIndex;
                int vertIndex = vertStart;
                float distanceFromStart = i * segmentLength;
                float pos = distanceFromStart / length;
                Vector3 center = startPoint + distanceFromStart * Vector3.forward;
                var vertInfos = GetVerticesAroundCenter(center, GetYRotation(pos), GetRadius(pos));

                // add the triangles
                for (var side = 0; side < vertsPerSide; side++)
                {
                    var (angle, vertex) = vertInfos[side];
                    section.AddVertex(vertex, GetUVForPosAndAngle(pos, angle));

                    if (side < numSides)
                    {
                        section.AddTriangle(vertIndex, vertIndex - vertsPerSide, vertIndex - vertsPerSide + 1);
                        section.AddTriangle(vertIndex - vertsPerSide + 1, vertIndex + 1, vertIndex);
                    }

                    vertIndex++;
                }

                // go back around and add the normals
                vertIndex = vertStart;
                for (var side = 0; side < vertsPerSide; side++)
                {
                    if (side == numSides)
                    {
                        section.AddNormal(section.Normals[vertIndex - numSides]);
                        continue;
                    }

                    var (_, vertex) = vertInfos[side];
                    var prevRow = vertIndex - vertsPerSide;
                    var side1 = section.Vertices[prevRow] - vertex;

                    var side2 = side == 0
                        ? section.Vertices[vertIndex - 2] - vertex
                        : section.Vertices[prevRow - 1] - vertex;

                    var side3 = side == numSides
                        ? section.Vertices[vertIndex - numSides] - vertex
                        : section.Vertices[prevRow + 1] - vertex;

                    var cross1 = Vector3.Cross(side1, side2).normalized;
                    var cross2 = Vector3.Cross(side3, side1).normalized;
                    var normal = -1 * (cross1 + cross2);
                    section.AddNormal(normal);
                    vertIndex++;
                }
            }

            return section;
        }
    }

    public class TailGenerator : CylinderGenerator
    {
        public TailGenerator(Vector3 startPoint, float length, int numSides, int numSegments, float radius) 
            : base(startPoint, length, numSides, numSegments, radius)
        {
        }
    }
}


