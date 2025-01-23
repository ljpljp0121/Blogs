using UnityEngine;

/// <summary>
/// 服务端游戏场景管理器
/// 服务端加载进入游戏做的一些初始化
/// </summary>
public class ServerGameSceneManager : MonoBehaviour
{
    private void Start()
    {
        DatabaseManager.Instance.Init();
        AOIManager.Instance.Init();
        ClientsManager.Instance.Init();
        ServerMapManager.Instance.Init();
    }
}

