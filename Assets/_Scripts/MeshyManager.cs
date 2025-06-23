using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using GLTFast;
using UnityEditor;

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
    public Material fallbackMaterial;

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
            Debug.Log("Sending prompt to Meshy: " + prompt);
            StartCoroutine(SendPromptToMeshy(prompt));
        }
        else
        {
            Debug.LogError("Prompt is empty. Cannot generate model.");
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
            JObject response = JObject.Parse(request.downloadHandler.text);
            string taskId = response["result"]?.ToString();

            if (!string.IsNullOrEmpty(taskId))
            {
                Debug.Log("Task ID received: " + taskId);
                StartCoroutine(CheckTaskProgress(taskId));
            }
            else
            {
                Debug.LogError("Task ID is missing. Spawning fallback cube.");
                SpawnFallbackCube("No Task ID");
            }
        }
        else
        {
            Debug.LogError("POST failed: " + request.downloadHandler.text);
            SpawnFallbackCube("POST Error");
        }
    }

    IEnumerator CheckTaskProgress(string taskId)
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
                Debug.Log("Model generation progress: " + progress + "%");

                if (status == "completed")
                {
                    string modelUrl = response["result"]?.ToString();
                    if (!string.IsNullOrEmpty(modelUrl))
                    {
                        Debug.Log("Model URL received: " + modelUrl);
                        StartCoroutine(LoadGLBModel(modelUrl));
                    }
                    else
                    {
                        Debug.LogError("Model URL missing. Spawning fallback cube.");
                        SpawnFallbackCube("No Model URL");
                    }
                    break;
                }
                else if (status == "failed")
                {
                    Debug.LogError("Generation failed. Spawning fallback cube.");
                    SpawnFallbackCube("Generation Failed");
                    break;
                }
            }
            else
            {
                Debug.LogError("GET failed: " + request.downloadHandler.text);
                SpawnFallbackCube("GET Error");
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
            Debug.LogError("❌ GLB model load failed.");
            SpawnFallbackCube("GLB Load Failed");
            yield break;
        }

        GameObject model = new GameObject("MeshyModel_" + DateTime.Now.ToString("HHmmss"));
        Task<bool> instantiatingTask = gltf.InstantiateMainSceneAsync(model.transform);

        while (!instantiatingTask.IsCompleted)
            yield return null;

        if (instantiatingTask.IsFaulted || !instantiatingTask.Result)
        {
            Debug.LogError("❌ Instantiation failed.");
            Destroy(model);
            SpawnFallbackCube("Instantiation Failed");
            yield break;
        }

        if (model.transform.childCount == 0 || model.GetComponentsInChildren<MeshFilter>().Length == 0)
        {
            Debug.LogError("❌ Model instantiated but has no mesh. Spawning fallback.");
            Destroy(model);
            SpawnFallbackCube("Empty Mesh");
            yield break;
        }

        model.transform.position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        model.transform.localScale = Vector3.one * 10f;
        Debug.Log("✅ Model spawned at: " + model.transform.position + " with children: " + model.transform.childCount);

        if (model.GetComponentInChildren<MeshRenderer>() == null)
        {
            model.AddComponent<MeshRenderer>();
            Debug.LogWarning("⚠️ MeshRenderer added.");
        }

        if (defaultMaterial != null)
        {
            foreach (var rend in model.GetComponentsInChildren<MeshRenderer>())
            {
                if (rend.material == null)
                    rend.material = defaultMaterial;
            }
        }

        if (model.GetComponentInChildren<Collider>() == null)
        {
            model.AddComponent<BoxCollider>();
        }

#if UNITY_EDITOR
        string path = "Assets/GeneratedPrefabs";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string prefabPath = Path.Combine(path, model.name + ".prefab");
        PrefabUtility.SaveAsPrefabAssetAndConnect(model, prefabPath, InteractionMode.UserAction);
        Debug.Log("💾 Model saved as prefab at: " + prefabPath);
#endif
    }

    void SpawnFallbackCube(string reason)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "FallbackCube_" + reason.Replace(" ", "_");
        cube.transform.position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        cube.transform.localScale = Vector3.one * 1.5f;

        if (fallbackMaterial != null)
        {
            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.material = fallbackMaterial;
            Debug.Log("Fallback material applied.");
        }

        Debug.Log("Fallback cube spawned due to: " + reason);
    }
}
