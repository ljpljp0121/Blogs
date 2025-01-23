using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// ���������
/// ��Ҫ�Ǹ���ͻ��˺ͷ����֮��ĳ�ʼ��
/// </summary>
public class NetManager : NetworkManager
{
    public static NetManager Instance { get; private set; }
    public UnityTransport unityTransport { get; private set; }
    public NetMessageManager netMessageManager { get; private set; }
    private Dictionary<GameObject, NetworkPrefabInstanceHandler> prefabHandlerDic;

    /// <summary>
    /// �ͻ��˻����˳�ʼ����������������е�
    /// </summary>
    /// <param name="isClient">TrueΪ�ͻ��ˣ�FalseΪ�����</param>
    public void Init(bool isClient)
    {
        Instance = this;
        unityTransport = this.GetComponent<UnityTransport>();
        netMessageManager = this.GetComponent<NetMessageManager>();
        if (isClient)
        {
            InitClient();
        }
        else
        {
            InitServer();
        }
        netMessageManager.Init();
        prefabHandlerDic = new Dictionary<GameObject, NetworkPrefabInstanceHandler>(NetworkConfig.Prefabs.Prefabs.Count);
        foreach (NetworkPrefab item in NetworkConfig.Prefabs.Prefabs)
        {
            NetworkPrefabInstanceHandler handler = new NetworkPrefabInstanceHandler(item.Prefab);
            prefabHandlerDic.Add(item.Prefab, handler);
            PrefabHandler.AddHandler(item.Prefab, handler);
        }
    }

    public void InitClient()
    {
        StartClient();
    }

    public void InitServer()
    {
        StartServer();
    }

    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="clientID">�ͻ���id</param>
    /// <param name="prefab">Ԥ����</param>
    /// <param name="position">����λ��</param>
    /// <returns></returns>
    public NetworkObject SpawnObject(ulong clientID, GameObject prefab, Vector3 position, Quaternion rotation)
    {

        //TODO:��������������� �����
        NetworkObject networkObject = prefabHandlerDic[prefab].Instantiate(clientID, position, rotation);
        networkObject.transform.position = position;
        networkObject.SpawnWithOwnership(clientID);
        networkObject.NetworkShow(clientID);
        return networkObject;
    }

    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="networkObject"></param>
    public void DestroyObject(NetworkObject networkObject)
    {
        networkObject.Despawn();
    }
}
