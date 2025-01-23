
using Cinemachine;
using JKFrame;
using System;
using UnityEngine;

/// <summary>
/// 玩家管理器
/// </summary>
public class PlayerManager : SingletonMono<PlayerManager>
{
    [SerializeField] private CinemachineFreeLook cinemachine;
    public PlayerController localPlayer { get; private set; }

    //玩家是否可以控制角色
    public bool playerControlEnable { get; private set; }


    public void Init()
    {
        EventSystem.AddTypeEventListener<InitLocalPlayerEvent>(OnInitLocalPlayerEvent);
        EventSystem.AddTypeEventListener<MouseActiveStateChangedEvent>(OnMouseActiveStateChangedEvent);
        ClientGlobal.Instance.ActiveMouse = false;
    }

    private void OnDestroy()
    {
        EventSystem.RemoveTypeEventListener<InitLocalPlayerEvent>(OnInitLocalPlayerEvent);
        EventSystem.RemoveTypeEventListener<MouseActiveStateChangedEvent>(OnMouseActiveStateChangedEvent);
    }


    private void OnMouseActiveStateChangedEvent(MouseActiveStateChangedEvent arg)
    {
        //目前之和鼠标显示是否关联
        playerControlEnable = !arg.activeState;
        cinemachine.enabled = playerControlEnable;
        if (IsLoadingCompleted())
        {
            localPlayer.canControl = playerControlEnable;
        }
    }

    private void OnInitLocalPlayerEvent(InitLocalPlayerEvent arg)
    {
        InitLocalPlayer(arg.localPlayer);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UI_GamePopupWindow gamePopupWindow = UISystem.GetWindow<UI_GamePopupWindow>();
            if (gamePopupWindow == null || gamePopupWindow.gameObject.activeInHierarchy)
            {
                UISystem.Show<UI_GamePopupWindow>();
            }
            else
            {
                UISystem.Close<UI_GamePopupWindow>();
            }
        }
    }

    /// <summary>
    /// 初始化本地角色
    /// </summary>
    /// <param name="player">本地角色</param>
    public void InitLocalPlayer(PlayerController player)
    {
        localPlayer = player;
        cinemachine.transform.position = localPlayer.transform.position;

#if !UNITY_SERVER || UNITY_EDITOR
        cinemachine.LookAt = localPlayer.cameraLookAtTarget;
        cinemachine.Follow = localPlayer.cameraFollowTarget;
        localPlayer.canControl = playerControlEnable;
#endif
    }

    public bool IsLoadingCompleted()
    {
        return localPlayer != null;
    }

}


