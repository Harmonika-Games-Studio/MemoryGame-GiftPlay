using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Harmonika.Tools;

public class FormController : MonoBehaviour
{
    //[Header("Parent Object")]
    //public GameObject parentPanel;

    [Header("Button")]
    public Button submitButton;

    [Header("UI Elements")]
    public List<FormInput> formInputs;
    public Action<JObject> OnSubmitEvent;

    void Start()
    {
        ActivateConfirmButtonCheck();
        SetupForm();
    }

    private void SetupForm()
    {
        foreach (var input in formInputs)
        {
            switch (input.inputType)
            {
                case LeadInputType.InputField:
                    TMP_InputField inputField = input.inputContainer.GetComponent<TMP_InputField>();
                    if (inputField != null)
                    {
                        inputField.onValueChanged.AddListener(delegate { ActivateConfirmButtonCheck(); });
                    }
                    break;

                case LeadInputType.Dropdown:
                    TMP_Dropdown dropdown = input.inputContainer.GetComponent<TMP_Dropdown>();
                    if (dropdown != null)
                    {
                        dropdown.onValueChanged.AddListener(delegate { ActivateConfirmButtonCheck(); });
                    }
                    break;

                case LeadInputType.Toggle:
                    Toggle toggle = input.inputContainer.GetComponent<Toggle>();
                    if (toggle != null)
                    {
                        toggle.onValueChanged.AddListener(delegate { ActivateConfirmButtonCheck(); });
                    }
                    break;
            }
        }

        submitButton.onClick.AddListener(SubmitForm);
    }

    private void SubmitForm()
    {
        JObject jsonData = GrabAllData();
        Debug.Log("Here's your data, bro! " + jsonData.ToString());

        OnSubmitEvent?.Invoke(jsonData);
    }

    private JObject GrabAllData()
    {
        JObject jsonObject = new JObject(); 

        foreach (var input in formInputs)
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

            jsonObject.Add(input.name, inputData);
        }

        return jsonObject;
    }

    private string ToggleToString(bool toggle)
    {
        return toggle ? "Sim" : "Não";
    }

    private string DropdownToString(TMP_Dropdown dropdown)
    {
        return dropdown.options[dropdown.value].text;
    }

    private string InputFieldToString(TMP_InputField inputField)
    {
        return inputField.text;
    }

    private void ActivateConfirmButtonCheck()
    {
        submitButton.gameObject.SetActive(CheckInputsFilled());
    }

    public bool CheckInputsFilled()
    {
        foreach (var input in formInputs)
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
}

