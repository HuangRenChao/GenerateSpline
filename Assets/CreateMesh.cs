using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMesh : MonoBehaviour {

    Vector3[] newVertices;
    Vector2[] newUV;
    int[] newTriangles;

    Mesh mesh;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void GenerateMesh() {
        mesh.Clear();

        // Do some calculations...
        //顶点
        mesh.vertices = newVertices;
        //UV
        mesh.uv = newUV;
        //三角面
        mesh.triangles = newTriangles;
    }
}
