using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

public class ChunkManager
{
    private Dictionary<Vector3, Chunk> _chunks;
    public Dictionary<Vector3, bool> ChunkState;

    public ChunkManager()
    {
        _chunks = new Dictionary<Vector3, Chunk>();
        ChunkState = new Dictionary<Vector3, bool>();
    }

    public bool ChunkExists(Vector3 pos) => ChunkState.ContainsKey(pos);
    public bool ChunkComplete(Vector3 pos) => ChunkState[pos];
    public Chunk GetChunk(Vector3 pos) => _chunks[pos];

    public void CreateChunk(Vector3 pos)
    {
        Profiler.BeginSample("Start Create");
        if (Constants.generateViaShaderCompute)
        {
            Profiler.BeginSample("ShaderCompute");
            var c = new Chunk(pos);
            _chunks[pos] = c;
            c.Generate();
            Profiler.EndSample();
        }
        else
        {
            Profiler.BeginSample("CPU generation");
            if (TerrainHandler.DoThreading){
                ChunkState[pos] = false;
                Profiler.BeginSample("Chunk, threaded");
                Task.Run(() => new Chunk(pos, true))
                    .ContinueWith(chunkTask => _chunks[chunkTask.Result.Pos] = chunkTask.Result)
                    .ContinueWith(task => ChunkState[task.Result.Pos] = true);
                Profiler.EndSample();
            }
            else
            {
                Profiler.BeginSample("Chunk, not threaded");
                _chunks[pos] = new Chunk(pos, true);
                Profiler.EndSample();
            }
            Profiler.EndSample();
        }
        Profiler.EndSample();
    }
    
    public void Destroy()
    {
        foreach (var c in _chunks.Values)
        {
            c.Destroy();
        }
    }
}