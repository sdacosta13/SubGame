using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

public class Marching
{
    float _threshold = 0.2f;
    int _chunkSize = Constants.chunkSize;
    bool[,,] _terrainMap;
    static System.Random _r = new System.Random();
    public List<Vector3> Vertices = new List<Vector3>(32 * 32 * 32);
    public List<int> Triangles = new List<int>(32 * 32 * 32);
    public Vector3[] Normals;
    private Vector3 _offset;
    private FastNoiseLite _fnl;

    public Marching(Vector3 pos)
    {
        _offset = pos;
        _fnl = new FastNoiseLite(HashLevelName());
    }

    public void PrintTerrain()
    {
        var s = "";
        for (var x = 0; x < _chunkSize; x++)
        {
            for (var z = 0; z < _chunkSize; z++)
                s += _terrainMap[x, 0, z] ? "1" : "0";

            s += "\n";
        }

        Debug.Log(s);
        throw new Exception();
    }

    private void PopulateTerrain()
    {
        Profiler.BeginSample("Wiping array");
        _terrainMap = new bool[_chunkSize + 1, _chunkSize + 1, _chunkSize + 1];
        Profiler.EndSample();
        Profiler.BeginSample("Loop start");
        for (var x = 0; x < _chunkSize + 1; x++)
        {
            for (var z = 0; z < _chunkSize + 1; z++)
            {
                for (var y = 0; y < _chunkSize + 1; y++)
                {
                    Profiler.BeginSample("Perlin gen");
                    // //var val = Perlin3D(new Vector3(x, y, z));
                    // var val = fnl.GetNoise(x + offset.x, y + offset.y, z + offset.z);
                    // Profiler.EndSample();
                    // Profiler.BeginSample("Assigning val");
                    // if (val > threshold)
                    // {
                    //     terrainMap[x, y, z] = true;
                    // }

                    _terrainMap[x, y, z] = _fnl.GetNoise(x + _offset.x, y + _offset.y, z + _offset.z) > _threshold;
                    Profiler.EndSample();
                }
            }
        }

        Profiler.EndSample();
    }

    int HashLevelName()
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
        // Profiler.BeginSample("Instantiate FNL");
        // var f = new FastNoiseLite(HashLevelName());
        // Profiler.EndSample();
        coord += _offset;
        return _fnl.GetNoise(coord.x, coord.y, coord.z);
    }

    public static int Debugcount = 100;

    /*private static float Perlin3D(Vector3 coord)
    {
        return (float) r.Next(0,100) /100f;
    }*/
    public void CreateMeshData()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Profiler.BeginSample("Populating terrain");
        PopulateTerrain();
        Profiler.EndSample();

        // Loop through each "cube" in our terrain.
        Profiler.BeginSample("Marching cubes");
        var cube = new bool[8];
        for (var x = 0; x < _chunkSize; x++)
        {
            for (var y = 0; y < _chunkSize; y++)
            {
                for (var z = 0; z < _chunkSize; z++)
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


    void MarchCube(Vector3 position, bool[] cube)
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
            Vertices.Add(vertPosition + _offset);
            Triangles.Add(Vertices.Count - 1);
            edgeIndex++;
        }
    }

    void CalculateNormals()
    {
        Normals = new Vector3[Vertices.Count];
        for (var i = 0; i < Triangles.Count; i += 3)
        {
            var vertA = Triangles[i + 0];
            var vertB = Triangles[i + 1];
            var vertC = Triangles[i + 2];
            var surfaceNorm = SurfaceNormalFromIndices(vertA, vertB, vertC);
            Normals[vertA] += surfaceNorm;
            Normals[vertB] += surfaceNorm;
            Normals[vertC] += surfaceNorm;
        }

        for (var i = 0; i < Normals.Length; i++)
        {
            Normals[i].Normalize();
        }
    }

    Vector3 SurfaceNormalFromIndices(int a, int b, int c)
    {
        var pointA = Vertices[a];
        var pointB = Vertices[b];
        var pointC = Vertices[c];
        return Vector3.Cross(pointB - pointA, pointC - pointA).normalized;
    }

    int GetCubeConfiguration(bool[] cube)
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
}