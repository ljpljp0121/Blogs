using JKFrame;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoginWindow : UI_WindowBase
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button submitBtn;
    [SerializeField] private Button registerBtn;
    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField passwordInputField;
    [SerializeField] private Toggle remerberAccountToggle;

    public override void Init()
    {
        closeBtn.onClick.AddListener(CloseBtnClick);
        submitBtn.onClick.AddListener(SubmitBtnClick);
        registerBtn.onClick.AddListener(RegisterBtnClick);
        nameInputField.onValueChanged.AddListener(OnInputFieldsValueChanged);
        passwordInputField.onValueChanged.AddListener(OnInputFieldsValueChanged);
        submitBtn.interactable = false;
    }

    public override void OnShow()
    {
        base.OnShow ();
        submitBtn.interactable = false;
        GameSetting gameSetting = ClientGlobal.Instance.gameSetting;

        nameInputField.text = gameSetting.rememberPlayerName != null ? gameSetting.rememberPlayerName : "";
        passwordInputField.text = gameSetting.rememberPassword != null ? gameSetting.rememberPassword : "";
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S2C_Login, OnS2C_Login);
    }

    public override void OnClose()
    {
        NetMessageManager.Instance.UnRegisterMessageCallback(MessageType.S2C_Login, OnS2C_Login);
    }

    private void CloseBtnClick()
    {
        UISystem.Close<UI_LoginWindow>();
    }

    private void SubmitBtnClick()
    {
        //记住账号
        if (remerberAccountToggle.isOn)
        {
            ClientGlobal.Instance.RemerberAccount(nameInputField.text, passwordInputField.text);
        }
        submitBtn.interactable = false;
        NetMessageManager.Instance.SendMessageToServer(MessageType.C2S_Login, new C2S_Login()
        {
            accountInfo = new AccountInfo()
            {
                playerName = nameInputField.text,
                passward = passwordInputField.text,
            }
        });
    }

    private void RegisterBtnClick()
    {
        UISystem.Close<UI_LoginWindow>();
        UISystem.Show<UI_RegisterWindow>();
    }

    private void OnInputFieldsValueChanged(string arg0)
    {
        submitBtn.interactable = AccountFormatUtility.CheckName(nameInputField.text)
        && AccountFormatUtility.CheckPassword(passwordInputField.text);
    }

    private void OnS2C_Login(ulong clientID, INetworkSerializable serializable)
    {
        submitBtn.interactable = true;
        S2C_Login netMessage = (S2C_Login)serializable;
        if (netMessage.errorCode == ErrorCode.None)
        {
            //进入游戏场景
            ClientGlobal.Instance.EnterGameScene();
        }
        else
        {
            UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalzationKey(netMessage.errorCode.ToString(), Color.red);
        }
    }

}
