using JKFrame;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ��Ϸ��������
/// ����Ϸ�������ص�ʱ�����һЩ��Ҫ�ĳ�ʼ��
/// </summary>
public class GameSceneLanunch : MonoBehaviour
{
    IEnumerator Start()
    {
        while (NetworkManager.Singleton == null)
        {
            yield return CoroutineTool.WaitForFrame();
        }
        EventSystem.TypeEventTrigger<GameSceneLanunchEvent>(default);


        Destroy(gameObject);
    }
}
