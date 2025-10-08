using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System.Net;

public class RealTimeTTSNPC : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField chatInput;
    public Button sendButton;
    public TMP_Text npcResponseText;

    [Header("Audio & LipSync")]
    public AudioSource npcAudio;
    public uLipSync.uLipSync lipSync;

    // 🔹 API ของ Botnoi Voice (ฟรี)
    private string apiUrl = "https://api-voice.botnoi.ai/api/service/generate_audio";
    private string token = "botnoi-token"; // เปลี่ยนเป็น token จริงของนายจากเว็บ Botnoi Voice

    void Start()
    {
        sendButton.onClick.AddListener(OnSendClicked);
    }

    void OnSendClicked()
    {
        string playerText = chatInput.text.Trim();
        if (string.IsNullOrEmpty(playerText)) return;

        npcResponseText.text = "กำลังพูด...";
        StartCoroutine(GenerateAndPlayTTS(playerText));
        chatInput.text = "";
    }

    IEnumerator GenerateAndPlayTTS(string text)
    {
        // 🔸 สร้าง JSON สำหรับส่งไป Botnoi Voice API
        string json = JsonUtility.ToJson(new TTSRequest
        {
            text = text,
            speaker = "1",   // เสียงผู้หญิงไทย
            format = "wav"
        });

        using (UnityWebRequest req = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + token);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                npcResponseText.text = "❌ พูดไม่ได้: " + req.error;
                yield break;
            }

            // 🔹 API จะส่งกลับ base64 ของไฟล์เสียง
            TTSResponse response = JsonUtility.FromJson<TTSResponse>(req.downloadHandler.text);
            byte[] audioData = System.Convert.FromBase64String(response.audioData);

            // 🔹 สร้างคลิปเสียงใน runtime ✅ (แก้ตรงนี้)
            AudioClip clip = WavUtility.ToAudioClip(audioData, 0, "NPC_TTS_Voice");
            npcAudio.clip = clip;
            npcAudio.Play();

            // ✅ uLipSync จะจับเสียงนี้และขยับปากอัตโนมัติ
            if (lipSync != null)
                lipSync.Play(clip);

            npcResponseText.text = text;
        }
    }
}

[System.Serializable]
public class TTSRequest
{
    public string text;
    public string speaker;
    public string format;
}

[System.Serializable]
public class TTSResponse
{
    public string audioData;
}
