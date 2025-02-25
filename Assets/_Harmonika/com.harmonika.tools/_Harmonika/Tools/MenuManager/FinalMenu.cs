using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FinalMenu : MonoBehaviour
{
    [SerializeField] private Button _backToMainMenu;

    public void AddBackToMainMenuButtonListener(UnityAction action)
    {
        _backToMainMenu.onClick.AddListener(action);
    }
}
