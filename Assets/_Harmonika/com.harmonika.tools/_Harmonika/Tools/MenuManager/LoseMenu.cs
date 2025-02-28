using Harmonika.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoseMenu : MonoBehaviour
{
    [SerializeField] private Button _backBtn;
    [SerializeField] private TMP_Text _loseText;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _backgroundFill;
    [SerializeField] private Image _userLogo;

    public string LoseText
    {
        get => _loseText.text;
        set
        {
            _loseText.text = value;
        }
    }

    public Button BackBtn { get => _backBtn; }

    public Image UserLogo { get => _userLogo; }

    public void ChangeVisualIdentity(Color primaryColor, Color secondaryColor, Color tertiaryColor, Color neutralColor)
    {
        _loseText.color = neutralColor;
        _backgroundImage.color = secondaryColor;
        _backgroundFill.color = primaryColor;
        _backBtn.colors = UIHelper.CreateProportionalColorBlock(tertiaryColor);
        _userLogo.sprite = Resources.Load<Sprite>("userLogo");
    }
}
