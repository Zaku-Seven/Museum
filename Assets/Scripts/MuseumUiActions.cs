using UnityEngine;

/// <summary>
/// UI button hooks for pause/win menus (wire OnClick in the Editor setup).
/// </summary>
public class MuseumUiActions : MonoBehaviour
{
    private MuseumAudioDirector audioDirector;

    private void Awake()
    {
        audioDirector = FindFirstObjectByType<MuseumAudioDirector>();
    }

    public void ContinueExploringAfterWin()
    {
        PlayClick();
        MuseumGameFlowController.Instance?.ContinueExploringAfterWin();
    }

    public void StartNewGame()
    {
        PlayClick();
        MuseumGameFlowController.Instance?.StartNewGame(clearTutorialHints: false);
    }

    public void StartNewGameClearTutorials()
    {
        PlayClick();
        MuseumGameFlowController.Instance?.StartNewGame(clearTutorialHints: true);
    }

    public void ResumeFromPause()
    {
        PlayClick();
        PauseMenuController.Instance?.ResumeFromButton();
    }

    public void OpenSettingsFromPause()
    {
        PlayClick();
        PauseMenuController.Instance?.OpenSettingsFromPause();
    }

    public void CloseSettings()
    {
        PlayClick();
        FindFirstObjectByType<SettingsMenuController>()?.CloseSettings();
    }

    public void NewGameFromPause()
    {
        PlayClick();
        PauseMenuController.Instance?.RequestNewGameFromPause();
    }

    public void MainMenuContinue()
    {
        PlayClick();
        MainMenuController.Instance?.ContinueSavedGame();
    }

    public void MainMenuStartFresh()
    {
        PlayClick();
        MainMenuController.Instance?.StartFreshSession();
    }

    public void MainMenuShowNewGameConfirm()
    {
        PlayClick();
        MainMenuController.Instance?.ShowNewGameConfirm();
    }

    public void MainMenuCancelNewGameConfirm()
    {
        PlayClick();
        MainMenuController.Instance?.CancelNewGameConfirm();
    }

    public void MainMenuConfirmNewGame()
    {
        PlayClick();
        MainMenuController.Instance?.ConfirmNewGame();
    }

    public void MainMenuOpenSettings()
    {
        PlayClick();
        MainMenuController.Instance?.OpenSettingsFromMainMenu();
    }

    public void OpenControlsHelp()
    {
        PlayClick();
        ControlsHelpController.Instance?.Open();
    }

    public void OpenCollectionJournal()
    {
        PlayClick();
        MuseumJournalController.Instance?.OpenFromPause();
    }

    public void CloseJournal()
    {
        PlayClick();
        MuseumJournalController.Instance?.Close();
    }

    public void CloseControlsHelp()
    {
        PlayClick();
        ControlsHelpController.Instance?.Close();
    }

    public void ResetSettingsToDefaults()
    {
        PlayClick();
        FindFirstObjectByType<SettingsMenuController>()?.ResetToDefaults();
    }

    private void PlayClick()
    {
        audioDirector?.PlayUiClick();
    }
}
