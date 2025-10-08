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
        // เพิ่ม Event Listener ให้ปุ่ม
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }

        // เพิ่ม Event เมื่อกด Enter ในช่อง Input
        if (chatInputField != null)
        {
            chatInputField.onSubmit.AddListener(OnInputSubmit);
        }
    }

    // เมื่อกดปุ่ม Send
    void OnSendButtonClicked()
    {
        SendMessage();
    }

    // เมื่อกด Enter ในช่อง Input
    void OnInputSubmit(string text)
    {
        SendMessage();
    }

    // ส่งข้อความไปยัง NPC
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
            // แสดงข้อความบน UI (ถ้ามี)
            if (npcResponseText != null)
            {
                npcResponseText.text = "คุณ: " + message;
            }

            // ส่งข้อความไปยัง NPC
            npcController.ReceivePlayerMessage(message);

            // ล้างช่อง Input
            chatInputField.text = "";

            // Focus กลับไปที่ช่อง Input
            chatInputField.ActivateInputField();
        }
    }
}