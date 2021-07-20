using System.Linq;
using UnityEngine;

namespace SnakesAndLadders
{
    public class EndGenerator : CylinderGenerator
    {
        private readonly float widthPower;
        private readonly float heightPower;
        private readonly float radialsBias;

        public EndGenerator(
            Vector3 startPoint,
            float length,
            int numSides,
            int numSegments,
            float radius,
            float radialsBias,
            float widthPower,
            float heightPower)
            : base(startPoint, length, numSides, numSegments, radius)
        {
            this.radialsBias = radialsBias;
            this.widthPower = widthPower;
            this.heightPower = heightPower;
        }

        protected override float GetRadialPos(float position) => Mathf.Pow(position, radialsBias);

        protected override void StartSection(MeshSection section)
        {
            var verts = Enumerable.Repeat((0f, Vector3.zero), numSides + 1).ToArray(); 
            for (var i = 0; i <= numSides; i++)
            {
                var (angle, vert) = verts[i];
                section.AddVertex(vert, Vector3.back, GetUVForPosAndAngle(0.0001f, angle));
            }
        }

        protected override Vector2 GetScaleAtPos(float position)
        {
            return new Vector2(Mathf.Pow(position, 1 / widthPower), Mathf.Pow(position, 1 / heightPower));
        }
    }
}
