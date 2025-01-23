
using Unity.Netcode;

public enum MessageType : byte
{
    None,
    C2S_Register,
    S2C_Register,
    C2S_Login,
    S2C_Login,
    C2S_EnterGame,
    C2S_Diconnect,
    S2C_Diconnect,
    C2S_ChatMessage,
    S2C_ChatMessage,
}

public enum ErrorCode : byte
{
    None,               //成功
    AccountFormat,      //账号格式错误
    NameDuplication,    //名称重复
    NameOrPassword,     //账号或密码错误
    AccountRepeatLogin, //重复登陆
}

public struct C2S_Register : INetworkSerializable
{
    public AccountInfo accountInfo;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        accountInfo.NetworkSerialize(serializer);
    }
}

public struct C2S_Login : INetworkSerializable
{
    public AccountInfo accountInfo;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        accountInfo.NetworkSerialize(serializer);
    }
}

public struct AccountInfo : INetworkSerializable
{
    public string playerName;
    public string passward;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref passward);
    }
}

public struct S2C_Register : INetworkSerializable
{
    public ErrorCode errorCode;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);
    }
}

public struct S2C_Login : INetworkSerializable
{
    public ErrorCode errorCode;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);
    }
}

public struct C2S_EnterGame : INetworkSerializable
{
    public ErrorCode errorCode; // 纯占位
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);
    }
}

public struct C2S_Diconnect : INetworkSerializable
{
    public ErrorCode errorCode; //纯占位
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);
    }
}

public struct S2C_Diconnect : INetworkSerializable
{
    public ErrorCode errorCode;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref errorCode);
    }
}

public struct C2S_ChatMessage : INetworkSerializable
{
    public string message;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref message);
    }
}

public struct S2C_ChatMessage : INetworkSerializable
{
    public string playerName;
    public string message;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref message);
    }
}