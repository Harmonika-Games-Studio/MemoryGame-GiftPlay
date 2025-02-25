using EntranceJew.UDateTime;
using Harmonika.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using NaughtyAttributes;

public class DataSync : MonoBehaviour
{
    private const string NEW_ALLOWED_DATE_KEY = "NewAllowedDate";
    private const string TEST_URL = "https://www.google.com";
    private const string FIRESTORE_API_KEY = "AIzaSyAKsGVs1xN5JKzv_CJLy_nr0lDpesZHK3Q";
    private const string FIRESTORE_PROJECT_ID = "harmonikagames-eventos";
    private const string BUBBLE_BASE_URL = "https://app.harmonikagames.com.br/api/1.1/wf";
    private const string BUBBLE_TEST_URL = "https://app.harmonikagames.com.br/version-test/api/1.1/wf";
    private const string BUBBLE_API_TOKEN = "80098fb946eaa832654166e173564c66";
    private const string BUBBLE_LEADS_EVENT_PATH = "/leads_event";
    private const string BUBBLE_PROJECT_INFO_EVENT_PATH = "/project_info_event";

    [SerializeField] private Button _syncDataButton;

    [BoxGroup("Configuration")][SerializeField] private int _appId;
    [BoxGroup("Configuration")][SerializeField] private UDateTime _maxAllowedDate;
    [BoxGroup("Configuration")][SerializeField] private DataBase _database;
    [BoxGroup("Configuration")][SerializeField] private bool _testVersion = true;
    [BoxGroup("Configuration")][SerializeField] private bool _syncPeriodically = true;
    [BoxGroup("Configuration")][SerializeField][ShowIf(nameof(_syncPeriodically))] private int _syncTime = 30;

    private JObject _leads = new();
    private UserInfo _userFields = new();
    private ProjectInfo _prjFields = new();
    private List<LeadCaptation >_leadCaptationList = new();

    private DateTime _newAllowedDate;
    private string _appSubscriptionId;
    private string _appDeviceUsedId;
    private string _appUserId;
    private string _appDatabaseId;
    private string _documentName;
    private string _documentId;
    private string _tempLocalDataPath;
    private string _permanentLocalDataPath;
    private string _url;
    private bool _isConnected;
    private bool _canSendLeads = false;

    #region Properties
    public bool IsNewAllowedDateExpired
    {
        get
        {
            DateTime newAllowedDate = DateTime.Parse(PlayerPrefs.GetString(NEW_ALLOWED_DATE_KEY));
            Debug.Log($"Data atual: {DateTime.Now}, Data extra m�xima permitida: {newAllowedDate}");
            return DateTime.Now > newAllowedDate;
        }
    }

    public int TempDataCount
    {
        get
        {
            List<Dictionary<string, string>> localData = LoadLocalData(_tempLocalDataPath);
            return localData != null ? localData.Count : 0;
        }
    }

    public int PermanentDataCount
    {
        get
        {
            List<Dictionary<string, string>> localData = LoadLocalData(_permanentLocalDataPath);
            return localData != null ? localData.Count : 0;
        }
    }

    public bool IsSyncButtonInteractable
    {
        set
        {
            _syncDataButton.interactable = value;
        }
        get
        {
            return _syncDataButton.interactable;
        }
    }

    public List<Dictionary<string, string>> PermanentData
    {
        get
        {
            return LoadLocalData(_permanentLocalDataPath);
        }
    }

    public int AppId
    {
        get => _appId;
        set
        {
            _appId = value;
            PlayerPrefs.SetInt("AppID", _appId);
            PlayerPrefs.Save();
        }
    }

    public bool TestVersion
    {
        get => _testVersion;
        set
        {
            _testVersion = value;
            UpdateDatabaseUrl();
            PlayerPrefs.SetInt("TestVersion", _testVersion ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    #endregion

    private void Awake()
    {
        UpdateDatabaseUrl();
        Setup();
    }

    private void OnDestroy()
    {
        // This is a good practice, though it's not strictly necessary in this case since this object is marked as 'Don't Destroy On Load'.
        _syncDataButton.onClick.RemoveAllListeners();
    }

    private void Start()
    {
        Invoke(nameof(InitialConnect), .3f);
        if (_leadCaptationList.Count == 0)
        {
            Debug.LogWarning("No LeadCaptation Script found, if you are trying to capture Leads, please, assign the componente correctly");
            return;
        }

        foreach (var leadCaptation in _leadCaptationList)
        {
            leadCaptation.OnSubmitEvent += AddDataToJObject;
        }

        //_leadCaptationList[0].OnSubmitEvent += (JObject a) => SendLeads(); //gambiarra de teste. Pra quando n tiver nenhum jogo usando isso

        _syncDataButton.onClick.AddListener(() => ConnectToDatabase(true));
        if (_syncPeriodically) PeriodicConnect();
    }

    public void AddDataToJObject(string key, string value)
    {
        if (_leads.ContainsKey(key))
            _leads[key] = value;
        else
        _leads.Add(key, value);
    }
    
    public void AddDataToJObject(string key, int value)
    {
        if (_leads.ContainsKey(key))
            _leads[key] = value;
        else
            _leads.Add(key, value);
    }
    
    public void AddDataToJObject(string key, float value)
    {
        if (_leads.ContainsKey(key))
            _leads[key] = value;
        else
            _leads.Add(key, value);
    }

    public void AddDataToJObject(JObject obj)
    {
        foreach (var property in obj.Properties())
        {
            if (_leads.ContainsKey(property.Name))
                _leads[property.Name] = property.Value;
            else
                _leads.Add(property.Name, property.Value);
        }
    }

    public void AddLeadCaptation(LeadCaptation leadCaptation)
    {
        _leadCaptationList.Add(leadCaptation);
    }

    public void SendLeads()
    {
        AppManager.Instance.DataSync.AddDataToJObject("dataHora", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"));

        Dictionary<string, string> fields = new Dictionary<string, string>();

        foreach (var property in _leads.Properties())
        {
            fields[property.Name] = property.Value.ToString();
        }

        SaveLocalData(_tempLocalDataPath, fields);
        SaveLocalData(_permanentLocalDataPath, fields);
        _leads = new();
    }

    public IEnumerator GetStatusFromDatabase()
    {
        if (_database == DataBase.Firebase)
            yield return StartCoroutine(GetStatusFromFirestore());
        else if (_database == DataBase.Bubble)
            yield return StartCoroutine(GetStatusFromBubble(_appId.ToString()));
    }

    /// <summary>
    /// Checks if the application has Internet access by attempting to connect to a reliable external address.
    /// </summary>
    /// <returns>Returns true if the Internet connection is available; otherwise, returns false.</returns>
    public static bool IsInternetAvailable()
    {
        try
        {
            using (var client = new WebClient())
            {
                using (client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    void Setup()
    {
        _tempLocalDataPath = Application.persistentDataPath + "/tempLocalData.json";
        _permanentLocalDataPath = Application.persistentDataPath + "/permanentLocalData.json";
    }

    private void UpdateDatabaseUrl()
    {
        _url = _testVersion ? BUBBLE_TEST_URL : BUBBLE_BASE_URL;
    }

    private void PeriodicConnect()
    {
        if (TempDataCount > 0)
            ConnectToDatabase(true);
        
        InvokeUtils.Invoke(PeriodicConnect, _syncTime);
    }

    private void InitialConnect()
    {
        ConnectToDatabase(false);
    }

    private void ConnectToDatabase(bool sendLeads)
    {
        //_canSendLeads is set to true only when this function is called from Sync Button
        _canSendLeads = sendLeads;

        if (_database == DataBase.Firebase)
            StartCoroutine(ConnectToFirestore());
        else if (_database == DataBase.Bubble)
            StartCoroutine(ConnectToBubble());
    }

    private void SaveNewLocalDueDate()
    {
        _newAllowedDate = DateTime.Now.AddDays(5);
        PlayerPrefs.SetString(NEW_ALLOWED_DATE_KEY, _newAllowedDate.ToString());
        PlayerPrefs.Save();

        Debug.Log($"Salvou nova data para dia {_newAllowedDate}");
    }

    private void SetDocumentId(string documentId)
    {
        _documentId = documentId;
        StartCoroutine(SyncLocalDataWithFirestore());
    }

    private void RemoveLocalData(Dictionary<string, string> fields)
    {
        List<Dictionary<string, string>> localData = LoadLocalData(_tempLocalDataPath);
        if (localData != null)
        {
            // Encontrar o item correspondente nos dados locais
            var itemToRemove = localData.FirstOrDefault(item => item.SequenceEqual(fields));
            if (itemToRemove != null)
            {
                localData.Remove(itemToRemove);
                // Salvar os dados atualizados de volta no arquivo local
                File.WriteAllText(_tempLocalDataPath, JsonConvert.SerializeObject(new Serialization<List<Dictionary<string, string>>>(localData)));
                Debug.Log("Dados locais removidos com sucesso.");
            }
            else
            {
                Debug.LogWarning("Item a ser removido n�o encontrado nos dados locais.");
            }
        }
    }

    private void SaveLocalData(string path, Dictionary<string, string> fields)
    {
        var localData = LoadLocalData(path) ?? new List<Dictionary<string, string>>();
        localData.Add(fields);
        var json = JsonConvert.SerializeObject(new Serialization<List<Dictionary<string, string>>>(localData));
        File.WriteAllText(path, json);

        Debug.Log("Campos salvos localmente: " + JsonConvert.SerializeObject(fields) + "\nSaved on " + path);
    }

    private List<Dictionary<string, string>> LoadLocalData(string path)
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<Serialization<List<Dictionary<string, string>>>>(json).Data;
            }
        }
        return new List<Dictionary<string, string>>();
    }

    //Firebase
    IEnumerator ConnectToFirestore() 
    {
        if (IsInternetAvailable())
        {
            yield return StartCoroutine(GetStatusFromFirestore());
        }
        else
        {
            Debug.LogWarning("No internet connection.");

            if (PlayerPrefs.HasKey(NEW_ALLOWED_DATE_KEY))
            {
                Debug.Log($"Data atual: {DateTime.Now}, Data m�xima permitida: {PlayerPrefs.GetString(NEW_ALLOWED_DATE_KEY)}");

                if (IsNewAllowedDateExpired)
                {
                    AppManager.Instance.BlockApplication(BlockType.NoConnectionAndDueDate);
                }
                else
                {
                    AppManager.Instance.ShowLeadsMessage("Sem conex�o com a internet. Verifique sua conex�o.");
                }

            }
            else if (DateTime.Now.Date > _maxAllowedDate.dateTime)
            {
                Debug.Log($"Data atual: {DateTime.Now}, Data m�xima permitida: {_maxAllowedDate.dateTime}");
                AppManager.Instance.BlockApplication(BlockType.NoConnectionAndDueDate);
            }
            else
            {
                AppManager.Instance.ShowLeadsMessage("Sem conex�o com a internet. Verifique sua conex�o.");
            }
        }

    }
    
    IEnumerator GetStatusFromFirestore()
    {
        // Desativa��o do Bot�o
        _syncDataButton.interactable = false;

        // Cria��o da URL
        string url = $"https://firestore.googleapis.com/v1/projects/{FIRESTORE_PROJECT_ID}/databases/(default)/documents:runQuery?key={FIRESTORE_API_KEY}";
        Debug.Log($"Sending request to URL: {url}");

        // Defini��o da Consulta JSON
        string jsonQuery = $@"
    {{
        'structuredQuery': {{
            'from': [{{
                'collectionId': 'apps'
            }}],
            'where': {{
                'fieldFilter': {{
                    'field': {{ 'fieldPath': 'id' }},
                    'op': 'EQUAL',
                    'value': {{ 'integerValue': '{_appId}' }}
                }}
            }}
        }}
    }}";

        // Prepara��o e envio da Requisi��o
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, jsonQuery);
        request.SetRequestHeader("Content-Type", "application/json");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonQuery);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        // Tratamento de Erros
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseBody = request.downloadHandler.text;
            JArray jsonArray = JArray.Parse(responseBody);

            if (jsonArray.Count > 0)
            {
                var document = jsonArray[0]["document"];

                _documentName = document["name"].ToString();

                string id = document["fields"]["id"]["integerValue"].ToString();
                string name = document["fields"]["name"]["stringValue"].ToString();
                bool allowed = bool.Parse(document["fields"]["liberado"]["booleanValue"].ToString());
                _appDeviceUsedId = document["fields"]["deviceid"]["stringValue"].ToString();
                _appUserId = document["fields"]["userid"]["stringValue"].ToString();
                _appDatabaseId = document["fields"]["databaseid"]["stringValue"].ToString();

                Debug.Log($"ID: {id}, Name: {name}, Allowed:{allowed}, PaymentID: {_appSubscriptionId}, DeviceID: {_appDeviceUsedId}, UserID: {_appUserId}, DatabaseID: {_appDatabaseId}");

                string currentDeviceId = SystemInfo.deviceUniqueIdentifier;

                if (string.IsNullOrEmpty(_appDeviceUsedId))
                {
                    StartCoroutine(UpdateDeviceIdInFirestore(currentDeviceId, allowed));
                }
                else if (_appDeviceUsedId == currentDeviceId)
                {
                    if (allowed)
                    {
                        SaveNewLocalDueDate();
                        AppBlocker.Instance.IsBlocked = false;

                        if (!string.IsNullOrEmpty(_appDatabaseId))
                        {
                            SetDocumentId(_appDatabaseId);
                        }
                        else
                        {
                            AppManager.Instance.ShowLeadsMessage("Banco de dados não encontrado.\nEntre em contato com o suporte.");
                        }
                    }
                    else
                    {
                        Debug.Log("Subscription is not active. Access denied.");
                        AppManager.Instance.ShowLeadsMessage("A sua licença expirou, entre em contato para regularizar a situação.");
                        AppManager.Instance.BlockApplication(BlockType.LicenseNotActive);
                    }
                }
                else
                {
                    yield return StartCoroutine(UpdateDeviceIdInFirestore(currentDeviceId, allowed));
                }

                _syncDataButton.interactable = true;
                yield break;
            }
        }
        else
        {
            Debug.LogError("Error getting documents: " + request.error);
            AppManager.Instance.ShowLeadsMessage("Erro ao acessar o banco de dados.\nCheque sua conex�o com internet ou entre em contato com o suporte.");
            AppManager.Instance.BlockApplication(BlockType.NoConnectionAndDueDate);
            Debug.LogError("No matching documents found.");
            AppManager.Instance.ShowLeadsMessage("Aplicativo n�o encontrado.\nEntre em contato com o suporte.");
            _syncDataButton.interactable = true;
        }
    }

    IEnumerator UpdateDeviceIdInFirestore(string deviceId, bool allowed)
    {
        string url = $"https://firestore.googleapis.com/v1/{_documentName}?key={FIRESTORE_API_KEY}&updateMask.fieldPaths=deviceid";
        Debug.Log($"Sending update request to URL: {url}");

        string jsonBody = $"{{ \"fields\": {{ \"deviceid\": {{ \"stringValue\": \"{deviceId}\" }} }} }}";

        UnityWebRequest request = new UnityWebRequest(url, "PATCH");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error updating device ID: " + request.error);
            AppManager.Instance.ShowLeadsMessage("Erro ao atualizar o dispositivo.\nCheque sua conex�o com internet ou entre em contato com o suporte.");
        }
        else
        {
            Debug.Log("Device ID updated successfully.");
            if (allowed)
            {
                if (!string.IsNullOrEmpty(_appDatabaseId))
                {
                    SetDocumentId(_appDatabaseId);
                }
                else
                {
                    AppManager.Instance.ShowLeadsMessage("Banco de dados n�o encontrado.\nEntre em contato com o suporte.");
                }
            }
            else
            {
                Debug.Log("Subscription is not active. Access denied.");
                AppManager.Instance.ShowLeadsMessage("A sua licen�a expirou, entre em contato para regularizar a situa��o.");
            }
        }
    }

    IEnumerator SyncLocalDataWithFirestore()
    {

        List<Dictionary<string, string>> localData = LoadLocalData(_tempLocalDataPath);
        if (localData != null)
        {
            int successCount = 0;
            AppManager.Instance.SyncMenu.TransferingData = true;

            bool documentExists = false;
            yield return StartCoroutine(CheckIfDocumentExists((exists) =>
            {
                if (exists)
                {
                    documentExists = true;
                }
            }));

            if (documentExists)
            {
                //start transfer UI

                foreach (var fields in localData)
                {

                    yield return StartCoroutine(SaveDocumentFirebase(fields, (success) =>
                    {
                        if (success)
                        {
                            successCount++;
                        }
                        AppManager.Instance.SyncMenu.UpdateDataCount();
                    }));
                }
                Debug.Log(successCount + " dados locais foram salvos no banco com sucesso.");
            }

            AppManager.Instance.SyncMenu.TransferingData = false;
            AppManager.Instance.ShowLeadsMessage($"Transfer�ncia de dados realizada com sucesso!\n\nQuantidade de dados tranferidos: {successCount}");
        }
    }

    IEnumerator SaveDocumentFirebase(Dictionary<string, string> fields, System.Action<bool> callback)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{FIRESTORE_PROJECT_ID}/databases/(default)/documents/databases/{_documentId}/leads?key={FIRESTORE_API_KEY}";

        JObject jsonBody = new JObject();
        JObject fieldsObject = new JObject();

        foreach (var field in fields)
        {
            fieldsObject[field.Key] = new JObject { { "stringValue", field.Value } };
        }

        jsonBody["fields"] = fieldsObject;

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonBody.ToString());
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error saving document: " + request.error);
            callback(false);
        }
        else
        {
            Debug.Log("Document saved successfully: " + request.downloadHandler.text);
            RemoveLocalData(fields); // Remove local data if the save is successful
            callback(true);
        }
    }

    IEnumerator CheckIfDocumentExists(System.Action<bool> callback)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{FIRESTORE_PROJECT_ID}/databases/(default)/documents/databases/{_documentId}?key={FIRESTORE_API_KEY}";

        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            callback(false);
        }
        else
        {
            callback(true);
        }
    }

    //Bubble
    IEnumerator ConnectToBubble()
    {
        if (IsInternetAvailable())
        {
            yield return StartCoroutine(GetStatusFromBubble(_appId.ToString()));
        }
        else
        {
            Debug.LogWarning("No internet connection.");

            if (PlayerPrefs.HasKey(NEW_ALLOWED_DATE_KEY))
            {
                Debug.Log($"Data atual: {DateTime.Now}, Data máxima permitida: {PlayerPrefs.GetString(NEW_ALLOWED_DATE_KEY)}");

                if (IsNewAllowedDateExpired)
                {
                    AppManager.Instance.BlockApplication(BlockType.NoConnectionAndDueDate);
                }
                else
                {
                    AppManager.Instance.ShowLeadsMessage("Sem conexão com a internet. Verifique sua conexão.");
                }

            }
            else if (DateTime.Now.Date > _maxAllowedDate.dateTime)
            {
                Debug.Log($"Data atual: {DateTime.Now}, Data máxima permitida: {_maxAllowedDate.dateTime}");
                AppManager.Instance.BlockApplication(BlockType.NoConnectionAndDueDate);
            }
            else
            {
                AppManager.Instance.ShowLeadsMessage("Sem conexão com a internet. Verifique sua conexão.");
            }
        }

    }

    IEnumerator GetStatusFromBubble(string id, Action<ProjectInfo> callback = null)
    {
        // Prepare the JSON data with the 'id' field
        string jsonData = $"{{\"{nameof(id)}\": \"" + id + "\"}";

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        // Create a UnityWebRequest for POST
        UnityWebRequest webRequest = new UnityWebRequest(_url + BUBBLE_PROJECT_INFO_EVENT_PATH, "POST");
        webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Authorization", "Bearer " + BUBBLE_API_TOKEN);

        // Send the request
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + webRequest.error);
            AppManager.Instance.ShowLeadsMessage("Banco de dados não encontrado.\nEntre em contato com o suporte.");
            callback?.Invoke(null);
        }
        else
        {
            string jsonResponse = webRequest.downloadHandler.text;
            Debug.Log("Received: " + jsonResponse);

            try
            {
                JObject root = JObject.Parse(jsonResponse);

                if ((string)root["status"] == "success" && root["response"]?["info"] != null)
                {
                    JObject infoJson = (JObject)root["response"]["info"];

                    ProjectInfo projectInfo = new();

                    projectInfo.liberado = infoJson[nameof(projectInfo.liberado)]?.ToString();
                    projectInfo.projectTitle = infoJson[nameof(projectInfo.projectTitle)]?.ToString();

                    Debug.Log($"ID: {id}, Name: {projectInfo.projectTitle}, Allowed:{projectInfo.liberado}, PaymentID: , DeviceID: , UserID: , DatabaseID: ");

                    Debug.Log($"Properties in ProjectInfo: {string.Join(", ", typeof(ProjectInfo).GetProperties().Select(p => p.Name))}");

                    //foreach (var property in typeof(ProjectInfo).GetProperties())
                    //{
                    //    var jsonKey = property.Name;
                    //    var jsonValue = infoJson[jsonKey]?.ToString() ?? string.Empty;
                    //    property.SetValue(projectInfo, jsonValue);
                    //}

                    if (bool.Parse(projectInfo.liberado))
                    {
                        SaveNewLocalDueDate();
                        AppBlocker.Instance.IsBlocked = false;

                    }
                    else
                    {
                        Debug.Log("Subscription is not active. Access denied.");
                        AppManager.Instance.ShowLeadsMessage("A sua licença expirou, entre em contato para regularizar a situação.");
                        AppManager.Instance.BlockApplication(BlockType.LicenseNotActive);
                    }

                    // Invoke the callback with the parsed data
                    callback?.Invoke(projectInfo);
                }
                else
                {
                    Debug.LogWarning("Response received but parsed object is null or incomplete.");

                    // Invoke the callback with null if parsing fails
                    callback?.Invoke(null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to parse response: " + e.Message);

                // Invoke the callback with null on exception
                callback?.Invoke(null);
            }
        }

        if (_canSendLeads) {
            StartCoroutine(SyncLocalDataWithBubble());
            _canSendLeads = false;
        }
    }

    IEnumerator SyncLocalDataWithBubble()
    {

        List<Dictionary<string, string>> localData = LoadLocalData(_tempLocalDataPath);
        if (localData != null)
        {
            int successCount = 0;
            AppManager.Instance.SyncMenu.TransferingData = true;

            bool documentExists = false;
            yield return StartCoroutine(CheckIfDocumentExists((exists) =>
            {
                if (exists)
                {
                    documentExists = true;
                }
            }));

            if (documentExists)
            {
                foreach (var fields in localData)
                {

                    yield return StartCoroutine(SendLeadsToBubble(fields, (success) =>
                    {
                        if (success)
                        {
                            successCount++;
                        }
                        AppManager.Instance.SyncMenu.UpdateDataCount();
                    }));
                }
                Debug.Log(successCount + " dados locais foram salvos no banco com sucesso.");
            }

            AppManager.Instance.SyncMenu.TransferingData = false;
            AppManager.Instance.ShowLeadsMessage($"Transferência de dados realizada com sucesso!\n\nQuantidade de dados tranferidos: {successCount}");
        }
    }

    IEnumerator SendLeadsToBubble(Dictionary<string, string> fields, System.Action<bool> callback)
    {
        bool success = false;
        LoadingScript.Instance.Loading = true;

        // Prepare the JSON data
        string jsonData = "{" +
            "\"cargo\": \"\"," +
            "\"autorizoContato\": \"\"," +
            "\"cep\": \"\"," +
            "\"cidade\": \"\"," +
            "\"cnpj\": \"\"," +
            "\"cpf\": \"\"," +
            "\"dataNascimento\": \"\"," +
            "\"email\": \"\"," +
            "\"empresa\": \"\"," +
            "\"estado\": \"\"," +
            "\"idade\": \"\"," +
            "\"id\": 0," +
            "\"pontos\": 0," +
            "\"nome\": \"\"," +
            "\"telefone\": \"\"," +
            "\"brinde\": \"\"," +
            "\"dataHora\": \"\"," +
            "\"tempo\": 0," +
            "\"ganhou\": \"\"," +
            "\"custom1\": \"\"," +
            "\"custom2\": \"\"," +
            "\"custom3\": \"\"," +
            "\"custom4\": \"\"," +
            "\"custom5\": \"\"" +
        "}";

        JObject fieldsObject = JObject.Parse(jsonData);
        fieldsObject["id"] = _appId;

        foreach (var field in fields)
        {
            fieldsObject[field.Key] = field.Value;
        }

        // Optionally, log the final userInfo JObject
        Debug.Log("Final userInfo JSON: " + fieldsObject.ToString());

        LoadingScript.Instance.Loading = false;


        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(fieldsObject.ToString());

        // Create a UnityWebRequest for POST
        UnityWebRequest webRequest = new UnityWebRequest(_url + BUBBLE_LEADS_EVENT_PATH, "POST");
        webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Authorization", "Bearer " + BUBBLE_API_TOKEN);

        // Send the request
        yield return webRequest.SendWebRequest();

        // Verifique o código HTTP da resposta
        if (webRequest.responseCode == 200)
        {
            Debug.Log("Requisi��o enviada com sucesso. C�digo HTTP 200 recebido.");
            success = true;

            // Log da resposta (opcional)
            Debug.Log("Resposta do servidor: " + webRequest.downloadHandler.text);
            RemoveLocalData(fields); // Remove local data if the save is successful

        }
        else
        {
            Debug.LogError($"Erro: C�digo HTTP {webRequest.responseCode}. Tentando novamente...");
            Debug.LogError("Erro detalhado: " + webRequest.error);
        }


        callback?.Invoke(success);

        LoadingScript.Instance.Loading = false;
    }

    [System.Serializable] private class Serialization<T>
    {
        public Serialization(T data)
        {
            Data = data;
        }
        public T Data;
    }
}

public class UserInfo
{
    public string autorizoContato = string.Empty;
    public string brinde = string.Empty;
    public string cargo = string.Empty;
    public string cep = string.Empty;
    public string cidade = string.Empty;
    public string cnpj = string.Empty;
    public string cpf = string.Empty;
    public string dataHora = string.Empty;
    public string dataNascimento = string.Empty;
    public string email = string.Empty;
    public string empresa = string.Empty;
    public string estado = string.Empty;
    public string idade = string.Empty;
    public string id = string.Empty;
    public string nome = string.Empty;
    public string telefone = string.Empty;
    public string custom1 = string.Empty;
    public string custom2 = string.Empty;
    public string custom3 = string.Empty;
    public string custom4 = string.Empty;
    public string custom5 = string.Empty;
}

public class ProjectInfo
{
    //public string arquivosCliente;
    //public string briefing;
    //public string buildLink;
    //public string camposLeads;
    //public string captaLeads;
    //public string cobrancaAtual;
    //public string cobrancasList;
    //public string codigoLicenca;
    //public string comentarios;
    //public string comercial;
    //public string configInicial;
    //public string coverArt;
    //public string dataEntrega;
    //public string dataModificacao;
    //public string databaseid;
    //public string dataCriacao;
    //public string dataFinalEvento;
    //public string dataInicialEvento;
    //public string descricao;
    //public string desenvolvedor;
    //public string desenvolvedores;
    //public string devStatus;
    //public string dispositivos;
    //public string distribuiBrindes;
    //public string finalizado;
    //public string id;
    //public string idProjeto;
    //public string idVisualProjetoURL;
    //public string idVisualProjetoArquivo;
    //public string implementa��oVisual;
    //public string isNormal;
    public string liberado;
    //public string listaBrindes;
    //public string listaLeads;
    //public string locacao;
    //public string lojas;
    //public string nota;
    //public string orientacao;
    //public string proprietario;
    //public string provaTeste;
    //public string status;
    //public string task;
    //public string testesAtivacao;
    //public string tipoDeAsset;
    //public string tipoDeAtivacao;
    //public string tipoDeBuild;
    //public string tipoDeConexao;
    public string projectTitle;
    //public string videosList;
    //public string webGameFrame;
}

public enum DataBase
{
    Bubble,
    Firebase
}