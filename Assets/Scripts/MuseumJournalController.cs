using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Curator journal: every painting, wing, and location. Open with J / R3 or pause menu Collection.
/// </summary>
public class MuseumJournalController : MonoBehaviour
{
    public static MuseumJournalController Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Text bodyText;

    private bool openedWhilePaused;
    private bool freezePushed;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        Instance = this;
        CloseImmediate();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (IsOpen && MuseumInput.CloseOverlayPressedThisFrame())
        {
            Close();
            return;
        }

        if (!MuseumInput.JournalPressedThisFrame())
        {
            return;
        }

        if (MainMenuController.Instance != null && MainMenuController.Instance.IsMainMenuOpen)
        {
            return;
        }

        if (ControlsHelpController.Instance != null && ControlsHelpController.Instance.IsOpen)
        {
            return;
        }

        if (MuseumGameFlowController.Instance != null && MuseumGameFlowController.Instance.IsWinModalActive)
        {
            return;
        }

        if (IsOpen)
        {
            Close();
        }
        else
        {
            OpenFromGameplay();
        }
    }

    public void OpenFromPause()
    {
        openedWhilePaused = PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused;
        OpenInternal();
    }

    public void OpenFromGameplay()
    {
        openedWhilePaused = false;
        OpenInternal();
    }

    public void Close()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        ReleaseFreezeIfNeeded();
        RestoreCursorForActiveOverlay();
    }

    private void OpenInternal()
    {
        if (bodyText != null)
        {
            bodyText.text = MuseumCollectionLog.BuildJournalText();
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }

        if (!openedWhilePaused && !freezePushed)
        {
            MuseumTimeScale.PushFreeze();
            freezePushed = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseImmediate()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void ReleaseFreezeIfNeeded()
    {
        if (freezePushed)
        {
            MuseumTimeScale.PopFreeze();
            freezePushed = false;
        }
    }

    private void RestoreCursorForActiveOverlay()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused)
        {
            return;
        }

        if (MainMenuController.Instance != null && MainMenuController.Instance.IsMainMenuOpen)
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
