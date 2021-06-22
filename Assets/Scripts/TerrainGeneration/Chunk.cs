using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public Vector3 pos;
    public Chunk(Vector3 _pos)
    {
        pos = _pos;
    }
    public void Generate()
    {
        Marching m = new Marching(pos);
        m.CreateMeshData();
        vertices = m.vertices;
        triangles = m.triangles;
    }
}
