using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Development-only hotkeys for rapid museum testing (stripped from release builds unless DEVELOPMENT_BUILD).
/// </summary>
public class MuseumDevCheats : MonoBehaviour
{
    [SerializeField] private bool enableCheats = true;

    private void Update()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        return;
#else
        if (!enableCheats || Keyboard.current == null)
        {
            return;
        }

        if (MainMenuController.Instance != null && MainMenuController.Instance.IsMainMenuOpen)
        {
            return;
        }

        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            CompleteIncompleteSection();
        }

        if (Keyboard.current.f10Key.wasPressedThisFrame)
        {
            AutoHangAllMatchingPaintings();
        }

        if (Keyboard.current.f8Key.wasPressedThisFrame)
        {
            MuseumSaveManager.Instance?.Save();
            Debug.Log("MuseumDevCheats: manual save triggered.");
        }
#endif
    }

    private static void CompleteIncompleteSection()
    {
        if (GallerySection.AllSections == null)
        {
            return;
        }

        for (int i = 0; i < GallerySection.AllSections.Count; i++)
        {
            GallerySection section = GallerySection.AllSections[i];
            if (section == null || section.IsComplete)
            {
                continue;
            }

            FillSection(section);
            Debug.Log($"MuseumDevCheats: filled section {section.DisplayName}.");
            return;
        }

        Debug.Log("MuseumDevCheats: all sections already complete.");
    }

    private static void FillSection(GallerySection section)
    {
        PaintingMount[] mounts = Object.FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        InteractablePainting[] paintings = Object.FindObjectsByType<InteractablePainting>(FindObjectsSortMode.None);

        for (int m = 0; m < mounts.Length; m++)
        {
            PaintingMount mount = mounts[m];
            if (mount == null || mount.IsOccupied)
            {
                continue;
            }

            if (mount.RequiredWing != section.SectionWing)
            {
                continue;
            }

            for (int p = 0; p < paintings.Length; p++)
            {
                InteractablePainting painting = paintings[p];
                if (painting == null || painting.CurrentMount != null)
                {
                    continue;
                }

                if (!mount.CanAccept(painting))
                {
                    continue;
                }

                mount.PlacePainting(painting.transform, painting.GetComponent<Rigidbody>());
                break;
            }
        }
    }

    private static void AutoHangAllMatchingPaintings()
    {
        PaintingMount[] mounts = Object.FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        InteractablePainting[] paintings = Object.FindObjectsByType<InteractablePainting>(FindObjectsSortMode.None);

        for (int m = 0; m < mounts.Length; m++)
        {
            PaintingMount mount = mounts[m];
            if (mount == null || mount.IsOccupied)
            {
                continue;
            }

            for (int p = 0; p < paintings.Length; p++)
            {
                InteractablePainting painting = paintings[p];
                if (painting == null || painting.CurrentMount != null)
                {
                    continue;
                }

                if (!mount.CanAccept(painting))
                {
                    continue;
                }

                mount.PlacePainting(painting.transform, painting.GetComponent<Rigidbody>());
                break;
            }
        }

        Debug.Log("MuseumDevCheats: auto-hung all available matching paintings.");
    }
}
