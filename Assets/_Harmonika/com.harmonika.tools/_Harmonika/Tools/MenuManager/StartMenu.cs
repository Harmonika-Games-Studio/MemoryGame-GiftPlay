using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
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

    public Button StartGameBtn
    {
        get => _startGameBtn;
    }

    public void ChangeVisualIdentity(Color primaryColor, Color secondaryColor, Color tertiaryColor, Color neutralColor)
    {
        _titleText.color = neutralColor;
        _backgroundImage.color = primaryColor;
        _backgroundFill.color = secondaryColor;
        ColorBlock cb = _startGameBtn.colors;
        cb.normalColor = tertiaryColor;
        _startGameBtn.colors = cb;
        _userLogo.sprite = Resources.Load<Sprite>("userLogo");
    }
}
