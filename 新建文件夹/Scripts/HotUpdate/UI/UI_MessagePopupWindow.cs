using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
public class UI_MessagePopupWindow : UI_WindowBase
{
    [SerializeField] private Text messageText;
    [SerializeField] private Image bglineImg;
    [SerializeField] private Image iconImg;
    [SerializeField] private new Animation animation;

    public void ShowMessageByLocalzationKey(string localzationKey, Color color)
    {
        string message = LocalizationSystem.GetContent<LocalizationStringData>(localzationKey, LocalizationSystem.LanguageType).content;
        ShowMessage(message, color);
    }

    [Button]
    public void ShowMessage(string message, Color color)
    {
        messageText.text = message;
        messageText.color = color;
        bglineImg.color = color;
        iconImg.color = color;
        animation.Play("Popup");
    }

    #region ¶¯»­ÊÂ¼þ
    private void OnPopupEnd()
    {
        animation.Stop();
        UISystem.Close<UI_MessagePopupWindow>();
    }

    #endregion
}
