using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

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
        public List<Vector3>? Vertices;
        public List<int>? Triangles;
        public Vector3[] Normals;
        public bool done;
        #nullable disable
    }
    public MeshData meshData;
    public Vector3 Pos;
    private FastNoiseLite _fnl = new FastNoiseLite(HashLevelName());
    //private string path = Application.persistentDataPath;

    public Chunk(Vector3 pos, bool doGenerate = false)
    {
        Pos = pos;
        meshData.Vertices = new List<Vector3>();
        meshData.Triangles = new List<int>();
        if (doGenerate) Generate();
    }
    
    public override string ToString()
    {
        return Pos.x + "@" + Pos.y + "@" + Pos.z;
    }
    
    public void Generate()
    {
        Profiler.BeginSample("Generating chunk");
        if (Constants.generateViaShaderCompute)
        {
            GenerateViaShaderCompute();
            TryToSetMesh();
        }
        else
        {
            Profiler.BeginSample("Marching instantiate");
            var m = new Marching(Pos);
            Profiler.EndSample();
            
            Profiler.BeginSample("Creating mesh data");
            CreateMeshData();
            meshData.done = true;
            Profiler.EndSample();

            //string json = JsonUtility.ToJson(meshData);
            //Directory.CreateDirectory(path + "/" +  Constants.levelName);
            //FileOperator.Write(path + "/" + Constants.levelName+ "/" + ToString() + ".json", json);
            //Debug.Log("Chunk " + pos.ToString() + " Complete");
        }
        Profiler.EndSample();
    }

    private void TryToSetMesh()
    {
        try
        {
            AsyncGPUReadback.Request(vertices);
            AsyncGPUReadback.Request(triangles);
            var vecs = new Vector3[32 * 32 * 32 * 15];
            vertices.GetData(vecs);
            foreach (var vec in vecs)
            {
                if (vec != null)
                {
                    meshData.Vertices.Add(vec);
                }
            }
            var tris = new int[32 * 32 * 32 * 15 * 3];
            triangles.GetData(tris);
            foreach (var tri in tris)
            {
                meshData.Triangles.Add(tri);
            }
            meshData.done = true;
        }
        catch(Exception)
        {
            meshData.done = false;
        }
        
        
    }

    private void GenerateViaShaderCompute()
    {
        tcTable = new ComputeBuffer(Constants.TriangleTable.Length, sizeof(int));
        tcTable.SetData(Constants.TriangleTable);
        boolData = new ComputeBuffer((int) Mathf.Pow(Constants.chunkSize + 1, 3), sizeof(float));
        boolData.SetData(GetPerlinData());
        triangles = new ComputeBuffer(32 * 32 * 32 * 15 * 3 * 3, sizeof(int));
        vertices = new ComputeBuffer(32 * 32 * 32 * 15 * 3, sizeof(float));
        int[] boolInt = { 0 };
        var kernelIndex = 0;
        cs = Resources.Load<ComputeShader>("ChunkGenerator");
        cs.GetKernelThreadGroupSizes(kernelIndex, out _, out _, out _);
        cs.SetInt("chunkSize", Constants.chunkSize);
        cs.SetFloats("offset", Pos.x, Pos.y, Pos.z);
        cs.SetBuffer(kernelIndex, "TriangleConnectionTable", tcTable);
        cs.SetBuffer(kernelIndex, "boolData", boolData);
        cs.SetBuffer(kernelIndex, "triangles", triangles);
        cs.SetBuffer(kernelIndex, "vertices", vertices);
        cs.Dispatch(kernelIndex, 32, 1, 1);
    }

    private int[,,] GetPerlinData()
    {
        var pd = new int[Constants.chunkSize + 1, Constants.chunkSize + 1, Constants.chunkSize + 1];
        for(var x = 0; x < Constants.chunkSize + 1; x++)
        {
            for(var z = 0; z < Constants.chunkSize + 1; z++)
            {
                for(var y = 0; y < Constants.chunkSize + 1; y++)
                {
                    if (Perlin3D(new Vector3(x, y, z)) > Constants.perlinThreshold) pd[x, y, z] = 1;
                    else pd[x, y, z] = 0;
                }
            }
        }
        return pd;
    }
    private static int HashLevelName()
    {
        var sum = 1;
        var bs = Encoding.ASCII.GetBytes(Constants.levelName);
        foreach (var b in bs)
            sum *= b;
        
        sum %= int.MaxValue;
        return sum;
    }
    private float Perlin3D(Vector3 coord)
    {
        var f = new FastNoiseLite(HashLevelName());
        coord += Pos;
        return f.GetNoise(coord.x, coord.y, coord.z);

    }
    public void Destroy()
    {
        tcTable?.Dispose();
        boolData?.Dispose();
        triangles?.Dispose();
        vertices?.Dispose();
    }

    
    
    // moved code to reduce objects
    // probably most optimisation needs to happen here somehow
    #region marching code

    private bool[,,] _terrainMap;
    private const float Threshold = 0.2f;

    private void CreateMeshData()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Profiler.BeginSample("Populating terrain");
        PopulateTerrain();
        Profiler.EndSample();

        // Loop through each "cube" in our terrain.
        Profiler.BeginSample("Marching cubes");
        var cube = new bool[8];
        for (var x = 0; x < Constants.chunkSize; x++)
        {
            for (var y = 0; y < Constants.chunkSize; y++)
            {
                for (var z = 0; z < Constants.chunkSize; z++)
                {
                    // Create an array of floats representing each corner of a cube and get the value from our terrainMap.
                    for (var i = 0; i < 8; i++)
                    {
                        var corner = new Vector3Int(x, y, z) + Constants.CornerTable[i];
                        cube[i] = _terrainMap[corner.x, corner.y, corner.z];
                    }

                    // Pass the value into our MarchCube function.
                    Profiler.BeginSample("Marching a Cube");
                    MarchCube(new Vector3(x, y, z), cube);
                    Profiler.EndSample();
                }
            }
        }
        Profiler.EndSample();

        Profiler.BeginSample("Calc normals");
        CalculateNormals();
        Profiler.EndSample();

        stopwatch.Stop();
        Debug.Log("Time taken to generate mesh data: " + stopwatch.ElapsedMilliseconds + "ms");
    }
    
    private void PopulateTerrain()
    {
        Profiler.BeginSample("Wiping array");
        _terrainMap = new bool[Constants.chunkSize + 1, Constants.chunkSize + 1, Constants.chunkSize + 1];
        Profiler.EndSample();
        
        Profiler.BeginSample("Loop start");
        for (var x = 0; x < Constants.chunkSize + 1; x++)
        {
            for (var z = 0; z < Constants.chunkSize + 1; z++)
            {
                for (var y = 0; y < Constants.chunkSize + 1; y++)
                {
                    Profiler.BeginSample("Perlin gen");
                    _terrainMap[x, y, z] = _fnl.GetNoise(x + Pos.x, y + Pos.y, z + Pos.z) > Threshold;
                    Profiler.EndSample();
                }
            }
        }
        Profiler.EndSample();
    }

    private void MarchCube(Vector3 position, bool[] cube)
    {
        // Get the configuration index of this cube.
        var configIndex = GetCubeConfiguration(cube);

        // If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
        if (configIndex == 0 || configIndex == 255)
            return;

        // Loop through the triangles. There are never more than 5 triangles to a cube and only three vertices to a triangle.
        // 3 * 5 = 15, so loop 15 times
        var edgeIndex = 0;
        for (var i = 0; i < 15; i++)
        {
            // Get the current indice. We increment triangleIndex through each loop.
            var indice = Constants.TriangleTable[configIndex, edgeIndex];

            // If the current edgeIndex is -1, there are no more indices and we can exit the function.

            if (indice == -1)
                return;

            // Get the vertices for the start and end of this edge.
            var vert1 = position + Constants.EdgeTable[indice, 0];
            var vert2 = position + Constants.EdgeTable[indice, 1];

            // Get the midpoint of this edge.
            var vertPosition = (vert1 + vert2) / 2f;

            // Add to our vertices and triangles list and incremement the edgeIndex.
            meshData.Vertices.Add(vertPosition + Pos);
            meshData.Triangles.Add(meshData.Vertices.Count - 1);
            edgeIndex++;
        }
    }

    private void CalculateNormals()
    {
        meshData.Normals = new Vector3[meshData.Vertices.Count];
        for (var i = 0; i < meshData.Triangles.Count; i += 3)
        {
            var vertA = meshData.Triangles[i + 0];
            var vertB = meshData.Triangles[i + 1];
            var vertC = meshData.Triangles[i + 2];
            var surfaceNorm = SurfaceNormalFromIndices(vertA, vertB, vertC);
            meshData.Normals[vertA] += surfaceNorm;
            meshData.Normals[vertB] += surfaceNorm;
            meshData.Normals[vertC] += surfaceNorm;
        }

        for (var i = 0; i < meshData.Normals.Length; i++)
        {
            meshData.Normals[i].Normalize();
        }
    }

    private int GetCubeConfiguration(bool[] cube)
    {
        // Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
        var configurationIndex = 0;
        int[] adder = {1, 2, 4, 8, 16, 32, 64, 128};
        for (var i = 0; i < 8; i++)
        {
            if (cube[i])
            {
                configurationIndex += adder[i];
            }
        }

        return configurationIndex;
    }

    private Vector3 SurfaceNormalFromIndices(int a, int b, int c)
    {
        var pointA = meshData.Vertices[a];
        var pointB = meshData.Vertices[b];
        var pointC = meshData.Vertices[c];
        return Vector3.Cross(pointB - pointA, pointC - pointA).normalized;
    }


    #endregion
}
