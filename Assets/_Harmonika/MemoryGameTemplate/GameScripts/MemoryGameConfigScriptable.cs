using UnityEngine;
using Harmonika.Tools;

[CreateAssetMenu(fileName = "MemoryGame Config", menuName = "Harmonika/ScriptableObjects/MemoryGame Config", order = 1)]
public class MemoryGameConfigScriptable : GameConfigScriptable
{
    [Space(5)]
    [Header("Configurable Variables")]
    public string gameName = "Jogo da <b>Mem�ria</b>";
    [Space(5)]
    [Header("Configurable Variables")]
    public int memorizationTime = 2;
    public int gameTime = 20;

    [Space(5)]
    [Header("Cards")]
    public GameObject _cardPrefab;
    public Sprite cardBack;
    public Sprite[] cardPairs;
}