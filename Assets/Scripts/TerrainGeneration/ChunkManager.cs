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
    public Dictionary<Vector3, Thread> Threads;
    public Dictionary<Vector3, bool> ChunkState;

    public ChunkManager()
    {
        _chunks = new Dictionary<Vector3, Chunk>();
        Threads = new Dictionary<Vector3, Thread>();
        ChunkState = new Dictionary<Vector3, bool>();
    }

    public bool ChunkExists(Vector3 pos) => _chunks.ContainsKey(pos);
    public bool ChunkComplete(Vector3 pos) => _chunks[pos].meshData.done;
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
            if (!Threads.ContainsKey(pos))
            {
                Profiler.BeginSample("Chunk Instantiation");
                var c = new Chunk(pos);
                _chunks[pos] = c;
                Profiler.EndSample();
                Profiler.BeginSample("Thread creation");
                var th = new Thread(c.Generate);
                th.Priority = System.Threading.ThreadPriority.BelowNormal;
                th.Name = pos + " Chunk Generator";
                Profiler.EndSample();
                Profiler.BeginSample("Thread starting");
                th.Start();
                Threads[pos] = th;
                Profiler.EndSample();
            }

            Profiler.EndSample();
        }

        Profiler.EndSample();
    }

    public void CreateChunkWithTask(Vector3 pos)
    {
        ChunkState[pos] = false;
        Task.Run(() => new Chunk(pos, true))
            .ContinueWith(chunkTask => _chunks[chunkTask.Result.pos] = chunkTask.Result)
            .ContinueWith(x => ChunkState[x.Result.pos] = true);
        
    }

    public void CreateChunk2(Vector3 pos)
    {
        Profiler.BeginSample("Chunk");
        Profiler.BeginSample("Instantiating chunk");
        var c = new Chunk(pos);
        _chunks[pos] = c;
        Profiler.EndSample();
        c.Generate();
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