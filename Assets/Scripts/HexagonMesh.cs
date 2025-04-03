using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexagonMesh : MonoBehaviour
{
    void Start()
    {
        MakeMeshData();
    }
    void MakeMeshData()
    {
        float dia = 1f;
        var vertices = new Vector3[]
        {
            new Vector3(dia / 2, 0, dia / 2),
            new Vector3(dia / 2, 0, 1),
            new Vector3(1, 0, 3 * (dia / 4)),
            new Vector3(1, 0, dia / 4),
            new Vector3(dia / 2, 0, 0),
            new Vector3(0, 0, dia / 4),
            new Vector3(0, 0, 3 * (dia / 4)),
        };

        var triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 6,
            0, 6, 1
        };
        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}