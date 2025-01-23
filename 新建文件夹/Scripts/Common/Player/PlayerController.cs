using JKFrame;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 玩家控制器(公共部分)
/// </summary>
public partial class PlayerController : NetworkBehaviour
{
    private NetVaribale<PlayerState> currentState = new NetVaribale<PlayerState>(PlayerState.None);

    /// <summary>
    /// 网络对象生成时
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
#if !UNITY_SERVER || UNITY_EDITOR
            Client_OnNetworkSpawn();
#endif
        }
        else
        {
#if UNITY_SERVER || UNITY_EDITOR
            Server_OnNetworkSpawn();
#endif
        }
    }

    // 相当于调用服务端上自身的本体
    [ServerRpc(RequireOwnership = false)]
    private void SendInputMoveDirServerRpc(Vector3 moveDir)
    {
#if UNITY_SERVER || UNITY_EDITOR
        Server_ReceiveMoveInput(moveDir);

#endif
    }

    /// <summary>
    /// 网络对象销毁时
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
#if !UNITY_SERVER || UNITY_EDITOR
            Client_OnNetworkDespawn();
#endif
        }
        else
        {
#if UNITY_SERVER || UNITY_EDITOR
            Server_OnNetworkDespawn();
#endif
        }

    }

}

#if !UNITY_SERVER || UNITY_EDITOR

/// <summary>
/// 玩家控制器(客户端部分)
/// </summary>
public partial class PlayerController : NetworkBehaviour
{
    public Transform cameraLookAtTarget;
    public Transform cameraFollowTarget;
    public bool canControl;

    private void Start()
    {
        //Start一定OnNetworkSpawn后执行，如果IsSpawned==false，说明是个异常对象
        if (!IsSpawned)
        {
            NetManager.Instance.DestroyObject(this.NetworkObject);
        }
    }

    /// <summary>
    /// 客户端上生成网络对象
    /// </summary>
    private void Client_OnNetworkSpawn()
    {
        if (IsOwner)
        {
            EventSystem.TypeEventTrigger(new InitLocalPlayerEvent() { localPlayer = this });
            this.AddUpdate(LocalClient_Update);
        }
    }

    private Vector3 lastInputDir = Vector3.zero;

    /// <summary>
    /// 本地客户端循环
    /// </summary>
    private void LocalClient_Update()
    {
        if (currentState.Value == PlayerState.None) { return; }
        Vector3 inputDir = Vector3.zero;
        if (canControl)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            inputDir = new Vector3(h, 0, v);
        }
        if (inputDir == Vector3.zero && lastInputDir == Vector3.zero) { return; }
        lastInputDir = inputDir;

        float cameraEulerAngleY = Camera.main.transform.eulerAngles.y;
        //让四元数和向量相乘:让这个向量按照四元数表达的角度进行旋转后的到一个新的方向
        SendInputMoveDirServerRpc(Quaternion.Euler(0, cameraEulerAngleY, 0) * inputDir);
    }

    private void Client_OnNetworkDespawn()
    {

    }
}

#endif

#if UNITY_SERVER || UNITY_EDITOR



/// <summary>
/// 玩家控制器(服务端部分)
/// </summary>
public partial class PlayerController : NetworkBehaviour, IStateMachineOwner
{

    public class InputData
    {
        public Vector3 moveDir;
    }
    #region 字段

    /// <summary>
    /// 序列化Bug，Inspector窗口传值会失败 [SerializeField]
    /// </summary>

    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float gravity;
    private CharacterController characterController;
    private Player_View playerView;
    private Animator animator;
    private Vector2Int currentAOICoord;
    private StateMachine stateMachine;

    #endregion

    #region 属性  

    public float MoveSpeed { get { return moveSpeed; } }
    public float RotateSpeed { get { return rotateSpeed; } }
    public CharacterController CharacterController { get { return characterController; } }
    public Animator Animator { get { return animator; } }
    public Player_View Player_View { get { return playerView; } }
    public float Gravity { get { return gravity; } }
    public InputData inputData { get; private set; }

    #endregion


    /// <summary>
    /// 服务端上生成网络对象
    /// </summary>
    private void Server_OnNetworkSpawn()
    {
        characterController = GetComponent<CharacterController>();
        playerView = this.GetComponentInChildren<Player_View>();
        animator = this.GetComponentInChildren<Animator>();
        stateMachine = new StateMachine();
        stateMachine.Init(this);
        inputData = new InputData();
        currentAOICoord = AOIUtility.GetCoordByWorldPosition(transform.position);
        AOIUtility.AddPlayer(this, currentAOICoord);
        ChangeState(PlayerState.Idle);
    }

    /// <summary>
    /// 服务端接收输入的移动指令
    /// </summary>
    /// <param name="moveDir"></param>
    private void Server_ReceiveMoveInput(Vector3 moveDir)
    {
        inputData.moveDir = moveDir.normalized;
        //状态类中根据输入情况进行运算
    }

    private void Server_OnNetworkDespawn()
    {
        stateMachine.Destroy();
        AOIUtility.RemovePlayer(this, currentAOICoord);
    }

    /// <summary>
    /// 改变状态进一步封装
    /// </summary>
    public void ChangeState(PlayerState newState)
    {
        currentState.Value = newState;
        switch (newState)
        {
            case PlayerState.Idle:
                stateMachine.ChangeState<PlayerIdleState>(this);
                break;
            case PlayerState.Move:
                stateMachine.ChangeState<PlayerMoveState>(this);
                break;
        }
    }

    /// <summary>
    /// 切换动画
    /// </summary>
    /// <param name="animationName">动画名</param>
    /// <param name="fixedTransitionDuration">过渡时间</param>
    public void PlayAnimation(string animationName, float fixedTransitionDuration = 0.25f)
    {
        animator.CrossFadeInFixedTime(animationName, fixedTransitionDuration);
    }

    /// <summary>
    /// 更新AOI坐标块
    /// </summary>
    public void UpdateAOICoord()
    {
        Vector2Int newCoord = AOIUtility.GetCoordByWorldPosition(transform.position);
        if (newCoord != currentAOICoord)//发生了地图块坐标变化
        {
            AOIUtility.UpdatePlayerCoord(this, currentAOICoord, newCoord);
            currentAOICoord = newCoord;
        }
    }

}

#endif