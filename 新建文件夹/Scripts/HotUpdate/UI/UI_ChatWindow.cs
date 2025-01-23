using JKFrame;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ChatWindow : UI_CustomWindowBase
{
    private const int itemCount = 15;
    [SerializeField] private Transform main;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform itemRoot;
    [SerializeField] private InputField chatInputField;
    private Image scrollRectImg;
    private Queue<UI_ChatWindowItem> itemQueue = new Queue<UI_ChatWindowItem>(itemCount);


    public override void Init()
    {
        scrollRectImg = scrollRect.GetComponent<Image>();
        chatInputField.onSubmit.AddListener(OnChatInputSubmit);

        main.OnMouseEnter(OnEnter);
        main.OnMouseExit(OnExit);
        OnExit(null);
    }

    public override void OnShow()
    {
        base.OnShow();
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S2C_ChatMessage, OnChatMessage);
    }

    private void OnEnter(PointerEventData data)
    {
        chatInputField.gameObject.SetActive(true);
        scrollRect.vertical = true;
        scrollRectImg.color = new Color(0, 0, 0, 0.2f);
    }

    private void OnExit(PointerEventData data)
    {
        chatInputField.gameObject.SetActive(false);
        scrollRect.vertical = false;
        scrollRectImg.color = new Color(0, 0, 0, 0);
    }

    private void OnChatInputSubmit(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;
        //����������Ϣ
        NetMessageManager.Instance.SendMessageToServer(MessageType.C2S_ChatMessage, new C2S_ChatMessage()
        {
            message = content
        });
        chatInputField.text = "";
        chatInputField.Select();
        chatInputField.ActivateInputField(); //����������³�Ϊ����
        //TODO:����������Ϣ

    }

    public void AddItem(string name, string content)
    {
        //���ԭ���������·������յ�����Ϣʱ��Ҫ�Զ��������·�
        bool onEnd = scrollRect.verticalNormalizedPosition <= 0.1f; //0���������·�
        //������Ϸ���
        if (itemQueue.Count >= itemCount)
        {
            DestroyItem(itemQueue.Dequeue());
        }
        UI_ChatWindowItem item = CreateItem();
        itemQueue.Enqueue(item);
        item.Init(name, content);
        //��һ��������գ���δ����֮ǰ(��Ϣ������) verticalNormalizedPosition��һֱ��0��
        //���»��Ʋ����ǵ�ǰ֡������ӳ�2֡ȥִ�в���
        if (!onEnd)
        {
            StartCoroutine(LookScorllToEnd());
        }

    }

    private IEnumerator LookScorllToEnd()
    {
        yield return CoroutineTool.WaitForFrames(2);
        scrollRect.verticalNormalizedPosition = 0;
    }

    private UI_ChatWindowItem CreateItem()
    {
        return ResSystem.InstantiateGameObject<UI_ChatWindowItem>(nameof(UI_ChatWindowItem), itemRoot);
    }

    private void DestroyItem(UI_ChatWindowItem item)
    {
        item.GameObjectPushPool();
    }

    public override void OnClose()
    {
        base.OnClose();
        NetMessageManager.Instance.UnRegisterMessageCallback(MessageType.S2C_ChatMessage, OnChatMessage);
        PoolSystem.ClearGameObject(nameof(UI_ChatWindowItem));
    }

    private void OnChatMessage(ulong clientID, INetworkSerializable serializable)
    {
        S2C_ChatMessage message = (S2C_ChatMessage)serializable;
        AddItem(message.playerName, message.message);
    }
}
