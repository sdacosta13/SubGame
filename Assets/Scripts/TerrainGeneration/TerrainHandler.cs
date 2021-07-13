using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

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
        var doThreading = true;
        Profiler.BeginSample("UpdateTerrain");
        var cameraPos = GameObject.Find("PlayerCamera").transform.position;
        cameraPos = cameraPos / Constants.chunkSize;
        var chunk = new Vector3((int) Mathf.Floor(cameraPos.x) * Constants.chunkSize,
            (int) Mathf.Floor(cameraPos.y) * Constants.chunkSize,
            (int) Mathf.Floor(cameraPos.z) * Constants.chunkSize); // Gives Vector to chunk start point
        for (var x = -Constants.chunkloadradius; x < Constants.chunkloadradius + 1; x++)
        {
            for (var z = -Constants.chunkloadradius; z < Constants.chunkloadradius + 1; z++)
            {
                for (var y = -Constants.chunkloadradius; y < Constants.chunkloadradius + 1; y++)
                {
                    UpdateChunk2(x, y, z, chunk, doThreading);
                }
            }
        }

        Profiler.EndSample();
    }

    private static void UpdateChunk2(int x, int y, int z, Vector3 chunk, bool doThreading)
    {
        var chunkToLoadPos = new Vector3(x, y, z) * Constants.chunkSize + chunk;
        if (!(gameObjects.ContainsKey(chunkToLoadPos)))
        {
            if (cm.ChunkState.ContainsKey(chunkToLoadPos))
            {
                // Chunk Object exists, not done for sure
                if (cm.ChunkState[chunkToLoadPos])
                {
                    SetupGameObject(chunkToLoadPos, cm.GetChunk(chunkToLoadPos));
                }
            }
            else
            {
                Profiler.BeginSample("Chunk creation");
                cm.CreateChunkWithTask(chunkToLoadPos);
                Profiler.EndSample();
            }
        }
    }

    private static void UpdateChunk(int x, int y, int z, Vector3 chunk, bool doThreading)
    {
        var chunkToLoadPos = new Vector3(x, y, z) * Constants.chunkSize + chunk;
        if (!(gameObjects.ContainsKey(chunkToLoadPos)))
        {
            if (cm.ChunkExists(chunkToLoadPos))
            {
                if (doThreading)
                {
                    // Chunk Object exists, not done for sure
                    if (cm.ChunkComplete(chunkToLoadPos))
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
                    SetupGameObject(chunkToLoadPos, cm.GetChunk(chunkToLoadPos));
                }
            }
            else
            {
                if (doThreading)
                {
                    Profiler.BeginSample("Chunk creation");
                    cm.CreateChunk(chunkToLoadPos);
                    Profiler.EndSample();
                }
                else
                {
                    Profiler.BeginSample("Creating chunk");
                    cm.CreateChunk2(chunkToLoadPos);
                    SetupGameObject(chunkToLoadPos, cm.GetChunk(chunkToLoadPos));
                    Profiler.EndSample();
                }
            }
        }
    }

    void AttemptLevelLoad(Vector3 chunkPos)
    {
        if (Constants.RWlevels)
        {
            var m = FileOperator.ReadMesh(chunkPos);
            SetupLoadedGameObject(chunkPos, m);
        }
    }

    static void SetupLoadedGameObject(Vector3 position, Mesh m)
    {
        var go = new GameObject();
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        go.GetComponent<MeshFilter>().mesh = m;
        gameObjects[position] = go;
    }

    static void SetupGameObject(Vector3 position, Chunk c)
    {
        // around 100ms lag
        Profiler.BeginSample("SettingUp");
        var go = new GameObject();
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        var m = new Mesh();
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
        Profiler.EndSample();
    }

    void OnDestroy()
    {
        cm.Destroy();
    }
}