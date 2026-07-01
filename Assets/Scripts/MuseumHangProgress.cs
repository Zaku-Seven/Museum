using UnityEngine;

/// <summary>
/// Museum-wide hang progress across every wall mount (slot-aware).
/// </summary>
public static class MuseumHangProgress
{
    public static int TotalMountSlots
    {
        get
        {
            PaintingMount[] mounts = Object.FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
            return mounts != null ? mounts.Length : 0;
        }
    }

    public static int CorrectlyHungCount
    {
        get
        {
            PaintingMount[] mounts = Object.FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
            if (mounts == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < mounts.Length; i++)
            {
                PaintingMount mount = mounts[i];
                if (mount != null && mount.IsCorrectlyOccupied())
                {
                    count++;
                }
            }

            return count;
        }
    }

    public static float Completion01
    {
        get
        {
            int total = TotalMountSlots;
            if (total <= 0)
            {
                return 0f;
            }

            return CorrectlyHungCount / (float)total;
        }
    }

    public static int CompletionPercent => Mathf.RoundToInt(Completion01 * 100f);

    public static string BuildSummaryLine()
    {
        int total = TotalMountSlots;
        if (total <= 0)
        {
            return string.Empty;
        }

        return $"{CorrectlyHungCount}/{total} mounts ({CompletionPercent}%)";
    }
}
