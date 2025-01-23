using JKFrame;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 网络消息管理器
/// 主要负责服务端和客户端们互相直接进行的一个消息的发送和接收，并制作监听
/// </summary>
public class NetMessageManager : SingletonMono<NetMessageManager>
{
    private CustomMessagingManager messagingManager => NetManager.Instance.CustomMessagingManager;
    private Dictionary<MessageType, Action<ulong, INetworkSerializable>> receiveMessageCallbackDic = new Dictionary<MessageType, Action<ulong, INetworkSerializable>>();

    public void Init()
    {
        messagingManager.OnUnnamedMessage += ReceiveMessage;
    }

    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="reader">读取的buffer</param>
    private void ReceiveMessage(ulong clientId, FastBufferReader reader)
    {
        try
        {
            reader.ReadValueSafe(out MessageType messageType);
            switch (messageType)
            {
                case MessageType.C2S_Register:
                    reader.ReadValueSafe(out C2S_Register C2S_Register);
                    TriggerMessageCallback(MessageType.C2S_Register, clientId, C2S_Register);
                    break;
                case MessageType.C2S_Login:
                    reader.ReadValueSafe(out C2S_Login C2S_Login);
                    TriggerMessageCallback(MessageType.C2S_Login, clientId, C2S_Login);
                    break;
                case MessageType.S2C_Register:
                    reader.ReadValueSafe(out S2C_Register S2C_Register);
                    TriggerMessageCallback(MessageType.S2C_Register, clientId, S2C_Register);
                    break;
                case MessageType.S2C_Login:
                    reader.ReadValueSafe(out S2C_Login S2C_Login);
                    TriggerMessageCallback(MessageType.S2C_Login, clientId, S2C_Login);
                    break;
                case MessageType.C2S_EnterGame:
                    reader.ReadValueSafe(out C2S_EnterGame C2S_EnterGame);
                    TriggerMessageCallback(MessageType.C2S_EnterGame, clientId, C2S_EnterGame);
                    break;
                case MessageType.C2S_Diconnect:
                    reader.ReadValueSafe(out C2S_Diconnect C2S_Diconnect);
                    TriggerMessageCallback(MessageType.C2S_Diconnect, clientId, C2S_Diconnect);
                    break;
                case MessageType.S2C_Diconnect:
                    reader.ReadValueSafe(out S2C_Diconnect S2C_Diconnect);
                    TriggerMessageCallback(MessageType.S2C_Diconnect, clientId, S2C_Diconnect);
                    break;
                case MessageType.C2S_ChatMessage:
                    reader.ReadValueSafe(out C2S_ChatMessage C2S_ChatMessage);
                    TriggerMessageCallback(MessageType.C2S_ChatMessage, clientId, C2S_ChatMessage);
                    break;
                case MessageType.S2C_ChatMessage:
                    reader.ReadValueSafe(out S2C_ChatMessage S2C_ChatMessage);
                    TriggerMessageCallback(MessageType.S2C_ChatMessage, clientId, S2C_ChatMessage);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("消息接收失败");
        }
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="messageType">消息类型</param>
    /// <param name="data">消息体</param>
    /// <returns>写入后的一个buffer</returns>
    private FastBufferWriter WriteData<T>(MessageType messageType, T data) where T : INetworkSerializable
    {
        //默认1024字节，不足时内部会自动拓展
        FastBufferWriter writer = new FastBufferWriter(1024, Allocator.Temp);
        using (writer)
        {
            writer.WriteValueSafe(messageType);//协议头
            writer.WriteValueSafe(data);//协议主体
        }
        return writer;
    }

    /// <summary>
    /// 发送消息给服务端
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="messageType">消息类型</param>
    /// <param name="data">消息体</param>
    public void SendMessageToServer<T>(MessageType messageType, T data) where T : INetworkSerializable
    {
        messagingManager.SendUnnamedMessage(NetManager.ServerClientId, WriteData(messageType, data));
    }

    /// <summary>
    /// 发送消息给客户端
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="messageType">消息类型</param>
    /// <param name="data">消息体</param>
    /// <param name="clientID">客户端ID</param>
    public void SendMessageToClient<T>(MessageType messageType, T data, ulong clientID) where T : INetworkSerializable
    {
        messagingManager.SendUnnamedMessage(clientID, WriteData(messageType, data));
    }

    /// <summary>
    /// 发送消息给多个客户端
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="messageType">消息类型</param>
    /// <param name="data">消息体</param>
    /// <param name="clientIDList">客户端ID列表</param>
    public void SendMessageToClients<T>(MessageType messageType, T data, IReadOnlyList<ulong> clientIDList) where T : INetworkSerializable
    {
        messagingManager.SendUnnamedMessage(clientIDList, WriteData(messageType, data));
    }

    /// <summary>
    /// 发送消息给所有客户端
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="messageType">消息类型</param>
    /// <param name="data">消息体</param>
    public void SendMessageToAllClient<T>(MessageType messageType, T data) where T : INetworkSerializable
    {
        messagingManager.SendUnnamedMessageToAll(WriteData(messageType, data));
    }

    /// <summary>
    /// 为消息类型注册监听事件
    /// </summary>
    /// <param name="messageType">消息类型</param>
    /// <param name="callback">监听回调事件</param>
    public void RegisterMessageCallback(MessageType messageType, Action<ulong, INetworkSerializable> callback)
    {
        if (receiveMessageCallbackDic.ContainsKey(messageType))
        {
            receiveMessageCallbackDic[messageType] += callback;
        }
        else
        {
            receiveMessageCallbackDic.Add(messageType, callback);
        }
    }

    /// <summary>
    /// 为消息类型注销监听事件
    /// </summary>
    /// <param name="messageType">消息类型</param>
    /// <param name="callback">监听回调事件</param>
    public void UnRegisterMessageCallback(MessageType messageType, Action<ulong, INetworkSerializable> callback)
    {
        if (receiveMessageCallbackDic.ContainsKey(messageType))
        {
            receiveMessageCallbackDic[messageType] -= callback;
        }
    }

    /// <summary>
    /// 触发对应消息类型监听事件
    /// </summary>
    /// <param name="messageType">消息类型</param>
    /// <param name="clientID">客户端ID</param>
    /// <param name="data">消息体</param>
    private void TriggerMessageCallback(MessageType messageType, ulong clientID, INetworkSerializable data)
    {
        if (receiveMessageCallbackDic.TryGetValue(messageType, out Action<ulong, INetworkSerializable> callback))
        {
            callback?.Invoke(clientID, data);
        }
    }
}

