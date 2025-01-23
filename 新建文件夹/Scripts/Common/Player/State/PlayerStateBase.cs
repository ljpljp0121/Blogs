#if UNITY_SERVER || UNITY_EDITOR

using JKFrame;

/// <summary>
/// Íæ¼Ò×´Ì¬»ùÀà
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