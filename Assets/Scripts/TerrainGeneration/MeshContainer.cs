using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
class MeshContainer
{
    private Dictionary<Vector3, GameObject> chunks = new Dictionary<Vector3, GameObject>();
   
    public MeshContainer()
    {

    }
    public void Add(Vector3 key, GameObject value)
    {
        chunks[key] = value;
    }
    public GameObject Get(Vector3 key)
    {
        GameObject c;
        try
        {
            c = chunks[key];
            return c;
        }
        catch (KeyNotFoundException)
        {
            GameObject go = new GameObject();
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<MeshRenderer>().enabled = true;
            go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            go.GetComponent<MeshFilter>().mesh = MeshLoader.GetMesh(key);
            Add(key, go);
            return go;
        }
    }
    public Dictionary<Vector3,GameObject>.ValueCollection Values()
    {
        return chunks.Values;
    }
    public Dictionary<Vector3, GameObject>.KeyCollection Keys()
    {
        return chunks.Keys;
    }
}

