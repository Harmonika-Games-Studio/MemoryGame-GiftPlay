using Harmonika.Tools;
using UnityEngine;
using UnityEngine.UI;

public class SupportMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _backButton;
    private void Awake()
    {
        _backButton.onClick.AddListener(() => AppManager.Instance.OpenMenu("MainMenu"));
    }
}