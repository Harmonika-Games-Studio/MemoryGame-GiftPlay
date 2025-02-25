using UnityEngine;
using Harmonika.MenuManager;

public class AutoLeadForm : LeadCaptation
{
    [Header("Automatic Data Fields")]
    public LeadDataConfig[] leadDataConfig;

    [Header("References")]
    [SerializeField] private Transform viewportContent;
    [SerializeField] private GameObject leadboxPrefab;

    protected override void Start()
    {
        InstantiateLeadboxes();
        base.Start();
    }

    private void InstantiateLeadboxes()
    {
        foreach (var config in leadDataConfig)
        {
            LeadFieldBox leadFieldBox = Instantiate(leadboxPrefab, viewportContent).GetComponent<LeadFieldBox>();
            leadFieldBox.ApplyConfig(config);
            _formInputs.Add(new FormInput(leadFieldBox.gameObject.name, leadFieldBox.InputType, leadFieldBox.GetInputObject(), leadFieldBox.IsOptional));
            leadFieldBox.onValueChanged += (string a) =>
            {
                bool aux = CheckInputsFilled();
                Debug.Log("CheckInputsFilled: " + aux);
                submitButton.gameObject.SetActive(aux);
            };
        }
    }
}