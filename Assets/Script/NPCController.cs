using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NPCController : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource npcAudioSource;

    [Header("Botnoi Voice API")]
    [Tooltip("ใส่ Token จาก Botnoi Voice API")]
    public string botnoiToken = "YOUR_BOTNOI_TOKEN";
    [Tooltip("เลือกเสียง: female1, male1, etc.")]
    public string voiceType = "female1";

    private Queue<string> messageQueue = new Queue<string>();
    private bool isPlaying = false;

    // Dictionary สำหรับเก็บคำตอบ
    private Dictionary<string, string[]> responses = new Dictionary<string, string[]>();

    void Start()
    {
        // ตั้งค่าคำตอบของ NPC
        InitializeResponses();
    }

    void InitializeResponses()
    {
        // เพิ่มคำตอบแบบต่างๆ (สามารถปรับแต่งได้)
        responses["สวัสดี"] = new string[]
        {
            "สวัสดีครับ ยินดีที่ได้รู้จักคุณ",
            "สวัสดีค่ะ มีอะไรให้ช่วยไหม",
            "สวัสดี! วันนี้เป็นอย่างไรบ้าง"
        };

        responses["ชื่อ"] = new string[]
        {
            "ผมชื่อ เอ็นพีซี ครับ",
            "คุณสามารถเรียกฉันว่า NPC Unity",
            "ฉันคือตัวช่วย NPC ในเกมนี้"
        };

        responses["ทำอะไร"] = new string[]
        {
            "ฉันคอยช่วยเหลือผู้เล่นในเกม",
            "ฉันสามารถคุยกับคุณได้นะ",
            "ฉันอยู่ที่นี่เพื่อตอบคำถามของคุณ"
        };

        responses["ขอบคุณ"] = new string[]
        {
            "ยินดีครับ",
            "ด้วยความยินดีค่ะ",
            "ไม่เป็นไรครับ มีอะไรถามได้เสมอ"
        };

        responses["ลาก่อน"] = new string[]
        {
            "ลาก่อนครับ แล้วพบกันใหม่",
            "บ๊ายบาย! ดูแลตัวเองด้วยนะ",
            "ไว้เจอกันใหม่ครับ"
        };
    }

    // ฟังก์ชันรับข้อความจากผู้เล่น
    public void ReceivePlayerMessage(string message)
    {
        string response = GetResponse(message);
        messageQueue.Enqueue(response);

        if (!isPlaying)
            StartCoroutine(ProcessQueue());
    }

    // เลือกคำตอบตามข้อความที่ได้รับ
    string GetResponse(string playerMessage)
    {
        playerMessage = playerMessage.ToLower().Trim();

        // ค้นหาคำสำคัญในข้อความ
        foreach (var key in responses.Keys)
        {
            if (playerMessage.Contains(key.ToLower()))
            {
                // สุ่มเลือกคำตอบจากรายการ
                string[] possibleResponses = responses[key];
                return possibleResponses[Random.Range(0, possibleResponses.Length)];
            }
        }

        // ถ้าไม่เจอคำตอบที่ตรงกัน
        string[] defaultResponses = new string[]
        {
            "ขอโทษครับ ฉันไม่เข้าใจ",
            "ช่วยพูดอีกครั้งได้ไหมครับ",
            "ฉันยังไม่เข้าใจคำนั้นเท่าไร ลองถามใหม่ได้ไหม"
        };
        return defaultResponses[Random.Range(0, defaultResponses.Length)];
    }

    // ประมวลผล Queue ข้อความ
    IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (messageQueue.Count > 0)
        {
            string currentMessage = messageQueue.Dequeue();
            yield return StartCoroutine(PlayBotnoiTTS(currentMessage));
        }

        isPlaying = false;
    }

    // เล่นเสียงผ่าน Botnoi Voice API
    IEnumerator PlayBotnoiTTS(string text)
    {
        string apiUrl = "https://api-voice.botnoi.ai/openapi/v1/generate_audio";

        // สร้าง JSON สำหรับส่งไป API
        string jsonData = JsonUtility.ToJson(new BotnoiRequest
        {
            text = text,
            speaker = voiceType,
            volume = 1.0f,
            speed = 1.0f,
            type_media = "wav"
        });

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Botnoi-Token", botnoiToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // แปลง response เป็น AudioClip
            BotnoiResponse response = JsonUtility.FromJson<BotnoiResponse>(request.downloadHandler.text);

            if (!string.IsNullOrEmpty(response.audio_url))
            {
                // โหลดไฟล์เสียงจาก URL
                yield return StartCoroutine(LoadAndPlayAudio(response.audio_url));
            }
        }
        else
        {
            Debug.LogError("Botnoi TTS Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    // โหลดและเล่นเสียง
    IEnumerator LoadAndPlayAudio(string audioUrl)
    {
        UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.WAV);
        yield return audioRequest.SendWebRequest();

        if (audioRequest.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(audioRequest);
            npcAudioSource.clip = clip;
            npcAudioSource.Play();

            // รอจนเสียงเล่นจบ
            yield return new WaitForSeconds(clip.length);
        }
        else
        {
            Debug.LogError("Audio Load Error: " + audioRequest.error);
        }
    }
}

// Class สำหรับ JSON Request
[System.Serializable]
public class BotnoiRequest
{
    public string text;
    public string speaker;
    public float volume;
    public float speed;
    public string type_media;
}

// Class สำหรับ JSON Response
[System.Serializable]
public class BotnoiResponse
{
    public string audio_url;
    public string message;
}