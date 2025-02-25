using Harmonika.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VersionMenu : MonoBehaviour
{
    [SerializeField] private Button _backButton;
    [SerializeField] private TMP_Text _versionTxt;

    private void Awake()
    {
        _backButton.onClick.AddListener(() => AppManager.Instance.OpenMenu("MainMenu"));
        _versionTxt.text = AppManager.Instance.Version;
    }
}
