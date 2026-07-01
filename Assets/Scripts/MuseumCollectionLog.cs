using System.Text;
using UnityEngine;

/// <summary>
/// Builds a curator journal listing every painting and its current state.
/// </summary>
public static class MuseumCollectionLog
{
    private static readonly StringBuilder Builder = new StringBuilder(512);

    public static string BuildJournalText(MuseumJournalFilter filter = MuseumJournalFilter.All)
    {
        Builder.Clear();
        Builder.AppendLine("COLLECTION LOG");
        Builder.AppendLine(MuseumHangProgress.BuildSummaryLine());
        Builder.AppendLine($"Filter: {FormatFilterLabel(filter)}");
        Builder.AppendLine();

        foreach (GalleryWing wing in System.Enum.GetValues(typeof(GalleryWing)))
        {
            AppendWingSection(wing, filter);
        }

        return Builder.ToString().TrimEnd();
    }

    public static bool MatchesFilter(InteractablePainting painting, MuseumJournalFilter filter)
    {
        if (painting == null)
        {
            return false;
        }

        return filter switch
        {
            MuseumJournalFilter.Unhung => painting.CurrentMount == null,
            MuseumJournalFilter.Hung => painting.CurrentMount != null,
            MuseumJournalFilter.Staged => SortingTable.IsStaged(painting.transform),
            _ => true
        };
    }

    private static string FormatFilterLabel(MuseumJournalFilter filter)
    {
        return filter switch
        {
            MuseumJournalFilter.Unhung => "Unhung only",
            MuseumJournalFilter.Hung => "Hung only",
            MuseumJournalFilter.Staged => "Sorting table",
            _ => "All paintings"
        };
    }

    private static void AppendWingSection(GalleryWing wing, MuseumJournalFilter filter)
    {
        InteractablePainting[] paintings = Object.FindObjectsByType<InteractablePainting>(FindObjectsSortMode.None);
        bool any = false;

        for (int i = 0; i < paintings.Length; i++)
        {
            InteractablePainting painting = paintings[i];
            if (painting == null || painting.Wing != wing || !MatchesFilter(painting, filter))
            {
                continue;
            }

            if (!any)
            {
                Builder.AppendLine($"{wing.ToString().ToUpper()}");
                any = true;
            }

            Builder.Append("  ").Append(GetStatusGlyph(painting)).Append(' ')
                .Append('"').Append(painting.PaintingTitle).Append('"')
                .Append(" — ").AppendLine(DescribeLocation(painting));
        }

        if (any)
        {
            Builder.AppendLine();
        }
    }

    private static char GetStatusGlyph(InteractablePainting painting)
    {
        if (painting.CurrentMount != null)
        {
            return painting.CurrentMount.IsCorrectlyOccupied() ? '✓' : '!';
        }

        if (SortingTable.IsStaged(painting.transform))
        {
            return '◆';
        }

        return '○';
    }

    private static string DescribeLocation(InteractablePainting painting)
    {
        if (painting.CurrentMount != null)
        {
            if (painting.CurrentMount.IsCorrectlyOccupied())
            {
                return "hung";
            }

            return "wrong mount";
        }

        if (SortingTable.IsStaged(painting.transform))
        {
            return "sorting table";
        }

        return "floor / loose";
    }
}
