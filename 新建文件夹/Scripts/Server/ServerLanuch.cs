using MongoDB.Bson.Serialization.Attributes;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// 服务端启动脚本
/// 会在服务端启动时做一些必要的初始化
/// </summary>
public class ServerLanuch : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 30;
        InitServers();
        SceneManager.LoadScene("Game");
    }

    private void InitServers()
    {
        ServerResManager.InstantiateNetworkManager().Init(false);
        Debug.Log("InitServers Succeed");
    }
}
