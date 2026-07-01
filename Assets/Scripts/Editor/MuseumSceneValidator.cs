using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

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
            warnings += WarnIfMissing<MountPlacementGhost>(cameraObject, report, "MountPlacementGhost");

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
            warnings += WarnIfMissing<MainMenuController>(player, report, "MainMenuController");
            warnings += WarnIfMissing<MuseumStatistics>(player, report, "MuseumStatistics");
            warnings += WarnIfMissing<FootstepController>(player, report, "FootstepController");
            warnings += WarnIfMissing<JumpLandAudio>(player, report, "JumpLandAudio");
            warnings += WarnIfMissing<WingCompassHud>(player, report, "WingCompassHud");
            warnings += WarnIfMissing<MuseumCelebrationFx>(player, report, "MuseumCelebrationFx");
            warnings += WarnIfMissing<MuseumAmbienceController>(player, report, "MuseumAmbienceController");
        }

        if (GameObject.Find("PlayerCamera")?.GetComponent<InspectZoomController>() == null)
        {
            report.AppendLine("WARN: PlayerCamera missing InspectZoomController (re-run Setup Full Museum).");
            warnings++;
        }

        if (GameObject.Find("MuseumAtmosphereVolume") == null)
        {
            report.AppendLine("INFO: No MuseumAtmosphereVolume — run Game → Setup Museum Atmosphere (optional).");
        }

        if (GameObject.Find("Environment")?.transform.Find("MuseumExpansion") == null)
        {
            report.AppendLine("INFO: No MuseumExpansion — run Game → Expand Museum Building (included in Setup Full Museum).");
        }

        if (GameObject.Find("Environment")?.transform.Find("MuseumExpansion/GalleryLighting") == null
            && GameObject.Find("Environment")?.transform.Find("MuseumExpansion")?.Find("GalleryLighting") == null)
        {
            report.AppendLine("INFO: No gallery accent lighting — run Game → Expand Museum Building.");
        }

        if (GameObject.Find("Environment")?.transform.Find("MuseumArchitecture") == null)
        {
            report.AppendLine("INFO: No MuseumArchitecture — run Game → Setup Museum Architecture (optional).");
        }

        FootstepSurface floorSurface = GameObject.Find("Floor")?.GetComponent<FootstepSurface>();
        if (floorSurface == null)
        {
            report.AppendLine("WARN: Floor missing FootstepSurface (re-run Setup Full Museum).");
            warnings++;
        }

        if (Object.FindFirstObjectByType<MuseumJournalController>() == null)
        {
            report.AppendLine("WARN: MuseumJournalController missing (re-run Setup Museum Gameplay).");
            warnings++;
        }

        GameObject hudCanvas = GameObject.Find("InteractionHUD");
        if (hudCanvas?.GetComponent<MuseumUiActions>() == null)
        {
            report.AppendLine("WARN: InteractionHUD missing MuseumUiActions (re-run Setup Museum Gameplay).");
            warnings++;
        }

        if (Object.FindFirstObjectByType<ControlsHelpController>() == null)
        {
            report.AppendLine("WARN: ControlsHelpController missing (re-run Setup Museum Gameplay).");
            warnings++;
        }

        PaintingMount[] mounts = Object.FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        if (mounts.Length == 0)
        {
            report.AppendLine("WARN: No PaintingMount instances in scene.");
            warnings++;
        }
        else if (mounts.Length < 15)
        {
            report.AppendLine($"INFO: Found {mounts.Length} mounts — expanded museum expects ~15 (run Expand Museum Building).");
        }
        else
        {
            warnings += ValidateMountSlots(mounts, report);
        }

        GallerySection[] sections = Object.FindObjectsByType<GallerySection>(FindObjectsSortMode.None);
        if (sections.Length == 0)
        {
            report.AppendLine("WARN: No GallerySection instances (run Setup Gallery Wings).");
            warnings++;
        }
        else if (sections.Length < 5)
        {
            report.AppendLine($"INFO: Found {sections.Length} gallery sections — expanded museum expects 5 (run Expand Museum Building).");
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
        else if (tables.Length == 1)
        {
            report.AppendLine("INFO: One sorting table — run Expand Museum Building for archive staging table.");
        }

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            report.AppendLine("WARN: Missing EventSystem — UI clicks will fail. Re-run Setup Museum Gameplay.");
            warnings++;
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

    private static int ValidateMountSlots(PaintingMount[] mounts, StringBuilder report)
    {
        int warnings = 0;
        for (int i = 0; i < mounts.Length; i++)
        {
            PaintingMount mount = mounts[i];
            if (mount == null || !mount.HasSpecificSlot)
            {
                continue;
            }

            if (!TryFindPaintingId(mount.RequiredPaintingId, out _))
            {
                report.AppendLine(
                    $"WARN: Mount {mount.name} requires painting id \"{mount.RequiredPaintingId}\" but no matching painting was found.");
                warnings++;
            }
        }

        return warnings;
    }

    private static bool TryFindPaintingId(string paintingId, out InteractablePainting painting)
    {
        painting = null;
        if (string.IsNullOrEmpty(paintingId))
        {
            return false;
        }

        InteractablePainting[] paintings = Object.FindObjectsByType<InteractablePainting>(FindObjectsSortMode.None);
        for (int i = 0; i < paintings.Length; i++)
        {
            InteractablePainting candidate = paintings[i];
            if (candidate == null)
            {
                continue;
            }

            if (string.Equals(paintingId, candidate.PaintingId, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(paintingId, candidate.SaveId, System.StringComparison.OrdinalIgnoreCase))
            {
                painting = candidate;
                return true;
            }
        }

        return false;
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
