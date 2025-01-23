using JKFrame;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ���������
/// ��Ҫ�Ǹ���������Ҫ��һЩ��Դ������
/// </summary>
[CreateAssetMenu(menuName = "Config/Server/ServerConfig")]
public class ServerConfig : ConfigBase
{
    [Header("ͨ��")]
    public GameObject NetworkManagerPrefab;
    public GameObject ServerGameScenePrefab;

    [Header("���")]
    public GameObject playerPrefab;
    public Vector3 playerDefaultPosition;

    [Header("����")]
    public Dictionary<string, GameObject> terrainDic;
#if UNITY_EDITOR
    [FolderPath] public string terrainFolderPath;
    [Button]
    public void SetTerrainDic()
    {
        if (terrainDic == null) terrainDic = new Dictionary<string, GameObject>();
        terrainDic.Clear();
        string[] files = Directory.GetFiles(terrainFolderPath);//����*.meta
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
