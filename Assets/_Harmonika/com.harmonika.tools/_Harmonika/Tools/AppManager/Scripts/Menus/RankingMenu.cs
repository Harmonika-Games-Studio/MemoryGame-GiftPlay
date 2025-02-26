using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // Para usar fun��es de ordena��o
using TMPro;

public class RankingMenu : MonoBehaviour {
    [Header("References")]
    [SerializeField] private GameObject _rankingPanel;  // Painel para exibir o ranking
    [SerializeField] private GameObject _playerBoxPrefab; // Prefab do player box
    [SerializeField] private TMP_Text _noRankingText;   // Texto de aviso para quando n�o houver jogadores
    [SerializeField] private TMP_Text _rankingQtt;   // Texto de aviso para quando n�o houver jogadores
    [SerializeField] private Transform _contentPanel;   // Content do ScrollView onde o ranking ser� exibido
    [SerializeField] private Button _backButton;

    [Header("Settings")]
    [SerializeField] private int _maxPlayersToShow = 10; // Quantidade m�xima de jogadores a exibir

    private void Awake() {
        if (_backButton != null) {
            _backButton.onClick.AddListener(() => AppManager.Instance.OpenMenu("MainMenu"));
        }

        _rankingQtt.text = $"(Exibi��o limitada em at� {_maxPlayersToShow} jogadores)";
    }

    public void LoadRanking()
    {
        if (!AppManager.Instance.Config.useLeads) 
        {
            _noRankingText.gameObject.SetActive(true);
            return;
            //Rodar um m�todo alternativo posteriormente.
        }

        // Limpar objetos anteriores do contentPanel
        foreach (Transform child in _contentPanel)
        {
            Destroy(child.gameObject); // Destr�i todos os objetos filhos
        }

        List<Dictionary<string, string>> players = AppManager.Instance.DataSync.PermanentData;

        if (players == null || players.Count == 0)
        {
            _rankingPanel.SetActive(false);
            if (_noRankingText)
                _noRankingText.gameObject.SetActive(true);
            
        }
        else
        {
            _rankingPanel.SetActive(true);
            if (_noRankingText)
                _noRankingText.gameObject.SetActive(false);

            // Filtrar e ordenar por pontua��o (ordem decrescente)
            var sortedPlayers = players
                .Where(p => p.ContainsKey("pontos")) // Verificar se o campo 'pontos' existe
                .OrderByDescending(p => int.Parse(p["pontos"])) // Ordenar pela pontua��o
                .Take(_maxPlayersToShow) // Limitar pela quantidade definida
                .ToList();

            // Exibir cada jogador no ranking
            foreach (var player in sortedPlayers)
            {
                // Instanciar um novo player_box no contentPanel
                GameObject newPlayerBox = Instantiate(_playerBoxPrefab, _contentPanel);

                // Obter as refer�ncias dos campos de texto no prefab
                TMP_Text nameText = newPlayerBox.transform.Find("text1").GetComponent<TMP_Text>();
                TMP_Text pointsText = newPlayerBox.transform.Find("text2").GetComponent<TMP_Text>();

                // Exibir o nome e a pontua��o do jogador
                nameText.text = player.ContainsKey("nome") ? player["nome"] : "Sem nome";
                pointsText.text = player["pontos"];
            }
        }
    }
}