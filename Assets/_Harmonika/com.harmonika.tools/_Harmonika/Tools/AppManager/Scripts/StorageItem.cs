using TMPro;
using UnityEngine;

public class StorageItem : MonoBehaviour
{
    [Space(10)]
    [Header("References")]
    [SerializeField] private TMP_InputField _textQtd;
    [SerializeField] private TMP_Text _textName;

    private string _itemName;
    private int _value;
    private int _prizeScore;
    private bool _initialized = false;

    public int Quantity
    {
        get => _value;
        set
        {
            _value = value;
            UpdateVisualText();
            PlayerPrefs.SetInt(ItemName, _value);
        }
    }

    public string ItemName
    {
        get => _itemName;
    }
    public int PrizeScore
    { 
        get => _prizeScore;
    }

    private void Awake()
    {
        _textQtd.onValueChanged.AddListener(OnValueChanged);
    }

    public void Initialize(string name, int value = 0, int prizeScore = 0)
    {
        if (_initialized)
        {
            Debug.LogError("This item is already initialized");
            return;
        }
        _itemName = name;
        Quantity = value;
        _prizeScore = prizeScore;
        _initialized = true;

        UpdateVisualText();
    }

    private void UpdateVisualText()
    {
        _textQtd.text = Quantity.ToString();
        _textName.text = ItemName;
    }

    private void OnValueChanged(string newValue)
    {
        if (int.TryParse(newValue, out int parsedValue))
        {
            Quantity = parsedValue;
        }
    }
}
