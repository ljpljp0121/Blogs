using JKFrame;
using UnityEngine;

/// <summary>
/// 服务端AOI工具类
/// </summary>
public static class AOIUtility
{
    public static float chunkSize { get; private set; }
    public static void Init(float chunkSize)
    {
        AOIUtility.chunkSize = chunkSize;
    }

    /// <summary>
    /// 将世界坐标转换为AOI地图块坐标
    /// </summary>
    /// <param name="worldPosition">世界坐标</param>
    /// <returns>地图块坐标</returns>
    public static Vector2Int GetCoordByWorldPosition(Vector3 worldPosition)
    {
        return new Vector2Int((int)(worldPosition.x / chunkSize), (int)(worldPosition.z / chunkSize));
    }

    /// <summary>
    /// 添加玩家并初始化AOI
    /// </summary>
    /// <param name="player">玩家脚本</param>
    /// <param name="AOICoord">所在AOI坐标</param>
    public static void AddPlayer(PlayerController player, Vector2Int AOICoord)
    {
        EventSystem.TypeEventTrigger(new AOIAddPlayerEvent()
        {
            player = player,
            coord = AOICoord
        });
    }

    /// <summary>
    /// 更新玩家AOI坐标
    /// </summary>
    /// <param name="player">玩家脚本</param>
    /// <param name="oldCoord">更新前玩家AOI坐标</param>
    /// <param name="newCoord">更新后玩家AOI坐标</param>
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
    /// 移除玩家并注销AOI
    /// </summary>
    /// <param name="player">玩家脚本</param>
    /// <param name="AOICoord">所在AOI坐标</param>
    public static void RemovePlayer(PlayerController player, Vector2Int AOICoord)
    {
        EventSystem.TypeEventTrigger(new AOIRemovePlayerEvent()
        {
            player = player,
            coord = AOICoord
        });
    }
}
