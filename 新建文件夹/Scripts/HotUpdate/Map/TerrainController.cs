using JKFrame;
using UnityEngine;

/// <summary>
/// ����״̬ö��
/// </summary>
public enum TerrainState
{
    Request, Enable, Disable
}

/// <summary>
/// �������ɿ�����
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
    /// ���ض�Ӧ��ͼ���������
    /// </summary>
    /// <param name="coord">��ͼ������</param>
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
    /// �������
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
    /// ������ѯ�������ص��β������غ�destroyTime�뽫������
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
    /// ��������
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
    /// ���ٵ���
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
