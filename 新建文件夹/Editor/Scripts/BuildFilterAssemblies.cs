using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class BuildFilterAssemblies : IFilterBuildAssemblies
{
    public int callbackOrder => 1;
    public static bool serverMode = false;
    public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
    {
        if (serverMode)
        {
            return assemblies.Where(ass =>
            {
                string assName = Path.GetFileNameWithoutExtension(ass);
                bool reserved = !ass.Contains("HotUpdate");
                if (!reserved)
                {
                    Debug.Log($"ServerBuildFilterAssemblies:过滤了{assName}程序集");
                }
                return reserved;
            }).ToArray();
        }
        else
        {
            return assemblies.Where(ass =>
            {
                string assName = Path.GetFileNameWithoutExtension(ass);
                bool reserved = !ass.Contains("Server");
                if (!reserved)
                {
                    Debug.Log($"ServerBuildFilterAssemblies:过滤了{assName}程序集");
                }
                return reserved;
            }).ToArray();
        }
    }
}
