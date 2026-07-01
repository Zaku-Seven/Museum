using System.Text;
using UnityEngine;

/// <summary>
/// Builds next-step guidance and per-wing progress summaries for the HUD.
/// </summary>
public static class MuseumObjectiveGuide
{
    private static readonly StringBuilder Builder = new StringBuilder(256);

    public static string BuildNextObjectiveLine(ArtPickup artPickup)
    {
        if (MuseumProgress.Instance != null && MuseumProgress.Instance.IsMuseumComplete)
        {
            return string.Empty;
        }

        if (artPickup != null && artPickup.IsHolding && artPickup.HeldPainting != null)
        {
            InteractablePainting held = artPickup.HeldPainting;
            int open = MuseumWingGuide.CountAcceptingMounts(held, Object.FindObjectsByType<PaintingMount>(FindObjectsSortMode.None));
            if (open > 0)
            {
                string section = ResolveWingSectionLabel(held.Wing);
                return $"Next: Hang \"{held.PaintingTitle}\" on the {held.Wing} wall ({section})";
            }

            return $"Next: \"{held.PaintingTitle}\" needs another {held.Wing} mount — try a different wing";
        }

        InteractablePainting loose = FindPriorityLoosePainting();
        if (loose != null)
        {
            return $"Next: Pick up \"{loose.PaintingTitle}\" ({loose.Wing}) from the floor or sorting table";
        }

        GallerySection incomplete = FindFirstIncompleteSection();
        if (incomplete != null)
        {
            return $"Next: Finish {incomplete.DisplayName} ({incomplete.CorrectlyFilledCount()}/{incomplete.MountCount} hung)";
        }

        return "Next: Explore the wings and hang every painting.";
    }

    public static string BuildWingProgressBreakdown()
    {
        if (GallerySection.AllSections == null || GallerySection.AllSections.Count == 0)
        {
            return string.Empty;
        }

        Builder.Clear();
        for (int i = 0; i < GallerySection.AllSections.Count; i++)
        {
            GallerySection section = GallerySection.AllSections[i];
            if (section == null)
            {
                continue;
            }

            if (Builder.Length > 0)
            {
                Builder.Append("  ·  ");
            }

            Builder.Append(section.DisplayName)
                .Append(' ')
                .Append(section.CorrectlyFilledCount())
                .Append('/')
                .Append(section.MountCount);

            if (section.IsComplete)
            {
                Builder.Append(" ✓");
            }
        }

        return Builder.ToString();
    }

    private static InteractablePainting FindPriorityLoosePainting()
    {
        GallerySection targetSection = FindFirstIncompleteSection();
        GalleryWing preferredWing = targetSection != null ? targetSection.SectionWing : GalleryWing.Modern;

        InteractablePainting[] paintings = Object.FindObjectsByType<InteractablePainting>(FindObjectsSortMode.None);
        InteractablePainting fallback = null;

        for (int i = 0; i < paintings.Length; i++)
        {
            InteractablePainting painting = paintings[i];
            if (painting == null || painting.CurrentMount != null)
            {
                continue;
            }

            if (painting.Wing == preferredWing)
            {
                return painting;
            }

            fallback ??= painting;
        }

        return fallback;
    }

    private static GallerySection FindFirstIncompleteSection()
    {
        if (GallerySection.AllSections == null)
        {
            return null;
        }

        for (int i = 0; i < GallerySection.AllSections.Count; i++)
        {
            GallerySection section = GallerySection.AllSections[i];
            if (section != null && !section.IsComplete)
            {
                return section;
            }
        }

        return null;
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
