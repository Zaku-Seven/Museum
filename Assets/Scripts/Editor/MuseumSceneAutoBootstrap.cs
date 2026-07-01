using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// When the active scene has no museum layout, prompt once to build it automatically.
/// </summary>
[InitializeOnLoad]
public static class MuseumSceneAutoBootstrap
{
    private const string PromptDismissedKey = "MuseumSceneAutoBootstrap_PromptDismissed";

    static MuseumSceneAutoBootstrap()
    {
        EditorApplication.delayCall += TryBootstrap;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        EditorApplication.delayCall += TryBootstrap;
    }

    [MenuItem("Game/Build Museum Now (Recommended)", priority = 0)]
    public static void BuildMuseumNowFromMenu()
    {
        EditorPrefs.DeleteKey(PromptDismissedKey);
        FullMuseumSetup.SetupFromMenu();
    }

    private static void TryBootstrap()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
        {
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        if (HasMuseumLayout())
        {
            return;
        }

        if (EditorPrefs.GetBool(PromptDismissedKey, false))
        {
            return;
        }

        int choice = EditorUtility.DisplayDialogComplex(
            "Build the museum?",
            "This scene has no museum layout yet (no GalleryWings or Museum_Architecture).\n\n" +
            "• Sorting Museum — full gameplay wings, mounts, paintings, HUD\n" +
            "• Whitebox Museum — Grand Lobby + Art / Fossil wings (greybox layout)\n\n" +
            "You can also use Game → Build Museum Now (Recommended) anytime.",
            "Sorting Museum",
            "Not now",
            "Whitebox Museum");

        switch (choice)
        {
            case 0:
                FullMuseumSetup.SetupFromMenu();
                break;
            case 2:
                WhiteboxMuseumSetup.BuildFromMenu();
                break;
            default:
                EditorPrefs.SetBool(PromptDismissedKey, true);
                Debug.LogWarning(
                    "MuseumSceneAutoBootstrap: Scene has no museum. Run Game → Build Museum Now (Recommended), " +
                    "Game → Setup Full Museum, or Game → Build Whitebox Museum.");
                break;
        }
    }

    private static bool HasMuseumLayout()
    {
        return GameObject.Find("GalleryWings") != null || GameObject.Find("Museum_Architecture") != null;
    }
}
