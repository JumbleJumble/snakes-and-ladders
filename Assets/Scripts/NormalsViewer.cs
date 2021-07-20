using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class NormalsViewer : MonoBehaviour
{
    private MeshFilter meshFilter;
    
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        var mesh = meshFilter.sharedMesh;
        var vertNorms = mesh.vertices.Zip(mesh.normals, (v, n) => (vert: v, normal: n));
        foreach (var (vert, normal) in vertNorms)
        {
            vert.Scale(transform.localScale);
            Debug.DrawRay(vert + transform.position, normal.normalized / 20, Color.cyan);
        }
    }
}
