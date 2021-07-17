using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class TerrainHandler : MonoBehaviour
{
    private static ChunkManager _cm;
    public static bool DoThreading = true;
    public static int MaxChunksLoadPerFrame = 3;

    private static Dictionary<Vector3, GameObject> _gameObjects;
    private GameObject _playerCam;

    public GameObject terrainPrefab;

    // Start is called before the first frame update
    private void Start()
    {
        _playerCam = GameObject.Find("PlayerCamera");
        _cm = new ChunkManager();
        _gameObjects = new Dictionary<Vector3, GameObject>();
    }
    
    // Update is called once per frame
    private void Update()
    {
        Profiler.BeginSample("UpdateTerrain");
        int chunksLoaded = 0;
        var cameraPos = _playerCam.transform.position;
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
                    UpdateChunk(x, y, z, chunk);
                }
            }
        }
        Profiler.EndSample();
        
        void UpdateChunk(int x, int y, int z, Vector3 chunkPos)
        {
            var chunkToLoadPos = new Vector3(x, y, z) * Constants.chunkSize + chunkPos;
            if (_gameObjects.ContainsKey(chunkToLoadPos)) return;
            if (_cm.ChunkExists(chunkToLoadPos))
            {
                if (DoThreading)
                {
                    if (_cm.ChunkComplete(chunkToLoadPos))
                    {
                        SetupGameObject(chunkToLoadPos, _cm.GetChunk(chunkToLoadPos));
                    }
                }
                else
                {
                    SetupGameObject(chunkToLoadPos, _cm.GetChunk(chunkToLoadPos));
                }
            }
            else if (MaxChunksLoadPerFrame == 0 || chunksLoaded < MaxChunksLoadPerFrame)
            {
                if (DoThreading)
                {
                    Profiler.BeginSample("Chunk creation");
                    _cm.CreateChunk(chunkToLoadPos);
                    Profiler.EndSample();
                }
                else
                {
                    Profiler.BeginSample("Creating chunk");
                    _cm.CreateChunk(chunkToLoadPos);
                    SetupGameObject(chunkToLoadPos, _cm.GetChunk(chunkToLoadPos));
                    Profiler.EndSample();
                }
                chunksLoaded++;
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

    private static void SetupLoadedGameObject(Vector3 position, Mesh m)
    {
        var go = new GameObject();
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        go.GetComponent<MeshFilter>().mesh = m;
        _gameObjects[position] = go;
    }

    private void SetupGameObject(Vector3 position, Chunk c)
    {
        // around 100ms lag
        // incurs massive garbage collection lag
        Profiler.BeginSample("SettingUp");
        
        Profiler.BeginSample("Making game object + adding components");
        // var go = new GameObject();
        // go.AddComponent<MeshFilter>();
        // go.AddComponent<MeshRenderer>();
        // go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        var go = Instantiate(terrainPrefab);
        Profiler.EndSample();
        
        Profiler.BeginSample("Assigning data");
        // maybe use unity mesh class instead of MeshData in chunk
        // this section still has around 20 ms of lag
        var m = new Mesh
        {
            vertices = c.meshData.Vertices?.ToArray(),
            triangles = c.meshData.Triangles?.ToArray()
        };
        if (!Constants.generateViaShaderCompute)
        {
            m.normals = c.meshData.Normals;
        }
        else
        {
            m.RecalculateNormals();
        }

        go.GetComponent<MeshFilter>().mesh = m;
        if (Constants.RWlevels)
        {
            FileOperator.WriteMesh(m, c.Pos);
        }

        _gameObjects[position] = go;
        Profiler.EndSample();
        
        Profiler.EndSample();
    }

    private void OnDestroy()
    {
        _cm.Destroy();
    }
}