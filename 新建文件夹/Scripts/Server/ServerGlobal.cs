using JKFrame;
using UnityEngine;


/// <summary>
/// 服务端全局
/// 服务端一打开的初始化
/// </summary>
public class ServerGlobal : SingletonMono<ServerGlobal>
{
    [SerializeField] private ServerConfig serverConfig;

    public ServerConfig ServerConfig { get { return serverConfig; } }
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        EventSystem.AddTypeEventListener<GameSceneLanunchEvent>(OnGameSceneLanunchEvent);
    }

    private void OnGameSceneLanunchEvent(GameSceneLanunchEvent @event)
    {
        ServerResManager.InstantiateServerGameScene();
    }
}
