using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

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
	public Marching(Vector3 pos)
    {
		offset = pos;
    }
	void PrintTerrain()
	{
		string s = "";
		for (int x = 0; x < chunkSize; x++)
		{
			for (int z = 0; z < chunkSize; z++)
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
		terrainMap = new bool[chunkSize + 1, chunkSize + 1, chunkSize+1];
		for (int x = 0; x < chunkSize + 1; x++)
		{
			for (int z = 0; z < chunkSize + 1; z++)
			{
				for (int y = 0; y < chunkSize + 1; y++)
				{
					float val = Perlin3D(new Vector3(x, y, z));
					if (val > this.threshold)
					{
						terrainMap[x, y, z] = true;
					}
				}
			}
		}
	}
	private int HashLevelName()
    {
		int sum = 1;
		byte[] bs = Encoding.ASCII.GetBytes(Constants.levelName);
		foreach(byte b in bs)
        {
			sum *= b;
        }
		sum = sum % int.MaxValue;
		return sum;
    }
	private float Perlin3D(Vector3 coord)
	{
		FastNoiseLite f = new FastNoiseLite(HashLevelName());
		coord += offset;
		return f.GetNoise(coord.x, coord.y, coord.z);
		
	}
	public static int debugcount = 100;
	/*private static float Perlin3D(Vector3 coord)
    {
		return (float) r.Next(0,100) /100f;
    }*/
	public void CreateMeshData()
	{
		PopulateTerrain();
		// Loop through each "cube" in our terrain.

		for (int x = 0; x < chunkSize; x++)
		{
			for (int y = 0; y < chunkSize; y++)
			{
				for (int z = 0; z < chunkSize; z++)
				{

					// Create an array of floats representing each corner of a cube and get the value from our terrainMap.
					bool[] cube = new bool[8];
					for (int i = 0; i < 8; i++)
					{

						Vector3Int corner = new Vector3Int(x, y, z) + Constants.CornerTable[i];
						cube[i] = terrainMap[corner.x, corner.y, corner.z];

					}

					// Pass the value into our MarchCube function.
					MarchCube(new Vector3(x, y, z), cube);

				}
			}
		}
		CalculateNormals();
	}
	

	void MarchCube(Vector3 position, bool[] cube)
	{

		// Get the configuration index of this cube.
		int configIndex = GetCubeConfiguration(cube);

		// If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
		if (configIndex == 0 || configIndex == 255)
			return;

		// Loop through the triangles. There are never more than 5 triangles to a cube and only three vertices to a triangle.
		int edgeIndex = 0;
		for (int i = 0; i < 5; i++)
		{
			for (int p = 0; p < 3; p++)
			{

				// Get the current indice. We increment triangleIndex through each loop.
				int indice = Constants.TriangleTable[configIndex, edgeIndex];

				// If the current edgeIndex is -1, there are no more indices and we can exit the function.
				
				if (indice == -1)
					return;

				// Get the vertices for the start and end of this edge.
				Vector3 vert1 = position + Constants.EdgeTable[indice, 0];
				Vector3 vert2 = position + Constants.EdgeTable[indice, 1];

				// Get the midpoint of this edge.
				Vector3 vertPosition = (vert1 + vert2) / 2f;

				// Add to our vertices and triangles list and incremement the edgeIndex.
				vertices.Add(vertPosition + offset);
				triangles.Add(vertices.Count - 1);
				edgeIndex++;

			}
		}
	}
	void CalculateNormals()
    {
		normals = new Vector3[vertices.Count];
		for(int i = 0; i < triangles.Count; i += 3)
        {
			int vertA = triangles[i + 0];
			int vertB = triangles[i + 1];
			int vertC = triangles[i + 2];
			Vector3 surfaceNorm = SurfaceNormalFromIndices(vertA, vertB, vertC);
			normals[vertA] += surfaceNorm;
			normals[vertB] += surfaceNorm;
			normals[vertC] += surfaceNorm;
        }
		for(int i = 0; i < normals.Length; i++)
        {
			normals[i].Normalize();
        }
    }
	Vector3 SurfaceNormalFromIndices(int A, int B, int C)
    {
		Vector3 pointA = vertices[A];
		Vector3 pointB = vertices[B];
		Vector3 pointC = vertices[C];
		return Vector3.Cross(pointB - pointA, pointC - pointA).normalized;
    }

	int GetCubeConfiguration(bool[] cube)
	{

		// Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
		int configurationIndex = 0;
		int[] adder = { 1, 2, 4, 8, 16, 32, 64, 128 };
		for (int i = 0; i < 8; i++)
		{
            if (cube[i])
            {
				configurationIndex += adder[i];
            }

		}

		return configurationIndex;

	}

	

}