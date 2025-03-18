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
using UnityEngine.Events;

public class DataSync : MonoBehaviour
{
    private const string NEW_ALLOWED_DATE_KEY = "NewAllowedDate";
    private const string TEST_URL = "https://www.google.com";
    private const string DATABASE_URL = "https://giftplay.com.br/";
    private const string SENDLEADS_ENDPOINT = "/leads/send";
    private const string GETSTATUS_ENDPOINT = "/builds/isBuildAtive";
    [SerializeField] private Button _syncDataButton;

    [BoxGroup("Configuration")][SerializeField] private int _appId;
    [BoxGroup("Configuration")][SerializeField] private UDateTime _maxAllowedDate;
    [BoxGroup("Configuration")][SerializeField] private bool _syncPeriodically = false;
    [BoxGroup("Configuration")][SerializeField][ShowIf(nameof(_syncPeriodically))] private int _syncTime = 30;

    private JObject _leads = new();
    private UserInfo _userFields = new();
    private ProjectInfo _prjFields = new();
    private List<LeadCaptation> _leadCaptationList = new();

    private DateTime _newAllowedDate;
    private string _authenticationToken;
    private string _tempLocalDataPath;
    private string _permanentLocalDataPath;
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
    #endregion

    public virtual void Awake()
    {
        Setup();
    }

    public virtual void OnDestroy()
    {
        // This is a good practice, though it's not strictly necessary in this case since this object is marked as 'Don't Destroy On Load'.
        _syncDataButton.onClick.RemoveAllListeners();
    }

    public virtual void Start()
    {
        Invoke(nameof(InitialConnect), .3f);

        _syncDataButton.onClick.AddListener(() => ConnectToDatabase(true));
        if (_syncPeriodically) PeriodicConnect();

        if (_leadCaptationList.Count == 0)
        {
            Debug.LogWarning("No LeadCaptation Script found, if you are trying to capture Leads, please, assign the componente correctly");
            return;
        }

        foreach (var leadCaptation in _leadCaptationList)
        {
            leadCaptation.OnSubmitEvent += AddDataToJObject;
        }
    }

    public void AddDataToJObject(string key, string value)
    {
        Debug.Log($"key:{key} value:{value}");
        if (_leads.ContainsKey(key))
            _leads[key] = value;
        else
            _leads.Add(key, value);
    }

    public void AddDataToJObject(string key, int value)
    {
        Debug.Log($"key:{key} value:{value}");
        if (_leads.ContainsKey(key))
            _leads[key] = value;
        else
            _leads.Add(key, value);
    }

    public void AddDataToJObject(string key, float value)
    {
        Debug.Log($"key:{key} value:{value}");
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

    public void SaveLeads()
    {
        AppManager.Instance.DataSync.AddDataToJObject("dataHora", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"));

        Dictionary<string, string> fields = new Dictionary<string, string>();

        foreach (var property in _leads.Properties())
        {
            fields[property.Name] = property.Value.ToString();
        }

#if !UNITY_WEBGL
        SaveLocalData(_tempLocalDataPath, fields);
        SaveLocalData(_permanentLocalDataPath, fields);
#else
      SendLeads(fields);
#endif
        _leads = new();
    }

    public void SendLeads(Dictionary<string, string> data, UnityAction<bool> callback = null)
    {
        StartCoroutine(SendLeadsRoutine(data, callback));
    }

    /// <summary>
    /// Checks if the application has Internet access by attempting to connect to a reliable external address.
    /// </summary>
    /// <returns>Returns true if the Internet connection is available; otherwise, returns false.</returns>
    public static bool IsInternetAvailable()
    {
#if UNITY_WEBGL
        return true;
#endif
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
        _authenticationToken = GetAuthenticationToken();
        Debug.Log("_authenticationToken: " + _authenticationToken);
    }

    string GetAuthenticationToken()
    {
        string filePath = Path.Combine(HarmonikaConstants.RESOURCES_PATH, "KEY.txt");

        if (!File.Exists(filePath))
        {
            Debug.LogError("Auth file KEY.txt not found at: " + filePath);
            return null;
        }
        
        string encryptedData = File.ReadAllText(filePath);


        string data = SecureDataHandler.DecryptBase64Data(encryptedData, "SammViado2469");
        return data;
    }

    private void PeriodicConnect()
    {
        if (TempDataCount > 0)
            ConnectToDatabase(true);

        InvokeUtility.Invoke(_syncTime, PeriodicConnect);
    }

    private void InitialConnect()
    {
        ConnectToDatabase(false);
    }

    private void ConnectToDatabase(bool sendLeads)
    {
        //_canSendLeads is set to true only when this function is called from Sync Button
        _canSendLeads = sendLeads;
        IsSyncButtonInteractable = false;

        StartCoroutine(ConnectToBubble());
    }

    private void SaveNewLocalDueDate()
    {
        _newAllowedDate = DateTime.Now.AddDays(5);
        PlayerPrefs.SetString(NEW_ALLOWED_DATE_KEY, _newAllowedDate.ToString());
        PlayerPrefs.Save();

        Debug.Log($"Salvou nova data para dia {_newAllowedDate}");
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

    IEnumerator ConnectToBubble()
    {
        if (IsInternetAvailable())
        {
            yield return StartCoroutine(GetStatusFromDatabase());
        }
        else
        {
            Debug.LogWarning("No internet connection.");
            IsSyncButtonInteractable = true;

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

    public IEnumerator GetStatusFromDatabase(Action<ProjectInfo> callback = null)
    {

        // Create a UnityWebRequest for POST
        UnityWebRequest webRequest = new UnityWebRequest(DATABASE_URL + GETSTATUS_ENDPOINT + _appId.ToString(), "GET");
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Authorization", "Basic " + _authenticationToken);

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

                    Debug.Log($"ID: {_appId.ToString()}, Name: {projectInfo.projectTitle}, Allowed:{projectInfo.liberado}, PaymentID: , DeviceID: , UserID: , DatabaseID: ");

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

        if (_canSendLeads)
        {
            StartCoroutine(SyncLocalDataWithDatabase());
            _canSendLeads = false;
        }

        IsSyncButtonInteractable = true;
    }

    IEnumerator SyncLocalDataWithDatabase()
    {

        List<Dictionary<string, string>> localData = LoadLocalData(_tempLocalDataPath);
        if (localData != null)
        {
            int successCount = 0;
            AppManager.Instance.SyncMenu.TransferingData = true;

            foreach (var fields in localData)
            {

                yield return StartCoroutine(SendLeadsToDatabase(fields, (success) => {
                    if (success)
                    {
                        successCount++;
                    }
                    AppManager.Instance.SyncMenu.UpdateDataCount();
                }));
            }
            Debug.Log(successCount + " dados locais foram salvos no banco com sucesso.");

            AppManager.Instance.SyncMenu.TransferingData = false;
            AppManager.Instance.ShowLeadsMessage($"Transferência de dados realizada com sucesso!\n\nQuantidade de dados tranferidos: {successCount}");
        }
    }

    IEnumerator SendLeadsRoutine(Dictionary<string, string> data, UnityAction<bool> callback = null)
    {

        if (data != null)
        {
            AppManager.Instance.SyncMenu.TransferingData = true;

            yield return StartCoroutine(SendLeadsToDatabase(data, (success) =>
            {
                if (success)
                {
                    callback?.Invoke(true);
                }
                else
                {
                    callback?.Invoke(false);
                    InvokeUtility.Invoke(1f, () => SendLeads(data, callback));
                }
                AppManager.Instance.SyncMenu.UpdateDataCount();
            }));
        }
    }

    IEnumerator SendLeadsToDatabase(Dictionary<string, string> fields, System.Action<bool> callback)
    {
        bool success = false;
        LoadingScript.Instance.Loading = true;

        JObject leadObject = JObject.FromObject(fields);

        JObject mainObject = new JObject();
        mainObject["id_build"] = _appId;

        JArray dataArray = new JArray();
        dataArray.Add(leadObject);

        mainObject["data"] = dataArray;

        Debug.Log("Final JSON to send: " + mainObject.ToString());

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(mainObject.ToString());

        UnityWebRequest webRequest = new UnityWebRequest(DATABASE_URL + SENDLEADS_ENDPOINT, "POST");
        webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Authorization", "Basic " + _authenticationToken);

        yield return webRequest.SendWebRequest();

        if (webRequest.responseCode == 200)
        {
            try
            {
                var jsonResponse = JObject.Parse(webRequest.downloadHandler.text);
                Debug.Log("Server response: " + jsonResponse.ToString());

                if (jsonResponse["status"]?.ToString() == "success")
                {
                    Debug.Log("Request sent successfully and lead saved on the server.");
                    success = true;
                    RemoveLocalData(fields);
                }
                else
                {
                    Debug.LogError("The server returned an unexpected status or was not successful.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing response: " + e.Message);
            }
        }
        else
        {
            Debug.LogError($"Error: HTTP Code {webRequest.responseCode}. Retrying...");
            Debug.LogError("Detailed error: " + webRequest.error);
        }

        callback?.Invoke(success);
        LoadingScript.Instance.Loading = false;
        yield return null;
    }

    [System.Serializable]
    private class Serialization<T>
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