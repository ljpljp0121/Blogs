using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using JKFrame;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildMenuItems
{
    public const string rootFolderPath = "Builds";
    public const string serverFolderPath = "Server";
    public const string clientFolderPath = "Client";

    [MenuItem("Project/Build/All")]
    public static void BuildAll()
    {
        Server();
        NewClient();
    }

    [MenuItem("Project/Build/Server")]
    public static void Server()
    {
        Debug.Log("开始构建服务器");
        BuildFilterAssemblies.serverMode = true;
         //关闭HybridCLR
        SettingsUtil.Enable = false;

        //关闭Addressables
        AddressableAssetSettingsDefaultObject.Settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;

        List<string> sceneList = new List<string>(EditorSceneManager.sceneCountInBuildSettings);
        for (int i = 0; i < EditorSceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath != null && !scenePath.Contains("Client"))
            {
                Debug.Log($"添加场景到构建列表: {scenePath}");
                sceneList.Add(scenePath);
            }     
        }

        string projectRootPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = sceneList.ToArray(),
            target = BuildTarget.StandaloneWindows64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            locationPathName = $"{projectRootPath}/{rootFolderPath}/{serverFolderPath}/Server.exe"
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);
          //开启HybridCLR
        SettingsUtil.Enable = true;

          //恢复Addressables设置
        AddressableAssetSettingsDefaultObject.Settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.PreferencesValue;

          Debug.Log("服务器构建完成");

    }

    [MenuItem("Project/Build/NewClient")]
    public static void NewClient()
    {
          Debug.Log("开始构建新客户端");

        BuildFilterAssemblies.serverMode = false;

        //执行HybridCLR预构建命令
        PrebuildCommand.GenerateAll();
        //生成DLL字节文件
        GenerateDllBytesFile();
        //清理Addressables目录
        string catalogPath = $"{Application.persistentDataPath}/com.unity.addressables";
        if (Directory.Exists(catalogPath)) Directory.Delete(catalogPath, true);

        List<string> sceneList = new List<string>(EditorSceneManager.sceneCountInBuildSettings);
        for (int i = 0; i < EditorSceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath != null && !scenePath.Contains("Server"))
            {
                Debug.Log($"添加场景到构建列表: {scenePath}");
                sceneList.Add(scenePath);
            }
        }

        string projectRootPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = sceneList.ToArray(),
            target = BuildTarget.StandaloneWindows64,
            subtarget = (int)StandaloneBuildSubtarget.Player,
            locationPathName = $"{projectRootPath}/{rootFolderPath}/{clientFolderPath}/Client.exe"
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);
        //Addressables自动生成更新

        Debug.Log("客户端构建完成");
    }

    [MenuItem("Project/Build/UpdateClient")]
    public static void UpdateClient()
    {
        Debug.Log("开始更新客户端");

        BuildFilterAssemblies.serverMode = false;
        CompileDllCommand.CompileDllActiveBuildTarget();
        GenerateDllBytesFile();
        //执行Addressables内容更新
        string path = ContentUpdateScript.GetContentStateDataPath(false);
        Debug.Log(path);
        AddressableAssetSettings addressableAssetSettings = AddressableAssetSettingsDefaultObject.Settings;
        ContentUpdateScript.BuildContentUpdate(addressableAssetSettings, path);

        Debug.Log("客户端更新完成");
    }

    [MenuItem("Project/Build/GenerateDllBytesFile")]
    public static void GenerateDllBytesFile()
    {
        Debug.Log("开始生成DLL字节文件");
        string aotDllDirPath = System.Environment.CurrentDirectory + "\\"
                                                                   + SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget).Replace('/', '\\');
        string hotupdateDllDirPath = System.Environment.CurrentDirectory + "\\"
                                                                         + SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget).Replace('/', '\\');
        string aotDllTextDirPath = System.Environment.CurrentDirectory
                                   + "\\Assets\\Scripts\\DllBytes\\Aot";
        string hotUpdateDllTextDirPath = System.Environment.CurrentDirectory
                                         + "\\Assets\\Scripts\\DllBytes\\HotUpdate";

        foreach (string dllName in SettingsUtil.AOTAssemblyNames)
        {
            string path = $"{aotDllDirPath}\\{dllName}.dll";
            if (File.Exists(path))
            {
                File.Copy(path, $"{aotDllTextDirPath}\\{dllName}.dll.bytes", true);
            }
            else
            {
                path = $"{hotupdateDllDirPath}\\{dllName}.dll";
                File.Copy(path, $"{aotDllTextDirPath}\\{dllName}.dll.bytes", true);
            }
        }
        foreach (string dllName in SettingsUtil.HotUpdateAssemblyNamesExcludePreserved)
        {
            File.Copy($"{hotupdateDllDirPath}\\{dllName}.dll", $"{hotUpdateDllTextDirPath}\\{dllName}.dll.bytes", true);
        }
        AssetDatabase.Refresh();
        Debug.Log("DLL字节文件生成完毕");
    }

    #region ����˲��Ժ�
    public static bool editorServerTest;
    public const string editorServerTestSymbolString = "SERVER_EDITOR_TEST";

    [MenuItem("Project/TestServer")]
    public static void TestServer()
    {
        editorServerTest = !editorServerTest;
        if (editorServerTest) JKFrameSetting.AddScriptCompilationSymbol(editorServerTestSymbolString);
        else JKFrameSetting.RemoveScriptCompilationSymbol(editorServerTestSymbolString);
        Menu.SetChecked("Project/TestServer", editorServerTest);
    }
    #endregion
}
