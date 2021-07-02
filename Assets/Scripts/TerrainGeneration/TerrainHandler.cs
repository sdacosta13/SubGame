using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
                            // Chunk Object exists, not done for sure
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
    void AttemptLevelLoad(Vector3 chunkPos)
    {
        if (Constants.RWlevels)
        {
            Mesh m = FileOperator.ReadMesh(chunkPos);
            SetupLoadedGameObject(chunkPos, m);

        }
    }
    static void SetupLoadedGameObject(Vector3 position, Mesh m)
    {
        GameObject go = new GameObject();
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        go.GetComponent<MeshFilter>().mesh = m;
        gameObjects[position] = go;
    }
    static void SetupGameObject(Vector3 position, Chunk c)
    {
        GameObject go = new GameObject();
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        Mesh m = new Mesh();
        m.vertices = c.meshData.vertices.ToArray();
        m.triangles = c.meshData.triangles.ToArray();
        if (!Constants.generateViaShaderCompute)
        {
            m.normals = c.meshData.normals;
        }
        else
        {
            m.RecalculateNormals();
        }
        go.GetComponent<MeshFilter>().mesh = m;
        if (Constants.RWlevels)
        {
            FileOperator.WriteMesh(m, c.pos);
        }
        gameObjects[position] = go;
    }
    void OnDestroy()
    {
        cm.Destroy();
    }
}
