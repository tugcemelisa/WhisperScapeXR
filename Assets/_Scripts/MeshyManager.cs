using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.IO;
using GLTFast;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class MeshyManager : MonoBehaviour
{
    [System.Serializable]
    public class MeshyResponse
    {
        public string status;
        public string result;
        public int progress;
    }

    public Transform spawnPoint;
    public Material defaultMaterial;
    public Camera mainCamera;

    private string apiKey;

    void Start()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "config.json");
        string json = File.ReadAllText(configPath);
        JObject config = JObject.Parse(json);
        apiKey = config["meshy_api_key"].ToString();
    }

    public void Generate3DModel(string prompt)
    {
        if (!string.IsNullOrEmpty(prompt))
        {
            StartCoroutine(SendPromptToMeshy(prompt));
        }
        else
        {
            Debug.LogError("❌ Prompt is empty. Cannot send to Meshy.");
        }
    }

    IEnumerator SendPromptToMeshy(string prompt)
    {
        string url = "https://api.meshy.ai/v2/text-to-3d";

        var body = new
        {
            prompt = prompt,
            quality = "high",
            format = "glb",
            mode = "preview"
        };

        string jsonBody = JsonConvert.SerializeObject(body);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            JObject response = JObject.Parse(responseText);
            string taskId = response["result"]?.ToString();

            if (!string.IsNullOrEmpty(taskId))
            {
                Debug.Log("✅ Task ID received: " + taskId);
                StartCoroutine(CheckTaskProgressV2(taskId));
            }
            else
            {
                Debug.LogError("❌ Task ID is empty.\nResponse: " + responseText);
            }
        }
        else
        {
            Debug.LogError("❌ Meshy API Error (POST): " + request.responseCode + " - " + request.downloadHandler.text);
        }
    }

    IEnumerator CheckTaskProgressV2(string taskId)
    {
        string checkUrl = $"https://api.meshy.ai/v2/text-to-3d/{taskId}";

        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get(checkUrl);
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                JObject response = JObject.Parse(request.downloadHandler.text);
                string status = response["status"]?.ToString();
                int progress = response["progress"]?.ToObject<int>() ?? 0;

                Debug.Log($"⏳ Model is loading... {progress}%");

                if (status == "completed")
                {
                    string modelUrl = response["result"]?.ToString();
                    if (!string.IsNullOrEmpty(modelUrl))
                    {
                        Debug.Log("📦 Model URL received: " + modelUrl);
                        StartCoroutine(LoadGLBModel(modelUrl));
                    }
                    else
                    {
                        Debug.LogError("❌ Model URL is empty.");
                    }
                    break;
                }
            }
            else
            {
                Debug.LogError("❌ Meshy API Error (GET): " + request.error);
                break;
            }

            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator LoadGLBModel(string url)
    {
        var gltf = new GltfImport();
        Task<bool> loadingTask = gltf.Load(new Uri(url));

        while (!loadingTask.IsCompleted)
            yield return null;

        if (loadingTask.IsFaulted || !loadingTask.Result)
        {
            Debug.LogError("❌ Failed to load GLB model.");
            yield break;
        }

        GameObject importedModel = new GameObject("MeshyModel_" + DateTime.Now.ToString("HHmmss"));

        Task<bool> instantiatingTask = gltf.InstantiateMainSceneAsync(importedModel.transform);
        while (!instantiatingTask.IsCompleted)
            yield return null;

        if (instantiatingTask.IsFaulted || !instantiatingTask.Result)
        {
            Debug.LogError("❌ Failed to instantiate GLB model.");
            yield break;
        }

        importedModel.transform.position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        importedModel.transform.localScale = Vector3.one * 10;
        importedModel.layer = LayerMask.NameToLayer("Default");
        Debug.Log("✅ Model spawned at: " + importedModel.transform.position);

        MeshRenderer renderer = importedModel.GetComponentInChildren<MeshRenderer>();
        if (renderer == null)
        {
            renderer = importedModel.AddComponent<MeshRenderer>();
            Debug.LogWarning("⚠️ No MeshRenderer found. Default MeshRenderer added.");
        }

        MeshFilter filter = importedModel.GetComponentInChildren<MeshFilter>();
        if (filter == null)
        {
            importedModel.AddComponent<MeshFilter>();
            Debug.LogWarning("⚠️ No MeshFilter found. Default MeshFilter added.");
        }

        if (renderer.material == null && defaultMaterial != null)
        {
            renderer.material = defaultMaterial;
            Debug.Log("🎨 Default material applied to the model.");
        }

        if (importedModel.GetComponentInChildren<Collider>() == null)
        {
            importedModel.AddComponent<BoxCollider>();
            Debug.LogWarning("⚠️ No Collider found. Default BoxCollider added.");
        }

        if (mainCamera != null)
        {
            mainCamera.transform.LookAt(importedModel.transform);
            Debug.Log("🎥 Camera now looking at the spawned model.");
            StartCoroutine(ResetCameraLookAfterDelay(importedModel.transform));
        }
    }

    IEnumerator ResetCameraLookAfterDelay(Transform target)
    {
        yield return new WaitForSeconds(3f);
        if (mainCamera != null && target != null)
        {
            mainCamera.transform.LookAt(target);
            Debug.Log("🎥 Camera realigned to model after delay.");
        }
    }
}
