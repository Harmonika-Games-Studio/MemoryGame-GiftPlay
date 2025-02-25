using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace Harmonika.Tools.Keyboard
{
    public class KeyboardView : MonoBehaviour, IPointerDownHandler
    {

        public KeyboardType selectedType;

        public void OnPointerDown(PointerEventData eventData)
        {

            KeyboardScript keyboardScpt = FindAnyObjectByType<KeyboardScript>();
            keyboardScpt.TextField = gameObject.GetComponent<TMP_InputField>();
            keyboardScpt.TextFieldView.text = gameObject.GetComponent<TMP_InputField>().text;

            keyboardScpt.keyboardObj.SetActive(true);
            keyboardScpt.SetLayout(selectedType);
        }
    }
}