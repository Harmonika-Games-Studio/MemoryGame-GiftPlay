using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DatabaseTesterConfigMenu : ConfigMenu
{
    [Header("Database Tester Config")]
    [SerializeField] private TMP_InputField _inputAppID;
    [SerializeField] private Toggle _toggleTestVersion;

    override protected void Awake()
    {
        base.Awake();
        _inputAppID.onSubmit.AddListener(OnAppIDValueChanged);
        _toggleTestVersion.onValueChanged.AddListener(OnToggleTestVersionValueChanged);
    }

    override protected void Start()
    {
        base.Start();
        UpdateInputValues();
    }

    void OnAppIDValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value) || int.Parse(value) <= 0)
        {
            value = "0";
        }
        AppManager.Instance.DataSync.AppId = int.Parse(value);

        _inputAppID.text = value;
    }

    void OnToggleTestVersionValueChanged(bool value)
    {
        AppManager.Instance.DataSync.TestVersion = value;
    }

    void UpdateInputValues()
    {
        int appID = PlayerPrefs.GetInt("AppID", AppManager.Instance.DataSync.AppId);
        bool testVersion = PlayerPrefs.GetInt("TestVersion", AppManager.Instance.DataSync.TestVersion ? 1 : 0) >= 1;

        _toggleTestVersion.isOn = testVersion;
        _inputAppID.text = appID.ToString();
    }
}
