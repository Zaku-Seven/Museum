using UnityEngine;

/// <summary>
/// UI button hooks for pause/win menus (wire OnClick in the Editor setup).
/// </summary>
public class MuseumUiActions : MonoBehaviour
{
    public void ContinueExploringAfterWin()
    {
        MuseumGameFlowController.Instance?.ContinueExploringAfterWin();
    }

    public void StartNewGame()
    {
        MuseumGameFlowController.Instance?.StartNewGame(clearTutorialHints: false);
    }

    public void StartNewGameClearTutorials()
    {
        MuseumGameFlowController.Instance?.StartNewGame(clearTutorialHints: true);
    }

    public void ResumeFromPause()
    {
        PauseMenuController.Instance?.ResumeFromButton();
    }

    public void OpenSettingsFromPause()
    {
        PauseMenuController.Instance?.OpenSettingsFromPause();
    }

    public void CloseSettings()
    {
        FindFirstObjectByType<SettingsMenuController>()?.CloseSettings();
    }

    public void NewGameFromPause()
    {
        PauseMenuController.Instance?.RequestNewGameFromPause();
    }

    public void MainMenuContinue()
    {
        MainMenuController.Instance?.ContinueSavedGame();
    }

    public void MainMenuStartFresh()
    {
        MainMenuController.Instance?.StartFreshSession();
    }

    public void MainMenuShowNewGameConfirm()
    {
        MainMenuController.Instance?.ShowNewGameConfirm();
    }

    public void MainMenuCancelNewGameConfirm()
    {
        MainMenuController.Instance?.CancelNewGameConfirm();
    }

    public void MainMenuConfirmNewGame()
    {
        MainMenuController.Instance?.ConfirmNewGame();
    }

    public void MainMenuOpenSettings()
    {
        MainMenuController.Instance?.OpenSettingsFromMainMenu();
    }
}
