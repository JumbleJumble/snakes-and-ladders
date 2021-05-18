using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class SnakeGenerator : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private bool dirty;

    const int numSections = 16;
    const float sectionLength = 0.1f;
    private const float tailLength = 0.15f;
    const int numSides = 32;
    const float radius = 0.1f;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }

    private void OnEnable()
    {
        dirty = true;
    }

    private void GenerateSnake()
    {
        var mesh = new Mesh() { name = "snake mesh" };
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        var tx = transform;
        vertices.Add(tx.position);
        normals.Add(-tx.up);
        for (int section = 0; section <= numSections; section++)
        {
            var centre = tx.position + tx.up * (sectionLength * section + tailLength);
            if (section > 0)
            {
                for (int side = 0; side < numSides; side++)
                {
                    var q = Quaternion.AngleAxis(side * 360f / numSides, tx.up);
                    var vertex = centre + q * tx.forward * radius;
                    vertices.Add(vertex);
                    var normal = -(centre - vertex).normalized / 3f;
                    normals.Add(normal);

                    int sideBase = (section - 1) * numSides + 1;
                    int vertexIdx = vertices.Count - 1;
                    if (section == 1)
                    {
                        AddTriangle(triangles, vertexIdx, 0, vertexIdx % numSides + 1);
                    }
                    else
                    {
                        int lowerSideBase = (section - 2) * numSides + 1;
                        var quad = new[]
                        {
                            sideBase + side, sideBase + (side + 1) % numSides,
                            lowerSideBase + (side + 1) % numSides, lowerSideBase + side
                        };

                        AddTriangle(triangles, quad[0], quad[2], quad[1]);
                        AddTriangle(triangles, quad[0], quad[3], quad[2]);
                    }
                }
            }
        }

        Assert.AreEqual(vertices.Count, normals.Count);
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();
        meshFilter.mesh = mesh;
    }

    private static void AddTriangle(List<int> triangles, int point1, int point2, int point3)
    {
        triangles.Add(point1);
        triangles.Add(point2);
        triangles.Add(point3);
    }

    // Start is called before the first frame update
    private void Start()
    {
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
