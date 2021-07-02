using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;


public static class FileOperator
{
    public static void WriteMesh(Mesh m, Vector3 pos)
    {
    }
    public static Mesh ReadMesh(Vector3 pos)
    {
        return null;
    }
    public static void Write(string path, string data)
    {
        try
        {
            StreamWriter sw = new StreamWriter(path);
            sw.Write(data);
        }
        catch (Exception e)
        {
            Debug.Log("Error Occured writing to: " + path);
            throw e;
        }
        
    }
    public static string Read(string path)
    {
        string stringAll;
        using (StreamReader sr = new StreamReader(path))
        {
            stringAll = sr.ReadToEnd();
        }
        return stringAll;
    }
}
