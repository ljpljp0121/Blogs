using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.UI;

public class UI_ChatWindowItem : MonoBehaviour
{
    [SerializeField] private Text text;
    public void Init(string name, string content)
    {
        text.text = $"<color=yellow>{name}</color> : {content}";
    }
}
