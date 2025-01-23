using JKFrame;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// AOI管理器
/// 在服务端初始化后使用
/// </summary>
public class AOIManager : SingletonMono<AOIManager>
{
    private readonly static Vector2Int defaultCoord;

    [SerializeField] private float chunkSize = 50;
    [SerializeField] private int visibleChunkRange = 1; //如果是1 则为周围一圈，九宫格
    //<chunkCoord,clientIDs> 对应AOI坐标块内的所有客户端ID
    private Dictionary<Vector2Int, HashSet<ulong>> chunkClientDic = new Dictionary<Vector2Int, HashSet<ulong>>();
    //<chunkCoord,serverObjectIDs> 对应AOI坐标块内的所有服务端对象
    private Dictionary<Vector2Int, HashSet<NetworkObject>> chunkServerObjectDic = new Dictionary<Vector2Int, HashSet<NetworkObject>>();

    static AOIManager()
    {
        defaultCoord = new Vector2Int(int.MinValue, int.MinValue);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        AOIUtility.Init(chunkSize);
        EventSystem.AddTypeEventListener<AOIAddPlayerEvent>(OnAOIAddPlayerEvent);
        EventSystem.AddTypeEventListener<AOIUpdatePlayerCoordEvent>(OnAOIUpdatePlayerCoordEvent);
        EventSystem.AddTypeEventListener<AOIRemovePlayerEvent>(OnRemovePlayerEvent);
    }

    private void OnDestroy()
    {
        EventSystem.RemoveTypeEventListener<AOIAddPlayerEvent>(OnAOIAddPlayerEvent);
        EventSystem.RemoveTypeEventListener<AOIUpdatePlayerCoordEvent>(OnAOIUpdatePlayerCoordEvent);
        EventSystem.RemoveTypeEventListener<AOIRemovePlayerEvent>(OnRemovePlayerEvent);
    }

    private void OnAOIAddPlayerEvent(AOIAddPlayerEvent arg)
    {
        InitClientAOI(arg.player.OwnerClientId, arg.coord);
    }

    private void OnAOIUpdatePlayerCoordEvent(AOIUpdatePlayerCoordEvent arg)
    {
        UpdateClientChunkCoord(arg.player.OwnerClientId, arg.oldCoord, arg.newCoord);
    }

    private void OnRemovePlayerEvent(AOIRemovePlayerEvent arg)
    {
        RemoveClientAOI(arg.player.OwnerClientId, arg.coord);
    }


    #region Client

    /// <summary>
    /// 初始化对应客户端AOI
    /// </summary>
    public void InitClientAOI(ulong clientID, Vector2Int chunkCoord)
    {
        UpdateClientChunkCoord(clientID, defaultCoord, chunkCoord);
    }

    /// <summary>
    /// 更新玩家在AOI地图块上的坐标
    /// </summary>
    /// <param name="clientID">玩家客户端ID</param>
    /// <param name="oldCoord">老的坐标</param>
    /// <param name="newCoord">新的坐标</param>
    public void UpdateClientChunkCoord(ulong clientID, Vector2Int oldCoord, Vector2Int newCoord)
    {
        if (oldCoord == newCoord) { return; }
        //从旧的地图块中移除
        RemoveClientAOI(clientID, oldCoord);

        //是否跨地图块移动(类似传送的情况)
        if (Vector2Int.Distance(oldCoord, newCoord) > 1.5f) //超过单个格子移动的极限距离
        {
            for (int x = -visibleChunkRange; x <= visibleChunkRange; x++)
            {
                for (int y = -visibleChunkRange; y <= visibleChunkRange; y++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + x, oldCoord.y + y);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + x, newCoord.y + y);
                    ShowAndHideForChunk(clientID, hideChunkCoord, showChunkCoord);
                }
            }
        }
        else //正常一个格子的移动距离
        {
            //上,旧的最下面一行隐藏，新的最上面一行显示，往下相反
            if (newCoord.y > oldCoord.y)
            {
                for (int i = -visibleChunkRange; i <= visibleChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + i, oldCoord.y - visibleChunkRange);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + i, newCoord.y + visibleChunkRange);
                    ShowAndHideForChunk(clientID, hideChunkCoord, showChunkCoord);
                }
            }
            else if (newCoord.y < oldCoord.y)
            {
                for (int i = -visibleChunkRange; i <= visibleChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + i, oldCoord.y + visibleChunkRange);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + i, newCoord.y - visibleChunkRange);
                    ShowAndHideForChunk(clientID, hideChunkCoord, showChunkCoord);
                }
            }
            //右,左
            if (newCoord.x > oldCoord.x)
            {
                for (int i = -visibleChunkRange; i <= visibleChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x - visibleChunkRange, oldCoord.y + i);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + visibleChunkRange, newCoord.y + i);
                    ShowAndHideForChunk(clientID, hideChunkCoord, showChunkCoord);
                }
            }
            else if (newCoord.x < oldCoord.x)
            {
                for (int i = -visibleChunkRange; i <= visibleChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + visibleChunkRange, oldCoord.y + i);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x - visibleChunkRange, newCoord.y + i);
                    ShowAndHideForChunk(clientID, hideChunkCoord, showChunkCoord);
                }
            }
        }

        //把客户端加入到当前块
        if (!chunkClientDic.TryGetValue(newCoord, out HashSet<ulong> newCoordClientIDs))
        {
            newCoordClientIDs = ResSystem.GetOrNew<HashSet<ulong>>();
            chunkClientDic.Add(newCoord, newCoordClientIDs);
        }
        newCoordClientIDs.Add(clientID);
    }

    /// <summary>
    /// 注销对应客户端AOI
    /// </summary>
    /// <param name="clientID">移除的客户端ID</param>
    /// <param name="coord">客户端所在坐标块</param>
    public void RemoveClientAOI(ulong clientID, Vector2Int coord)
    {
        if (chunkClientDic.TryGetValue(coord, out HashSet<ulong> clientIDs))
        {
            //如果当前坐标下没有玩家，则回收容器
            if (clientIDs.Remove(clientID) && clientIDs.Count == 0)
            {
                clientIDs.ObjectPushPool();
                chunkClientDic.Remove(coord);
            }
        }
    }


    /// <summary>
    /// 某个地图块的全部客户端和服务端对象对某个客户端不可见，
    /// 同时另一个地图块的全部客户端和服务端对象对某个客户端可见
    /// </summary>
    /// <param name="clientID">客户端ID</param>
    /// <param name="hideChunkCoord">需要不可见的地图块</param>
    /// <param name="showChunkCoord">需要可见的地图块</param>
    private void ShowAndHideForChunk(ulong clientID, Vector2Int hideChunkCoord, Vector2Int showChunkCoord)
    {
        HideClientForChunkClients(clientID, hideChunkCoord);
        HideChunkServerObjectForClient(clientID, showChunkCoord);
        ShowClientForChunkClients(clientID, showChunkCoord);
        ShowChunkServerObjectForClient(clientID, showChunkCoord);
    }

    //某个地图块的全部客户端对象对某个客户端可见
    private void ShowClientForChunkClients(ulong clientID, Vector2Int chunkCoord)
    {
        if (chunkClientDic.TryGetValue(chunkCoord, out HashSet<ulong> clientIDs))
        {
            foreach (var _clientID in clientIDs)
            {
                ClientMutualShow(clientID, _clientID);
            }
        }
    }

    //某个地图块的全部客户端对象对某个客户端不可见
    private void HideClientForChunkClients(ulong clientID, Vector2Int chunkCoord)
    {
        if (chunkClientDic.TryGetValue(chunkCoord, out HashSet<ulong> clientIDs))
        {
            foreach (var _clientID in clientIDs)
            {
                ClientMutualHide(clientID, _clientID);
            }
        }
    }

    //使客户端互相可见
    private void ClientMutualShow(ulong clientA, ulong clientB)
    {
        if (clientA == clientB) { return; }
        if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorkObjectDic)
            && NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientB, out Dictionary<ulong, NetworkObject> bNetWorkObjectDic))
        {
            //使A对于B可见
            foreach (NetworkObject aItem in aNetWorkObjectDic.Values)
            {
                if (!aItem.IsNetworkVisibleTo(clientB))
                {
                    aItem.NetworkShow(clientB);
                }
            }

            //使B对于A可见
            foreach (NetworkObject bItem in bNetWorkObjectDic.Values)
            {
                if (!bItem.IsNetworkVisibleTo(clientA))
                {
                    bItem.NetworkShow(clientA);
                }
            }
        }
    }

    //使客户端互相不可见
    private void ClientMutualHide(ulong clientA, ulong clientB)
    {
        if (clientA == clientB) { return; }
        if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorkObjectDic)
            && NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientB, out Dictionary<ulong, NetworkObject> bNetWorkObjectDic))
        {
            //使A对于B不可见
            foreach (NetworkObject aItem in aNetWorkObjectDic.Values)
            {
                if (aItem.IsNetworkVisibleTo(clientB))
                {
                    aItem.NetworkHide(clientB);
                }
            }

            //使B对于A不可见
            foreach (NetworkObject bItem in bNetWorkObjectDic.Values)
            {
                if (bItem.IsNetworkVisibleTo(clientA))
                {
                    bItem.NetworkHide(clientA);
                }
            }
        }
    }

    //某个地图块的全部服务端对象对某个客户端可见
    private void ShowChunkServerObjectForClient(ulong clientID, Vector2Int chunkCoord)
    {
        if (chunkServerObjectDic.TryGetValue(chunkCoord, out HashSet<NetworkObject> serverObjects))
        {
            foreach (NetworkObject serverObject in serverObjects)
            {
                if (!serverObject.IsNetworkVisibleTo(clientID))
                {
                    serverObject.NetworkShow(clientID);
                }
            }
        }
    }

    //某个地图块的全部服务端对象对某个客户端不可见
    private void HideChunkServerObjectForClient(ulong clientID, Vector2Int chunkCoord)
    {
        if (chunkServerObjectDic.TryGetValue(chunkCoord, out HashSet<NetworkObject> serverObjects))
        {
            foreach (NetworkObject serverObject in serverObjects)
            {
                if (serverObject.IsNetworkVisibleTo(clientID))
                {
                    serverObject.NetworkHide(clientID);
                }
            }
        }
    }

    #endregion

    #region Server

    /// <summary>
    /// 初始化服务端对象AOI
    /// </summary>
    public void InitServerAOI(NetworkObject serverObject, Vector2Int chunkCoord)
    {
        UpdateServerObjectChunkCoord(serverObject, defaultCoord, chunkCoord);
    }

    /// <summary>
    /// 更新服务端对象在AOI地图块上的坐标
    /// </summary>
    /// <param name="clientID">服务端对象</param>
    /// <param name="oldCoord">老的坐标</param>
    /// <param name="newCoord">新的坐标</param>
    public void UpdateServerObjectChunkCoord(NetworkObject serverObject, Vector2Int oldCoord, Vector2Int newCoord)
    {
        if (oldCoord == newCoord) { return; }
        //从旧的地图块中移除
        RemoveServerObject(serverObject, oldCoord);

        //是否跨地图块移动(类似传送的情况)
        if (Vector2Int.Distance(oldCoord, newCoord) > 1.5f) //超过单个格子移动的极限距离
        {
            for (int x = -visibleChunkRange; x <= visibleChunkRange; x++)
            {
                for (int y = -visibleChunkRange; y <= visibleChunkRange; y++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + x, oldCoord.y + y);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + x, newCoord.y + y);
                    ShowAndHideClientsForServerObject(serverObject, hideChunkCoord, showChunkCoord);
                }
            }
        }
        else //正常一个格子的移动距离
        {
            //上,旧的最下面一行隐藏，新的最上面一行显示，往下相反
            if (newCoord.y > oldCoord.y)
            {
                for (int i = -visibleChunkRange; i <= visibleChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + i, oldCoord.y - visibleChunkRange);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + i, newCoord.y + visibleChunkRange);
                    ShowAndHideClientsForServerObject(serverObject, hideChunkCoord, showChunkCoord);
                }
            }
            else if (newCoord.y < oldCoord.y)
            {
                for (int i = -visibleChunkRange; i <= visibleChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + i, oldCoord.y + visibleChunkRange);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + i, newCoord.y - visibleChunkRange);
                    ShowAndHideClientsForServerObject(serverObject, hideChunkCoord, showChunkCoord);
                }
            }
            //右,左
            if (newCoord.x > oldCoord.x)
            {
                for (int i = -visibleChunkRange; i <= visibleChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x - visibleChunkRange, oldCoord.y + i);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + visibleChunkRange, newCoord.y + i);
                    ShowAndHideClientsForServerObject(serverObject, hideChunkCoord, showChunkCoord);
                }
            }
            else if (newCoord.x < oldCoord.x)
            {
                for (int i = -visibleChunkRange; i <= visibleChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + visibleChunkRange, oldCoord.y + i);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x - visibleChunkRange, newCoord.y + i);
                    ShowAndHideClientsForServerObject(serverObject, hideChunkCoord, showChunkCoord);
                }
            }
        }

        //把服务端对象加入到当前块
        if (!chunkServerObjectDic.TryGetValue(newCoord, out HashSet<NetworkObject> serverObjects))
        {
            serverObjects = ResSystem.GetOrNew<HashSet<NetworkObject>>();
            chunkServerObjectDic.Add(newCoord, serverObjects);
        }
        serverObjects.Add(serverObject);
    }

    /// <summary>
    /// 从地图块中移除指定服务端对象
    /// </summary>
    /// <param name="serverObject">服务端对象</param>
    /// <param name="chunkCoord">地图块</param>
    public void RemoveServerObject(NetworkObject serverObject, Vector2Int chunkCoord)
    {
        if (chunkServerObjectDic.TryGetValue(chunkCoord, out HashSet<NetworkObject> serverObjects))
        {
            serverObjects.Remove(serverObject);
        }
    }

    /// <summary>
    /// 使一个服务端对象对某个地图块下的全部客户端不可见
    /// 同时使一个服务端对象对某个地图块下的全部客户端可见，
    /// </summary>
    /// <param name="serverObject">服务端对象</param>
    /// <param name="hideChunkCoord">需要不可见的地图块</param>
    /// <param name="showChunkCoord">需要可见的地图块</param>
    private void ShowAndHideClientsForServerObject(NetworkObject serverObject, Vector2Int hideChunkCoord, Vector2Int showChunkCoord)
    {
        ShowClientsForServerObject(serverObject, showChunkCoord);
        HideClientsForServerObject(serverObject, hideChunkCoord);
    }

    //使一个服务端对象对某个地图块下的全部客户端可见
    private void ShowClientsForServerObject(NetworkObject serverObject, Vector2Int chunkCoord)
    {
        if (chunkClientDic.TryGetValue(chunkCoord, out HashSet<ulong> clientIDs))
        {
            foreach (ulong clientID in clientIDs)
            {
                if (!serverObject.IsNetworkVisibleTo(clientID))
                {
                    serverObject.NetworkShow(clientID);
                }
            }
        }
    }

    //使一个服务端对象对某个地图块下的全部客户端不可见
    private void HideClientsForServerObject(NetworkObject serverObject, Vector2Int chunkCoord)
    {
        if (chunkClientDic.TryGetValue(chunkCoord, out HashSet<ulong> clientIDs))
        {
            foreach (ulong clientID in clientIDs)
            {
                if (serverObject.IsNetworkVisibleTo(clientID))
                {
                    serverObject.NetworkHide(clientID);
                }
            }
        }
    }

    #endregion

}
