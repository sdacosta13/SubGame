using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;

namespace Assets.Scripts.Terrain
{
    public struct Chunk : IJob
    {
        [ReadOnly]
        public Vector3 position;
        [WriteOnly]
        public NativeList<Vector3> vertices;
        [WriteOnly]
        public NativeList<int> triangles;
        [WriteOnly]
        public NativeList<Vector3> normals;

        public bool done;
        public Chunk(Vector3 _pos)
        {
            position = _pos;
            done = false;
            vertices = new NativeList<Vector3>(Allocator.Persistent);
            triangles = new NativeList<int>(Allocator.Persistent);
            normals = new NativeList<Vector3>(Allocator.Persistent);
        }
        public void Execute()
        {
            Marching march = new Marching(position);
            march.CreateMeshData();
            List<Vector3> verts = new List<Vector3>();
            foreach (Vector3 data1 in vertices)
            {
                vertices.Add(data1);
                verts.Add(data1);
            }
            foreach(int data2 in triangles)
            {
                triangles.Add(data2);
            }
            for(int i = 0; i < (int) verts.Count / 3; i += 3)
            {
                Vector3 A = verts[0];
                Vector3 B = verts[1];
                Vector3 C = verts[2];
                normals.Add(Vector3.Cross((B - A).normalized, (C - A).normalized));
                normals.Add(Vector3.Cross((C - B).normalized, (A - B).normalized));
                normals.Add(Vector3.Cross((A - C).normalized, (B - C).normalized));
            }
            done = true;
        }
    }
}
