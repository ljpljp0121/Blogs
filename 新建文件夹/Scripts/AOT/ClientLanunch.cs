using JKFrame;
using UnityEngine;

/// <summary>
/// 客户端启动脚本
/// 在这个脚本中会在客户端启动时做一些必要的初始化
/// </summary>
public class ClientLanunch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        GetComponent<HotUpdateSystem>().StartHotUpdate(null, (bool succeed) =>
        {
            if (succeed)
            {
                OnHotUpdateSucceed();
            }
        });
    }

    private void OnHotUpdateSucceed()
    {
        ResSystem.InstantiateGameObject("ClientGlobal");
    }
}
