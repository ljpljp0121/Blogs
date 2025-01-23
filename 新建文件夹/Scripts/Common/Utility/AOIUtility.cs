using JKFrame;
using UnityEngine;

/// <summary>
/// �����AOI������
/// </summary>
public static class AOIUtility
{
    public static float chunkSize { get; private set; }
    public static void Init(float chunkSize)
    {
        AOIUtility.chunkSize = chunkSize;
    }

    /// <summary>
    /// ����������ת��ΪAOI��ͼ������
    /// </summary>
    /// <param name="worldPosition">��������</param>
    /// <returns>��ͼ������</returns>
    public static Vector2Int GetCoordByWorldPosition(Vector3 worldPosition)
    {
        return new Vector2Int((int)(worldPosition.x / chunkSize), (int)(worldPosition.z / chunkSize));
    }

    /// <summary>
    /// �����Ҳ���ʼ��AOI
    /// </summary>
    /// <param name="player">��ҽű�</param>
    /// <param name="AOICoord">����AOI����</param>
    public static void AddPlayer(PlayerController player, Vector2Int AOICoord)
    {
        EventSystem.TypeEventTrigger(new AOIAddPlayerEvent()
        {
            player = player,
            coord = AOICoord
        });
    }

    /// <summary>
    /// �������AOI����
    /// </summary>
    /// <param name="player">��ҽű�</param>
    /// <param name="oldCoord">����ǰ���AOI����</param>
    /// <param name="newCoord">���º����AOI����</param>
    public static void UpdatePlayerCoord(PlayerController player, Vector2Int oldCoord, Vector2Int newCoord)
    {
        EventSystem.TypeEventTrigger(new AOIUpdatePlayerCoordEvent()
        {
            player = player,
            oldCoord = oldCoord,
            newCoord = newCoord
        });
    }

    /// <summary>
    /// �Ƴ���Ҳ�ע��AOI
    /// </summary>
    /// <param name="player">��ҽű�</param>
    /// <param name="AOICoord">����AOI����</param>
    public static void RemovePlayer(PlayerController player, Vector2Int AOICoord)
    {
        EventSystem.TypeEventTrigger(new AOIRemovePlayerEvent()
        {
            player = player,
            coord = AOICoord
        });
    }
}
