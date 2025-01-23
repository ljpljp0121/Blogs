using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.UI;
using System;


public class UI_GameSettingsWindow : UI_CustomWindowBase
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button prevLanguageBtn;
    [SerializeField] private Button nextLanguageBtn;
    [SerializeField] private Image languageImg;
    [SerializeField] private Sprite englishIcon;
    [SerializeField] private Sprite chineseIcon;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundVolumeSlider;


    public override void Init()
    {
        closeBtn.onClick.AddListener(CloseBtnClick);
        prevLanguageBtn.onClick.AddListener(LanguageBtnClick);
        nextLanguageBtn.onClick.AddListener(LanguageBtnClick);
        musicVolumeSlider.onValueChanged.AddListener(MusicVolumeSliderValueChanged);
        soundVolumeSlider.onValueChanged.AddListener(SoundVolumeSliderValueChanged);
    }

    public override void OnShow()
    {
        base.OnShow();
        SetLanguageIcon(ClientGlobal.Instance.gameBaseSetting.LanguageType);
        musicVolumeSlider.SetValueWithoutNotify(ClientGlobal.Instance.gameSetting.musicVolume);
        soundVolumeSlider.SetValueWithoutNotify(ClientGlobal.Instance.gameSetting.soundVolume);
    }

    private void CloseBtnClick()
    {
        UISystem.Close<UI_GameSettingsWindow>();
    }

    private void LanguageBtnClick()
    {
        LocalizationSystem.LanguageType = LocalizationSystem.LanguageType == LanguageType.SimplifiedChinese ? LanguageType.English : LanguageType.SimplifiedChinese;
        ClientGlobal.Instance.gameBaseSetting.LanguageType = LocalizationSystem.LanguageType;
        SetLanguageIcon(LocalizationSystem.LanguageType);
    }

    private void SetLanguageIcon(LanguageType languageType)
    {
        languageImg.sprite = languageType == LanguageType.SimplifiedChinese ? chineseIcon : englishIcon;
    }

    private void MusicVolumeSliderValueChanged(float newValue)
    {
        ClientGlobal.Instance.gameSetting.musicVolume = newValue;
        AudioSystem.BGVolume = newValue;
        EventSystem.TypeEventTrigger<MusicVolumeChangedEvent>(default);
    }

    private void SoundVolumeSliderValueChanged(float newValue)
    {
        ClientGlobal.Instance.gameSetting.soundVolume = newValue;
        AudioSystem.EffectVolume = newValue;
    }

    public override void OnClose()
    {
        base.OnClose();
        ClientGlobal.Instance.SaveGameSetting();
        ClientGlobal.Instance.SaveGameBaseSetting();
    }
}
