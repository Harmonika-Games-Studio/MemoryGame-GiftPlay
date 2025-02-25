using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Harmonika.Tools.Keyboard
{
    public class KeyboardScript : MonoBehaviour
    {

        [Header("Configuration")]
        public Color32 textColor;
        //public Color32 buttonColor;
        public TMP_FontAsset font;

        [Space(20)]

        [HideInInspector] public TMP_InputField TextField;
        public GameObject keyboardObj;
        public TMP_Text TextFieldView;
        public List<GameObject> layouts;
        public List<Image> btnImages;

        private void Start()
        {
            SetColors();
        }

        public void alphabetFunction(string alphabet)
        {

            TextField.text = TextField.text + alphabet;
            TextFieldView.text = TextField.text;

            if (layouts[1].activeInHierarchy)
            {
                if (layouts[2].activeInHierarchy)
                {
                    SetLayout(KeyboardType.AlphaLower);
                }
                else
                {
                    SetLayout(KeyboardType.AlphaLowerNoSymbol);
                }
            }
        }

        public void CapsUncaps(int whichWay)
        {
            if (whichWay == 0)
            {
                if (layouts[2].activeInHierarchy)
                {
                    SetLayout(KeyboardType.AlphaLower);
                }
                else
                {
                    SetLayout(KeyboardType.AlphaLowerNoSymbol);
                }
            }
            else if (whichWay == 1)
            {
                if (layouts[7].activeInHierarchy)
                {
                    SetLayout(KeyboardType.AlphaUpper);
                }
                else
                {
                    SetLayout(KeyboardType.AlphaUpperNoSymbol);
                }
            }

        }

        public void BackSpace()
        {

            if (TextField.text.Length > 0) TextField.text = TextField.text.Remove(TextField.text.Length - 1);
            TextFieldView.text = TextField.text;
        }

        public void CloseAllLayouts()
        {

            foreach (var item in layouts)
            {
                item.SetActive(false);
            }
        }

        public void SetLayout(KeyboardType type)
        {

            layouts[5].SetActive(false);

            switch (type)
            {
                case KeyboardType.AlphaLower:
                    ShowLayout(layouts[0]);
                    break;
                case KeyboardType.AlphaUpper:
                    ShowLayout(layouts[1]);
                    break;
                case KeyboardType.Symbols:
                    ShowLayout(layouts[2]);
                    break;
                case KeyboardType.Numeric:
                    ShowLayout(layouts[3]);
                    layouts[4].SetActive(false);
                    break;
                case KeyboardType.AlphaLowerNoSymbol:
                    ShowLayout(layouts[0]);
                    layouts[2].SetActive(false);
                    layouts[7].SetActive(false);
                    break;
                case KeyboardType.AlphaUpperNoSymbol:
                    ShowLayout(layouts[1]);
                    layouts[2].SetActive(false);
                    layouts[7].SetActive(false);
                    break;
                case KeyboardType.AlphaLowerEmail:
                    ShowLayout(layouts[0]);
                    layouts[5].SetActive(true);
                    break;
                default:
                    break;
            }
        }

        public void OnEndEdit()
        {
            TextField.onSubmit.Invoke(TextField.text);
        }

        public void ShowLayout(GameObject SetLayout)
        {
            CloseAllLayouts();
            SetLayout.SetActive(true);
            layouts[4].SetActive(true);
            layouts[6].SetActive(true);
            layouts[7].SetActive(true);
        }


        private void SetColors()
        {

            keyboardObj.SetActive(true);
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>();
            foreach (var item in texts)
            {
                item.color = textColor;
                item.font = font;
            }

            foreach (var item in btnImages)
            {
                item.color = textColor;
            }

            TextFieldView.color = Color.white;

            keyboardObj.SetActive(false);
        }
    }
}