using UnityEngine;
using Harmonika.Tools;
using Harmonika.Tools.Keyboard;
using NaughtyAttributes;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.Windows;

namespace Harmonika.MenuManager
{
    public class LeadFieldBox : MonoBehaviour
    {
        [OnValueChanged(nameof(OnTypeValueChanged))]
        [SerializeField] private LeadInputType _type;
        [SerializeField] private TMP_Text _fieldName;

        [SerializeField] private TMP_InputField _input_field;
        [SerializeField] private TMP_Dropdown _dropdown;
        [SerializeField] private ToggleWithText _toggle;
        [SerializeField] private bool _isOptional;
        //[SerializeField] private CustomToggleGroup _toggleGroup;
        //[SerializeField] private SearchableDropdown _searchDropdown;

        public UnityAction<string> onValueChanged;

        #region Properties
        public string FieldName { get => _fieldName.text; }
        public LeadInputType InputType { get => _type; }
        public bool IsOptional { get => _isOptional; }
        #endregion

        private void Start()
        {
            onValueChanged += (string a) => Debug.Log("Value changed to: " + a);
        }

        private string LimitCharacters(string value, int maxLength)
        {
            if (value.Length > maxLength)
            {
                value = value.Substring(0, maxLength);
            }
            return value;
        }

        public LeadInputType Type
        {
            get => _type;
            
            set 
            {
                _type = value;
                OnTypeValueChanged();
            }
        }

        public GameObject GetInputObject()
        {
            switch (Type)
            {
                case LeadInputType.InputField:
                    return _input_field.gameObject;
                case LeadInputType.Dropdown:
                    return _dropdown.gameObject;
                case LeadInputType.Toggle:
                    return _toggle.gameObject;
                //case LeadInputType.ToggleGroup:
                //    return _toggleGroup.gameObject;
                //case LeadInputType.SearchableDropdown:
                    //return _searchDropdown.inputField.gameObject; // TODO: Isso pode estar errado, tem que testar
                default:
                    return null;
            }
        }

        public void ApplyConfig(LeadDataConfig config)
        {
            gameObject.name = config.fieldName;
            Type = config.inputType;
            _isOptional = config.isOptional;

            if (config.showName)
            {
                string result = Regex.Replace(config.fieldName, "(\\B[A-Z])", " $1");
                result = char.ToUpper(result[0]) + result.Substring(1);

                _fieldName.text = result;
            }
            else _fieldName.text = string.Empty;

            KeyboardView keyboardView;

            switch (Type)
            {
                case LeadInputType.InputField:
                    keyboardView = _input_field.gameObject.AddComponent<KeyboardView>();
                    keyboardView.selectedType = config.inputDataConfig.keyboardType;
                    _input_field.placeholder.GetComponent<TextMeshProUGUI>().text = config.inputDataConfig.fieldPlaceholder;
                    _input_field.onSubmit.AddListener((string a) => _input_field.text = a.Format(config.inputDataConfig.parseableFields));
                    _input_field.onValueChanged.AddListener((string a) => onValueChanged.Invoke(a));
                    break;

                case LeadInputType.Dropdown:
                    _dropdown.placeholder.GetComponent<TextMeshProUGUI>().text = config.dropdownDataConfig.fieldPlaceholder;
                    _dropdown.onValueChanged.AddListener((int a) => onValueChanged.Invoke(a.ToString()));

                    _dropdown.options.Clear();
                    foreach (var option in config.dropdownDataConfig.options)
                    {
                        _dropdown.options.Add(new TMP_Dropdown.OptionData(option));
                    }
                    break;

                case LeadInputType.Toggle:
                    _toggle.tmpText.text = config.toggleDataConfig.text;
                    _toggle.onValueChanged.AddListener((bool a) => onValueChanged.Invoke(a.ToString()));
                    break;
                
                    //case LeadInputType.ToggleGroup:
                //    break;

                //case LeadInputType.SearchableDropdown: // TODO: Arrumar esse componente não adicionando corretamente ao form input
                //    keyboardView = _searchDropdown.gameObject.GetComponentInChildren<TMP_InputField>().gameObject.AddComponent<KeyboardView>();
                //    keyboardView.selectedType = config.searchableDropdownDataConfig.keyboardType;
                //    _searchDropdown.inputField.placeholder.GetComponent<TextMeshProUGUI>().text = config.searchableDropdownDataConfig.fieldPlaceholder;
                //    _searchDropdown.inputField.onValueChanged.AddListener((string a) => onValueChanged.Invoke());

                //    _searchDropdown.options.Clear();
                //    foreach (var option in config.searchableDropdownDataConfig.options)
                //    {
                //        _searchDropdown.options.Add(new TMP_Dropdown.OptionData(option));
                //    }
                //        _searchDropdown.SetupOriginalInputs();
                //    break;
            }
        }

        private void OnTypeValueChanged()
        {
            switch (_type)
            {
                case LeadInputType.InputField:
                    Debug.Log("Type changed to InputField");
                    _input_field.gameObject.SetActive(true);
                    _dropdown.gameObject.SetActive(false);
                    //_searchDropdown.gameObject.SetActive(false);
                    _toggle.gameObject.SetActive(false);
                    //_toggleGroup.gameObject.SetActive(false);
                    break;
                case LeadInputType.Dropdown:
                    Debug.Log("Type changed to Dropdown");
                    _input_field.gameObject.SetActive(false);
                    _dropdown.gameObject.SetActive(true);
                    //_searchDropdown.gameObject.SetActive(false);
                    _toggle.gameObject.SetActive(false);
                    //_toggleGroup.gameObject.SetActive(false);
                    break;
                case LeadInputType.Toggle:
                    Debug.Log("Type changed to ToggleGroup");
                    _input_field.gameObject.SetActive(false);
                    _dropdown.gameObject.SetActive(false);
                    //_searchDropdown.gameObject.SetActive(false);
                    _toggle.gameObject.SetActive(true);
                    //_toggleGroup.gameObject.SetActive(false);
                    break;
                //case LeadInputType.ToggleGroup:
                //    Debug.Log("Type changed to ToggleGroup");
                //    _input_field.gameObject.SetActive(false);
                //    _dropdown.gameObject.SetActive(false);
                //    _searchDropdown.gameObject.SetActive(false);
                //    _toggle.gameObject.SetActive(false);
                //    _toggleGroup.gameObject.SetActive(true);
                //    break;
                //case LeadInputType.SearchableDropdown:
                //    Debug.Log("Type changed to Dropdown");
                //    _input_field.gameObject.SetActive(false);
                //    _dropdown.gameObject.SetActive(false);
                //    _searchDropdown.gameObject.SetActive(true);
                //    _toggle.gameObject.SetActive(false);
                //    _toggleGroup.gameObject.SetActive(false);
                //     break;
            }
        }
    }
}   