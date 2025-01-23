using JKFrame;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UI_RegisterWindow : UI_WindowBase
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button submitBtn;
    [SerializeField] private Button loginBtn;
    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField passwordInputField;
    [SerializeField] private InputField repeatInputField;

    public override void Init()
    {
        closeBtn.onClick.AddListener(CloseBtnClick);
        submitBtn.onClick.AddListener(SubmitBtnClick);
        loginBtn.onClick.AddListener(LoginBtnClick);
        nameInputField.onValueChanged.AddListener(OnInputFieldsValueChanged);
        passwordInputField.onValueChanged.AddListener(OnInputFieldsValueChanged);
        repeatInputField.onValueChanged.AddListener(OnInputFieldsValueChanged);
        
    }

    public override void OnShow()
    {
        base.OnShow();
        submitBtn.interactable = false;
        passwordInputField.text = "";
        repeatInputField.text = "";
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S2C_Register, OnS2C_Register);
    }

    public override void OnClose()
    {
        NetMessageManager.Instance.UnRegisterMessageCallback(MessageType.S2C_Register, OnS2C_Register);
    }

    private void CloseBtnClick()
    {
        UISystem.Close<UI_RegisterWindow>();
    }

    private void LoginBtnClick()
    {
        UISystem.Close<UI_RegisterWindow>();
        UISystem.Show<UI_LoginWindow>();
    }

    private void OnInputFieldsValueChanged(string arg0)
    {
        submitBtn.interactable = AccountFormatUtility.CheckName(nameInputField.text)
            && AccountFormatUtility.CheckPassword(passwordInputField.text)
            && passwordInputField.text == repeatInputField.text;
    }

    private void SubmitBtnClick()
    {
        submitBtn.interactable = false;
        NetMessageManager.Instance.SendMessageToServer(MessageType.C2S_Register, new C2S_Register()
        {
            accountInfo = new AccountInfo()
            {
                playerName = nameInputField.text,
                passward = passwordInputField.text,
            }
        });
    }

    private void OnS2C_Register(ulong clientID, INetworkSerializable serializable)
    {
        S2C_Register netMessage = (S2C_Register)serializable;
        if (netMessage.errorCode == ErrorCode.None)
        {
            UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalzationKey("×¢²áÒÑ³É¹¦", Color.green);
        }
        else
        {
            UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalzationKey(netMessage.errorCode.ToString(), Color.red);
        }
        submitBtn.interactable = true;
    }

}
