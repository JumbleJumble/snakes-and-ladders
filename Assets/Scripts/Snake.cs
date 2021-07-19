using UnityEngine;

namespace SnakesAndLadders
{
    public class Snake : MeshGenerator
    {
        public int numSides = 4;
        public int numSegments = 4;

        public AnimationCurve tailWidthCurve = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve tailHeightCurve = AnimationCurve.Linear(0, 1, 1, 0);

        public override Mesh GenerateMesh()
        {
            if (numSides == 0)
            {
                return null; 
            }

            var bodyGenerator = new CylinderGenerator(Vector3.zero, 1, numSides, numSegments, 0.5f);
            var bodySection = bodyGenerator.CreateSection();
            return bodySection.CreateMesh("Snake");
        }
    }
}
