using JKFrame;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 游戏场景启动
/// 在游戏场景加载的时候会做一些必要的初始化
/// </summary>
public class GameSceneLanunch : MonoBehaviour
{
    IEnumerator Start()
    {
        while (NetworkManager.Singleton == null)
        {
            yield return CoroutineTool.WaitForFrame();
        }
        EventSystem.TypeEventTrigger<GameSceneLanunchEvent>(default);


        Destroy(gameObject);
    }
}
