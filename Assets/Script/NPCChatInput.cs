using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatInputManager : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField chatInputField;
    public Button sendButton;
    public TMP_Text npcResponseText;

    [Header("NPC Reference")]
    public NPCController npcController;

    void Start()
    {
        // ���� Event Listener ������
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }

        // ���� Event ����͡� Enter 㹪�ͧ Input
        if (chatInputField != null)
        {
            chatInputField.onSubmit.AddListener(OnInputSubmit);
        }
    }

    // ����͡����� Send
    void OnSendButtonClicked()
    {
        SendMessage();
    }

    // ����͡� Enter 㹪�ͧ Input
    void OnInputSubmit(string text)
    {
        SendMessage();
    }

    // �觢�ͤ�����ѧ NPC
    void SendMessage()
    {
        if (chatInputField == null || npcController == null)
        {
            Debug.LogError("Missing references!");
            return;
        }

        string message = chatInputField.text.Trim();

        if (!string.IsNullOrEmpty(message))
        {
            // �ʴ���ͤ����� UI (�����)
            if (npcResponseText != null)
            {
                npcResponseText.text = "�س: " + message;
            }

            // �觢�ͤ�����ѧ NPC
            npcController.ReceivePlayerMessage(message);

            // ��ҧ��ͧ Input
            chatInputField.text = "";

            // Focus ��Ѻ价���ͧ Input
            chatInputField.ActivateInputField();
        }
    }
}