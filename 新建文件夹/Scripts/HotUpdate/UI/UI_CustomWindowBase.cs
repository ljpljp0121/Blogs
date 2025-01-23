using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

public abstract class UI_CustomWindowBase : UI_WindowBase
{
    [SerializeField] private bool activeMouse = true;
    private static bool oldMouseLockState;

    public override void OnShow()
    {
        base.OnShow();
        oldMouseLockState = ClientGlobal.Instance.ActiveMouse;
        ClientGlobal.Instance.ActiveMouse = activeMouse;
    }


    public override void OnClose()
    {
        base.OnClose();
        ClientGlobal.Instance.ActiveMouse = oldMouseLockState;
    }
}
