using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshLoader : MonoBehaviour
{
    
    MeshContainer goList = new MeshContainer();
    static ChunkManager chunkManager;
    MeshLoader()
    {

    }
    void Start()
    {
        chunkManager = new ChunkManager(Constants.chunkSize);
        for (int x = -Constants.maxChunkNum * Constants.chunkSize; x < Constants.maxChunkNum * Constants.chunkSize; x += Constants.chunkSize)
        {
            for (int z = -Constants.maxChunkNum * Constants.chunkSize; z < Constants.maxChunkNum * Constants.chunkSize; z += Constants.chunkSize)
            {
                for (int y = -Constants.maxChunkNum * Constants.chunkSize; y < Constants.maxChunkNum * Constants.chunkSize; y += Constants.chunkSize)
                {
                    GameObject go = new GameObject();
                    go.AddComponent<MeshFilter>();
                    go.AddComponent<MeshRenderer>();
                    go.GetComponent<MeshRenderer>().enabled = true;
                    go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
                    go.GetComponent<MeshFilter>().mesh = GetMesh(new Vector3(x, y, z));
                    goList.Add(new Vector3(x, y, z), go);
                }
            }
        }
    }
    void Update()
    {
        Vector3 cameraPos = GameObject.Find("PlayerCamera").transform.position;
        cameraPos = cameraPos / Constants.chunkSize;
        Vector3 chunk = new Vector3((int) Mathf.Floor(cameraPos.x)*Constants.chunkSize, (int) Mathf.Floor(cameraPos.y) * Constants.chunkSize, (int) Mathf.Floor(cameraPos.z) * Constants.chunkSize);
        //goList[chunk].GetComponent<MeshRenderer>().enabled = true;
        foreach(GameObject go in goList.Values())
        {
            go.GetComponent<MeshRenderer>().enabled = false;
        }
        for(int x = -4 * Constants.chunkSize; x < 4 * Constants.chunkSize; x += Constants.chunkSize)
        {
            for(int z = -4 * Constants.chunkSize; z < 4 * Constants.chunkSize; z+= Constants.chunkSize)
            {
                for(int y = -4 * Constants.chunkSize; y < 4 * Constants.chunkSize; y += Constants.chunkSize)
                {
                    goList.Get(new Vector3(x, y, z) + chunk).GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }
    }
    public static Mesh GetMesh(Vector3 pos)
    {
        Mesh m = new Mesh();
        m.indexFormat = m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Chunk c = chunkManager.GetChunkAt(pos);
        m.vertices = c.vertices.ToArray();
        m.triangles = c.triangles.ToArray();
        m.RecalculateNormals();
        return m;
    }
}
