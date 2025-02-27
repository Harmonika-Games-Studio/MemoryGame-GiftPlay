using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Harmonika.Tools;

public class LeadsMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _leadListPanel;
    [SerializeField] private TMP_Text _noLeadsText;    
    [SerializeField] private GameObject _leadBoxPrefab;
    [SerializeField] private Transform _contentPanel;  
    [SerializeField] private Button _backButton;

    [Space(10)]
    [Header("TitleReferences")]
    [SerializeField] private TMP_Text title1;
    [SerializeField] private TMP_Text title2;
    [SerializeField] private TMP_Text title3;


    private void Awake()
    {
        _backButton.onClick.AddListener(() => AppManager.Instance.OpenMenu("MainMenu"));
    }

    public void LoadLeads() {
        foreach (Transform child in _contentPanel)
        {
            Destroy(child.gameObject);
        }

        List<Dictionary<string, string>> leads = AppManager.Instance.DataSync.PermanentData;
        if (leads == null || leads.Count == 0)
        {
            _leadListPanel.SetActive(false);
            _noLeadsText.gameObject.SetActive(true);
        }
        else 
        {
            _leadListPanel.SetActive(true);
            _noLeadsText.gameObject.SetActive(false);

            int leadCount = Mathf.Min(leads.Count, 100);

            for (int i = 0; i < leadCount; i++) {
                Dictionary<string, string> lead = leads[i];

                UpdateTitles(lead);

                GameObject newLeadBox = Instantiate(_leadBoxPrefab, _contentPanel);

                TMP_Text text1 = newLeadBox.transform.Find("text1").GetComponent<TMP_Text>();
                TMP_Text text2 = newLeadBox.transform.Find("text2").GetComponent<TMP_Text>();
                TMP_Text text3 = newLeadBox.transform.Find("text3").GetComponent<TMP_Text>();

                int fieldIndex = 0;
                foreach (KeyValuePair<string, string> field in lead) {
                    string obscuredText = ObscureText(field.Value);

                    if (fieldIndex == 0) text1.text = obscuredText;
                    else if (fieldIndex == 1) text2.text = obscuredText;
                    else if (fieldIndex == 2) text3.text = obscuredText;

                    fieldIndex++;

                    if (fieldIndex >= 3) break;
                }

                if (fieldIndex < 3) {
                    if (fieldIndex == 1) {
                        text2.gameObject.SetActive(false);
                        text3.gameObject.SetActive(false);
                    } else if (fieldIndex == 2) {
                        text3.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    void UpdateTitles(Dictionary<string, string> lead) {
        title1.text = "";
        title2.text = "";
        title3.text = "";

        int titleIndex = 0;
        foreach (KeyValuePair<string, string> field in lead) {
            if (titleIndex == 0) title1.text = field.Key;
            else if (titleIndex == 1) title2.text = field.Key;
            else if (titleIndex == 2) title3.text = field.Key;

            titleIndex++;

            if (titleIndex >= 3) break;
        }
    }

    string ObscureText(string originalText) {
        if (string.IsNullOrEmpty(originalText)) return "";

        string limitedText = originalText.Length > 10 ? originalText.Substring(0, 10) : originalText;

        if (limitedText.Length > 2)
        {
            string visiblePart = limitedText.Substring(0, 2);
            string obscuredPart = new string('*', limitedText.Length - 2);
            return visiblePart + obscuredPart;
        } 

        else
        {
            return limitedText;
        }
    }
}