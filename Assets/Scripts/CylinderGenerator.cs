using UnityEngine;

namespace SnakesAndLadders
{
    public class CylinderGenerator
    {
        protected readonly Vector3 startPoint;
        protected readonly float length;
        protected readonly int numSides;
        protected readonly int numSegments;
        protected readonly float radius;

        public CylinderGenerator(Vector3 startPoint, float length, int numSides, int numSegments, float radius)
        {
            this.startPoint = startPoint;
            this.length = length;
            this.numSides = numSides;
            this.numSegments = numSegments;
            this.radius = radius;
        }

        protected virtual Vector2 GetScaleAtPos(float position) => new Vector2(1, 1);

        private Vector3 StretchVector(Vector3 vector, Vector2 scale)
        {
            return new Vector3(vector.x * scale.x, vector.y * scale.y, vector.z);
        }

        protected virtual float GetYRotation(float position) => 0;

        protected virtual Vector3 GetCenterForPos(float position) => startPoint + position * Vector3.forward;

        protected virtual float GetRadialPos(float position) => position;

        protected (float angle, Vector3 vert)[] GetVertsAroundCenter(
            Vector3 center,
            float yRotation,
            Vector2 scale)
        {
            var result = new (float angle, Vector3 vert)[numSides + 1];
            float degreesPerSide = 360f / numSides;
            var axis = Quaternion.AngleAxis(yRotation, Vector3.up) * Vector3.forward;
            for (int i = 0; i <= numSides; i++)
            {
                var angle = -180 + degreesPerSide * i;
                var rotation = Quaternion.AngleAxis(angle, axis);
                var squareOffset = rotation * (Vector3.up * radius);
                var stretched = StretchVector(squareOffset, scale);
                result[i] = (angle, center + stretched);
            }

            return result;
        }

        protected Vector2 GetUVForPosAndAngle(float pos, float angle)
        { var u = Mathf.InverseLerp(-180, 180, angle);
            return new Vector2(u, pos);
        }

        protected virtual void StartSection(MeshSection section)
        {
            var scale = GetScaleAtPos(0);
            var verticesAroundCenter = GetVertsAroundCenter(startPoint, GetYRotation(0), scale);
            foreach (var v in verticesAroundCenter)
            {
                section.AddVertex(v.vert, v.vert, GetUVForPosAndAngle(0, v.angle));
            }
        }

        public MeshSection CreateSection()
        {
            var section = new MeshSection();

            // first make points around the start point
            StartSection(section);
            int vertsPerSide = numSides + 1;
            for (int segment = 0; segment < numSegments; segment++)
            {
                int vertStart = section.NextVertexIndex;
                int vertIndex = vertStart;
                var segmentFrac = (segment + 1f) / numSegments;
                float pos = GetRadialPos(segmentFrac);
                Vector3 center = GetCenterForPos(pos);
                var scale = GetScaleAtPos(pos);
                var vertInfos = GetVertsAroundCenter(center, GetYRotation(pos), scale);

                for (var side = 0; side < vertsPerSide; side++)
                {
                    var (angle, vert) = vertInfos[side];
                    var normal = GetNormal(section, side, vertIndex, vertsPerSide, vertInfos);

                    section.AddVertex(vert, normal, GetUVForPosAndAngle(pos, angle));

                    if (side < numSides)
                    {
                        section.AddTriangle(vertIndex, vertIndex - vertsPerSide, vertIndex - vertsPerSide + 1);
                        section.AddTriangle(vertIndex - vertsPerSide + 1, vertIndex + 1, vertIndex);
                    }

                    vertIndex++;
                }
            }

            return section;
        }

        private Vector3 GetNormal(
            MeshSection section,
            int side,
            int vertIndex,
            int vertsPerSide,
            (float angle, Vector3 vert)[] vertInfos,
            bool showDebug = false)
        {
            Vector3 normal;
            var vert = vertInfos[side].vert;
            if (side == numSides)
            {
                normal = section.Normals[vertIndex - numSides];
            }
            else
            {
                var prevRow = vertIndex - vertsPerSide;
                var side1 = section.Vertices[prevRow] - vert;
                var side2 = (vertInfos[(side + numSides - 1) % numSides].vert - vert).normalized;
                var side3 = (vertInfos[(side + 1) % numSides].vert - vert).normalized;

                var cross1 = Vector3.Cross(side2, side1).normalized;
                var cross2 = Vector3.Cross(side1, side3).normalized;
                normal = cross1 + cross2;

                if (showDebug)
                {
                    Debug.DrawRay(vert, side1, Color.red);
                    Debug.DrawRay(vert, side2, Color.green);
                    Debug.DrawRay(vert, side3, Color.blue);
                    Debug.DrawRay(vert, cross1, Color.yellow);
                    Debug.DrawRay(vert, cross2, Color.magenta);
                }
            }

            return normal;
        }
    }
}
