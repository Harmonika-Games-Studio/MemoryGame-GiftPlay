//using JetBrains.Annotations;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;

//public class BubbleSync : MonoBehaviour {

//    [SerializeField]
//    private string baseUrl = "https://app.harmonikagames.com.br/api/1.1/wf";
//    private string apiToken = "80098fb946eaa832654166e173564c66";
//    private string updateUserPath = "/update_user";
//    private string checkRePath = "/check_re";

//    public bool testVersion;
//    public bool webVersion;
//    public UserInfo fields;

//    public static BubbleSync instance;

//    private void Awake() {
//        if (instance == null) {
//            instance = this;
//            DontDestroyOnLoad(gameObject);
//        } else {
//            Destroy(gameObject);
//        }
//    }


//    private void Start() {
//        fields = new UserInfo();

//        baseUrl = testVersion ? "https://app.harmonikagames.com.br/version-test/api/1.1/wf" : "https://app.harmonikagames.com.br/api/1.1/wf";
//    }

//    public void NewFields() {

//        fields = new UserInfo();
//    }
//    #region Check RE

//    public IEnumerator CheckRERequest(string re, Action<UserInfo> callback) {

//        LoadingView.instance.TurnLoadingView(true);

//        // Prepare the JSON data with the 're' field
//        string jsonData = "{\"re\": \"" + re + "\"}";

//        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

//        // Create a UnityWebRequest for POST
//        UnityWebRequest webRequest = new UnityWebRequest(baseUrl + checkRePath, "POST");
//        webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
//        webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
//        webRequest.SetRequestHeader("Content-Type", "application/json");
//        webRequest.SetRequestHeader("Authorization", "Bearer " + apiToken);

//        // Send the request
//        yield return webRequest.SendWebRequest();

//        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError) {
//            Debug.LogError("Error: " + webRequest.error);

//            // Invoke the callback with null on error
//            callback?.Invoke(null);
//        } else {
//            string jsonResponse = webRequest.downloadHandler.text;
//            Debug.Log("Received: " + jsonResponse);

//            try {
//                // Parse the response using JObject from Newtonsoft.Json
//                JObject root = JObject.Parse(jsonResponse);

//                // Check if status is "success" and response exists
//                if ((string)root["status"] == "success" && root["response"]?["info"] != null) {
//                    JObject infoJson = (JObject)root["response"]["info"];

//                    // Map fields from the JSON to UserInfo object
//                    UserInfo userInfo = new UserInfo {
//                        Nome = infoJson["Nome"]?.ToString() ?? string.Empty,
//                        RE = infoJson["RE"]?.ToString() ?? string.Empty,
//                        orientacaoSexual = infoJson["orientacaoSexual"]?.ToString() ?? string.Empty,
//                        raca = infoJson["raca"]?.ToString() ?? string.Empty,
//                        identidadeGenero = infoJson["identidadeGenero"]?.ToString() ?? string.Empty,
//                        origem = infoJson["origem"]?.ToString() ?? string.Empty
//                    };

//                    fields = userInfo;

//                    // Invoke the callback with the parsed data
//                    callback?.Invoke(userInfo);
//                } else {
//                    Debug.LogWarning("Response received but parsed object is null or incomplete.");

//                    // Invoke the callback with null if parsing fails
//                    callback?.Invoke(null);
//                }
//            }
//            catch (Exception e) {
//                Debug.LogError("Failed to parse response: " + e.Message);

//                // Invoke the callback with null on exception
//                callback?.Invoke(null);
//            }
//        }
//        LoadingView.instance.TurnLoadingView(false);
//    }

//    // Class to map the response JSON
//    [System.Serializable]
//    public class UserInfo {
//        public string Nome;
//        public string RE;
//        public string orientacaoSexual;
//        public string raca;
//        public string identidadeGenero;
//        public string origem;
//    } 

//    #endregion

//    #region UpdateInfo

//    public void SetFieldsNotAccepted() {

//        fields.orientacaoSexual = "Prefiro não responder";
//        fields.raca = "Prefiro não responder";
//        fields.identidadeGenero = "Prefiro não responder";
//    }

//    public void UpdateFieldsWithCallback(Action<bool> callback) {
//        fields.origem = webVersion ? "Web" : "Tablet";
//        Debug.Log("Start SendLead to Bubble coroutine with retries");
//        StartCoroutine(UpdateUserRequestWithRetry(fields, callback));
//    }

//    private IEnumerator UpdateUserRequestWithRetry(UserInfo fieldData, Action<bool> callback) {

//        LoadingView.instance.TurnLoadingView(true);

//        int maxRetries = 3;
//        int attempt = 0;
//        bool success = false;

//        while (attempt < maxRetries && !success) {
//            attempt++;
//            Debug.Log($"Attempt {attempt} to send lead.");

//            // Prepare the JSON data
//            string jsonData = "{\"orientacaoSexual\": \"" + fieldData.orientacaoSexual + "\", \"raca\": \"" + fieldData.raca + "\", \"identidadeGenero\": \"" + fieldData.identidadeGenero +
//                "\", \"re\": \"" + fieldData.RE + "\", \"origem\": \"" + fieldData.origem + "\"}";

//            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

//            // Create a UnityWebRequest for POST
//            UnityWebRequest webRequest = new UnityWebRequest(baseUrl + updateUserPath, "POST");
//            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
//            webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
//            webRequest.SetRequestHeader("Content-Type", "application/json");
//            webRequest.SetRequestHeader("Authorization", "Bearer " + apiToken);

//            // Send the request
//            yield return webRequest.SendWebRequest();

//            // Verifique o código HTTP da resposta
//            if (webRequest.responseCode == 200) {
//                Debug.Log("Requisição enviada com sucesso. Código HTTP 200 recebido.");
//                success = true;

//                // Log da resposta (opcional)
//                Debug.Log("Resposta do servidor: " + webRequest.downloadHandler.text);
//            } else {
//                Debug.LogError($"Erro: Código HTTP {webRequest.responseCode}. Tentando novamente...");
//                Debug.LogError("Erro detalhado: " + webRequest.error);
//            }

//            // Atraso antes da próxima tentativa
//            if (!success && attempt < maxRetries) {
//                yield return new WaitForSeconds(1);
//            }
//        }

//        // Callback with the final result
//        callback?.Invoke(success);

//        // Log result to UI
//        if (success) {
//            Debug.Log("Lead sending completed successfully.");
//        } else {
//            Debug.LogError("Failed to send lead after multiple attempts.");
//        }

//        LoadingView.instance.TurnLoadingView(false);
//    }


//    #endregion
//}

