using Harmonika.Menu;
using Harmonika.Tools;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AppBlocker : MonoBehaviour
{
    private static AppBlocker _instance;

    [SerializeField] private MenuPanel[] _menus;

    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _supportButton;
    [SerializeField] private Button _quitButton;

    [SerializeField] private GameObject pnlBlockOffline;
    [SerializeField] private GameObject pnlBlockLicense;

    [SerializeField] private BlockMenu _blockMenu;

    private MenuManager _menuManager;
    private BlockType _type;

    #region Properties
    /// <summary>
    /// Returns AppBlocker's single static instance.
    /// </summary>
    public static AppBlocker Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AppBlocker>();

                if (_instance == null)
                {
                    Debug.LogError("Houve uma tentativa de acesso a uma instância vazia do AppBlocker, mas o objeto não está na cena." +
                        "\nPor favor adicione o AppBlocker na cena antes de continuar!");
                }
            }
            return _instance;
        }
    }

    public bool IsBlocked 
    {
        get => 1 == PlayerPrefs.GetInt("isBlocked", 0);
        set 
        {
            PlayerPrefs.SetInt("isBlocked", value ? 1 : 0);
            Debug.Log("IsBlocked: " + value);

            if (!value)
                _menuManager.CloseMenus();
            else
                _menuManager.OpenMenu("BlockMenu");
        }
    }

    public BlockType BlockType
    {
        get => _type;
        set 
        {
            Debug.Log("BlockType: " + value);
            _type = value;
            switch (value)
            {
                case BlockType.NoConnectionAndDueDate:
                    pnlBlockOffline.SetActive(true);
                    pnlBlockLicense.SetActive(false);
                    _connectButton.gameObject.SetActive(true);
                    break;
                case BlockType.LicenseNotActive:
                    pnlBlockOffline.SetActive(false);
                    pnlBlockLicense.SetActive(true);
                    _connectButton.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }
    }
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

        AssignMenuManager();
        AssignButtons();
    }

    private void Start()
    {
        StartCoroutine(ConnectAndLoad());
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
            _menuManager.Initialize(null, _menus);
        }
    }

    private void AssignButtons()
    {
        _connectButton.onClick.AddListener(() => StartCoroutine(ConnectAndLoad()));
        _supportButton.onClick.AddListener(() => _menuManager.OpenMenu("SupportMenu"));
        _quitButton.onClick.AddListener(() => _menuManager.OpenMenu("QuitMenu"));
    }

    private IEnumerator ConnectAndLoad()
    {
        LoadingScript.Instance.Loading = true;

        yield return StartCoroutine(AppManager.Instance.DataSync.GetStatusFromDatabase());

        yield return new WaitForSeconds(.3f);
        LoadingScript.Instance.Loading = false;

        Debug.Log("Processo completo!");
    }

    public void OpenBlockPanel(BlockType blockType)
    {
        AppBlocker.Instance.IsBlocked = true;
        BlockType = blockType;
        IsBlocked = true;
    }
}