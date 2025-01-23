using JKFrame;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenuWindow : UI_WindowBase
{
    [SerializeField] private Button loginBtn;
    [SerializeField] private Button registerBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private Button exitBtn;

    public override void Init()
    {
        loginBtn.onClick.AddListener(LoginBtnClick);
        registerBtn.onClick.AddListener(RegisterBtnClick);
        settingBtn.onClick.AddListener(SettingBtnClick);
        exitBtn.onClick.AddListener(ExitBtnClick);
    }

    private void LoginBtnClick()
    {
        UISystem.Show<UI_LoginWindow>();
    }

    private void RegisterBtnClick()
    {
        UISystem.Show<UI_RegisterWindow>();
    }

    private void SettingBtnClick()
    {
        UISystem.Show<UI_GameSettingsWindow>();
    }

    private void ExitBtnClick()
    {
        Application.Quit();
    }
}
