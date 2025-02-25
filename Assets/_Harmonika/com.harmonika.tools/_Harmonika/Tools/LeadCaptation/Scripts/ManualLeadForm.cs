using Harmonika.MenuManager;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace Harmonika.Tools.MenuManager
{
    
    public class ManualLeadForm : LeadCaptation
    {
        public List<LeadFieldBox> _leadFieldBoxes = new();

        protected override void Start()
        {
            ConfigureLeadboxes();
            base.Start();
        }

        private void ConfigureLeadboxes()
        {
            foreach (var fieldBox in _leadFieldBoxes)
            {
                _formInputs.Add(new FormInput(fieldBox.gameObject.name, fieldBox.InputType, fieldBox.GetInputObject(), fieldBox.IsOptional));
                fieldBox.onValueChanged += (string a) =>
                {
                    bool aux = CheckInputsFilled();
                    Debug.Log("CheckInputsFilled: " + aux);
                    submitButton.gameObject.SetActive(aux);
                };
            }
        }
    }
}