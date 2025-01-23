using System;
using UnityEngine;

/// <summary>
/// 四叉树类
/// </summary>
public class QuadTree
{
    private static MapConfig mapConfig;
    private static Action<Vector2Int> onTerrainEnable;
    private static Action<Vector2Int> onTerrainDisable;
    private static Func<Bounds, bool> onCheckVisibility;

    /// <summary>
    /// 四叉树节点类
    /// </summary>
    private class Node
    {
        public Bounds bounds;
        private Node leftAndTop;
        private Node leftAndBottom;
        private Node rightAndBottom;
        private Node rightAndTop;
        private bool isTerrain;
        private Vector2Int terrainCoord;

        public Node(Bounds bounds, bool divide)
        {
            this.bounds = bounds;
            isTerrain = CheckTerrain(out terrainCoord);
            if (divide && bounds.size.x > mapConfig.minQuadTreeNodeSize)
            {
                Divide();
            }
        }


        //查看是否是最小颗粒
        private bool CheckTerrain(out Vector2Int coord)
        {
            Vector3 size = bounds.size;
            bool isTerrain = (size.x == mapConfig.terrainSize && size.z == mapConfig.terrainSize);
            coord = Vector2Int.zero;
            if (isTerrain)
            {
                coord.x = (int)(bounds.center.x / mapConfig.terrainSize);
                coord.y = (int)(bounds.center.z / mapConfig.terrainSize);
                //剔除mapSize以外的符合尺寸的terrain
                isTerrain = Mathf.Abs(coord.x) < mapConfig.maxCoordAbsX && Mathf.Abs(coord.y) < mapConfig.maxCoordAbsY;
            }
            return isTerrain;
        }

        //分裂
        private void Divide()
        {
            float halfSize = bounds.size.x / 2;
            float positionOffset = halfSize / 2;
            float halfHeight = mapConfig.terrainMaxHeight / 2;
            Vector3 childSize = new Vector3(halfSize, mapConfig.terrainMaxHeight, halfSize);
            leftAndTop = new Node(new Bounds(new Vector3(bounds.center.x - positionOffset, halfHeight, bounds.center.z + positionOffset), childSize), true);
            leftAndBottom = new Node(new Bounds(new Vector3(bounds.center.x - positionOffset, halfHeight, bounds.center.z - positionOffset), childSize), true);
            rightAndTop = new Node(new Bounds(new Vector3(bounds.center.x + positionOffset, halfHeight, bounds.center.z + positionOffset), childSize), true);
            rightAndBottom = new Node(new Bounds(new Vector3(bounds.center.x + positionOffset, halfHeight, bounds.center.z - positionOffset), childSize), true);
        }

        private bool active = false;

        public void CheckVisibility()
        {
            bool newActiveState = onCheckVisibility(bounds);
            //原本可见，现在可见
            //原本不可见，现在可见
            //if ((active && newActiveState) || (!active && newActiveState))
            if (newActiveState)
            {
                if (isTerrain)
                {
                    onTerrainEnable.Invoke(terrainCoord);
                }
                else
                {
                    leftAndTop?.CheckVisibility();
                    leftAndBottom?.CheckVisibility();
                    rightAndTop?.CheckVisibility();
                    rightAndBottom?.CheckVisibility();
                }
            }
            //原本可见，现在不可见
            else if (active && !newActiveState)
            {
                Disable();
            }
            active = newActiveState;
        }

        public void Disable()
        {
            leftAndTop?.Disable();
            leftAndBottom?.Disable();
            rightAndTop?.Disable();
            rightAndBottom?.Disable();
            if (isTerrain)
            {
                onTerrainDisable.Invoke(terrainCoord);
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// 绘制四叉树节点
        /// </summary>
        public void Draw()
        {
            Gizmos.color = isTerrain ? Color.green : Color.white;
            Gizmos.DrawCube(bounds.center, bounds.size * 0.9f);
            Gizmos.color = Color.white;
            leftAndTop?.Draw();
            leftAndBottom?.Draw();
            rightAndTop?.Draw();
            rightAndBottom?.Draw();
        }
#endif
    }


    private Node rootNode;

    public QuadTree(MapConfig config, Action<Vector2Int> terrainEnable, Action<Vector2Int> terrainDisable, Func<Bounds, bool> checkVisibility)
    {
        mapConfig = config;
        onTerrainEnable = terrainEnable;
        onTerrainDisable = terrainDisable;
        onCheckVisibility = checkVisibility;
        Bounds rootBounds = new Bounds(new Vector3(0, mapConfig.terrainMaxHeight / 2, 0)
        , new Vector3(mapConfig.quadTreeSize, mapConfig.terrainMaxHeight, mapConfig.quadTreeSize));
        rootNode = new Node(rootBounds, true);
    }

    public void CheckVisibility()
    {
        rootNode.CheckVisibility();
    }

#if UNITY_EDITOR

    /// <summary>
    /// 绘制四叉树
    /// </summary>
    public void Draw()
    {
        rootNode?.Draw();
    }

#endif
}
