using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ResultMenu : MonoBehaviour
{
    [SerializeField] private Button _backToMainMenu;
    [SerializeField] private TMP_Text _resultTitle;
    [SerializeField] private TMP_Text _resultText;

    public string ResultTitle
    {
        get => _resultTitle.text;
        set
        {
            _resultTitle.text = value;
        }
    }
    public string ResultText
    {
        get => _resultText.text;
        set
        {
            _resultText.text = value;
        }
    }

    public void AddNextMenuButtonListener(UnityAction action)
    {
        _backToMainMenu.onClick.AddListener(action);
    }
}
