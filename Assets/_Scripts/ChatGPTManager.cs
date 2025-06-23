using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ConfigData
{
    public string openai_api_key;
}

public class ChatGPTManager : MonoBehaviour
{
    private string apiKey;
    public string lastGeneratedPrompt { get; private set; }  

    void Awake()
    {
        LoadAPIKey();
    }

    void LoadAPIKey()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "config.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            ConfigData config = JsonUtility.FromJson<ConfigData>(json);
            apiKey = config.openai_api_key;
        }
        else
        {
            Debug.LogError("config.json not found in StreamingAssets!");
        }
    }

    public void RequestSuggestion(string prompt, Action<string> onResponse)
    {
        lastGeneratedPrompt = prompt;
        StartCoroutine(SendPrompt(prompt, onResponse));
    }

    IEnumerator SendPrompt(string promptText, Action<string> onResponse)
    {
        string jsonBody = "{\"model\": \"gpt-4\", \"messages\": [{\"role\": \"user\", \"content\": \"" + promptText + "\"}]}";
        UnityWebRequest req = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        byte[] body = Encoding.UTF8.GetBytes(jsonBody);

        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string result = req.downloadHandler.text;
            var wrapper = JsonUtility.FromJson<ChatResponseWrapper>("{\"wrapper\":" + result + "}");
            string aiText = wrapper.wrapper.choices[0].message.content.Trim();

            onResponse(aiText);
        }
        else
        {
            Debug.LogError("ChatGPT API Error: " + req.error);
            onResponse("Error: " + req.error);
        }
    }

    [Serializable]
    public class ChatResponseWrapper
    {
        public ChatResponse wrapper;
    }

    [Serializable]
    public class ChatResponse
    {
        public Choice[] choices;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }

    [Serializable]
    public class Message
    {
        public string content;
    }
}
