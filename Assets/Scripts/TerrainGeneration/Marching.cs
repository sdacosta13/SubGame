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
    float threshold = 0.2f;
    int chunkSize = Constants.chunkSize;
    bool[,,] terrainMap;
    static System.Random r = new System.Random();
    public List<Vector3> vertices = new List<Vector3>(32 * 32 * 32);
    public List<int> triangles = new List<int>(32 * 32 * 32);
    public Vector3[] normals;
    private Vector3 offset;
    private FastNoiseLite fnl;

    public Marching(Vector3 pos)
    {
        offset = pos;
        fnl = new FastNoiseLite(HashLevelName());
    }

    void PrintTerrain()
    {
        var s = "";
        for (var x = 0; x < chunkSize; x++)
        {
            for (var z = 0; z < chunkSize; z++)
            {
                if (terrainMap[x, 0, z])
                {
                    s += "1";
                }
                else
                {
                    s += "0";
                }
            }

            s += "\n";
        }

        Debug.Log(s);
        throw new Exception();
    }

    void PopulateTerrain()
    {
        Profiler.BeginSample("Wiping array");
        terrainMap = new bool[chunkSize + 1, chunkSize + 1, chunkSize + 1];
        Profiler.EndSample();
        Profiler.BeginSample("Loop start");
        for (var x = 0; x < chunkSize + 1; x++)
        {
            for (var z = 0; z < chunkSize + 1; z++)
            {
                for (var y = 0; y < chunkSize + 1; y++)
                {
                    Profiler.BeginSample("Perlin gen");
                    //var val = Perlin3D(new Vector3(x, y, z));
                    var val = fnl.GetNoise(x + offset.x, y + offset.y, z + offset.z);
                    Profiler.EndSample();
                    Profiler.BeginSample("Assigning val");
                    if (val > threshold)
                    {
                        terrainMap[x, y, z] = true;
                    }

                    Profiler.EndSample();
                }
            }
        }

        Profiler.EndSample();
    }

    private int HashLevelName()
    {
        var sum = 1;
        var bs = Encoding.ASCII.GetBytes(Constants.levelName);
        foreach (var b in bs)
        {
            sum *= b;
        }

        sum %= int.MaxValue;
        return sum;
    }

    private float Perlin3D(Vector3 coord)
    {
        // Profiler.BeginSample("Instantiate FNL");
        // var f = new FastNoiseLite(HashLevelName());
        // Profiler.EndSample();
        coord += offset;
        return fnl.GetNoise(coord.x, coord.y, coord.z);
    }

    public static int debugcount = 100;

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
        for (var x = 0; x < chunkSize; x++)
        {
            for (var y = 0; y < chunkSize; y++)
            {
                for (var z = 0; z < chunkSize; z++)
                {
                    // Create an array of floats representing each corner of a cube and get the value from our terrainMap.
                    for (var i = 0; i < 8; i++)
                    {
                        var corner = new Vector3Int(x, y, z) + Constants.CornerTable[i];
                        cube[i] = terrainMap[corner.x, corner.y, corner.z];
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
            vertices.Add(vertPosition + offset);
            triangles.Add(vertices.Count - 1);
            edgeIndex++;
        }
    }

    void CalculateNormals()
    {
        normals = new Vector3[vertices.Count];
        for (var i = 0; i < triangles.Count; i += 3)
        {
            var vertA = triangles[i + 0];
            var vertB = triangles[i + 1];
            var vertC = triangles[i + 2];
            var surfaceNorm = SurfaceNormalFromIndices(vertA, vertB, vertC);
            normals[vertA] += surfaceNorm;
            normals[vertB] += surfaceNorm;
            normals[vertC] += surfaceNorm;
        }

        for (var i = 0; i < normals.Length; i++)
        {
            normals[i].Normalize();
        }
    }

    Vector3 SurfaceNormalFromIndices(int A, int B, int C)
    {
        var pointA = vertices[A];
        var pointB = vertices[B];
        var pointC = vertices[C];
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