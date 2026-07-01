using System.Text;
using UnityEngine;

/// <summary>
/// Builds a curator journal listing every painting and its current state.
/// </summary>
public static class MuseumCollectionLog
{
    private static readonly StringBuilder Builder = new StringBuilder(512);

    public static string BuildJournalText()
    {
        Builder.Clear();
        Builder.AppendLine("COLLECTION LOG");
        Builder.AppendLine(MuseumHangProgress.BuildSummaryLine());
        Builder.AppendLine();

        foreach (GalleryWing wing in System.Enum.GetValues(typeof(GalleryWing)))
        {
            AppendWingSection(wing);
        }

        return Builder.ToString().TrimEnd();
    }

    private static void AppendWingSection(GalleryWing wing)
    {
        InteractablePainting[] paintings = Object.FindObjectsByType<InteractablePainting>(FindObjectsSortMode.None);
        bool any = false;

        for (int i = 0; i < paintings.Length; i++)
        {
            InteractablePainting painting = paintings[i];
            if (painting == null || painting.Wing != wing)
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
