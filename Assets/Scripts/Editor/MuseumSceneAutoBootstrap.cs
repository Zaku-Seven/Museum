using System.Reflection;
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

        if (SupportsWhiteboxMuseum())
        {
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
                    InvokeWhiteboxBuild();
                    break;
                default:
                    DismissPrompt();
                    break;
            }

            return;
        }

        if (EditorUtility.DisplayDialog(
                "Build the museum?",
                "This scene has no museum layout yet.\n\nRun the full sorting museum setup now?",
                "Build Sorting Museum",
                "Not now"))
        {
            FullMuseumSetup.SetupFromMenu();
        }
        else
        {
            DismissPrompt();
        }
    }

    private static void DismissPrompt()
    {
        EditorPrefs.SetBool(PromptDismissedKey, true);
        Debug.LogWarning(
            "MuseumSceneAutoBootstrap: Scene has no museum. Run Game → Build Museum Now (Recommended) " +
            "or Game → Setup Full Museum.");
    }

    private static bool SupportsWhiteboxMuseum()
    {
        return typeof(MuseumSceneAutoBootstrap).Assembly.GetType("WhiteboxMuseumSetup") != null;
    }

    private static void InvokeWhiteboxBuild()
    {
        MethodInfo build = typeof(MuseumSceneAutoBootstrap).Assembly
            .GetType("WhiteboxMuseumSetup")
            ?.GetMethod("BuildFromMenu", BindingFlags.Public | BindingFlags.Static);
        build?.Invoke(null, null);
    }

    private static bool HasMuseumLayout()
    {
        return GameObject.Find("GalleryWings") != null || GameObject.Find("Museum_Architecture") != null;
    }
}
