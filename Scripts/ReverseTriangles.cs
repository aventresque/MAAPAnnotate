using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Flip all triangles, thus reversing all normals of a specific mesh
public class ReverseTriangles : MonoBehaviour {

    private MeshFilter meshFilter;

	// Use this for initialization
	void Start ()
    {
        this.GetComponent<MeshRenderer>().enabled = true;
        //reverse the triangles
        meshFilter = this.GetComponent<MeshFilter>();
        int[] tris = meshFilter.mesh.triangles;
        //reverse triangle winding order
        for (int ind = 0; ind < tris.Length / 3.0f; ind++)
        {
            int tmp = tris[ind * 3 + 0];
            tris[ind * 3 + 0] = tris[ind * 3 + 1];
            tris[ind * 3 + 1] = tmp;
        }
        //apply changes
        meshFilter.mesh.triangles = tris;
        meshFilter.mesh.RecalculateNormals();
        //apply a collider so that the background collides too
        this.gameObject.AddComponent<MeshCollider>();
    }
}
