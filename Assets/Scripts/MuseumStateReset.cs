using UnityEngine;

/// <summary>
/// Returns every painting, mount, and staging zone to its default layout.
/// </summary>
public static class MuseumStateReset
{
    public static void ResetMuseumScene(bool clearTutorialHints)
    {
        ClearAllMounts();
        ResetAllPaintingsToSpawn();
        ClearSortingTables();
        ClearPlayerCarry();

        if (GallerySection.AllSections != null)
        {
            for (int i = 0; i < GallerySection.AllSections.Count; i++)
            {
                GallerySection section = GallerySection.AllSections[i];
                section?.ForceRefreshCompletion();
            }
        }

        MuseumProgress.Instance?.ResetProgress();
        MuseumGameFlowController.Instance?.ExitWinModal();

        if (clearTutorialHints)
        {
            TutorialHints.ClearAllHints();
        }
    }

    private static void ClearAllMounts()
    {
        PaintingMount[] mounts = Object.FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        for (int i = 0; i < mounts.Length; i++)
        {
            PaintingMount mount = mounts[i];
            if (mount == null || !mount.IsOccupied)
            {
                continue;
            }

            InteractablePainting painting = mount.TakeDownPainting();
            if (painting != null)
            {
                painting.ResetToDefaultSpawn();
            }
        }
    }

    private static void ResetAllPaintingsToSpawn()
    {
        InteractablePainting[] paintings = Object.FindObjectsByType<InteractablePainting>(FindObjectsSortMode.None);
        for (int i = 0; i < paintings.Length; i++)
        {
            InteractablePainting painting = paintings[i];
            if (painting == null)
            {
                continue;
            }

            if (painting.CurrentMount != null)
            {
                painting.CurrentMount.TakeDownPainting();
            }

            SortingTable.UnregisterIfPresent(painting.transform);
            painting.ResetToDefaultSpawn();
        }
    }

    private static void ClearSortingTables()
    {
        SortingTable[] tables = Object.FindObjectsByType<SortingTable>(FindObjectsSortMode.None);
        for (int i = 0; i < tables.Length; i++)
        {
            tables[i]?.ClearAllStaged();
        }
    }

    private static void ClearPlayerCarry()
    {
        ArtPickup pickup = Object.FindFirstObjectByType<ArtPickup>();
        pickup?.ClearSession();
    }
}
