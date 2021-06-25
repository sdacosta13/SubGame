using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChunkManager
{
    private Dictionary<Vector3, Chunk> chunks;
    public Dictionary<Vector3, Thread> threads;
    public ChunkManager()
    {
        chunks = new Dictionary<Vector3, Chunk>();
        threads = new Dictionary<Vector3, Thread>();
    }
    public bool chunkExists(Vector3 pos)
    {
        return chunks.ContainsKey(pos);
    }
    public bool chunkComplete(Vector3 pos)
    {
        return chunks[pos].meshData.done;
    }
    public Chunk GetChunk(Vector3 pos)
    {
        return chunks[pos];
    }
    public void CreateChunk(Vector3 pos)
    {
        if (Constants.generateViaShaderCompute)
        {
            Chunk c = new Chunk(pos);
            chunks[pos] = c;
            c.Generate();
        }
        else
        {
            if (!threads.ContainsKey(pos))
            {
                Chunk c = new Chunk(pos);
                chunks[pos] = c;
                Thread th = new Thread(c.Generate);
                th.Start();
                threads[pos] = th;
            }
        }
        

    }
    public void Destroy()
    {
        foreach(Chunk c in chunks.Values)
        {
            c.Destroy();
        }
    }
}
