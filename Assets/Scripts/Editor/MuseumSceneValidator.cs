using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Read-only scene sanity check for the museum sorting prototype.
/// </summary>
public static class MuseumSceneValidator
{
    [MenuItem("Game/Validate Museum Scene")]
    public static void ValidateFromMenu()
    {
        var report = new StringBuilder();
        int errors = 0;
        int warnings = 0;

        report.AppendLine("Museum scene validation");

        GameObject player = GameObject.Find("Player");
        GameObject cameraObject = GameObject.Find("PlayerCamera");
        if (player == null)
        {
            report.AppendLine("ERROR: Player not found.");
            errors++;
        }

        if (cameraObject == null)
        {
            report.AppendLine("ERROR: PlayerCamera not found.");
            errors++;
        }
        else
        {
            errors += RequireComponent<ArtPickup>(cameraObject, report);
            errors += RequireComponent<InteractionHUD>(cameraObject, report);
            errors += RequireComponent<MountAimHighlighter>(cameraObject, report);

            Transform holdPoint = cameraObject.transform.Find("HoldPoint");
            if (holdPoint == null)
            {
                report.AppendLine("WARN: HoldPoint missing on PlayerCamera (run Fix Carry Settings).");
                warnings++;
            }
        }

        if (player != null)
        {
            errors += RequireComponent<FirstPersonController>(player, report);
            warnings += WarnIfMissing<PauseMenuController>(player, report, "PauseMenuController");
            warnings += WarnIfMissing<MuseumProgress>(player, report, "MuseumProgress");
            warnings += WarnIfMissing<MuseumSaveManager>(player, report, "MuseumSaveManager");
            warnings += WarnIfMissing<MuseumGameFlowController>(player, report, "MuseumGameFlowController");
            warnings += WarnIfMissing<MuseumAudioDirector>(player, report, "MuseumAudioDirector");
            warnings += WarnIfMissing<SettingsMenuController>(player, report, "SettingsMenuController");
        }

        if (GameObject.Find("InteractionHUD")?.GetComponent<MuseumUiActions>() == null)
        {
            report.AppendLine("WARN: InteractionHUD missing MuseumUiActions (re-run Setup Museum Gameplay).");
            warnings++;
        }

        PaintingMount[] mounts = Object.FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        if (mounts.Length == 0)
        {
            report.AppendLine("WARN: No PaintingMount instances in scene.");
            warnings++;
        }

        GallerySection[] sections = Object.FindObjectsByType<GallerySection>(FindObjectsSortMode.None);
        if (sections.Length == 0)
        {
            report.AppendLine("WARN: No GallerySection instances (run Setup Gallery Wings).");
            warnings++;
        }

        InteractablePainting[] paintings = Object.FindObjectsByType<InteractablePainting>(FindObjectsSortMode.None);
        if (paintings.Length == 0)
        {
            report.AppendLine("WARN: No InteractablePainting instances.");
            warnings++;
        }

        MuseumEntityId[] entityIds = Object.FindObjectsByType<MuseumEntityId>(FindObjectsSortMode.None);
        if (entityIds.Length < paintings.Length)
        {
            report.AppendLine("WARN: Some paintings/mounts lack MuseumEntityId (re-run Setup Full Museum).");
            warnings++;
        }

        SortingTable[] tables = Object.FindObjectsByType<SortingTable>(FindObjectsSortMode.None);
        if (tables.Length == 0)
        {
            report.AppendLine("INFO: No SortingTable (optional until Gallery Wings runs).");
        }

        report.AppendLine($"Summary: {errors} error(s), {warnings} warning(s).");

        if (errors > 0)
        {
            Debug.LogError(report.ToString());
        }
        else if (warnings > 0)
        {
            Debug.LogWarning(report.ToString());
        }
        else
        {
            Debug.Log(report.ToString());
        }
    }

    private static int RequireComponent<T>(GameObject target, StringBuilder report) where T : Component
    {
        if (target.GetComponent<T>() == null)
        {
            report.AppendLine($"ERROR: {target.name} missing {typeof(T).Name}.");
            return 1;
        }

        return 0;
    }

    private static int WarnIfMissing<T>(GameObject target, StringBuilder report, string label) where T : Component
    {
        if (target.GetComponent<T>() == null)
        {
            report.AppendLine($"WARN: {target.name} missing {label}.");
            return 1;
        }

        return 0;
    }
}
