using Harmonika.Tools;
using NaughtyAttributes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LeadCaptation : MonoBehaviour
{
    [Header("Button")]
    public Button submitButton;

    public UnityAction<JObject> OnSubmitEvent;

    protected List<FormInput> _formInputs = new();

    public List<FormInput> FormInputs { get => _formInputs; }

    protected virtual void Awake()
    {
        AppManager.Instance.DataSync.AddLeadCaptation(this);
    }

    protected virtual void Start()
    {
        submitButton.gameObject.SetActive(CheckInputsFilled());
        SetupForm();
    }

    private void SetupForm()
    {
        foreach (var input in _formInputs)
        {
            switch (input.inputType)
            {
                case LeadInputType.InputField:
                    TMP_InputField inputField = input.inputContainer.GetComponent<TMP_InputField>();
                    if (inputField != null)
                    {
                        inputField.onValueChanged.AddListener(delegate { submitButton.gameObject.SetActive(CheckInputsFilled()); });
                    }
                    break;

                case LeadInputType.Dropdown:
                    TMP_Dropdown dropdown = input.inputContainer.GetComponent<TMP_Dropdown>();
                    if (dropdown != null)
                    {
                        dropdown.onValueChanged.AddListener(delegate { submitButton.gameObject.SetActive(CheckInputsFilled()); });
                    }
                    break;

                case LeadInputType.Toggle:
                    Toggle toggle = input.inputContainer.GetComponent<Toggle>();
                    if (toggle != null)
                    {
                        toggle.onValueChanged.AddListener(delegate { submitButton.gameObject.SetActive(CheckInputsFilled()); });
                    }
                    break;
            }
        }

        submitButton.onClick.AddListener(SubmitForm);
    }

    protected virtual void SubmitForm()
    {
        JObject jsonData = GrabAllData();
        Debug.Log("Here's your data, manito! " + jsonData.ToString());

        OnSubmitEvent?.Invoke(jsonData);
    }

    JObject GrabAllData()
    {
        JObject jsonObject = new JObject();
        jsonObject.RemoveAll();

        foreach (var input in _formInputs)
        {
            string inputData = string.Empty;

            switch (input.inputType)
            {
                case LeadInputType.InputField:
                    TMP_InputField inputField = input.inputContainer.GetComponent<TMP_InputField>();
                    if (inputField != null)
                    {
                        inputData = InputFieldToString(inputField);
                    }
                    break;

                case LeadInputType.Dropdown:
                    TMP_Dropdown dropdown = input.inputContainer.GetComponent<TMP_Dropdown>();
                    if (dropdown != null)
                    {
                        inputData = DropdownToString(dropdown);
                    }
                    break;

                case LeadInputType.Toggle:
                    Toggle toggle = input.inputContainer.GetComponent<Toggle>();
                    if (toggle != null)
                    {
                        inputData = ToggleToString(toggle.isOn);
                    }
                    break;
            }

            string uniqueKey = input.name;
            int counter = 1;

            while (jsonObject.ContainsKey(uniqueKey))
            {
                uniqueKey = input.name + "_" + counter;
                counter++;
            }
            jsonObject.Add(uniqueKey, inputData);
        }

        return jsonObject;
    }

    public bool CheckInputsFilled()
    {
        foreach (var input in _formInputs)
        {
            if (input.isOptional)
                continue;

            switch (input.inputType)
            {
                case LeadInputType.InputField:
                    TMP_InputField inputField = input.inputContainer.GetComponent<TMP_InputField>();
                    if (string.IsNullOrEmpty(inputField.text))
                        return false;
                    break;

                case LeadInputType.Dropdown:
                    TMP_Dropdown dropdown = input.inputContainer.GetComponent<TMP_Dropdown>();
                    if (dropdown.value == -1)
                        return false;
                    break;

                case LeadInputType.Toggle:
                    Toggle toggle = input.inputContainer.GetComponent<Toggle>();
                    if (!toggle.isOn)
                        return false;
                    break;
            }
        }

        return true;
    }

    private string ToggleToString(bool toggle)
    {
        return toggle ? "Sim" : "Não";
    }

    private string DropdownToString(TMP_Dropdown dropdown)
    {
        if (dropdown.value == -1) return string.Empty;
        return dropdown.options[dropdown.value].text;
    }

    private string InputFieldToString(TMP_InputField inputField)
    {
        return inputField.text;
    }

    public void ClearAllFields()
    {
        foreach (FormInput formInput in _formInputs)
        {
            switch (formInput.inputType)
            {
                case LeadInputType.InputField:
                    TMP_InputField inputField = formInput.inputContainer.GetComponent<TMP_InputField>();
                    inputField.text = string.Empty;
                    break;

                case LeadInputType.Dropdown:
                    TMP_Dropdown dropdown = formInput.inputContainer.GetComponent<TMP_Dropdown>();
                    dropdown.value = -1;
                    break;

                case LeadInputType.Toggle:
                    Toggle toggle = formInput.inputContainer.GetComponent<Toggle>();
                    toggle.isOn = false;
                    break;
            }
        }
    }
}

[System.Serializable]
public class ToggleDataConfig
{
    public string text;
}

[System.Serializable]
public class InputDataConfig
{
    public KeyboardType keyboardType;
    public ParseableFields parseableFields;
    public string fieldPlaceholder;

    public InputDataConfig(KeyboardType keyboardType, ParseableFields parseableFields, string fieldPlaceholder)
    {
        this.keyboardType = keyboardType;
        this.parseableFields = parseableFields;
        this.fieldPlaceholder = fieldPlaceholder;
    }

    public InputDataConfig(string keyboardType, string parseableFields, string fieldPlaceholder)
    {
        if (System.Enum.TryParse(keyboardType, out KeyboardType parsedKeyboardType))
            this.keyboardType = parsedKeyboardType;
        else
            throw new System.ArgumentException($"Invalid KeyboardType: {keyboardType}");

        if (System.Enum.TryParse(parseableFields, out ParseableFields parsedParseableFields))
            this.parseableFields = parsedParseableFields;
        else
            throw new System.ArgumentException($"Invalid ParseableFields: {parseableFields}");

        this.fieldPlaceholder = fieldPlaceholder;
    }
}

[System.Serializable]
public class DropdownDataConfig
{
    public string fieldPlaceholder;
    public string[] options;
}

[System.Serializable]
public class FormInput
{
    public string name;
    public LeadInputType inputType;
    public GameObject inputContainer;
    public bool isOptional;

    public FormInput(string name, LeadInputType inputType, GameObject inputContainer, bool isOptional = false)
    {
        this.name = name;
        this.inputType = inputType;
        this.inputContainer = inputContainer;
        this.isOptional = isOptional;
    }
}


[System.Serializable]
public class LeadDataConfig
{
    [Header("Config")]
    public string fieldName;
    public LeadID id; 
    public bool showName = true;
    public bool isOptional;

    [AllowNesting]
    [OnValueChanged(nameof(OnTypeValueChanged))]
    public LeadInputType inputType;

    private bool _inputField, _dropdown, _toggle;

    [AllowNesting]
    [ShowIf(nameof(_toggle))]
    public ToggleDataConfig toggleDataConfig;

    [AllowNesting]
    [ShowIf(nameof(_inputField))]
    public InputDataConfig inputDataConfig;

    [AllowNesting]
    [ShowIf(nameof(_dropdown))]
    public DropdownDataConfig dropdownDataConfig;

    void OnTypeValueChanged()
    {
        switch (inputType)
        {
            case LeadInputType.InputField:
                _inputField = true;
                _dropdown = false;
                _toggle = false;
                break;
            case LeadInputType.Dropdown:
                _inputField = false;
                _dropdown = true;
                _toggle = false;
                break;
            case LeadInputType.Toggle:
                _inputField = false;
                _dropdown = false;
                _toggle = true;
                break;
        }
    }
    private void FakeUsage() //Fake method to avoid 3 annoying warnings in the console
    {
        if (_inputField) { }
        if (_dropdown) { }
        if (_toggle) { }
    }
}