using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Harmonika.Tools;

public class BlockMenu : MonoBehaviour
{
    [Header("References")]
    public GameObject pnlBlockOffline;
    public GameObject pnlBlockLicense;
    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _supportButton;
    [SerializeField] private Button _quitButton;

    public void ShowOfflineText()
    {
        pnlBlockOffline.SetActive(true);
        pnlBlockLicense.SetActive(false);
        _connectButton.gameObject.SetActive(true);
    }

    public void ShowLicenseBlockedText()
    {
        pnlBlockOffline.SetActive(false);
        pnlBlockLicense.SetActive(true);
        _connectButton.gameObject.SetActive(false);
    }

    public void OpenBlockPanel(BlockType blockType)
    {
        AppBlocker.Instance.IsBlocked = true;
        switch (blockType)
        {
            case BlockType.NoConnectionAndDueDate:
                ShowOfflineText();
                break;
            case BlockType.LicenseNotActive:
                ShowLicenseBlockedText();
                break;
        }
    }
}