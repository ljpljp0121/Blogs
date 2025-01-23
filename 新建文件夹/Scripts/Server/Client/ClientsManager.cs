
using JKFrame;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ���ڷ���˶����ӵĿͻ���������
/// ��Ҫ�Ǹ���ͻ��˵����ӵ�¼ע��ע��
/// </summary>
public class ClientsManager : SingletonMono<ClientsManager>
{
    private Dictionary<ClientState, HashSet<Client>> clientStateDic;
    //Key:clientID
    public Dictionary<ulong, Client> clientIDDic;
    //Key:�˺� Value:ClientID
    private Dictionary<string, ulong> accountDic;

    public void Init()
    {
        clientStateDic = new Dictionary<ClientState, HashSet<Client>>()
        {
            {ClientState.Connected, new HashSet<Client>(100) },
            {ClientState.Logined, new HashSet<Client>(100) },
            {ClientState.Gaming, new HashSet<Client>(100) },
        };
        clientIDDic = new Dictionary<ulong, Client>(100);
        accountDic = new Dictionary<string, ulong>(100);
        NetManager.Instance.OnClientConnectedCallback += OnClientConnected;
        NetManager.Instance.OnClientDisconnectCallback += OnClientNetCodeDisConnected;
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C2S_Register, OnClientRegister);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C2S_Login, OnClientLogin);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C2S_EnterGame, OnClientEnterGame);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C2S_Diconnect, OnClientDiconnect);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C2S_ChatMessage, OnClientChatMessage);
    }

    /// <summary>
    /// ���ÿͻ��˵�״̬
    /// </summary>
    /// <param name="clientID">�ͻ���ID</param>
    /// <param name="newState">Ҫ���õ�״̬</param>
    private void SetClientState(ulong clientID, ClientState newState)
    {
        if (clientIDDic.TryGetValue(clientID, out Client client))
        {
            clientStateDic[client.clientState].Remove(client);
            clientStateDic[newState].Add(client);
            client.clientState = newState;
        }
    }

    /// <summary>
    /// �ͻ�������
    /// </summary>
    private void OnClientConnected(ulong clientID)
    {

        Client client = ResSystem.GetOrNew<Client>();
        client.clientID = clientID;
        clientIDDic.Add(clientID, client);
        SetClientState(clientID, ClientState.Connected);
    }

    /// <summary>
    /// �ͻ���ע��
    /// </summary>
    /// <param name="clientID">�ͻ���ID</param>
    /// <param name="serializable"></param>
    private void OnClientRegister(ulong clientID, INetworkSerializable serializable)
    {
        C2S_Register netMessage = (C2S_Register)serializable;
        AccountInfo accountInfo = netMessage.accountInfo;
        S2C_Register result = new S2C_Register() { errorCode = ErrorCode.None };
        //У���ʽ
        if (!AccountFormatUtility.CheckName(accountInfo.playerName)
            || !AccountFormatUtility.CheckPassword(accountInfo.passward))
        {
            result.errorCode = ErrorCode.AccountFormat;
        }
        //У���Ƿ��������
        else if (DatabaseManager.Instance.GetPlayerData(accountInfo.playerName) != null)
        {
            result.errorCode = ErrorCode.NameDuplication;
        }
        else
        {
            //����ʵ�ʵ��˺�����
            PlayerData playerData = ResSystem.GetOrNew<PlayerData>();
            playerData.characterData = new CharacterData()
            {
                position = ServerResManager.ServerConfig.playerDefaultPosition,
            };
            playerData.name = accountInfo.playerName;
            playerData.password = accountInfo.passward;
            DatabaseManager.Instance.CreatePlayerData(playerData);
        }
        //�ظ��ͻ���
        NetMessageManager.Instance.SendMessageToClient(MessageType.S2C_Register, result, clientID);
    }

    /// <summary>
    /// �ͻ��˵�¼
    /// </summary>
    /// <param name="clientID">�ͻ���ID</param>
    /// <param name="serializable"></param>
    private void OnClientLogin(ulong clientID, INetworkSerializable serializable)
    {
        C2S_Login netMessage = (C2S_Login)serializable;
        AccountInfo accountInfo = netMessage.accountInfo;
        S2C_Login result = new S2C_Login() { errorCode = ErrorCode.None };
        //У���ʽ
        if (!AccountFormatUtility.CheckName(accountInfo.playerName)
            || !AccountFormatUtility.CheckPassword(accountInfo.passward))
        {
            result.errorCode = ErrorCode.AccountFormat;
        }
        //У���Ƿ�����Ѿ�ע��
        else
        {
            PlayerData playerData = DatabaseManager.Instance.GetPlayerData(accountInfo.playerName);
            if (playerData == null || playerData.password != accountInfo.passward)
            {
                result.errorCode = ErrorCode.NameOrPassword;
            }
            else
            {
                //��鼷��
                if (accountDic.TryGetValue(accountInfo.playerName, out ulong oldClientID))
                {
                    //ͨ���ɿͻ���
                    NetMessageManager.Instance.SendMessageToClient(MessageType.S2C_Diconnect, new S2C_Diconnect()
                    {
                        errorCode = ErrorCode.AccountRepeatLogin,
                    }, oldClientID);

                    //���þɿͻ���Ϊ�����ӵ���δ��¼״̬
                    SetClientState(oldClientID, ClientState.Connected);

                    //���ܴ��ڵĽ�ɫ��Ҫ����
                    if (clientIDDic.TryGetValue(oldClientID, out Client oldClient))
                    {
                        if (oldClient.playerController != null)
                        {
                            NetManager.Instance.DestroyObject(oldClient.playerController.NetworkObject);
                            oldClient.playerController = null;
                        }
                        oldClient.playerData = null;
                    }
                }
                accountDic[accountInfo.playerName] = clientID;
                //��ҵ�½�ɹ�,����Client��playerdata
                clientIDDic[clientID].playerData = playerData;
                SetClientState(clientID, ClientState.Logined);
            }
        }
        //�ظ��ͻ���
        NetMessageManager.Instance.SendMessageToClient(MessageType.S2C_Login, result, clientID);
    }

    /// <summary>
    /// �ͻ�����ȫ�Ͽ�����
    /// </summary>
    /// <param name="clientID"></param>
    private void OnClientNetCodeDisConnected(ulong clientID)
    {
        if (clientIDDic.Remove(clientID, out Client client))
        {
            clientStateDic[client.clientState].Remove(client);
            if (client.playerData != null)
            {
                accountDic.Remove(client.playerData.name);
            }
            //Ŀǰ����Netcode�Լ��Ĺ����ͻ��˵��߻��Զ�����������������
            if (client.playerController != null)
            {
                NetManager.Instance.DestroyObject(client.playerController.NetworkObject);
            }
            client.playerData = null;
            client.playerController = null;
            client.OnDestroy();
        }
    }

    /// <summary>
    /// �ͻ��˶Ͽ����ӵ��˵�
    /// </summary>
    private void OnClientDiconnect(ulong clientID, INetworkSerializable serializable)
    {
        Client client = clientIDDic[clientID];
        SetClientState(clientID, ClientState.Connected);
        //���ÿͻ���Ϊ�����ӵ���δ��¼״̬
        if (client.playerController != null)
        {
            NetManager.Instance.DestroyObject(client.playerController.NetworkObject);
            client.playerController = null;
        }
        if (client.playerData != null)
        {
            accountDic.Remove(client.playerData.name);
            client.playerData = null;
        }
        //�ظ���Ϣ
        NetMessageManager.Instance.SendMessageToClient<S2C_Diconnect>(MessageType.S2C_Diconnect, default, clientID);
    }

    /// <summary>
    /// �ͻ�����ʽ������Ϸ
    /// </summary>
    /// <param name="clientID"></param>
    private void OnClientEnterGame(ulong clientID, INetworkSerializable serializable)
    {
        // ����ظ��ͻ��ˣ�ֱ�Ӵ�����ɫ
        Client client = clientIDDic[clientID];
        if (client.clientState == ClientState.Gaming) return;
        SetClientState(clientID, ClientState.Gaming);

        PlayerData playerData = client.playerData;
        CharacterData characterData = playerData.characterData;
        NetworkObject playerObj = NetManager.Instance.SpawnObject(clientID, ServerResManager.ServerConfig.playerPrefab, characterData.position, Quaternion.Euler(0, characterData.rotation_Y, 0));
        client.playerController = playerObj.GetComponent<PlayerController>();
        //TODO:��ҿ���ʹ�ò�ͬ������֮���
    }

    /// <summary>
    /// �ͻ��˷���������Ϣ
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void OnClientChatMessage(ulong clientID, INetworkSerializable serializable)
    {
        string chatMessage = ((C2S_ChatMessage)serializable).message;
        if (string.IsNullOrWhiteSpace(chatMessage)) return;
        if (!clientIDDic.TryGetValue(clientID, out Client sourceClient) || sourceClient.playerData == null) return;
        //���͸�������Ϸ״̬�µĿͻ���
        if (clientStateDic.TryGetValue(ClientState.Gaming, out HashSet<Client> clients))
        {
            S2C_ChatMessage message = new S2C_ChatMessage { playerName = sourceClient.playerData.name, message = chatMessage };
            foreach (Client client in clients)
            {
                NetMessageManager.Instance.SendMessageToClient(MessageType.S2C_ChatMessage, message, client.clientID);
            }
        }
    }

}

