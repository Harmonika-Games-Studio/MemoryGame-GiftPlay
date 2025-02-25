using Harmonika.Tools;
using Harmonika.Menu;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AppManager : MonoBehaviour
{
    private static AppManager _instance;

    [Header("AppManager Configuration")]
    public bool useLeads = false;
    public GameConfigScriptable gameConfig;
    [SerializeField] private MenuPanel[] _menus;

    [Space(10)]
    [Header("Button References")]
    [SerializeField] private CanvasGroup _menuBackground;
    [SerializeField] private Button _hiddenButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _syncButton;
    [SerializeField] private Button _storageButton;
    [SerializeField] private Button _rankingButton;
    [SerializeField] private Button _leadsButton;
    [SerializeField] private Button _configButton;
    [SerializeField] private Button _versionButton;
    [SerializeField] private Button _tutorialButton;
    [SerializeField] private Button _supportButton;
    [SerializeField] private Button _quitButton;

    [Space(10)]
    [Header("Menu Script References")]
    [SerializeField] private SyncMenu _syncMenu;
    [SerializeField] private StorageMenu _storageMenu;
    [SerializeField] private RankingMenu _rankingMenu;
    [SerializeField] private LeadsMenu _leadsMenu;
    [SerializeField] private ConfigMenu _configMenu;
    [SerializeField] private VersionMenu _versionMenu;
    [SerializeField] private TutorialMenu _tutorialMenu;
    [SerializeField] private SupportMenu _supportMenu;
    [SerializeField] private QuitMenu _quitMenu;

    [Space(10)]
    [Header("Script References")]
    [SerializeField] Storage _storage;
    [SerializeField] DataSync _dataSync;

    private const string VERSION = "v0.2.9";

    private int _hidenButtonClickCount = 0;
    private Coroutine _dashboardTimer;
    private MenuManager _menuManager;

    #region Properties
    /// <summary>
    /// Returns AppManager's single static instance.
    /// </summary>
    public static AppManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AppManager>();

                if (_instance == null)
                {
                    Debug.LogError("Houve uma tentativa de acesso a uma instância vazia do AppManager, mas o objeto não está na cena." +
                        "\nPor favor adicione o AppManager na cena antes de continuar!");
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Returns AppManager's version.
    /// </summary>
    public string Version
    {
        get { return VERSION; }
    }

    /// <summary>
    /// Returns StorageMenu's reference.
    /// </summary>
    public Storage Storage
    {
        get => _storage;
    }

    /// <summary>
    /// Returns DataSync's reference.
    /// </summary>
    public DataSync DataSync
    {
        get => _dataSync;
    }

    /// <summary>
    /// Returns SyncMenu reference.
    /// </summary>
    public SyncMenu SyncMenu
    {
        get => _syncMenu;
    }

    /// <summary>
    /// Returns True if application is blocked and save on playerprefs.
    /// </summary>
#endregion

    private void Awake()
    {
        #region Singleton
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion

        AssignMenus();
        AssignMenuManager();
        AssignButtons();
    }

    private void Start()
    {
        ApplyScriptableConfig();
    }

    private void OnDestroy()
    {
        // This is a good practice, though it's not strictly necessary in this case since this object is marked as 'Don't Destroy On Load'.
        _hiddenButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Open menu with name.
    /// </summary>
    public void OpenMenu(string name)
    {
        if (AppBlocker.Instance.IsBlocked)
            AppBlocker.Instance.IsBlocked = true;

        if(string.IsNullOrEmpty(name) || name == "CloseAll")
        {
            _menuManager.CloseMenus();
        }
        else 
        {
            _menuManager.OpenMenu(name);
        }
    } 

    public void ActivateHiddenButton(bool value) {
        _hiddenButton.gameObject.SetActive(value);
    }

    public void BlockApplication(BlockType type)
    {
        AppBlocker.Instance.IsBlocked = true;
        AppBlocker.Instance.BlockType = type;
    }

    public void ApplyScriptableConfig()
    {
        if (gameConfig != null)
        {
            useLeads = gameConfig.useLeads;
            Storage.itemsConfig = gameConfig.storageItems;
        }

        Storage.Setup();
    }

    #region This methods need to turn into toast or dialogs on aplication.
    public void ShowLeadsMessage(string text)
    {
        SyncMenu.ShowMessage(text);
        InvokeUtils.Invoke(DisableLeadsMessage, 6f);
    }

    private void DisableLeadsMessage()
    {
        SyncMenu.DisableMessage();
    }
    #endregion

    private void AssignMenus()
    {
        _syncMenu = GetComponentInChildren<SyncMenu>();
        _storageMenu = GetComponentInChildren<StorageMenu>();
        _rankingMenu = GetComponentInChildren<RankingMenu>();
        _leadsMenu = GetComponentInChildren<LeadsMenu>();
        _configMenu = GetComponentInChildren<ConfigMenu>();
        _versionMenu = GetComponentInChildren<VersionMenu>();
        _tutorialMenu = GetComponentInChildren<TutorialMenu>();
        _supportMenu = GetComponentInChildren<SupportMenu>();
        _quitMenu = GetComponentInChildren<QuitMenu>();
    }

    private void AssignMenuManager()
    {
        if (TryGetComponent(out MenuManager menuManager))
        {
            _menuManager = menuManager;
        }
        else
        {
            _menuManager = gameObject.AddComponent<MenuManager>();
            _menuManager.Initialize(_menuBackground, _menus);
        }
    }

    private void AssignButtons()
    {
        _hiddenButton.onClick.AddListener(() => AppManagerHiddenButton());
        _syncButton.onClick.AddListener(() => OpenMenu("SyncMenu"));
        _syncButton.onClick.AddListener(() => SyncMenu.UpdateDataCount());
        _storageButton.onClick.AddListener(() => OpenMenu("StorageMenu"));
        _rankingButton.onClick.AddListener(() => OpenMenu("RankingMenu"));
        _rankingButton.onClick.AddListener(() => _rankingMenu.LoadRanking());
        _leadsButton.onClick.AddListener(() => OpenMenu("LeadsMenu"));
        _leadsButton.onClick.AddListener(() => _leadsMenu.LoadLeads());
        _configButton.onClick.AddListener(() => OpenMenu("ConfigMenu"));
        _versionButton.onClick.AddListener(() => OpenMenu("VersionMenu"));
        _tutorialButton.onClick.AddListener(() => OpenMenu("TutorialMenu"));
        _supportButton.onClick.AddListener(() => OpenMenu("SupportMenu"));
        _quitButton.onClick.AddListener(() => OpenMenu("QuitMenu"));

        _closeButton.onClick.AddListener(() =>
        { 
            if (AppBlocker.Instance.IsBlocked)
            {
                AppBlocker.Instance.IsBlocked = true;
            }
            _menuManager.CloseMenus();
        });
    }

    public void AppManagerHiddenButton()
    {
        if (_hidenButtonClickCount == 3)
        {
            OpenMenu("MainMenu");
            _hidenButtonClickCount = 0;
        }
        else
        {
            if (_dashboardTimer != null) StopCoroutine(_dashboardTimer);
                StartCoroutine(DashboardTimer(3));
            _hidenButtonClickCount++;
        }
    }

    IEnumerator DashboardTimer(float time)
    {
        yield return new WaitForSeconds(time);
        _hidenButtonClickCount = 0;
    }
}
