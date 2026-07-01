using UnityEngine;

/// <summary>
/// Dev-only on-screen stats (toggle via <see cref="MuseumDevCheats"/> F7).
/// </summary>
public class MuseumDebugOverlay : MonoBehaviour
{
    public static MuseumDebugOverlay Instance { get; private set; }

    public bool IsVisible { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Toggle()
    {
        IsVisible = !IsVisible;
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;
    }

    private void OnGUI()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        return;
#else
        if (!IsVisible)
        {
            return;
        }

        const int width = 420;
        const int height = 120;
        var rect = new Rect(12f, 12f, width, height);
        GUI.Box(rect, "Museum debug (F7)");

        GUILayout.BeginArea(new Rect(rect.x + 10f, rect.y + 24f, width - 20f, height - 34f));
        GUILayout.Label(MuseumHangProgress.BuildSummaryLine());
        if (MuseumStatistics.Instance != null)
        {
            GUILayout.Label(MuseumStatistics.Instance.BuildWinStatsLine());
        }

        GallerySection incomplete = FindFirstIncompleteSection();
        if (incomplete != null)
        {
            GUILayout.Label($"Next wing: {incomplete.DisplayName}");
        }
        else
        {
            GUILayout.Label("All sections complete");
        }

        GUILayout.EndArea();
#endif
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
}
