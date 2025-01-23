using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// 网络管理器
/// 主要是负责客户端和服务端之间的初始化
/// </summary>
public class NetManager : NetworkManager
{
    public static NetManager Instance { get; private set; }
    public UnityTransport unityTransport { get; private set; }
    public NetMessageManager netMessageManager { get; private set; }
    private Dictionary<GameObject, NetworkPrefabInstanceHandler> prefabHandlerDic;

    /// <summary>
    /// 客户端或服务端初始化，这个是最先运行的
    /// </summary>
    /// <param name="isClient">True为客户端，False为服务端</param>
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
    /// 生成网络对象
    /// </summary>
    /// <param name="clientID">客户端id</param>
    /// <param name="prefab">预制体</param>
    /// <param name="position">生成位置</param>
    /// <returns></returns>
    public NetworkObject SpawnObject(ulong clientID, GameObject prefab, Vector3 position, Quaternion rotation)
    {

        //TODO:后续怎加网络对象 对象池
        NetworkObject networkObject = prefabHandlerDic[prefab].Instantiate(clientID, position, rotation);
        networkObject.transform.position = position;
        networkObject.SpawnWithOwnership(clientID);
        networkObject.NetworkShow(clientID);
        return networkObject;
    }

    /// <summary>
    /// 销毁网络对象
    /// </summary>
    /// <param name="networkObject"></param>
    public void DestroyObject(NetworkObject networkObject)
    {
        networkObject.Despawn();
    }
}
