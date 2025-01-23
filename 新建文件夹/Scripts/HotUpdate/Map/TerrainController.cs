using JKFrame;
using UnityEngine;

/// <summary>
/// 地形状态枚举
/// </summary>
public enum TerrainState
{
    Request, Enable, Disable
}

/// <summary>
/// 地形生成控制器
/// </summary>
public class TerrainController
{
    public static MapConfig mapConfig;
    static TerrainController()
    {
        mapConfig = ClientMapManager.Instance.MapConfig;
    }

    public Terrain terrain;
    public TerrainState state;
    public float destroyTime;

    /// <summary>
    /// 加载对应地图块坐标地形
    /// </summary>
    /// <param name="coord">地图块坐标</param>
    public void Load(Vector2Int coord)
    {
        Vector2Int resCoord = coord + mapConfig.terrainResKeyCoordOffset;
        string resKey = $"{resCoord.x}_{resCoord.y}";
        state = TerrainState.Request;
        ResSystem.InstantiateGameObjectAsync<Terrain>(resKey, (terrain) =>
        {
            this.terrain = terrain;
            terrain.basemapDistance = 100;
            terrain.heightmapPixelError = 50;
            terrain.heightmapMaximumLOD = 1;
            terrain.detailObjectDensity = 0.9f;
            terrain.treeDistance = 10;
            terrain.treeCrossFadeLength = 10;
            terrain.treeMaximumFullLODCount = 10;
            terrain.transform.position = new Vector3(coord.x * mapConfig.terrainSize, 0, coord.y * mapConfig.terrainSize);
            if (state == TerrainState.Disable)
            {
                terrain.gameObject.SetActive(false);
            }
        }, ClientMapManager.Instance.transform, null, false);
    }

    /// <summary>
    /// 激活地形
    /// </summary>
    public void Enable()
    {
        if (state != TerrainState.Enable)
        {
            destroyTime = 0;
            state = TerrainState.Enable;
            if (terrain != null)
            {
                terrain.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 不断轮询查找隐藏地形并在隐藏后destroyTime秒将其销毁
    /// </summary>
    /// <returns></returns>
    public bool CheckAndDestroy()
    {
        if (state == TerrainState.Disable)
        {
            destroyTime += Time.deltaTime;
            if (destroyTime >= ClientMapManager.Instance.destroyTerrainTime)
            {
                Destroy();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 地形隐藏
    /// </summary>
    public void Disable()
    {
        if (state != TerrainState.Disable)
        {
            state = TerrainState.Disable;
            if (terrain != null)
            {
                terrain.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 销毁地形
    /// </summary>
    public void Destroy()
    {
        if (terrain != null)
        {
            ResSystem.UnloadInstance(terrain.gameObject);
        }
        destroyTime = 0;
        terrain = null;
        this.ObjectPushPool();
    }
}
