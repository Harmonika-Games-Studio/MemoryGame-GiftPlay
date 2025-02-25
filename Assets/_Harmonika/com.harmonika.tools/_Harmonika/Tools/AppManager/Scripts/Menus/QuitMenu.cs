using Harmonika.Tools;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class QuitMenu : MonoBehaviour
{
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _minimizeButton;
    [SerializeField] private Button _quitButton;
    private void Awake()
    {
        _backButton.onClick.AddListener(() => AppManager.Instance.OpenMenu("MainMenu"));
        _minimizeButton.onClick.AddListener(MinimizeWindow);
        _quitButton.onClick.AddListener(() => Application.Quit());
    }

    #region MINIMIZE
    [DllImport("User32.dll")]
    private static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);

    [DllImport("User32.dll")]
    private static extern System.IntPtr GetForegroundWindow();

    private const int SW_MINIMIZE = 6;

    private void MinimizeWindow()
    {
        System.IntPtr hWnd = GetForegroundWindow();

        ShowWindow(hWnd, SW_MINIMIZE);
    }
    #endregion
}
