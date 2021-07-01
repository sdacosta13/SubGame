using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Scripts.Terrain
{
    class ChunkManager
    {
        public Dictionary<Vector3, Chunk> chunks = new Dictionary<Vector3, Chunk>();
        public Dictionary<Vector3, JobHandle> jobHandles = new Dictionary<Vector3, JobHandle>();
        public ChunkManager()
        {

        }

        public bool ChunkComplete(Vector3 chunkToLoadPos)
        {
            return chunks[chunkToLoadPos].done;
        }

        public bool ChunkExists(Vector3 chunkToLoadPos)
        {
            return chunks.ContainsKey(chunkToLoadPos);
        }

        public Chunk GetChunk(Vector3 chunkToLoadPos)
        {
            return chunks[chunkToLoadPos];
        }

        public void CreateChunk(Vector3 chunkToLoadPos)
        {
            Chunk c = new Chunk(chunkToLoadPos);
            chunks[chunkToLoadPos] = c;
            jobHandles[chunkToLoadPos] = c.Schedule();
        }
    }
}
