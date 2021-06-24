using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class Chunk
{
    ComputeBuffer tcTable;
    ComputeBuffer perlinData;
    ComputeBuffer triangles;
    ComputeBuffer vertices;
    ComputeShader cs;
    public struct MeshData
    {
        // ? means it is nullable
        #nullable enable
        public List<Vector3>? vertices;
        public List<int>? triangles;
        #nullable disable
    }
    public MeshData meshData = new MeshData();
    public Vector3 pos;
    private string path = Application.persistentDataPath;
    public Chunk(Vector3 _pos)
    {
        pos = _pos;
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
        }
        else
        {
            Marching m = new Marching(pos);
            m.CreateMeshData();
            this.meshData.vertices = m.vertices;
            this.meshData.triangles = m.triangles;
            //string json = JsonUtility.ToJson(meshData);
            //Directory.CreateDirectory(path + "/" +  Constants.levelName);
            //FileOperator.Write(path + "/" + Constants.levelName+ "/" + ToString() + ".json", json);
            //Debug.Log("Chunk " + pos.ToString() + " Complete");
        }

    }
    public void GenerateViaShaderCompute()
    {
        SetupShaderCompute();
        Vector3[] vecs = new Vector3[32 * 32 * 32 * 15];
        vertices.GetData(vecs);
        foreach(Vector3 vec in vecs){
            if (vec != null)
            {
                meshData.vertices.Add(vec);
            }
        }
        int[] tris = new int[32 * 32 * 32 * 15 * 3];
        triangles.GetData(tris);
        foreach(int tri in tris)
        {
            if(tri > -1) {
                meshData.triangles.Add(tri);
            }
        }

    }
    public void SetupShaderCompute()
    {
        tcTable = new ComputeBuffer(Constants.TriangleTable.Length, sizeof(int));
        tcTable.SetData(Constants.TriangleTable);
        perlinData = new ComputeBuffer((int) Mathf.Pow(Constants.chunkSize + 1, 3), sizeof(float));
        perlinData.SetData(GetPerlinData());
        triangles = new ComputeBuffer(32 * 32 * 32 * 15 * 3 * 3, sizeof(int));
        vertices = new ComputeBuffer(32 * 32 * 32 * 15 * 3, sizeof(float));

        cs = Resources.Load<ComputeShader>("/Resources/ComputeShaders/ChunkGenerator.compute");
        Debug.Log(cs.ToString());
        cs.SetInt("chunkSize", Constants.chunkSize);
        cs.SetFloats("offset", pos.x, pos.y, pos.z);
        cs.SetBuffer(0, "TriangleConnectionTable", tcTable);
        cs.SetBuffer(0, "perlinData", perlinData);
        cs.Dispatch(0, 32, 32, 32);
    }
    public float[,,] GetPerlinData()
    {
        float[,,] pd = new float[Constants.chunkSize + 1, Constants.chunkSize + 1, Constants.chunkSize + 1];
        for(int x = 0; x < Constants.chunkSize + 1; x++)
        {
            for(int z = 0; z < Constants.chunkSize + 1; z++)
            {
                for(int y = 0; y < Constants.chunkSize + 1; y++)
                {
                    pd[x,y,z] = Perlin3D(new Vector3(x, y, z));
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
        if(perlinData != null)
        {
            perlinData.Dispose();
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
