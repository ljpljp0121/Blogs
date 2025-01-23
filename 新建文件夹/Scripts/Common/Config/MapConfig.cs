using JKFrame;
using UnityEngine;

/// <summary>
/// 地图配置
/// 主要是配置地图中地块以及四叉树管理的一些参数
/// </summary>
[CreateAssetMenu(menuName = "Config/MapConfig")]
public class MapConfig : ConfigBase
{
    //四叉树尺寸,300 * 4 * 4 * 4 = 19200
    public float quadTreeSize = 19200;
    public Vector2 mapSize = new Vector2(12000, 12000);     //地图尺寸
    public float terrainSize = 300;                         //地块尺寸
    public float minQuadTreeNodeSize = 300;                 //四叉树最小颗粒度
    public float terrainMaxHeight = 200f;                   //地块的最大高度

    [Header("自动变化参数")]
    public Vector2Int terrainResKeyCoordOffset;             
    public int maxCoordAbsX;
    public int maxCoordAbsY;

    private void Reset()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        terrainResKeyCoordOffset = new Vector2Int((int)(mapSize.x / terrainSize / 2), (int)(mapSize.y / terrainSize / 2));
        maxCoordAbsX = (int)(mapSize.x / terrainSize / 2);
        maxCoordAbsY = (int)(mapSize.y / terrainSize / 2);
    }
}
