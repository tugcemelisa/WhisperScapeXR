using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;

public class ImageGenerator : MonoBehaviour
{
    public RawImage imageDisplay;
    private string apiKey;

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

    public void GenerateImageFromPrompt(string prompt, Action<Texture2D> onComplete)
    {
        StartCoroutine(GetImageFromDallE(prompt, onComplete));
    }

    IEnumerator GetImageFromDallE(string prompt, Action<Texture2D> onComplete)
    {
        string url = "https://api.openai.com/v1/images/generations";
        var requestBody = new
        {
            prompt = prompt,
            n = 1,
            size = "1024x1024"
        };

        string json = JsonConvert.SerializeObject(requestBody);

        using (UnityWebRequest postRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            postRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            postRequest.downloadHandler = new DownloadHandlerBuffer();
            postRequest.SetRequestHeader("Content-Type", "application/json");
            postRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return postRequest.SendWebRequest();

            if (postRequest.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<DallEResponse>(postRequest.downloadHandler.text);
                string imageUrl = response.data[0].url;

                using (UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(imageUrl))
                {
                    yield return imageRequest.SendWebRequest();

                    if (imageRequest.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(imageRequest);
                        onComplete?.Invoke(texture);
                    }
                    else
                    {
                        Debug.LogError("Image download failed: " + imageRequest.error);
                        onComplete?.Invoke(null);
                    }
                }
            }
            else
            {
                Debug.LogError("DALL·E API Error: " + postRequest.error);
                onComplete?.Invoke(null);
            }
        }
    }

    [Serializable]
    public class ConfigData
    {
        public string openai_api_key;
    }

    [Serializable]
    public class DallEResponse
    {
        public ImageData[] data;
    }

    [Serializable]
    public class ImageData
    {
        public string url;
    }
}
