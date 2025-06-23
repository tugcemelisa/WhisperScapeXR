using UnityEngine;
using UnityEditor;
using System.Net;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEditor.SceneManagement;

public class MeshyBridgeWindow : EditorWindow
{
    private static MeshyBridge bridgeInstance;
    private static bool isBridgeRunning = false;
    private GUIContent runButtonContent;
    private GUIContent stopButtonContent;

    [MenuItem("Meshy/Bridge")]
    public static void ShowWindow()
    {
        var window = GetWindow<MeshyBridgeWindow>("Meshy Bridge");
        window.minSize = new Vector2(250, 100);
        window.maxSize = new Vector2(400, 150);
    }

    private void OnEnable()
    {
        // ��ʼ����ť����
        runButtonContent = new GUIContent("Run Bridge");
        stopButtonContent = new GUIContent("Bridge ON");

        // ����Ž�ʵ���Ƿ����
        isBridgeRunning = bridgeInstance != null;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Space(10);

        // ����һ���Զ�����ʽ
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.fixedHeight = 40;

        // �����Ž�״̬���ð�ť��ɫ
        Color originalColor = GUI.backgroundColor;
        if (isBridgeRunning)
        {
            GUI.backgroundColor = new Color(0.4f, 0.6f, 1.0f); // ��ɫ��ʾ������
        }

        // ��ʾ״̬�л���ť
        GUIContent currentContent = isBridgeRunning ? stopButtonContent : runButtonContent;
        if (GUILayout.Button(currentContent, buttonStyle))
        {
            ToggleBridgeState();
        }

        // �ָ�ԭʼ��ɫ
        GUI.backgroundColor = originalColor;

        EditorGUILayout.EndVertical();
    }

    private void ToggleBridgeState()
    {
        if (isBridgeRunning)
        {
            StopBridge();
        }
        else
        {
            StartBridge();
        }
    }

    private static void StartBridge()
    {
        if (bridgeInstance == null)
        {
            var go = new GameObject("MeshyBridge");
            bridgeInstance = go.AddComponent<MeshyBridge>();
            isBridgeRunning = true;
            Debug.Log("Meshy Bridge started");
        }
    }

    private static void StopBridge()
    {
        if (bridgeInstance != null)
        {
            bridgeInstance.StopServer();
            DestroyImmediate(bridgeInstance.gameObject);
            bridgeInstance = null;
            isBridgeRunning = false;
            Debug.Log("Meshy Bridge stopped");
        }
    }

    // ȷ�����ڹر�ʱ����Ӱ���Ž�״̬
    private void OnDestroy()
    {
        // ���ڹر�ʱ��ֹͣ�Ž�
    }
}

// ����ԭ�еĲ˵������������Ϊ˽��
public static class MeshyBridgeCommands
{
    private static void StartBridge()
    {
        MeshyBridgeWindow.ShowWindow();
        var windowType = typeof(MeshyBridgeWindow);
        var method = windowType.GetMethod("StartBridge", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method.Invoke(null, null);
    }
}

[ExecuteInEditMode]
public class MeshyBridge : MonoBehaviour
{
    // ��ӻ���·���ֶ�
    private string _tempCachePath;

    private Thread serverThread;
    private Thread guardThread;
    private bool _serverStop = false;
    private TcpListener listener;
    private Queue<MeshTransfer> importQueue = new Queue<MeshTransfer>();

    [System.Serializable]
    public class MeshTransfer
    {
        public string file_format;
        public string path;
        public string name;
        public int frameRate; // ���֡���ֶ�
    }

    void Start()
    {
        Debug.Log("[Meshy Bridge] ��ʼ��ʼ��������");
        // �����߳���Ԥ�Ȼ�ȡ��ʱ·��
        _tempCachePath = Application.temporaryCachePath;
        try
        {
            StartServer();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Meshy Bridge] ����ʧ��: {e.Message}\n{e.StackTrace}");
        }
    }

    public void StartServer()
    {
        Debug.Log("[Meshy Bridge] ���������������߳�");
        _serverStop = false;
        serverThread = new Thread(RunServer);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private void GuardJob()
    {
        while (!_serverStop)
        {
            Thread.Sleep(200);
        }

        if (listener != null)
        {
            listener.Stop();
            Debug.Log("[Meshy Bridge] Guard thread shutting down server");
        }
    }

    void RunServer()
    {
        listener = new TcpListener(IPAddress.Any, 5326);
        listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); // ����׽�������ѡ��
        listener.Start();

        guardThread = new Thread(GuardJob);
        guardThread.IsBackground = true;
        guardThread.Start();

        Debug.Log("[Meshy Bridge] Listening on port 5326");

        while (!_serverStop)
        {
            if (listener.Pending())
            {
                using (TcpClient client = listener.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                {
                    ProcessClientRequest(stream);
                }
            }
            Thread.Sleep(100);
        }

        listener.Stop();
        Debug.Log("[Meshy Bridge] Server stopped");
    }

    public void StopServer()
    {
        _serverStop = true;

        if (serverThread != null && serverThread.IsAlive)
        {
            serverThread.Join();
        }

        if (guardThread != null && guardThread.IsAlive)
        {
            guardThread.Join();
        }
    }

    private readonly string[] allowedOrigins = new string[]
    {
        "https://www.meshy.ai",
        "https://app-staging.meshy.ai",
        "http://localhost:3700"
    };

    void ProcessClientRequest(NetworkStream stream)
    {
        try
        {
            Debug.Log("[Meshy Bridge] ��ʼ����ͻ�������");
            byte[] buffer = new byte[1024 * 16]; // ���󻺳���
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log($"[Meshy Bridge] �յ���������({bytesRead}�ֽ�):\n{request}");

            // ��������ͷ - ��Ӵ�����
            var requestLines = request.Split(new[] { "\r\n" }, StringSplitOptions.None);

            // ����Ƿ����㹻��������
            if (requestLines.Length == 0)
            {
                Debug.LogWarning("[Meshy Bridge] ����Ϊ��");
                SendErrorResponse(stream, "Empty request");
                return;
            }

            // ��ȫ�������󷽷���·��
            string[] requestParts = requestLines[0].Split(' ');
            if (requestParts.Length < 2)
            {
                Debug.LogWarning("[Meshy Bridge] �����и�ʽ����ȷ: " + requestLines[0]);
                SendErrorResponse(stream, "Invalid request format");
                return;
            }

            string method = requestParts[0];
            string path = requestParts[1];
            string origin = GetHeaderValue(requestLines, "Origin");

            // ����OPTIONSԤ������
            if (method == "OPTIONS")
            {
                SendOptionsResponse(stream, origin);
                return;
            }

            // ����GET״̬����
            if (method == "GET" && (path == "/status" || path == "/ping"))
            {
                SendStatusResponse(stream, origin);
                return;
            }

            // ����POST��������
            if (method == "POST" && path == "/import")
            {
                ProcessImportRequest(stream, request, origin);
                return;
            }

            // δ֪·��
            SendNotFoundResponse(stream, origin);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Meshy Bridge] �������쳣: {e.GetType().Name} - {e.Message}\n{e.StackTrace}");
            try
            {
                SendErrorResponse(stream, e.Message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Meshy Bridge] ���ʹ�����Ӧʱ����: {ex.Message}");
            }
        }
    }

    // �����Ӧ���ݽṹ
    [System.Serializable]
    private class ImportResponseData
    {
        public string status;
        public string message;
        public string path;
    }

    private void ProcessImportRequest(NetworkStream stream, string request, string origin)
    {



        try
        {
            // ��ȡJSON����
            int jsonStart = request.IndexOf('{');
            if (jsonStart < 0)
            {
                Debug.LogWarning("[Meshy Bridge] �޷��ҵ�JSON��ʼ���");
                throw new Exception("Invalid request format: JSON not found");
            }

            string jsonBody = request.Substring(jsonStart);
            // ��֤�Ƿ�����ЧJSON
            if (!jsonBody.Trim().StartsWith("{") || !jsonBody.Trim().EndsWith("}"))
            {
                Debug.LogError("��Ч��JSON��ʽ");
                throw new Exception("Invalid JSON format");
            }
            Debug.Log($"[Meshy Bridge] ����JSON: {jsonBody}");

            // �޸�Ϊʹ���Զ��������JSON
            var data = JsonUtility.FromJson<ImportRequestData>(jsonBody);
            if (data == null)
            {
                Debug.LogWarning("[Meshy Bridge] JSON����ʧ��");
                throw new Exception("Failed to parse JSON data");
            }

            if (string.IsNullOrEmpty(data.url))
            {
                Debug.LogWarning("[Meshy Bridge] URLΪ��");
                throw new Exception("Missing required field: url");
            }

            if (string.IsNullOrEmpty(data.format))
            {
                Debug.LogWarning("[Meshy Bridge] ��ʽΪ��,ʹ��Ĭ��ֵglb");
                data.format = "glb";
            }

            // ������ʱ�ļ�·��
            string fileName = $"bridge_model";
            string filePath = Path.Combine(Path.GetTempPath(), "Meshy", fileName);

            // ȷ��Ŀ¼����
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            Debug.Log($"[Meshy Bridge] ��ʼ�����ļ�: {data.url}");
            using (var client = new WebClient())
            {
                client.DownloadFile(data.url, filePath);
            }
            // ��ȡ�ļ�ͷ�ж��ļ�����
            string fileExtension = ".glb"; // Ĭ��glb
            byte[] header = new byte[4];
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(header, 0, 4);
                if (header[0] == 'P' && header[1] == 'K' && header[2] == 0x03 && header[3] == 0x04)
                {
                    fileExtension = ".zip";
                }
                else if (header[0] == 'g' && header[1] == 'l' && header[2] == 'T' && header[3] == 'F')
                {
                    fileExtension = ".glb";
                }
            }

            // �������ļ������ȷ��չ��
            string finalPath = filePath + fileExtension;
            File.Move(filePath, finalPath);
            filePath = finalPath;
            Debug.Log($"[Meshy Bridge] �ļ��������: {filePath}");

            // ���뵼�����
            lock (importQueue)
            {
                importQueue.Enqueue(new MeshTransfer
                {
                    file_format = data.format,
                    path = filePath,
                    name = data.name ?? "",
                    frameRate = data.frameRate
                });
            }

            // ���ͳɹ���Ӧ
            var responseData = new ImportResponseData
            {
                status = "ok",
                message = "File queued for import",
                path = filePath
            };

            string jsonResponse = JsonUtility.ToJson(responseData);

            string response = string.Join("\r\n",
                $"HTTP/1.1 200 OK",
                $"Access-Control-Allow-Origin: {GetAllowedOrigin(origin)}",
                "Content-Type: application/json; charset=utf-8",
                "Connection: close",
                $"Content-Length: {jsonResponse.Length}",
                "",
                jsonResponse);

            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);
            stream.Flush();
            Debug.Log("[Meshy Bridge] ����������ɹ�");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Meshy Bridge] ����������ʧ��: {e.Message}");
            SendErrorResponse(stream, e.Message);
        }
    }

    // ����������ݽṹ
    [System.Serializable]
    private class ImportRequestData
    {
        public string url;
        public string format;
        public string name;
        public int frameRate = 30; // Ĭ��30֡
    }

    private string GetAllowedOrigin(string origin)
    {
        return Array.Exists(allowedOrigins, o => string.Equals(o, origin, StringComparison.OrdinalIgnoreCase)) ?
            origin : "https://www.meshy.ai";
    }

    private void SendOptionsResponse(NetworkStream stream, string origin)
    {
        string response = $"HTTP/1.1 200 OK\r\n" +
                        $"Access-Control-Allow-Origin: {GetAllowedOrigin(origin)}\r\n" +
                        "Access-Control-Allow-Methods: POST, GET, OPTIONS\r\n" +
                        "Access-Control-Allow-Headers: *\r\n" +
                        "Access-Control-Max-Age: 86400\r\n\r\n";
        stream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
    }

    [System.Serializable]
    private class StatusResponseData
    {
        public string status = "ok";
        public string dcc = "unity";
        public string version;
    }

    private void SendStatusResponse(NetworkStream stream, string origin)
    {
        // ������Ӧ���ݶ���
        var responseData = new StatusResponseData
        {
            dcc = "unity",
            status = "ok",
            version = Application.unityVersion
        };

        // ת��ΪJSON
        string jsonResponse = JsonUtility.ToJson(responseData);

        // ��������HTTP��Ӧ
        string response = string.Join("\r\n",
            "HTTP/1.1 200 OK",
            $"Access-Control-Allow-Origin: {GetAllowedOrigin(origin)}",
            "Content-Type: application/json; charset=utf-8",
            "Connection: close",
            $"Content-Length: {jsonResponse.Length}",
            "",
            jsonResponse);

        // ������Ӧ
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        stream.Write(responseBytes, 0, responseBytes.Length);
        stream.Flush();

        Debug.Log($"[Meshy Bridge] ����״̬��Ӧ: {jsonResponse}");

    }

    private void SendNotFoundResponse(NetworkStream stream, string origin)
    {
        string response = $"HTTP/1.1 404 Not Found\r\n" +
                        $"Access-Control-Allow-Origin: {GetAllowedOrigin(origin)}\r\n" +
                        "Content-Type: application/json\r\n\r\n" +
                        JsonUtility.ToJson(new { status = "path not found" });
        stream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
    }

    private void SendErrorResponse(NetworkStream stream, string message)
    {
        try
        {
            string jsonBody = JsonUtility.ToJson(new { status = "error", message });
            string response = string.Join("\r\n",
                "HTTP/1.1 500 Internal Server Error",
                "Access-Control-Allow-Origin: *",
                "Content-Type: application/json; charset=utf-8",
                "Connection: close",
                $"Content-Length: {jsonBody.Length}",
                "",
                jsonBody);

            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);
            stream.Flush();
        }
        catch (Exception e)
        {
            Debug.LogError($"���ʹ�����Ӧʧ��: {e.Message}");
        }
    }

    private string GetHeaderValue(string[] headers, string headerName)
    {
        foreach (var header in headers)
        {
            if (header.StartsWith(headerName + ":"))
                return header.Substring(headerName.Length + 1).Trim();
        }
        return "";
    }

    void Update()
    {
        // ���������
        lock (importQueue)
        {
            while (importQueue.Count > 0)
            {
                var transfer = importQueue.Dequeue();
                ProcessMeshTransfer(transfer);
            }
        }
    }

    private void ProcessMeshTransfer(MeshTransfer transfer)
    {
        try
        {
            string fileExtension = Path.GetExtension(transfer.path)?.ToLower();
            switch (fileExtension)
            {
                case ".glb":
                    ImportModelWithMaterial(transfer);
                    break;
                case ".zip":
                    ProcessZipFile(transfer);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing mesh: {e.Message}");
        }
        finally
        {
            CleanupTempFile(transfer.path);
        }
    }

    private void ImportModelWithMaterial(MeshTransfer transfer)
    {
        try
        {
            // ȷ��Ŀ��Ŀ¼����
            string importDir = "Assets/MeshyImports";
            if (!Directory.Exists(importDir))
            {
                Directory.CreateDirectory(importDir);
                AssetDatabase.Refresh();
            }

            // ���ɸ���������ļ���
            string modelName = string.IsNullOrEmpty(transfer.name) ? "Meshy_Model" : transfer.name;
            // �����ļ����еķǷ��ַ�
            modelName = string.Join("_", modelName.Split(Path.GetInvalidFileNameChars()));

            // ��ȡ�ļ���չ��
            string extension = Path.GetExtension(transfer.path);
            if (string.IsNullOrEmpty(extension))
            {
                extension = $".{transfer.file_format}";
            }

            // ����Ψһ�ļ���
            string uniqueFileName = $"{modelName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}{extension}";
            string relativePath = Path.Combine(importDir, uniqueFileName);

            // ���Դ�ļ��Ƿ����
            if (!File.Exists(transfer.path))
            {
                Debug.LogError($"Դ�ļ�������: {transfer.path}");
                return;
            }

            // �ȸ����ļ���Ŀ��λ��
            File.Copy(transfer.path, relativePath, true);

            AssetDatabase.ForceReserializeAssets(new[] { relativePath });

            // ����ģ��
            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);

            // ��ȡ�����ģ��
            GameObject importedObject = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
            if (importedObject != null)
            {
                // ʹ�ô�������ƻ����ɵ�����
                importedObject.name = uniqueFileName;

                // ���Ĭ�ϲ���
                AddDefaultMaterial(importedObject);

                // ȷ���޸ı���
                EditorUtility.SetDirty(importedObject);
                AssetDatabase.SaveAssets();

                // �����߳���ʵ����ģ�͵�����
                EditorApplication.delayCall += () =>
                {
                    // ʵ����ģ�͵�����
                    GameObject sceneObject = PrefabUtility.InstantiatePrefab(importedObject) as GameObject;
                    if (sceneObject != null)
                    {
                        // ���ñ任
                        sceneObject.transform.position = Vector3.zero;
                        sceneObject.transform.rotation = Quaternion.identity;
                        sceneObject.transform.localScale = Vector3.one;

                        // ѡ������ӵĶ���
                        Selection.activeGameObject = sceneObject;

                        // ȷ�����������Ϊ���޸�
                        EditorSceneManager.MarkSceneDirty(sceneObject.scene);

                        Debug.Log($"[Meshy Bridge] ģ������ӵ�����: {sceneObject.name}");
                    }
                };
            }
            Debug.Log($"[Meshy Bridge] ����ģ�ͳɹ�: {relativePath}, ����: {uniqueFileName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Meshy Bridge] ����ģ��ʧ��: {e.Message}\n{e.StackTrace}");
        }
    }

    private void AddDefaultMaterial(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && renderer.sharedMaterials.Length == 0)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.name = "Meshy_Material";
            renderer.sharedMaterial = material;
        }
    }

    private void ProcessZipFile(MeshTransfer transfer)
    {
        // �޸�Ϊʹ��Ԥ�Ȼ�ȡ����ʱ·��
        string extractPath = Path.Combine(_tempCachePath, "extracted");
        ZipFile.ExtractToDirectory(transfer.path, extractPath);

        // �����ѹ����ļ�
        foreach (string file in Directory.GetFiles(extractPath, "*.glb", SearchOption.AllDirectories))
        {
            MeshTransfer newTransfer = new MeshTransfer
            {
                file_format = "glb",
                path = file,
                name = transfer.name
            };
            ImportModelWithMaterial(newTransfer);
        }

        // ������ʱĿ¼
        Directory.Delete(extractPath, true);
    }

    private void CleanupTempFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error cleaning up: {e.Message}");
        }
    }

    void OnDestroy()
    {
        StopServer();
    }
}