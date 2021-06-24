using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FileOperator
{
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
