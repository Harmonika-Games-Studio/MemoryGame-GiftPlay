using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Harmonika.Tools;

public class LeadsMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _leadListPanel;  // O painel que contém a lista de leads
    [SerializeField] private TMP_Text _noLeadsText;      // Texto de aviso para quando não houver leads
    [SerializeField] private GameObject _leadBoxPrefab;  // Prefab do lead_box
    [SerializeField] private Transform _contentPanel;    // O content do ScrollView onde os leads serão exibidos
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
            // Se não há leads, desativar o painel leadListPanel e mostrar o texto de aviso
            _leadListPanel.SetActive(false);
            _noLeadsText.gameObject.SetActive(true);
        }
        else 
        {
            // Se há leads, desativar o texto de aviso e ativar o painel
            _leadListPanel.SetActive(true);
            _noLeadsText.gameObject.SetActive(false);

            // Limitar o número de leads a 100
            int leadCount = Mathf.Min(leads.Count, 100);

            for (int i = 0; i < leadCount; i++) {
                Dictionary<string, string> lead = leads[i];

                // Atualizar os títulos dos campos com base nos nomes dos campos do dicionário
                UpdateTitles(lead);

                // Instanciar um novo lead_box no contentPanel
                GameObject newLeadBox = Instantiate(_leadBoxPrefab, _contentPanel);

                // Obter as referências dos campos de texto no prefab
                TMP_Text text1 = newLeadBox.transform.Find("text1").GetComponent<TMP_Text>();
                TMP_Text text2 = newLeadBox.transform.Find("text2").GetComponent<TMP_Text>();
                TMP_Text text3 = newLeadBox.transform.Find("text3").GetComponent<TMP_Text>();

                // Mostrar os 3 primeiros campos, se existirem
                int fieldIndex = 0;
                foreach (KeyValuePair<string, string> field in lead) {
                    string obscuredText = ObscureText(field.Value);

                    if (fieldIndex == 0) text1.text = obscuredText;
                    else if (fieldIndex == 1) text2.text = obscuredText;
                    else if (fieldIndex == 2) text3.text = obscuredText;

                    fieldIndex++;

                    if (fieldIndex >= 3) break; // Parar após os 3 primeiros campos
                }

                // Caso existam menos de 3 campos, esconde os campos não utilizados
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

    // Função que atualiza os títulos com base nos nomes dos campos do lead
    void UpdateTitles(Dictionary<string, string> lead) {
        // Resetar os títulos para vazio
        title1.text = "";
        title2.text = "";
        title3.text = "";

        // Mostrar os nomes dos 3 primeiros campos, se existirem
        int titleIndex = 0;
        foreach (KeyValuePair<string, string> field in lead) {
            if (titleIndex == 0) title1.text = field.Key;
            else if (titleIndex == 1) title2.text = field.Key;
            else if (titleIndex == 2) title3.text = field.Key;

            titleIndex++;

            if (titleIndex >= 3) break; // Parar após os 3 primeiros campos
        }
    }

    // Função que limita a exibição do texto a 10 caracteres e oculta com "*" a partir do terceiro
    string ObscureText(string originalText) {
        if (string.IsNullOrEmpty(originalText)) return "";

        // Limitar o texto a 10 caracteres
        string limitedText = originalText.Length > 10 ? originalText.Substring(0, 10) : originalText;

        // Se o texto tiver mais de 2 caracteres, obscurece do 3º em diante
        if (limitedText.Length > 2)
        {
            string visiblePart = limitedText.Substring(0, 2);
            string obscuredPart = new string('*', limitedText.Length - 2);
            return visiblePart + obscuredPart;
        } 

        else
        {
            return limitedText; // Se o texto for muito curto, apenas retorna ele
        }
    }
}