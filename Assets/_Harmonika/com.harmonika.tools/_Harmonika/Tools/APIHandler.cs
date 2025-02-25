using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class APIHandler : MonoBehaviour
{
    public class APICallBack
    {
        public bool success = false;
        public string message;
        public JObject data;
    }

    [Header("API Configuration")]
    [SerializeField] private string apiLink;
    [SerializeField] private string protocol;

    private string fullLink;

    private void Awake()
    {
        fullLink = CreateFullLink();
    }

    public string CreateFullLink()
    {
        return $"{protocol}://{apiLink}";
    }

    // Public method for GET request using async/await
    public async Task<APICallBack> GetCallAsync(string endpoint)
    {
        string url = $"{fullLink}/{endpoint}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            APICallBack apiCall = new APICallBack();

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield(); // Wait until the request is done
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                apiCall.success = false;
                apiCall.message = request.error;
            }
            else
            {
                apiCall.success = true;

                try
                {
                    apiCall.data = JObject.Parse(request.downloadHandler.text);
                    apiCall.message = "Success";
                }
                catch (System.Exception ex)
                {
                    apiCall.success = false;
                    apiCall.message = $"Error parsing JSON: {ex.Message}";
                }
            }

            return apiCall;
        }
    }

    // Public method for POST request using async/await
    public async Task<APICallBack> PostCallAsync(string endpoint, string jsonBody)
    {
        string url = $"{fullLink}/{endpoint}";
        Debug.Log("Call URL " + url);
        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            APICallBack apiCall = new APICallBack();

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield(); // Wait until the request is done
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                apiCall.success = false;
                apiCall.message = request.error;
            }
            else
            {
                apiCall.success = true;

                try
                {
                    apiCall.data = JObject.Parse(request.downloadHandler.text);
                    apiCall.message = "Success";
                }
                catch (System.Exception ex)
                {
                    apiCall.success = false;
                    apiCall.message = $"Error parsing JSON: {ex.Message}";
                }
            }

            return apiCall;
        }
    }
}