using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Harmonika.MenuManager
{
    //Atention: To this code work properly, a change is necessary on TMP_Dropdown.cs, on line 443!
    //The line  m_Value = Mathf.Clamp(value, m_Placeholder ? -1 : 0, options.Count - 1); need to be changed to:
    // •  "m_Value = Mathf.Clamp(value, -1, options.Count - 1);"
    //Please, make this change if you wnat to use Searchable Dropdowns on your project.

    public class SearchableDropdown : TMP_Dropdown
    {
        [HideInInspector] public TMP_InputField inputField;
        private List<string> originalOptions = new List<string>();
        private Coroutine searchCoroutine;
        private float _delay = 0f;
        //public int value;

        protected override void Awake()
        {
            inputField = GetComponentInChildren<TMP_InputField>();
            SetupOriginalInputs();
        }

        protected override void Start()
        {
            base.Start();
            inputField.onSubmit.AddListener(OnSearchInputChanged);
            inputField.onSelect.AddListener((string a) => { inputField.text = string.Empty; });
            onValueChanged.AddListener(OnValueChanged);
        }

        public void SetupOriginalInputs()
        {
            foreach (var option in options)
            {
                originalOptions.Add(option.text);
            }
        }

        private void OnSearchInputChanged(string searchText)
        {
            if (string.IsNullOrEmpty(inputField.text)) return;

            if (searchCoroutine != null) StopCoroutine(searchCoroutine);
            searchCoroutine = StartCoroutine(DelayedFilterDropdown(searchText));
        }

        private void OnValueChanged(int i)
        {
            if (i == -1) return;

            value = i;

            Debug.Log($"Option selected: {originalOptions[value]}");

            inputField.onSubmit.RemoveAllListeners();

            inputField.text = originalOptions[value];

            inputField.onSubmit.AddListener(OnSearchInputChanged);

            Invoke(nameof(Deselect), 0.1f);
        }

        void Deselect()
        {
            onValueChanged.RemoveListener(OnValueChanged);
            base.value = -1;
            onValueChanged.AddListener(OnValueChanged);
        }

        private IEnumerator DelayedFilterDropdown(string searchText)
        {
            yield return new WaitForSeconds(_delay);
            UpdateDropdownOptions(searchText);
        }

        private void UpdateDropdownOptions(string filter)
        {
            var filteredOptions = originalOptions
                .Where(option => option.ToLower().Contains(filter.ToLower()))
                .OrderBy(option => option)
                .ToList();

            options.Clear();
            foreach (var option in filteredOptions)
            {
                options.Add(new OptionData(option));
            }

            RefreshShownValue();
            Show();
        }
    }
}
