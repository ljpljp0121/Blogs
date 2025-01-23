using JKFrame;
using UnityEngine;

/// <summary>
/// ��ͼ����
/// ��Ҫ�����õ�ͼ�еؿ��Լ��Ĳ��������һЩ����
/// </summary>
[CreateAssetMenu(menuName = "Config/MapConfig")]
public class MapConfig : ConfigBase
{
    //�Ĳ����ߴ�,300 * 4 * 4 * 4 = 19200
    public float quadTreeSize = 19200;
    public Vector2 mapSize = new Vector2(12000, 12000);     //��ͼ�ߴ�
    public float terrainSize = 300;                         //�ؿ�ߴ�
    public float minQuadTreeNodeSize = 300;                 //�Ĳ�����С������
    public float terrainMaxHeight = 200f;                   //�ؿ�����߶�

    [Header("�Զ��仯����")]
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
