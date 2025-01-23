
using JKFrame;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 用于服务端对连接的客户端做管理
/// 主要是负责客户端的连接登录注册注销
/// </summary>
public class ClientsManager : SingletonMono<ClientsManager>
{
    private Dictionary<ClientState, HashSet<Client>> clientStateDic;
    //Key:clientID
    public Dictionary<ulong, Client> clientIDDic;
    //Key:账号 Value:ClientID
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
    /// 设置客户端的状态
    /// </summary>
    /// <param name="clientID">客户端ID</param>
    /// <param name="newState">要设置的状态</param>
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
    /// 客户端连接
    /// </summary>
    private void OnClientConnected(ulong clientID)
    {

        Client client = ResSystem.GetOrNew<Client>();
        client.clientID = clientID;
        clientIDDic.Add(clientID, client);
        SetClientState(clientID, ClientState.Connected);
    }

    /// <summary>
    /// 客户端注册
    /// </summary>
    /// <param name="clientID">客户端ID</param>
    /// <param name="serializable"></param>
    private void OnClientRegister(ulong clientID, INetworkSerializable serializable)
    {
        C2S_Register netMessage = (C2S_Register)serializable;
        AccountInfo accountInfo = netMessage.accountInfo;
        S2C_Register result = new S2C_Register() { errorCode = ErrorCode.None };
        //校验格式
        if (!AccountFormatUtility.CheckName(accountInfo.playerName)
            || !AccountFormatUtility.CheckPassword(accountInfo.passward))
        {
            result.errorCode = ErrorCode.AccountFormat;
        }
        //校验是否已有玩家
        else if (DatabaseManager.Instance.GetPlayerData(accountInfo.playerName) != null)
        {
            result.errorCode = ErrorCode.NameDuplication;
        }
        else
        {
            //生成实际的账号数据
            PlayerData playerData = ResSystem.GetOrNew<PlayerData>();
            playerData.characterData = new CharacterData()
            {
                position = ServerResManager.ServerConfig.playerDefaultPosition,
            };
            playerData.name = accountInfo.playerName;
            playerData.password = accountInfo.passward;
            DatabaseManager.Instance.CreatePlayerData(playerData);
        }
        //回复客户端
        NetMessageManager.Instance.SendMessageToClient(MessageType.S2C_Register, result, clientID);
    }

    /// <summary>
    /// 客户端登录
    /// </summary>
    /// <param name="clientID">客户端ID</param>
    /// <param name="serializable"></param>
    private void OnClientLogin(ulong clientID, INetworkSerializable serializable)
    {
        C2S_Login netMessage = (C2S_Login)serializable;
        AccountInfo accountInfo = netMessage.accountInfo;
        S2C_Login result = new S2C_Login() { errorCode = ErrorCode.None };
        //校验格式
        if (!AccountFormatUtility.CheckName(accountInfo.playerName)
            || !AccountFormatUtility.CheckPassword(accountInfo.passward))
        {
            result.errorCode = ErrorCode.AccountFormat;
        }
        //校验是否玩家已经注册
        else
        {
            PlayerData playerData = DatabaseManager.Instance.GetPlayerData(accountInfo.playerName);
            if (playerData == null || playerData.password != accountInfo.passward)
            {
                result.errorCode = ErrorCode.NameOrPassword;
            }
            else
            {
                //检查挤号
                if (accountDic.TryGetValue(accountInfo.playerName, out ulong oldClientID))
                {
                    //通过旧客户端
                    NetMessageManager.Instance.SendMessageToClient(MessageType.S2C_Diconnect, new S2C_Diconnect()
                    {
                        errorCode = ErrorCode.AccountRepeatLogin,
                    }, oldClientID);

                    //设置旧客户端为已连接但是未登录状态
                    SetClientState(oldClientID, ClientState.Connected);

                    //可能存在的角色需要销毁
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
                //玩家登陆成功,关联Client和playerdata
                clientIDDic[clientID].playerData = playerData;
                SetClientState(clientID, ClientState.Logined);
            }
        }
        //回复客户端
        NetMessageManager.Instance.SendMessageToClient(MessageType.S2C_Login, result, clientID);
    }

    /// <summary>
    /// 客户端完全断开连接
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
            //目前爱用Netcode自己的管理，客户端掉线会自动清除所述的网络对象
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
    /// 客户端断开连接到菜单
    /// </summary>
    private void OnClientDiconnect(ulong clientID, INetworkSerializable serializable)
    {
        Client client = clientIDDic[clientID];
        SetClientState(clientID, ClientState.Connected);
        //设置客户端为已连接但是未登录状态
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
        //回复消息
        NetMessageManager.Instance.SendMessageToClient<S2C_Diconnect>(MessageType.S2C_Diconnect, default, clientID);
    }

    /// <summary>
    /// 客户端正式进入游戏
    /// </summary>
    /// <param name="clientID"></param>
    private void OnClientEnterGame(ulong clientID, INetworkSerializable serializable)
    {
        // 无需回复客户端，直接创建角色
        Client client = clientIDDic[clientID];
        if (client.clientState == ClientState.Gaming) return;
        SetClientState(clientID, ClientState.Gaming);

        PlayerData playerData = client.playerData;
        CharacterData characterData = playerData.characterData;
        NetworkObject playerObj = NetManager.Instance.SpawnObject(clientID, ServerResManager.ServerConfig.playerPrefab, characterData.position, Quaternion.Euler(0, characterData.rotation_Y, 0));
        client.playerController = playerObj.GetComponent<PlayerController>();
        //TODO:玩家可能使用不同的武器之类的
    }

    /// <summary>
    /// 客户端发送聊天消息
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void OnClientChatMessage(ulong clientID, INetworkSerializable serializable)
    {
        string chatMessage = ((C2S_ChatMessage)serializable).message;
        if (string.IsNullOrWhiteSpace(chatMessage)) return;
        if (!clientIDDic.TryGetValue(clientID, out Client sourceClient) || sourceClient.playerData == null) return;
        //发送给所有游戏状态下的客户端
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

