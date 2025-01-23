using JKFrame;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 客户端地图管理器，主要是做四叉树管理
/// </summary>
public class ClientMapManager : SingletonMono<ClientMapManager>
{
    public float destroyTerrainTime;
    [SerializeField] private MapConfig mapConfig;
    [SerializeField] private Camera playerCamera;

    public MapConfig MapConfig { get { return mapConfig; } }
    private QuadTree quadTree;
    private Dictionary<Vector2Int, TerrainController> terrainControllerDic = new Dictionary<Vector2Int, TerrainController>(300);
    private List<Vector2Int> destroyTerrainCorrds = new List<Vector2Int>(100);
    private Plane[] cameraPlanes = new Plane[6];
    Vector2Int playerTerrainCoord;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        quadTree = new QuadTree(mapConfig, EnableTerrain, DisableTerrain, CheckVisibility);
    }

    private void Update()
    {
        if (playerCamera == null || quadTree == null) return;
        GeometryUtility.CalculateFrustumPlanes(playerCamera, cameraPlanes);

        //确保加载玩家当前所在块
        playerTerrainCoord = GetTerrainCoordByWorldPos(playerCamera.transform.position);
        EnableTerrain(playerTerrainCoord);

        quadTree.CheckVisibility();

        foreach (var item in terrainControllerDic)
        {
            if (item.Value.CheckAndDestroy())
            {
                destroyTerrainCorrds.Add(item.Key);
            }
        }

        foreach (Vector2Int item in destroyTerrainCorrds)
        {
            terrainControllerDic.Remove(item);
        }
        destroyTerrainCorrds.Clear();
    }

    private void EnableTerrain(Vector2Int coord)
    {
        if (!terrainControllerDic.TryGetValue(coord, out TerrainController controller))
        {
            controller = ResSystem.GetOrNew<TerrainController>();
            controller.Load(coord);
            terrainControllerDic.Add(coord, controller);
        }
        controller.Enable();
    }

    private void DisableTerrain(Vector2Int coord)
    {
        if (terrainControllerDic.TryGetValue(coord, out TerrainController controller))
        {
            controller.Disable();
        }
    }

    private Vector2Int GetTerrainCoordByWorldPos(Vector3 worldPos)
    {
        return new Vector2Int((int)(worldPos.x / mapConfig.terrainSize), (int)(worldPos.z / mapConfig.terrainSize));
    }

    private Vector3 GetWorldPosByTerrainCoord(Vector2Int terrainCoord)
    {
        return new Vector3(terrainCoord.x * mapConfig.terrainSize, 0, terrainCoord.y * mapConfig.terrainSize);
    }

    private bool CheckVisibility(Bounds bounds)
    {
        //希望实际可见范围大一些
        bounds.size *= 2;
        if (GeometryUtility.TestPlanesAABB(cameraPlanes, bounds))
        {
            return true;
        }
        //玩家当前地块附近九宫格也要显示
        Vector3 boundsCenter = GetWorldPosByTerrainCoord(playerTerrainCoord);
        Bounds playerTerrainBounds = new Bounds(boundsCenter, new Vector3(mapConfig.terrainSize, mapConfig.terrainMaxHeight / 3, mapConfig.terrainSize) * 3);
        return bounds.Intersects(playerTerrainBounds);
    }

    public bool IsLoadingCompleted()
    {
        if (terrainControllerDic.Count == 0) return false;
        foreach (var item in terrainControllerDic.Values)
        {
            if (item.terrain == null)
            {
                return false;
            }
        }
        return true;
    }

#if UNITY_EDITOR

    public bool drawGizoms;
    private void OnDrawGizmos()
    {
        if (drawGizoms)
        {
            quadTree?.Draw();
        }
    }

#endif

}
