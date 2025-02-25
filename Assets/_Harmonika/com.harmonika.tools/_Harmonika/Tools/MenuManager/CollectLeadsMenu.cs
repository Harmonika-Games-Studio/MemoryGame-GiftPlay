using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CollectLeadsMenu : MonoBehaviour
{
    [SerializeField] private Button _continueGame;
    [SerializeField] private Button _back;
    [SerializeField] private TMP_Text _collectLeadsTitle;
    [SerializeField] private LeadCaptation _leadCaptation;

    public string LeadsText
    {
        get => _collectLeadsTitle.text;
        set
        {
            _collectLeadsTitle.text = value;
        }
    }

    public LeadCaptation Leads { get => _leadCaptation; }

    public void AddContinueGameButtonListener(UnityAction action)
    {
        _continueGame.onClick.AddListener(action);
    }
    
    public void AddBackButtonListener(UnityAction action)
    {
        _back.onClick.AddListener(action);
    }

    public void ChangeVisualIdentity(Color primary)
    {
        _collectLeadsTitle.color = primary;
    }

    public void ClearAllFields()
    {
        _leadCaptation.ClearAllFields();
    }
}
