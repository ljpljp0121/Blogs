using UnityEngine;

/// <summary>
/// 生成本地角色事件
/// </summary>
public struct InitLocalPlayerEvent
{
    public PlayerController localPlayer;
}

/// <summary>
/// AOI添加角色事件
/// </summary>
public struct AOIAddPlayerEvent
{
    public PlayerController player;
    public Vector2Int coord;
}

/// <summary>
/// AOI更新角色坐标事件
/// </summary>
public struct AOIUpdatePlayerCoordEvent
{
    public PlayerController player;
    public Vector2Int oldCoord;
    public Vector2Int newCoord;
}

/// <summary>
/// AOI销毁角色事件
/// </summary>
public struct AOIRemovePlayerEvent
{
    public PlayerController player;
    public Vector2Int coord;
}