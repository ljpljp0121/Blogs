using UnityEngine;
using JKFrame;
using System.Collections;

/// <summary>
/// 客户端游戏场景管理器
/// 主要是做在客户端游戏场景加载时的初始化
/// </summary>

public class ClientGameSceneManager : MonoBehaviour
{
    private void Start()
    {
        UISystem.CloseAllWindow();
        ClientMapManager.Instance.Init();
        PlayerManager.Instance.Init();
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        LoadingWindow loadingWindow = UISystem.Show<LoadingWindow>();
        loadingWindow.Set("Loading...");
        //申请进入游戏
        NetMessageManager.Instance.SendMessageToServer(MessageType.C2S_EnterGame, default(C2S_EnterGame));
        float progress = 0;
        loadingWindow.UpdateProgres(progress, 100);
        yield return CoroutineTool.WaitForFrame();
        while (!ClientMapManager.Instance.IsLoadingCompleted())
        {
            yield return CoroutineTool.WaitForFrame();
            if (progress < 99)
            {
                progress += 0.1f;
                loadingWindow.UpdateProgres(progress, 100);
            }
        }
        progress = 99;
        loadingWindow.UpdateProgres(progress, 100);
        while (!PlayerManager.Instance.IsLoadingCompleted())
        {
            yield return CoroutineTool.WaitForFrame();
        }
        progress = 100;
        loadingWindow.UpdateProgres(progress, 100);
        UISystem.Close<LoadingWindow>();
        UISystem.Show<UI_ChatWindow>();
    }
}

