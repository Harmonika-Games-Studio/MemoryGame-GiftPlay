using Harmonika.Tools;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private Image _cardFront;
    [SerializeField] private Image _cardBack;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _backgroundFill;
    [SerializeField] private Button _startGameBtn;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Image _userLogo;

    public string TitleText
    {
        get => _titleText.text;
        set
        {
            _titleText.text = value;
        }
    }

    public Button StartGameBtn { get => _startGameBtn; }

    public Image UserLogo { get => _userLogo; }

    public void ChangeVisualIdentity(Color primaryColor, Color secondaryColor, Color tertiaryColor, Color neutralColor)
    {
        _titleText.color = neutralColor;
        _backgroundImage.color = primaryColor;
        _backgroundFill.color = secondaryColor;
        _startGameBtn.colors = UIHelper.CreateProportionalColorBlock(tertiaryColor);
        _userLogo.sprite = Resources.Load<Sprite>("userLogo");
    }

    public void UpdateStartMenuCards(string cardFrontName, string cardBackName)
    {
        _cardFront.sprite = Resources.Load<Sprite>(cardFrontName);
        _cardBack.sprite = Resources.Load<Sprite>(cardBackName);
    }
    
    public void UpdateStartMenuCards(Sprite cardFrontName, Sprite cardBackName)
    {
        _cardFront.sprite = cardFrontName;
        _cardBack.sprite = cardBackName;
    }
}