using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Toggles pause with Escape: unlocks the cursor, shows an overlay, and disables player
/// movement / pickup. Integrates settings and blocks pause during the win modal.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Text pauseMessageText;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private ArtPickup artPickup;
    [SerializeField] private MountAimHighlighter mountAimHighlighter;
    [SerializeField] private SettingsMenuController settingsMenu;

    [Header("Copy")]
    [SerializeField] private string pauseMessage = "PAUSED\nPress Esc to resume";

    public bool IsPaused { get; private set; }

    public static PauseMenuController Instance { get; private set; }

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Awake()
    {
        ResolveReferences();
        SetPaused(false, force: true);
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        if (MuseumGameFlowController.Instance != null && MuseumGameFlowController.Instance.IsWinModalActive)
        {
            return;
        }

        if (MainMenuController.Instance != null && MainMenuController.Instance.IsMainMenuOpen)
        {
            return;
        }

        if (settingsMenu != null && settingsMenu.IsSettingsOpen)
        {
            settingsMenu.CloseSettings();
            return;
        }

        SetPaused(!IsPaused);
    }

    private void LateUpdate()
    {
        if (IsPaused || ShouldBlockCursorLock())
        {
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ForceUnpause()
    {
        SetPaused(false, force: true);
        settingsMenu?.CloseSettings();
    }

    public void OpenSettingsFromPause()
    {
        if (!IsPaused)
        {
            SetPaused(true);
        }

        settingsMenu?.OpenSettings();
    }

    public void ResumeFromButton()
    {
        SetPaused(false);
    }

    public void RequestNewGameFromPause()
    {
        ForceUnpause();
        MuseumGameFlowController.Instance?.StartNewGame(clearTutorialHints: false);
    }

    private bool ShouldBlockCursorLock()
    {
        if (MuseumGameFlowController.Instance != null && MuseumGameFlowController.Instance.IsWinModalActive)
        {
            return true;
        }

        return MuseumProgress.Instance != null && MuseumProgress.Instance.IsMuseumComplete
            && MuseumGameFlowController.Instance != null && !MuseumGameFlowController.Instance.IsWinModalActive
            && settingsMenu != null && settingsMenu.IsSettingsOpen;
    }

    private void ResolveReferences()
    {
        if (firstPersonController == null)
        {
            firstPersonController = GetComponent<FirstPersonController>();
        }

        if (settingsMenu == null)
        {
            settingsMenu = GetComponent<SettingsMenuController>();
        }

        if (artPickup == null || mountAimHighlighter == null)
        {
            Transform cameraTransform = transform.Find("PlayerCamera");
            if (cameraTransform != null)
            {
                if (artPickup == null)
                {
                    artPickup = cameraTransform.GetComponent<ArtPickup>();
                }

                if (mountAimHighlighter == null)
                {
                    mountAimHighlighter = cameraTransform.GetComponent<MountAimHighlighter>();
                }
            }
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void SetPaused(bool paused, bool force = false)
    {
        if (!force && paused == IsPaused)
        {
            return;
        }

        IsPaused = paused;

        if (!paused)
        {
            settingsMenu?.CloseSettings();
        }

        if (firstPersonController != null)
        {
            firstPersonController.enabled = !paused;
        }

        if (artPickup != null)
        {
            artPickup.enabled = !paused && !(MuseumProgress.Instance?.IsMuseumComplete ?? false);
        }

        if (mountAimHighlighter != null)
        {
            mountAimHighlighter.enabled = !paused;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(paused);
        }

        if (pauseMessageText != null)
        {
            pauseMessageText.text = pauseMessage;
        }

        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }
}
