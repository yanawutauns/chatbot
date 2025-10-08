using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NPCController : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource npcAudioSource;

    [Header("Botnoi Voice API")]
    [Tooltip("��� Token �ҡ Botnoi Voice API")]
    public string botnoiToken = "YOUR_BOTNOI_TOKEN";
    [Tooltip("���͡���§: female1, male1, etc.")]
    public string voiceType = "female1";

    private Queue<string> messageQueue = new Queue<string>();
    private bool isPlaying = false;

    // Dictionary ����Ѻ�纤ӵͺ
    private Dictionary<string, string[]> responses = new Dictionary<string, string[]>();

    void Start()
    {
        // ��駤�Ҥӵͺ�ͧ NPC
        InitializeResponses();
    }

    void InitializeResponses()
    {
        // �����ӵͺẺ��ҧ� (����ö��Ѻ����)
        responses["���ʴ�"] = new string[]
        {
            "���ʴդ�Ѻ �Թ�շ�������ѡ�س",
            "���ʴդ�� ���������������",
            "���ʴ�! �ѹ��������ҧ�ú�ҧ"
        };

        responses["����"] = new string[]
        {
            "������ ��繾ի� ��Ѻ",
            "�س����ö���¡�ѹ��� NPC Unity",
            "�ѹ��͵�Ǫ��� NPC ������"
        };

        responses["������"] = new string[]
        {
            "�ѹ��ª�������ͼ��������",
            "�ѹ����ö��¡Ѻ�س���",
            "�ѹ�����������͵ͺ�Ӷ���ͧ�س"
        };

        responses["�ͺ�س"] = new string[]
        {
            "�Թ�դ�Ѻ",
            "���¤����Թ�դ��",
            "������ä�Ѻ �����ö��������"
        };

        responses["�ҡ�͹"] = new string[]
        {
            "�ҡ�͹��Ѻ ���Ǿ��ѹ����",
            "���º��! ���ŵ���ͧ���¹�",
            "����͡ѹ�����Ѻ"
        };
    }

    // �ѧ��ѹ�Ѻ��ͤ����ҡ������
    public void ReceivePlayerMessage(string message)
    {
        string response = GetResponse(message);
        messageQueue.Enqueue(response);

        if (!isPlaying)
            StartCoroutine(ProcessQueue());
    }

    // ���͡�ӵͺ�����ͤ���������Ѻ
    string GetResponse(string playerMessage)
    {
        playerMessage = playerMessage.ToLower().Trim();

        // ���Ҥ��Ӥѭ㹢�ͤ���
        foreach (var key in responses.Keys)
        {
            if (playerMessage.Contains(key.ToLower()))
            {
                // �������͡�ӵͺ�ҡ��¡��
                string[] possibleResponses = responses[key];
                return possibleResponses[Random.Range(0, possibleResponses.Length)];
            }
        }

        // �������ͤӵͺ���ç�ѹ
        string[] defaultResponses = new string[]
        {
            "���ɤ�Ѻ �ѹ�������",
            "���¾ٴ�ա�����������Ѻ",
            "�ѹ�ѧ������㨤ӹ������� �ͧ������������"
        };
        return defaultResponses[Random.Range(0, defaultResponses.Length)];
    }

    // �����ż� Queue ��ͤ���
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

    // ������§��ҹ Botnoi Voice API
    IEnumerator PlayBotnoiTTS(string text)
    {
        string apiUrl = "https://api-voice.botnoi.ai/openapi/v1/generate_audio";

        // ���ҧ JSON ����Ѻ��� API
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
            // �ŧ response �� AudioClip
            BotnoiResponse response = JsonUtility.FromJson<BotnoiResponse>(request.downloadHandler.text);

            if (!string.IsNullOrEmpty(response.audio_url))
            {
                // ��Ŵ������§�ҡ URL
                yield return StartCoroutine(LoadAndPlayAudio(response.audio_url));
            }
        }
        else
        {
            Debug.LogError("Botnoi TTS Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    // ��Ŵ���������§
    IEnumerator LoadAndPlayAudio(string audioUrl)
    {
        UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.WAV);
        yield return audioRequest.SendWebRequest();

        if (audioRequest.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(audioRequest);
            npcAudioSource.clip = clip;
            npcAudioSource.Play();

            // �ͨ����§��蹨�
            yield return new WaitForSeconds(clip.length);
        }
        else
        {
            Debug.LogError("Audio Load Error: " + audioRequest.error);
        }
    }
}

// Class ����Ѻ JSON Request
[System.Serializable]
public class BotnoiRequest
{
    public string text;
    public string speaker;
    public float volume;
    public float speed;
    public string type_media;
}

// Class ����Ѻ JSON Response
[System.Serializable]
public class BotnoiResponse
{
    public string audio_url;
    public string message;
}