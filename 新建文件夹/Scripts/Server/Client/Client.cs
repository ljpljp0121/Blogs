using JKFrame;

/// <summary>
/// �ͻ�������
/// ÿ���ͻ��˶�����һ��
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
