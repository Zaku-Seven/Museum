using UnityEngine;

/// <summary>
/// Coordinates win modal, new game, and gameplay input blocking.
/// </summary>
public class MuseumGameFlowController : MonoBehaviour
{
    public static MuseumGameFlowController Instance { get; private set; }

    [SerializeField] private GameObject winPanel;
    [SerializeField] private bool allowMovementAfterWin = true;

    public bool IsWinModalActive { get; private set; }
    public bool BlockGameplayInput => IsWinModalActive;

    private void OnEnable()
    {
        Instance = this;
        MuseumGameEvents.MuseumCompleted += EnterWinModal;
    }

    private void OnDisable()
    {
        MuseumGameEvents.MuseumCompleted -= EnterWinModal;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void EnterWinModal()
    {
        if (IsWinModalActive)
        {
            return;
        }

        IsWinModalActive = true;

        MuseumTimeScale.PushFreeze();

        PauseMenuController.Instance?.ForceUnpause();

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        SetGameplayEnabled(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitWinModal()
    {
        if (IsWinModalActive)
        {
            MuseumTimeScale.PopFreeze();
        }

        IsWinModalActive = false;

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (!MuseumProgress.Instance?.IsMuseumComplete ?? true)
        {
            SetGameplayEnabled(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (allowMovementAfterWin)
        {
            SetGameplayEnabled(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>Hide win overlay but keep completion state; player can keep walking.</summary>
    public void ContinueExploringAfterWin()
    {
        ExitWinModal();

        if (allowMovementAfterWin)
        {
            SetGameplayEnabled(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void StartNewGame(bool clearTutorialHints = false)
    {
        MuseumSaveManager.Instance?.ClearSave();
        MuseumStateReset.ResetMuseumScene(clearTutorialHints);
        MuseumSaveManager.Instance?.Save();
    }

    public void StartNewGameAndClearTutorials()
    {
        StartNewGame(clearTutorialHints: true);
    }

    private void SetGameplayEnabled(bool enabled)
    {
        FirstPersonController fps = GetComponent<FirstPersonController>();
        if (fps != null)
        {
            fps.enabled = enabled;
        }

        Transform cameraTransform = transform.Find("PlayerCamera");
        if (cameraTransform != null)
        {
            ArtPickup pickup = cameraTransform.GetComponent<ArtPickup>();
            if (pickup != null)
            {
                pickup.enabled = enabled && !(MuseumProgress.Instance?.IsMuseumComplete ?? false);
            }

            MountAimHighlighter highlighter = cameraTransform.GetComponent<MountAimHighlighter>();
            if (highlighter != null)
            {
                highlighter.enabled = enabled;
            }
        }
    }
}
