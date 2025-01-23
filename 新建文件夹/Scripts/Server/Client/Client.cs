using JKFrame;

/// <summary>
/// 客户端数据
/// 每个客户端都会有一个
/// </summary>
public class Client
{
    public ulong clientID;
    public ClientState clientState;
    public PlayerData playerData;
    public PlayerController playerController;

    public void OnDestroy()
    {
        playerData = null;
        this.ObjectPushPool();
    }
}
