using JKFrame;
using UnityEngine;


/// <summary>
/// �����ȫ��
/// �����һ�򿪵ĳ�ʼ��
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
