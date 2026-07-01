using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reference panel for keyboard and gamepad bindings.
/// </summary>
public class ControlsHelpController : MonoBehaviour
{
    public static ControlsHelpController Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Text bodyText;

    [TextArea(8, 20)]
    [SerializeField] private string helpCopy =
        "KEYBOARD\n" +
        "E — pick up / stack    Scroll — active item\n" +
        "Click — place/drop     Q — throw\n" +
        "Right-click — undo     Esc — pause\n" +
        "WASD — move            Space — jump\n\n" +
        "GAMEPAD\n" +
        "A — pick up    B — throw    X — place    Y — undo\n" +
        "LB / RB — stack item    Start — pause\n" +
        "Left stick — move    Right stick — look\n\n" +
        "DEV (Editor / Development build)\n" +
        "F9 — fill one section    F10 — auto-hang all    F8 — save";

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        Instance = this;
        if (bodyText != null)
        {
            bodyText.text = helpCopy;
        }

        Close();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Open()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (MainMenuController.Instance != null && MainMenuController.Instance.IsMainMenuOpen)
        {
            return;
        }

        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused)
        {
            return;
        }

        if (MuseumGameFlowController.Instance != null && MuseumGameFlowController.Instance.IsWinModalActive)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
