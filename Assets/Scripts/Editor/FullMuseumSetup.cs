using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click scene bootstrap: runs every Game setup menu in the recommended order.
/// </summary>
public static class FullMuseumSetup
{
    [MenuItem("Game/Setup Full Museum")]
    public static void SetupFromMenu()
    {
        Debug.Log("Full Museum setup: starting all setup steps in order…");

        PrototypeSceneSetup.SetupFromMenu();
        ArtPickupSceneSetup.SetupFromMenu();
        MuseumGameplaySetup.SetupFromMenu();
        GalleryContentSetup.SetupFromMenu();
        PaintingCatalogSetup.CreateDefaultCatalog();
        PaintingDefinitionLinker.LinkScenePaintings();
        MountPlacardSetup.AddPlacardsFromMenu();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("Full Museum setup complete. Press Play to sort paintings by wing.");
    }
}
