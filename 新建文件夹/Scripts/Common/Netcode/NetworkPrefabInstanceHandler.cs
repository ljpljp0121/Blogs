using JKFrame;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 用于添加或删除网络预制件（即具有NetworkObject组件的预制件）的自定义生成和销毁处理程序
/// </summary>
public class NetworkPrefabInstanceHandler : INetworkPrefabInstanceHandler
{
    private GameObject prefab;

    public NetworkPrefabInstanceHandler(GameObject prefab)
    {
        this.prefab = prefab;
    }

    public void Destroy(NetworkObject networkObject)
    {
        networkObject.GameObjectPushPool();
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        NetworkObject networkObject = PoolSystem.GetGameObject<NetworkObject>(prefab.name);
        if (networkObject == null)
        {
            networkObject = GameObject.Instantiate(prefab).GetComponent<NetworkObject>();
            networkObject.name = prefab.name;
        }
        networkObject.transform.position = position;
        networkObject.transform.rotation = rotation;
        return networkObject;
    }
}
