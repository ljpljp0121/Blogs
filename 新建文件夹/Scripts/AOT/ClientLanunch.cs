using JKFrame;
using UnityEngine;

/// <summary>
/// �ͻ��������ű�
/// ������ű��л��ڿͻ�������ʱ��һЩ��Ҫ�ĳ�ʼ��
/// </summary>
public class ClientLanunch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        GetComponent<HotUpdateSystem>().StartHotUpdate(null, (bool succeed) =>
        {
            if (succeed)
            {
                OnHotUpdateSucceed();
            }
        });
    }

    private void OnHotUpdateSucceed()
    {
        ResSystem.InstantiateGameObject("ClientGlobal");
    }
}
