using UnityEngine;

namespace SnakesAndLadders
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public abstract class MeshGenerator : MonoBehaviour
    {
        private bool dirty;
        private MeshFilter meshFilter;

        private void OnValidate()
        {
            dirty = true;
        }

        private void OnEnable()
        {
            meshFilter = GetComponent<MeshFilter>();
            dirty = true;
        }

        // Update is called once per frame
        private void Update()
        {
            if (dirty)
            {
                RegenerateMesh();
            }
        }

        private void RegenerateMesh()
        {
            meshFilter.mesh = GenerateMesh();
        }

        public abstract Mesh GenerateMesh();
    }
}
