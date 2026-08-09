using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Text;

public class RoboflowTest : MonoBehaviour
{
    [Header("Roboflow Config")]
    private string apiKey = "";

    private string modelEndpoint = "https://serverless.roboflow.com/coco-3ltty/1";

    void Start()
    {
        // Load the API key from a local, gitignored text file instead of hardcoding it in this script.
        // This keeps the key out of your public GitHub repo entirely.
        string keyPath = Path.Combine(Application.streamingAssetsPath, "roboflow_apikey.txt");

        if (!File.Exists(keyPath))
        {
            Debug.LogError("[RoboflowTest] Could not find roboflow_apikey.txt in StreamingAssets. " +
                "Create this file and paste your Roboflow API key inside it (no quotes, no extra spaces/lines).");
            return;
        }

        apiKey = File.ReadAllText(keyPath).Trim();

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("[RoboflowTest] roboflow_apikey.txt was found but is empty. Paste your API key inside it.");
            return;
        }

        StartCoroutine(SendTestImageToRoboflow());
    }

    IEnumerator SendTestImageToRoboflow()
    {
        // Step 1: Load the test image from StreamingAssets
        string imagePath = Path.Combine(Application.streamingAssetsPath, "testimage.jpg");
        Debug.Log("[RoboflowTest] Loading image from: " + imagePath);

        byte[] imageBytes;

        // On Android, StreamingAssets is inside a compressed APK, so we must use UnityWebRequest to read it.
        // On Windows/Editor, we can read directly. This handles both cases.
        if (imagePath.Contains("://") || imagePath.Contains(":///"))
        {
            UnityWebRequest fileRequest = UnityWebRequest.Get(imagePath);
            yield return fileRequest.SendWebRequest();

            if (fileRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[RoboflowTest] Failed to load test image: " + fileRequest.error);
                yield break;
            }
            imageBytes = fileRequest.downloadHandler.data;
        }
        else
        {
            imageBytes = File.ReadAllBytes(imagePath);
        }

        Debug.Log("[RoboflowTest] Image loaded, size: " + imageBytes.Length + " bytes");

        // Step 2: Convert image to base64 (this is the format Roboflow's serverless endpoint expects)
        string base64Image = System.Convert.ToBase64String(imageBytes);

        // Step 3: Build the request
        string urlWithKey = modelEndpoint + "?api_key=" + apiKey;

        byte[] bodyRaw = Encoding.UTF8.GetBytes(base64Image);

        UnityWebRequest request = new UnityWebRequest(urlWithKey, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        Debug.Log("[RoboflowTest] Sending request to Roboflow...");

        // Step 4: Send and wait for response
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[RoboflowTest] SUCCESS! Raw response:");
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("[RoboflowTest] Request failed: " + request.error);
            Debug.LogError("[RoboflowTest] Response code: " + request.responseCode);
            Debug.LogError("[RoboflowTest] Response body: " + request.downloadHandler.text);
        }
    }
}
