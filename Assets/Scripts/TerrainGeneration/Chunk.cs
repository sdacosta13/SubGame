using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class Chunk
{
    ComputeBuffer tcTable;
    ComputeBuffer boolData;
    ComputeBuffer triangles;
    ComputeBuffer vertices;
    ComputeShader cs;
    public struct MeshData
    {
        // ? means it is nullable
        #nullable enable
        public List<Vector3>? vertices;
        public List<int>? triangles;
        public bool done;
        #nullable disable
    }
    public MeshData meshData = new MeshData();
    public Vector3 pos;
    private string path = Application.persistentDataPath;
    public Chunk(Vector3 _pos)
    {
        pos = _pos;
        meshData.vertices = new List<Vector3>();
        meshData.triangles = new List<int>();
    }
    public override string ToString()
    {
        return pos.x.ToString() + "@" + pos.y.ToString() + "@" + pos.z.ToString();
    }
    public void Generate()
    {
        if (Constants.generateViaShaderCompute)
        {
            GenerateViaShaderCompute();
            TryToSetMesh();
        }
        else
        {
            Marching m = new Marching(pos);
            m.CreateMeshData();
            this.meshData.vertices = m.vertices;
            this.meshData.triangles = m.triangles;
            this.meshData.done = true;
            //string json = JsonUtility.ToJson(meshData);
            //Directory.CreateDirectory(path + "/" +  Constants.levelName);
            //FileOperator.Write(path + "/" + Constants.levelName+ "/" + ToString() + ".json", json);
            //Debug.Log("Chunk " + pos.ToString() + " Complete");
        }

    }
    public void TryToSetMesh()
    {
        try
        {
            AsyncGPUReadback.Request(vertices);
            AsyncGPUReadback.Request(triangles);
            Vector3[] vecs = new Vector3[32 * 32 * 32 * 15];
            vertices.GetData(vecs);
            foreach (Vector3 vec in vecs)
            {
                if (vec != null)
                {
                    meshData.vertices.Add(vec);
                }
            }
            int[] tris = new int[32 * 32 * 32 * 15 * 3];
            triangles.GetData(tris);
            foreach (int tri in tris)
            {
                meshData.triangles.Add(tri);
            }
            meshData.done = true;
        }
        catch(Exception)
        {
            meshData.done = false;
        }
        
        
    }
    public void GenerateViaShaderCompute()
    {
        tcTable = new ComputeBuffer(Constants.TriangleTable.Length, sizeof(int));
        tcTable.SetData(Constants.TriangleTable);
        boolData = new ComputeBuffer((int) Mathf.Pow(Constants.chunkSize + 1, 3), sizeof(float));
        boolData.SetData(GetPerlinData());
        triangles = new ComputeBuffer(32 * 32 * 32 * 15 * 3 * 3, sizeof(int));
        vertices = new ComputeBuffer(32 * 32 * 32 * 15 * 3, sizeof(float));
        int[] boolInt = { 0 };
        int kernalIndex = 0;
        cs = Resources.Load<ComputeShader>("ChunkGenerator");
        cs.GetKernelThreadGroupSizes(kernalIndex, out _, out _, out _);
        cs.SetInt("chunkSize", Constants.chunkSize);
        cs.SetFloats("offset", pos.x, pos.y, pos.z);
        cs.SetBuffer(kernalIndex, "TriangleConnectionTable", tcTable);
        cs.SetBuffer(kernalIndex, "boolData", boolData);
        cs.SetBuffer(kernalIndex, "triangles", triangles);
        cs.SetBuffer(kernalIndex, "vertices", vertices);
        cs.Dispatch(kernalIndex, 32, 1, 1);
    }
    public int[,,] GetPerlinData()
    {
        int[,,] pd = new int[Constants.chunkSize + 1, Constants.chunkSize + 1, Constants.chunkSize + 1];
        for(int x = 0; x < Constants.chunkSize + 1; x++)
        {
            for(int z = 0; z < Constants.chunkSize + 1; z++)
            {
                for(int y = 0; y < Constants.chunkSize + 1; y++)
                {
                    if (Perlin3D(new Vector3(x, y, z)) > Constants.perlinThreshold) pd[x, y, z] = 1;
                    else pd[x, y, z] = 0;
                }
            }
        }
        return pd;
    }
    private int HashLevelName()
    {
        int sum = 1;
        byte[] bs = Encoding.ASCII.GetBytes(Constants.levelName);
        foreach (byte b in bs)
        {
            sum *= b;
        }
        sum = sum % int.MaxValue;
        return sum;
    }
    private float Perlin3D(Vector3 coord)
    {
        FastNoiseLite f = new FastNoiseLite(HashLevelName());
        coord += pos;
        return f.GetNoise(coord.x, coord.y, coord.z);

    }
    public void Destroy()
    {
        if(tcTable != null)
        {
            tcTable.Dispose();
        }
        if(boolData != null)
        {
            boolData.Dispose();
        }
        if(triangles != null)
        {
            triangles.Dispose();
        }
        if (vertices != null)
        {
            vertices.Dispose();
        }        
    }
}
