using JKFrame;
using UnityEngine;

/// <summary>
/// 服务端地图管理器
/// 负责服务端地图的初始化
/// </summary>
public class ServerMapManager : SingletonMono<ServerMapManager>
{
    [SerializeField] private MapConfig mapConfig;
    [SerializeField] private int testRange; //过滤掉外围几圈地图块
    public void Init()
    {
        //一次性加载全部地图
        int width = (int)(mapConfig.mapSize.x / mapConfig.terrainSize);
        int height = (int)(mapConfig.mapSize.y / mapConfig.terrainSize);

        for (int x = 0 + testRange / 2; x < width - testRange / 2; x++)
        {
            for (int y = 0 + testRange / 2; y < height - testRange / 2; y++)
            {
                Vector2Int resCoord = new Vector2Int(x, y);
                string resKey = $"{resCoord.x}_{resCoord.y}";
                Vector2Int terrainCoord = resCoord - mapConfig.terrainResKeyCoordOffset;
                Vector3 pos = new Vector3(terrainCoord.x * mapConfig.terrainSize, 0, terrainCoord.y * mapConfig.terrainSize);
                ServerResManager.InstantiateTerrain(resKey, transform, pos);
            }
        }
    }

}
