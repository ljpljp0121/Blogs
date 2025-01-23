using JKFrame;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ¼ÓÔØ´°¿Ú
/// </summary>
[UIWindowData(nameof(LoadingWindow), false, nameof(LoadingWindow), 4)]
public class LoadingWindow : UI_WindowBase
{
    [SerializeField] private Text descriptionText;
    [SerializeField] private Slider LoadingBarSlider;
    [SerializeField] private Text progressText;

    public void Set(string description)
    {
        descriptionText.text = description;
    }

    public void UpdateProgres(float current, float max)
    {
        LoadingBarSlider.maxValue = max;
        LoadingBarSlider.value = current;
        progressText.text = $"{Math.Round(current, 2)} / {Math.Round(max, 2)}";
    }

    public void UpdateDownloadProgres(float currentBytes, float maxBytes)
    {
        float currentMB = currentBytes / 1024f / 1024f;
        float maxMB = maxBytes / 1024f / 1024f;
        LoadingBarSlider.maxValue = maxMB;
        LoadingBarSlider.value = currentMB;
        progressText.text = $"{Math.Round(currentMB, 2)}MB / {Math.Round(currentMB, 2)}MB";
    }
}
