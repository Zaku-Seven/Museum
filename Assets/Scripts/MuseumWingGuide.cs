using UnityEngine;

/// <summary>
/// Builds contextual hints while the player carries a painting (open mounts, wing labels).
/// </summary>
public static class MuseumWingGuide
{
    /// <summary>
    /// Counts empty mounts that would accept <paramref name="painting"/>.
    /// </summary>
    public static int CountAcceptingMounts(InteractablePainting painting, PaintingMount[] mounts)
    {
        if (painting == null || mounts == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < mounts.Length; i++)
        {
            PaintingMount mount = mounts[i];
            if (mount != null && mount.CanAccept(painting))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Short HUD line, e.g. "2 open Modern mounts · Modern (South)".
    /// </summary>
    public static string BuildGuidance(InteractablePainting painting, PaintingMount[] mounts)
    {
        if (painting == null)
        {
            return string.Empty;
        }

        int open = CountAcceptingMounts(painting, mounts);
        string wingLabel = ResolveWingSectionLabel(painting.Wing);

        if (open == 0)
        {
            return $"{painting.Wing} gallery full — try another wing or take down a painting";
        }

        string mountWord = open == 1 ? "mount" : "mounts";
        return $"{open} open {painting.Wing} {mountWord} · {wingLabel}";
    }

    private static string ResolveWingSectionLabel(GalleryWing wing)
    {
        if (GallerySection.AllSections != null)
        {
            for (int i = 0; i < GallerySection.AllSections.Count; i++)
            {
                GallerySection section = GallerySection.AllSections[i];
                if (section != null && section.SectionWing == wing)
                {
                    return section.DisplayName;
                }
            }
        }

        return wing.ToString();
    }
}
