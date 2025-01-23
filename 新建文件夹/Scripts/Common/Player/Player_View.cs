using System;
using UnityEngine;

/// <summary>
/// 玩家视图层
/// </summary>
public class Player_View : MonoBehaviour
{
#if UNITY_SERVER || UNITY_EDITOR
    private Animator animator;

    private Action<Vector3, Quaternion> rootMotionAction;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorMove()
    {
        rootMotionAction?.Invoke(animator.deltaPosition, animator.deltaRotation);
    }

    /// <summary>
    /// 设置玩家根运动监听事件
    /// </summary>
    /// <param name="rootMotionAction"></param>
    public void SetRootMotionAction(Action<Vector3, Quaternion> rootMotionAction)
    {
        this.rootMotionAction = rootMotionAction;
    }

    /// <summary>
    /// 移除玩家根运动监听事件
    /// </summary>
    public void CleanRootMotionAction()
    {
        this.rootMotionAction = null;
    }
#endif

    private void FootStep()
    {

    }
}
