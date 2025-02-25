using UnityEngine;
using UnityEngine.UI;
using Harmonika.Tools;
using System.Collections.Generic;
using TMPro;
using NUnit.Framework.Internal;

public class StorageMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _backButton;

    private void Awake()
    {
        _backButton.onClick.AddListener(() => AppManager.Instance.OpenMenu("MainMenu"));
    }
}