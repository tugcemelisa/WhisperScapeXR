using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ShapEClient : MonoBehaviour
{
    [Header("Prompt Settings")]
    public string prompt; // Write your text prompt here in the Inspector
    public string serverURL = "http://localhost:5000/generate";
    public string outputFileName = "generated_model.fbx"; // You can change to .glb if needed

    public bool instantiatePrefabInScene = true;

    [ContextMenu("Generate Model")]
    public void GenerateModel()
    {
        StartCoroutine(SendPrompt());
    }

    IEnumerator SendPrompt()
    {
        Debug.Log("Sending prompt to server: " + prompt);

        var requestData = new
        {
            prompt = prompt,
            format = "fbx" // or "glb" if your pipeline is using .glb
        };

        string json = JsonUtility.ToJson(requestData);

        UnityWebRequest www = new UnityWebRequest(serverURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("ERROR: " + www.error);
        }
        else
        {
            Debug.Log("Received response from server.");

            // Try to parse base64
            string jsonResponse = www.downloadHandler.text;
            string base64String;

            try
            {
                var wrapper = JsonUtility.FromJson<Wrapper>(jsonResponse);
                base64String = wrapper.file;
            }
            catch
            {
                Debug.LogError("JSON parsing failed! Check if server returned correct JSON {\"file\": \"...\"}");
                yield break;
            }

            byte[] modelData;

            try
            {
                modelData = System.Convert.FromBase64String(base64String);
            }
            catch
            {
                Debug.LogError("Base64 decoding failed! Check if server returned clean base64.");
                yield break;
            }

            string modelFolder = Path.Combine(Application.dataPath, "Models");
            Directory.CreateDirectory(modelFolder);
            string fullPath = Path.Combine(modelFolder, outputFileName);
            File.WriteAllBytes(fullPath, modelData);
            Debug.Log("✅ 3D Model saved to: " + fullPath);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
            string relativePath = "Assets/Models/" + outputFileName;

            if (instantiatePrefabInScene)
            {
                UnityEngine.Object model = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(relativePath);
                GameObject instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
                instance.transform.position = Vector3.zero;

                //Save as Prefab
                string prefabFolder = "Assets/Prefabs";
                Directory.CreateDirectory(prefabFolder);
                string prefabPath = prefabFolder + "/" + Path.GetFileNameWithoutExtension(outputFileName) + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                Debug.Log(" Saved prefab to: " + prefabPath);
            }
#endif
        }
    }

    [System.Serializable]
    public class Wrapper
    {
        public string file;
    }
}
