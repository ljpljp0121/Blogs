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
        //TODO:��Ϸ����UI����
        UISystem.Show<UI_GameSettingsWindow>();
    }

    private void BackBtnClick()
    {
        //�˳����˵�����
        NetMessageManager.Instance.SendMessageToServer<C2S_Diconnect>(MessageType.C2S_Diconnect, default);
        //�ȴ�����˻ظ���Ϣ���˳�����½��������ClientGlobal������
        //ClientGlobal.Instance.EnterLoginScene();
    }

    private void QuitBtnClick()
    {
        //��ȫ�ر�Ӧ�ò�����������������
        Application.Quit();
    }
}
