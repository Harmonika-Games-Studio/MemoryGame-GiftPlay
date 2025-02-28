using Harmonika.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VictoryMenu : MonoBehaviour
{
    [SerializeField] private Button _backBtn;
    [SerializeField] private TMP_Text _victoryText;
    [SerializeField] private TMP_Text _examplePrizeText;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _backgroundFill;
    [SerializeField] private Image _userLogo;

    public string VictoryText
    {
        get => _victoryText.text;
        set
        {
            _victoryText.text = value;
        }
    }

    public Button BackBtn { get => _backBtn; }

    public Image UserLogo { get => _userLogo; }

    public void ChangeVisualIdentity(Color primaryColor, Color secondaryColor, Color tertiaryColor, Color neutralColor)
    {
        _victoryText.color = neutralColor;
        _examplePrizeText.color = neutralColor;
        _backgroundImage.color = secondaryColor;
        _backgroundFill.color = primaryColor;
        _backBtn.colors = UIHelper.CreateProportionalColorBlock(tertiaryColor);
        _userLogo.sprite = Resources.Load<Sprite>("userLogo");
    }

    public void ChangePrizeText(string prizeName)
    {
        _examplePrizeText.text = prizeName;
    }
}
