using UnityEngine;

namespace SnakesAndLadders
{
    public class Snake : MeshGenerator
    {
        public int numSides = 4;
        public int numSegments = 4;

        [Header("Tail")]
        public float tailWidthPower = 1;
        public float tailHeightPower = 1;
        public float tailRadialsBias = 1;
        public float tailTextureVPower = 1;

        public override Mesh GenerateMesh()
        {
            if (numSides == 0)
            {
                return null;
            }

            var bodyGenerator = new CylinderGenerator(
                Vector3.zero,
                numSides,
                numSegments,
                0.5f,
                1);

            var tailGenerator = new EndGenerator(
                Vector3.zero,
                numSides,
                numSegments,
                0.5f,
                tailTextureVPower,
                tailRadialsBias,
                tailWidthPower,
                tailHeightPower);

            var bodySection = tailGenerator.CreateSection();
            return bodySection.CreateMesh("Snake");
        }
    }
}
