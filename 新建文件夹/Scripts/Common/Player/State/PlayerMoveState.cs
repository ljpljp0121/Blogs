#if UNITY_SERVER || UNITY_EDITOR

using System;
using UnityEngine;


/// <summary>
/// 玩家移动状态
/// </summary>
public class PlayerMoveState : PlayerStateBase
{
    public override void Enter()
    {
        player.PlayAnimation("Move");
        player.Player_View.SetRootMotionAction(OnRootMotion);
    }

    public override void Update()
    {
        if (player.inputData.moveDir == Vector3.zero)
        {
            player.ChangeState(PlayerState.Idle);
            return;
        }
        //旋转
        player.Player_View.transform.rotation = Quaternion.RotateTowards(player.Player_View.transform.rotation,
            Quaternion.LookRotation(player.inputData.moveDir), Time.deltaTime * player.RotateSpeed);
    }

    public override void Exit()
    {
        player.Player_View.CleanRootMotionAction();
    }

    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        player.Animator.speed = player.MoveSpeed;
        deltaPosition.y -= player.Gravity * Time.deltaTime;
        player.CharacterController.Move(deltaPosition);
        //更新AOI
        if (deltaPosition != Vector3.zero)
        {
            player.UpdateAOICoord();
        }
    }

}

#endif
