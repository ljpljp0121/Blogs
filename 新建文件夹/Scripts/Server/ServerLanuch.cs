using MongoDB.Bson.Serialization.Attributes;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// ����������ű�
/// ���ڷ��������ʱ��һЩ��Ҫ�ĳ�ʼ��
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
