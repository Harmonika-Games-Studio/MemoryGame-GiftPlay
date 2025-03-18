using UnityEngine;
using UnityEngine.UI;
using Harmonika.Tools;
using TMPro;

public class SyncMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _backButton;
    [SerializeField] private TMP_Text _messageTxt;
    [SerializeField] private TMP_Text _leadsToSyncTxt;
    [SerializeField] private TMP_Text _collectedLeadsTxt;
    [SerializeField] private GameObject _transferDataPanel;

    private bool _transferingData;

    public bool TransferingData
    {
        set
        {
            _transferingData = value;
            _transferDataPanel.SetActive(_transferingData);
        }
        get
        {
            return _transferingData;
        }
    }
    private void Awake()
    {
        _backButton.onClick.AddListener(() => AppManager.Instance.OpenMenu("MainMenu"));
    }

    public void ShowMessage(string text)
    {
        _messageTxt.text = text; // TODO: Program a Toast Feature
        InvokeUtility.Invoke(6f, DisableMessage);
    }

    public void DisableMessage()
    {
        _messageTxt.text = ""; // TODO: Program a Toast Feature
    }

    public void UpdateDataCount()
    {
        UpdateTempDataCount(AppManager.Instance.DataSync.TempDataCount);
        UpdatePermanentDataCount(AppManager.Instance.DataSync.PermanentDataCount);
    }

    private void UpdateTempDataCount(int count)
    {
        if (count == 0)
            AppManager.Instance.DataSync.IsSyncButtonInteractable = false;
        else
            AppManager.Instance.DataSync.IsSyncButtonInteractable = true;
        _leadsToSyncTxt.text = $"Qtd. de leads para sincronizar: <color=#000000><b>{count.ToString()} </b></color>";
    }

    private void UpdatePermanentDataCount(int count)
    {
        _collectedLeadsTxt.text = $"Qtd. total de leads coletados: <color=#000000><b>{count.ToString()}</b></color>";
    }
}
