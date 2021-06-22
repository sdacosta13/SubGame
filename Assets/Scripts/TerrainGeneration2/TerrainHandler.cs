using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    static ChunkManager cm;
    static Dictionary<Vector3, GameObject> gameObjects;
    // Start is called before the first frame update
    void Start()
    {
        cm = new ChunkManager();
        gameObjects = new Dictionary<Vector3, GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraPos = GameObject.Find("PlayerCamera").transform.position;
        cameraPos = cameraPos / Constants.chunkSize;
        Vector3 chunk = new Vector3((int)Mathf.Floor(cameraPos.x) * Constants.chunkSize, 
                                    (int)Mathf.Floor(cameraPos.y) * Constants.chunkSize, 
                                    (int)Mathf.Floor(cameraPos.z) * Constants.chunkSize); // Gives Vector to chunk start point
        for(int x = -Constants.chunkloadradius; x < Constants.chunkloadradius+1; x++)
        {
            for(int z = -Constants.chunkloadradius; z < Constants.chunkloadradius+1; z++)
            {
                for(int y = -Constants.chunkloadradius; y < Constants.chunkloadradius+1; y++)
                {
                    Vector3 chunkToLoadPos = new Vector3(x, y, z) * Constants.chunkSize + chunk;
                    if(!(gameObjects.ContainsKey(chunkToLoadPos))){
                        if (cm.chunkExists(chunkToLoadPos))
                        {
                            if (cm.chunkComplete(chunkToLoadPos))
                            {
                                SetupGameObject(chunkToLoadPos, cm.GetChunk(chunkToLoadPos));
                            }
                            else
                            {
                                // Chunk is still generating so do nothing
                            }
                        }
                        else
                        {
                            cm.CreateChunk(chunkToLoadPos);
                        }
                    }
                }
            }
        }
    }
    static void SetupGameObject(Vector3 position, Chunk c)
    {
        GameObject go = new GameObject();
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        Mesh m = new Mesh();
        m.vertices = c.vertices.ToArray();
        m.triangles = c.triangles.ToArray();
        m.RecalculateNormals();
        go.GetComponent<MeshFilter>().mesh = m;
        gameObjects[position] = go;
    }
}
