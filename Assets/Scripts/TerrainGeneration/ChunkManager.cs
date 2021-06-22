using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChunkManager
{
    public Dictionary<Vector3, Chunk> chunks = new Dictionary<Vector3, Chunk>();
    public ChunkManager(int chunkRange)
    {
        GenerateChunksInRange(chunkRange);
    }
    public Chunk GetChunkAt(Vector3 pos)
    {
        Chunk c;
        try
        {
            c = chunks[pos];
        }
        catch (KeyNotFoundException)
        {
            c = GenerateChunk(pos);
        }
        return c;
    }
    void GenerateChunksInRange(int chunkRange)
    {
        List<Thread> th = new List<Thread>();
        for (int x = -Constants.maxChunkNum * Constants.chunkSize; x < Constants.maxChunkNum * Constants.chunkSize; x += Constants.chunkSize)
        {
            for (int z = -Constants.maxChunkNum * Constants.chunkSize; z < Constants.maxChunkNum * Constants.chunkSize; z += Constants.chunkSize)
            {
                for (int y = -Constants.maxChunkNum * Constants.chunkSize; y < Constants.maxChunkNum * Constants.chunkSize; y += Constants.chunkSize)
                {
                    Vector3 cur = new Vector3(x, y, z);
                    Chunk chunkAtPos = new Chunk(cur);
                    chunks[cur] = chunkAtPos;
                    th.Add(new Thread(new ThreadStart(chunkAtPos.Generate)));
                    th[th.Count - 1].Start();
                }
            }
        }
        for(int i = 0; i < th.Count; i++)
        {
            th[i].Join();
        }
    }
    Chunk GenerateChunk(Vector3 pos)
    {
        Chunk c = new Chunk(pos);
        c.Generate();
        chunks[pos] = c;
        return c;
    }
}
