using JKFrame;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;


/// <summary>
/// 客户端全局
/// 客户端一打开就会做做的初始化以及全局都可能用到的方法
/// </summary>
public class ClientGlobal : SingletonMono<ClientGlobal>
{
    public GameSetting gameSetting { get; private set; }
    public GameBaseSetting gameBaseSetting { get; private set; }

    private bool activeMouse;
    public bool ActiveMouse
    {
        get => activeMouse;
        set
        {
            activeMouse = value;
            Cursor.lockState = activeMouse ? CursorLockMode.None : CursorLockMode.Locked;
            EventSystem.TypeEventTrigger(new MouseActiveStateChangedEvent { activeState = activeMouse });
        }
    }


    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        LoadGameSetting();
        NetworkVaribaleSerializationBinder.Init();
        InitWindowData();
        LocalizationSystem.GlobalConfig = ResSystem.LoadAsset<LocalizationConfig>("GlobalLocalizationConfig");
        ResSystem.InstantiateGameObject<NetManager>("NetworkManager").Init(true);
        EventSystem.AddTypeEventListener<GameSceneLanunchEvent>(OnGameSceneLanunchEvent);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S2C_Diconnect, OnS2C_Diconnect);
        JKLog.Succeed("InitClient");
        EnterLoginScene();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ActiveMouse = !ActiveMouse;
        }
    }

    private void InitWindowData()
    {
        UISystem.AddUIWindowData<UI_MainMenuWindow>(new UIWindowData(false, nameof(UI_MainMenuWindow), 0));
        UISystem.AddUIWindowData<UI_MessagePopupWindow>(new UIWindowData(true, nameof(UI_MessagePopupWindow), 4));
        UISystem.AddUIWindowData<UI_RegisterWindow>(new UIWindowData(false, nameof(UI_RegisterWindow), 1));
        UISystem.AddUIWindowData<UI_LoginWindow>(new UIWindowData(false, nameof(UI_LoginWindow), 1));
        UISystem.AddUIWindowData<UI_GamePopupWindow>(new UIWindowData(false, nameof(UI_GamePopupWindow), 3));
        UISystem.AddUIWindowData<UI_GameSettingsWindow>(new UIWindowData(false, nameof(UI_GameSettingsWindow), 3));
        UISystem.AddUIWindowData<UI_ChatWindow>(new UIWindowData(false, nameof(UI_ChatWindow), 2));
    }

    private void OnGameSceneLanunchEvent(GameSceneLanunchEvent @event)
    {
        ResSystem.InstantiateGameObject("ClientGameScene");
    }

    private void LoadGameSetting()
    {
        gameSetting = SaveSystem.LoadSetting<GameSetting>();
        gameBaseSetting = SaveSystem.LoadSetting<GameBaseSetting>();

        if (gameSetting == null)
        {
            gameSetting = new GameSetting();
            SaveGameSetting();
        }
        if (gameBaseSetting == null)
        {
            gameBaseSetting = new GameBaseSetting();
            gameBaseSetting.LanguageType = Application.systemLanguage == SystemLanguage.ChineseSimplified ? LanguageType.SimplifiedChinese : LanguageType.English;
            SaveGameBaseSetting();
        }
        LocalizationSystem.LanguageType = gameBaseSetting.LanguageType;
        AudioSystem.BGVolume = gameSetting.musicVolume;
        AudioSystem.EffectVolume = gameSetting.soundVolume;
    }

    public void SaveGameSetting()
    {
        SaveSystem.SaveSetting(gameSetting);
    }

    public void SaveGameBaseSetting()
    {
        SaveSystem.SaveSetting(gameBaseSetting);
    }

    public void RemerberAccount(string name, string password)
    {
        gameSetting.rememberPlayerName = name;
        gameSetting.rememberPassword = password;
        SaveGameSetting();
    }

    public void EnterLoginScene()
    {
        UISystem.CloseAllWindow();
        Addressables.LoadSceneAsync("LoginScene").WaitForCompletion();
    }

    public void EnterGameScene()
    {
        SceneSystem.LoadSceneAsync("Game");
    }

    private void OnS2C_Diconnect(ulong clientID, INetworkSerializable serializable)
    {
        S2C_Diconnect message = (S2C_Diconnect)serializable;
        if (message.errorCode != ErrorCode.None)
        {
            UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalzationKey(message.errorCode.ToString(), Color.red);
            Invoke(nameof(EnterLoginScene), 1);
        }
        else
        {
            EnterLoginScene();
        }
    }

}