using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    public GameObject serverTestPrefab;
    public NetworkObject testInstance;

    private void OnGUI()
    {
#if UNITY_SERVER || UNITY_EDITOR
        if (NetManager.Instance.IsServer)
        {
            if (ServerTestObj.Instance != null)
            {
                GUILayout.Label("Position:" + ServerTestObj.Instance.transform.position);
            }
            if (testInstance == null && Input.GetKeyDown(KeyCode.E))
            {
                testInstance = NetManager.Instance.SpawnObject(NetManager.ServerClientId, serverTestPrefab, Vector3.zero, Quaternion.identity);
            }
            if (testInstance != null && Input.GetKeyDown(KeyCode.F))
            {
                NetManager.Instance.DestroyObject(testInstance);
                testInstance = null;
            }
        }
#endif

#if !UNITY_SERVER ||  UNITY_EDITOR
        if (NetManager.Instance.IsClient && PlayerManager.Instance.localPlayer != null)
        {
            //�ӳ�
            GUILayout.Label("Delay:" + ClientRTTInfo.Instance.rttMs);
            //��ǰ����
            GUILayout.Label("Position:" + PlayerManager.Instance.localPlayer.transform.position);
            //����˶�������
            if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(NetManager.ServerClientId, out Dictionary<ulong, NetworkObject> temp))
            {
                GUILayout.Label("ServerObjects:" + temp.Count);
            }
            //�����ͻ��˶�������
            int clientObjects = 0;
            foreach (var item in NetManager.Instance.SpawnManager.OwnershipToObjectsTable)
            {
                if (item.Key != NetManager.ServerClientId && item.Key != NetManager.Instance.LocalClientId)
                {
                    clientObjects += item.Value.Count;
                }
            }
            GUILayout.Label("OtherClientObjects:" + clientObjects);
        }
#endif
    }
}
