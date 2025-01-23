
#if UNITY_SERVER || UNITY_EDITOR

using UnityEngine;

/// <summary>
/// Íæ¼ÒÕ¾Á¢×´Ì¬
/// </summary>
public class PlayerIdleState : PlayerStateBase
{
    public override void Enter()
    {
        player.PlayAnimation("Idle");
    }
    public override void Update()
    {
        base.Update();
        if (player.inputData.moveDir != Vector3.zero)
        {
            player.ChangeState(PlayerState.Move);
        }
        if (!player.CharacterController.isGrounded)
        {
            player.CharacterController.Move(new Vector3(0, -player.Gravity * Time.deltaTime, 0));
        }
    }
}

#endif