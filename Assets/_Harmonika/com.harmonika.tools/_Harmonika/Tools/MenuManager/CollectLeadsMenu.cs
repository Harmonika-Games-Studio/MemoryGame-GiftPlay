using Harmonika.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CollectLeadsMenu : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _backgroundFill;
    [SerializeField] private Button _continueGameBtn;
    [SerializeField] private Button _backBtn;
    [SerializeField] private TMP_Text _collectLeadsTitle;
    [SerializeField] private LeadCaptation _leadCaptation;
    [SerializeField] private Image _userLogo;

    public string LeadsText
    {
        get => _collectLeadsTitle.text;
        set
        {
            _collectLeadsTitle.text = value;
        }
    }

    public Button ContinueGameBtn { get => _continueGameBtn; }

    public Image UserLogo { get => _userLogo; }

    public Button BackBtn { get => _backBtn; }

    public LeadCaptation Leads { get => _leadCaptation; }

    public void ChangeVisualIdentity(Color primaryColor, Color secondaryColor, Color tertiaryColor, Color neutralColor)
    {
        _collectLeadsTitle.color = neutralColor;
        _backgroundImage.color = primaryColor;
        _backgroundFill.color = secondaryColor;
        _continueGameBtn.colors = UIHelper.CreateProportionalColorBlock(tertiaryColor); ;
        _userLogo.sprite = Resources.Load<Sprite>("userLogo");
    }

    public void ClearAllFields()
    {
        _leadCaptation.ClearAllFields();
    }
}
