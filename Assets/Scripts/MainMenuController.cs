using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Title screen shown at session start: Continue (if save exists), New Game, or Enter Museum.
/// Defers save load until the player chooses Continue.
/// </summary>
[DefaultExecutionOrder(-100)]
public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance { get; private set; }

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject newGameConfirmPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Text subtitleText;

    [SerializeField] private bool showMenuOnStart = true;

    public bool IsMainMenuOpen { get; private set; }

    private void Awake()
    {
        Instance = this;

        if (continueButton != null)
        {
            continueButton.interactable = MuseumSaveManager.HasExistingSave();
        }

        if (subtitleText != null)
        {
            subtitleText.text = MuseumSaveManager.HasExistingSave()
                ? "Welcome back, curator."
                : "Sort paintings by wing. Hang every gallery.";
        }

        if (showMenuOnStart)
        {
            OpenMainMenu();
        }
        else
        {
            CloseAllMenus();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static bool ShouldDeferSaveLoad()
    {
        return Instance != null && Instance.IsMainMenuOpen;
    }

    public void OpenMainMenu()
    {
        IsMainMenuOpen = true;

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (newGameConfirmPanel != null)
        {
            newGameConfirmPanel.SetActive(false);
        }

        SetGameplayEnabled(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ContinueSavedGame()
    {
        MuseumSaveManager.Instance?.Load();
        CloseAllMenus();
    }

    public void StartFreshSession()
    {
        CloseAllMenus();
    }

    public void ShowNewGameConfirm()
    {
        if (newGameConfirmPanel != null)
        {
            newGameConfirmPanel.SetActive(true);
        }
    }

    public void CancelNewGameConfirm()
    {
        if (newGameConfirmPanel != null)
        {
            newGameConfirmPanel.SetActive(false);
        }
    }

    public void ConfirmNewGame()
    {
        MuseumGameFlowController.Instance?.StartNewGame(clearTutorialHints: false);
        CancelNewGameConfirm();
        CloseAllMenus();
    }

    public void OpenSettingsFromMainMenu()
    {
        SettingsMenuController settings = GetComponent<SettingsMenuController>();
        settings?.OpenSettings();
    }

    private void CloseAllMenus()
    {
        IsMainMenuOpen = false;

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (newGameConfirmPanel != null)
        {
            newGameConfirmPanel.SetActive(false);
        }

        SetGameplayEnabled(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
                pickup.enabled = enabled;
            }

            MountAimHighlighter highlighter = cameraTransform.GetComponent<MountAimHighlighter>();
            if (highlighter != null)
            {
                highlighter.enabled = enabled;
            }
        }
    }
}
