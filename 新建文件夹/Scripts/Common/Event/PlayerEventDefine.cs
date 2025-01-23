using UnityEngine;

/// <summary>
/// ���ɱ��ؽ�ɫ�¼�
/// </summary>
public struct InitLocalPlayerEvent
{
    public PlayerController localPlayer;
}

/// <summary>
/// AOI��ӽ�ɫ�¼�
/// </summary>
public struct AOIAddPlayerEvent
{
    public PlayerController player;
    public Vector2Int coord;
}

/// <summary>
/// AOI���½�ɫ�����¼�
/// </summary>
public struct AOIUpdatePlayerCoordEvent
{
    public PlayerController player;
    public Vector2Int oldCoord;
    public Vector2Int newCoord;
}

/// <summary>
/// AOI���ٽ�ɫ�¼�
/// </summary>
public struct AOIRemovePlayerEvent
{
    public PlayerController player;
    public Vector2Int coord;
}