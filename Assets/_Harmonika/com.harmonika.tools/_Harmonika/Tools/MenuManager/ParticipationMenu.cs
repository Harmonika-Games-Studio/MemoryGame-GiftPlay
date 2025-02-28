using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ParticipationMenu : MonoBehaviour
{
    [SerializeField] private Button _backBtn;
    [SerializeField] private TMP_Text _participationText;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _backgroundFill;
    [SerializeField] private Image _userLogo;

    public string VictoryText
    {
        get => _participationText.text;
        set
        {
            _participationText.text = value;
        }
    }

    public Button BackBtn { get => _backBtn; }

    public void ChangeVisualIdentity(Color primaryColor, Color secondaryColor, Color tertiaryColor, Color neutralColor)
    {
        _participationText.color = neutralColor;
        _backgroundImage.color = secondaryColor;
        _backgroundFill.color = primaryColor;

        ColorBlock cb = _backBtn.colors;
        cb.normalColor = tertiaryColor;
        _backBtn.colors = cb;
        _userLogo.sprite = Resources.Load<Sprite>("userLogo");
    }
}
