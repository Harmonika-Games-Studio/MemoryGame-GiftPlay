using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VictoryMenu : MonoBehaviour
{
    [SerializeField] private Button _backToMainMenu;
    [SerializeField] private TMP_Text _victoryText;
    [SerializeField] private TMP_Text _examplePrizeText;

    public string VictoryText
    {
        get => _victoryText.text;
        set
        {
            _victoryText.text = value;
        }
    }

    public void AddBackToMainMenuButtonListener(UnityAction action)
    {
        _backToMainMenu.onClick.AddListener(action);
    }

    public void ChangeVisualIdentity(Color a)
    {
        _victoryText.color = a;
    }

    public void ChangePrizeText(string prizeName)
    {
        _examplePrizeText.text = prizeName;
    }
}
