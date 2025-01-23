using UnityEngine;

/// <summary>
/// �������Դ������
/// �������������Դ������
/// </summary>
public static class ServerResManager
{
    public static ServerConfig ServerConfig { get; private set; }
    static ServerResManager()
    {
        ServerConfig = ServerGlobal.Instance.ServerConfig;
    }

    public static NetManager InstantiateNetworkManager()
    {
        GameObject prefab = ServerConfig.NetworkManagerPrefab;
        GameObject instance = GameObject.Instantiate(prefab);
        return instance.GetComponent<NetManager>();
    }

    public static GameObject InstantiateServerGameScene()
    {
        GameObject prefab = ServerConfig.ServerGameScenePrefab;
        return GameObject.Instantiate(prefab);
    }

    public static GameObject InstantiateTerrain(string resKey, Transform parent, Vector3 position)
    {
        GameObject prefab = ServerConfig.terrainDic[resKey];
        GameObject instance = GameObject.Instantiate(prefab, position, Quaternion.identity, parent);
        instance.GetComponent<Terrain>().enabled = false;
        return instance;
    }
}
