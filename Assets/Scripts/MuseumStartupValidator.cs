using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Logs actionable warnings at Play start when the scene was not fully wired.
/// </summary>
[DefaultExecutionOrder(-200)]
public class MuseumStartupValidator : MonoBehaviour
{
    [SerializeField] private bool logAsErrorWhenIncomplete;

    private void Awake()
    {
        Validate();
    }

    private void Validate()
    {
        int issues = 0;

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            LogIssue("Missing EventSystem — UI buttons will not work. Run Game → Setup Full Museum.");
            issues++;
        }

        if (FindFirstObjectByType<ArtPickup>() == null)
        {
            LogIssue("Missing ArtPickup on PlayerCamera.");
            issues++;
        }

        if (FindFirstObjectByType<MuseumSaveManager>() == null)
        {
            LogIssue("Missing MuseumSaveManager on Player.");
            issues++;
        }

        PaintingMount[] mounts = FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        if (mounts.Length < 15)
        {
            LogIssue($"Expected ~15 wall mounts for expanded museum; found {mounts.Length}. Run Game → Expand Museum Building.");
            issues++;
        }

        if (GallerySection.AllSections.Count < 5)
        {
            LogIssue($"Expected 5 gallery sections; found {GallerySection.AllSections.Count}. Run Game → Expand Museum Building.");
            issues++;
        }

        SortingTable[] tables = FindObjectsByType<SortingTable>(FindObjectsSortMode.None);
        if (tables.Length == 0)
        {
            LogIssue("No SortingTable — run Game → Setup Gallery Wings.");
            issues++;
        }

        if (issues == 0)
        {
            Debug.Log("MuseumStartupValidator: scene looks feature-complete. Remaining work is art/audio/visual polish.");
        }
    }

    private void LogIssue(string message)
    {
        if (logAsErrorWhenIncomplete)
        {
            Debug.LogError($"MuseumStartupValidator: {message}");
        }
        else
        {
            Debug.LogWarning($"MuseumStartupValidator: {message}");
        }
    }
}
