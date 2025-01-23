using Unity.Netcode;
using UnityEngine;

public class ServerTestObj : NetworkBehaviour
{
    public float moveSpeed = 3;
    public static ServerTestObj Instance;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
#if UNITY_SERVER || UNITY_EDITOR
        Instance = this;
        AOIManager.Instance.InitServerAOI(NetworkObject, Vector2Int.zero);
#endif
    }

    private void Update()
    {
#if UNITY_SERVER || UNITY_EDITOR
        if (IsServer)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = new Vector3(h, 0, v).normalized;
            HandleMovement(inputDir);
        }
#endif
    }

    private void HandleMovement(Vector3 inputDir)
    {
        //Vector2Int oldCoord = AOIUtility.GetCoordByWorldPosition(transform.position);
        //transform.Translate(Time.deltaTime * moveSpeed * inputDir);
        //Vector2Int newCoord = AOIUtility.GetCoordByWorldPosition(transform.position);
        //if (newCoord != oldCoord)
        //{
        //    AOIManager.Instance.UpdateServerObjectChunkCoord(NetworkObject, oldCoord, newCoord);
        //}
    }
}
