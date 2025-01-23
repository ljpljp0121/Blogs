using JKFrame;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// AOI������
/// �ڷ���˳�ʼ����ʹ��
/// </summary>
public class AOIManager : SingletonMono<AOIManager>
{
    private readonly static Vector2Int defaultCoord;

    [SerializeField] private float chunkSize = 50;
    [SerializeField] private int visibleChunkRange = 1; //�����1 ��Ϊ��ΧһȦ���Ź���
    //<chunkCoord,clientIDs> ��ӦAOI������ڵ����пͻ���ID
    private Dictionary<Vector2Int, HashSet<ulong>> chunkClientDic = new Dictionary<Vector2Int, HashSet<ulong>>();
    //<chunkCoord,serverObjectIDs> ��ӦAOI������ڵ����з���˶���
    private Dictionary<Vector2Int, HashSet<NetworkObject>> chunkServerObjectDic = new Dictionary<Vector2Int, HashSet<NetworkObject>>();

    static AOIManager()
    {
        defaultCoord = new Vector2Int(int.MinValue, int.MinValue);
    }

    /// <summary>
    /// ��ʼ��
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
    /// ��ʼ����Ӧ�ͻ���AOI
    /// </summary>
    public void InitClientAOI(ulong clientID, Vector2Int chunkCoord)
    {
        UpdateClientChunkCoord(clientID, defaultCoord, chunkCoord);
    }

    /// <summary>
    /// ���������AOI��ͼ���ϵ�����
    /// </summary>
    /// <param name="clientID">��ҿͻ���ID</param>
    /// <param name="oldCoord">�ϵ�����</param>
    /// <param name="newCoord">�µ�����</param>
    public void UpdateClientChunkCoord(ulong clientID, Vector2Int oldCoord, Vector2Int newCoord)
    {
        if (oldCoord == newCoord) { return; }
        //�Ӿɵĵ�ͼ�����Ƴ�
        RemoveClientAOI(clientID, oldCoord);

        //�Ƿ���ͼ���ƶ�(���ƴ��͵����)
        if (Vector2Int.Distance(oldCoord, newCoord) > 1.5f) //�������������ƶ��ļ��޾���
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
        else //����һ�����ӵ��ƶ�����
        {
            //��,�ɵ�������һ�����أ��µ�������һ����ʾ�������෴
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
            //��,��
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

        //�ѿͻ��˼��뵽��ǰ��
        if (!chunkClientDic.TryGetValue(newCoord, out HashSet<ulong> newCoordClientIDs))
        {
            newCoordClientIDs = ResSystem.GetOrNew<HashSet<ulong>>();
            chunkClientDic.Add(newCoord, newCoordClientIDs);
        }
        newCoordClientIDs.Add(clientID);
    }

    /// <summary>
    /// ע����Ӧ�ͻ���AOI
    /// </summary>
    /// <param name="clientID">�Ƴ��Ŀͻ���ID</param>
    /// <param name="coord">�ͻ������������</param>
    public void RemoveClientAOI(ulong clientID, Vector2Int coord)
    {
        if (chunkClientDic.TryGetValue(coord, out HashSet<ulong> clientIDs))
        {
            //�����ǰ������û����ң����������
            if (clientIDs.Remove(clientID) && clientIDs.Count == 0)
            {
                clientIDs.ObjectPushPool();
                chunkClientDic.Remove(coord);
            }
        }
    }


    /// <summary>
    /// ĳ����ͼ���ȫ���ͻ��˺ͷ���˶����ĳ���ͻ��˲��ɼ���
    /// ͬʱ��һ����ͼ���ȫ���ͻ��˺ͷ���˶����ĳ���ͻ��˿ɼ�
    /// </summary>
    /// <param name="clientID">�ͻ���ID</param>
    /// <param name="hideChunkCoord">��Ҫ���ɼ��ĵ�ͼ��</param>
    /// <param name="showChunkCoord">��Ҫ�ɼ��ĵ�ͼ��</param>
    private void ShowAndHideForChunk(ulong clientID, Vector2Int hideChunkCoord, Vector2Int showChunkCoord)
    {
        HideClientForChunkClients(clientID, hideChunkCoord);
        HideChunkServerObjectForClient(clientID, showChunkCoord);
        ShowClientForChunkClients(clientID, showChunkCoord);
        ShowChunkServerObjectForClient(clientID, showChunkCoord);
    }

    //ĳ����ͼ���ȫ���ͻ��˶����ĳ���ͻ��˿ɼ�
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

    //ĳ����ͼ���ȫ���ͻ��˶����ĳ���ͻ��˲��ɼ�
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

    //ʹ�ͻ��˻���ɼ�
    private void ClientMutualShow(ulong clientA, ulong clientB)
    {
        if (clientA == clientB) { return; }
        if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorkObjectDic)
            && NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientB, out Dictionary<ulong, NetworkObject> bNetWorkObjectDic))
        {
            //ʹA����B�ɼ�
            foreach (NetworkObject aItem in aNetWorkObjectDic.Values)
            {
                if (!aItem.IsNetworkVisibleTo(clientB))
                {
                    aItem.NetworkShow(clientB);
                }
            }

            //ʹB����A�ɼ�
            foreach (NetworkObject bItem in bNetWorkObjectDic.Values)
            {
                if (!bItem.IsNetworkVisibleTo(clientA))
                {
                    bItem.NetworkShow(clientA);
                }
            }
        }
    }

    //ʹ�ͻ��˻��಻�ɼ�
    private void ClientMutualHide(ulong clientA, ulong clientB)
    {
        if (clientA == clientB) { return; }
        if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorkObjectDic)
            && NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientB, out Dictionary<ulong, NetworkObject> bNetWorkObjectDic))
        {
            //ʹA����B���ɼ�
            foreach (NetworkObject aItem in aNetWorkObjectDic.Values)
            {
                if (aItem.IsNetworkVisibleTo(clientB))
                {
                    aItem.NetworkHide(clientB);
                }
            }

            //ʹB����A���ɼ�
            foreach (NetworkObject bItem in bNetWorkObjectDic.Values)
            {
                if (bItem.IsNetworkVisibleTo(clientA))
                {
                    bItem.NetworkHide(clientA);
                }
            }
        }
    }

    //ĳ����ͼ���ȫ������˶����ĳ���ͻ��˿ɼ�
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

    //ĳ����ͼ���ȫ������˶����ĳ���ͻ��˲��ɼ�
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
    /// ��ʼ������˶���AOI
    /// </summary>
    public void InitServerAOI(NetworkObject serverObject, Vector2Int chunkCoord)
    {
        UpdateServerObjectChunkCoord(serverObject, defaultCoord, chunkCoord);
    }

    /// <summary>
    /// ���·���˶�����AOI��ͼ���ϵ�����
    /// </summary>
    /// <param name="clientID">����˶���</param>
    /// <param name="oldCoord">�ϵ�����</param>
    /// <param name="newCoord">�µ�����</param>
    public void UpdateServerObjectChunkCoord(NetworkObject serverObject, Vector2Int oldCoord, Vector2Int newCoord)
    {
        if (oldCoord == newCoord) { return; }
        //�Ӿɵĵ�ͼ�����Ƴ�
        RemoveServerObject(serverObject, oldCoord);

        //�Ƿ���ͼ���ƶ�(���ƴ��͵����)
        if (Vector2Int.Distance(oldCoord, newCoord) > 1.5f) //�������������ƶ��ļ��޾���
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
        else //����һ�����ӵ��ƶ�����
        {
            //��,�ɵ�������һ�����أ��µ�������һ����ʾ�������෴
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
            //��,��
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

        //�ѷ���˶�����뵽��ǰ��
        if (!chunkServerObjectDic.TryGetValue(newCoord, out HashSet<NetworkObject> serverObjects))
        {
            serverObjects = ResSystem.GetOrNew<HashSet<NetworkObject>>();
            chunkServerObjectDic.Add(newCoord, serverObjects);
        }
        serverObjects.Add(serverObject);
    }

    /// <summary>
    /// �ӵ�ͼ�����Ƴ�ָ������˶���
    /// </summary>
    /// <param name="serverObject">����˶���</param>
    /// <param name="chunkCoord">��ͼ��</param>
    public void RemoveServerObject(NetworkObject serverObject, Vector2Int chunkCoord)
    {
        if (chunkServerObjectDic.TryGetValue(chunkCoord, out HashSet<NetworkObject> serverObjects))
        {
            serverObjects.Remove(serverObject);
        }
    }

    /// <summary>
    /// ʹһ������˶����ĳ����ͼ���µ�ȫ���ͻ��˲��ɼ�
    /// ͬʱʹһ������˶����ĳ����ͼ���µ�ȫ���ͻ��˿ɼ���
    /// </summary>
    /// <param name="serverObject">����˶���</param>
    /// <param name="hideChunkCoord">��Ҫ���ɼ��ĵ�ͼ��</param>
    /// <param name="showChunkCoord">��Ҫ�ɼ��ĵ�ͼ��</param>
    private void ShowAndHideClientsForServerObject(NetworkObject serverObject, Vector2Int hideChunkCoord, Vector2Int showChunkCoord)
    {
        ShowClientsForServerObject(serverObject, showChunkCoord);
        HideClientsForServerObject(serverObject, hideChunkCoord);
    }

    //ʹһ������˶����ĳ����ͼ���µ�ȫ���ͻ��˿ɼ�
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

    //ʹһ������˶����ĳ����ͼ���µ�ȫ���ͻ��˲��ɼ�
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
