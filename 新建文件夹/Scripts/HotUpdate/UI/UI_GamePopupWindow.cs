using JKFrame;
using UnityEngine;
using UnityEngine.UI;

public class UI_GamePopupWindow : UI_CustomWindowBase
{
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private Button backBtn;
    [SerializeField] private Button quitBtn;
    public override void Init()
    {
        continueBtn.onClick.AddListener(ContinueBtnClick);
        settingBtn.onClick.AddListener(SettingBtnClick);
        backBtn.onClick.AddListener(BackBtnClick);
        quitBtn.onClick.AddListener(QuitBtnClick);
    }

    private void ContinueBtnClick()
    {
        UISystem.Close<UI_GamePopupWindow>();
    }

    private void SettingBtnClick()
    {
        //TODO:游戏设置UI窗口
        UISystem.Show<UI_GameSettingsWindow>();
    }

    private void BackBtnClick()
    {
        //退出到菜单场景
        NetMessageManager.Instance.SendMessageToServer<C2S_Diconnect>(MessageType.C2S_Diconnect, default);
        //等待服务端回复消息后退出到登陆场景，由ClientGlobal来监听
        //ClientGlobal.Instance.EnterLoginScene();
    }

    private void QuitBtnClick()
    {
        //完全关闭应用不用理会网络管理问题
        Application.Quit();
    }
}
