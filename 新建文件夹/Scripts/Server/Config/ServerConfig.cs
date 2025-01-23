using JKFrame;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 服务端配置
/// 主要是负责服务端需要的一些资源的配置
/// </summary>
[CreateAssetMenu(menuName = "Config/Server/ServerConfig")]
public class ServerConfig : ConfigBase
{
    [Header("通用")]
    public GameObject NetworkManagerPrefab;
    public GameObject ServerGameScenePrefab;

    [Header("玩家")]
    public GameObject playerPrefab;
    public Vector3 playerDefaultPosition;

    [Header("地形")]
    public Dictionary<string, GameObject> terrainDic;
#if UNITY_EDITOR
    [FolderPath] public string terrainFolderPath;
    [Button]
    public void SetTerrainDic()
    {
        if (terrainDic == null) terrainDic = new Dictionary<string, GameObject>();
        terrainDic.Clear();
        string[] files = Directory.GetFiles(terrainFolderPath);//包含*.meta
        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].EndsWith(".meta"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(files[i]);
                terrainDic.Add(prefab.name, prefab);
            }
        }
    }
#endif
}
