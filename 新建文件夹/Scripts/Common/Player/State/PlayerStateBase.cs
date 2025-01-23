#if UNITY_SERVER || UNITY_EDITOR

using JKFrame;

/// <summary>
/// ���״̬����
/// </summary>
public class PlayerStateBase : StateBase
{
    protected PlayerController player;
    public override void Init(IStateMachineOwner owner)
    {
        base.Init(owner);
        player = (PlayerController)owner;
    }
}

#endif