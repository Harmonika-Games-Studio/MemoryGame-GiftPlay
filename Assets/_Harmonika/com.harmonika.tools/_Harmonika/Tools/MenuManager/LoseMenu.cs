using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoseMenu : MonoBehaviour
{
    [SerializeField] private Button _backToMainMenu;
    [SerializeField] private TMP_Text _loseText;

    public string LoseText
    {
        get => _loseText.text;
        set
        {
            _loseText.text = value;
        }
    }

    public void AddBackToMainMenuButtonListener(UnityAction action)
    {
        _backToMainMenu.onClick.AddListener(action);
    }

    public void ChangeVisualIdentity(Color a)
    {
        _loseText.color = a;
    }
}
