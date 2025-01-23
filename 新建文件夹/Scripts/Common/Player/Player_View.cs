using System;
using UnityEngine;

/// <summary>
/// �����ͼ��
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
    /// ������Ҹ��˶������¼�
    /// </summary>
    /// <param name="rootMotionAction"></param>
    public void SetRootMotionAction(Action<Vector3, Quaternion> rootMotionAction)
    {
        this.rootMotionAction = rootMotionAction;
    }

    /// <summary>
    /// �Ƴ���Ҹ��˶������¼�
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
