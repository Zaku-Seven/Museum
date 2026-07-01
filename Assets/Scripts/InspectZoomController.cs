using UnityEngine;

/// <summary>
/// Narrows FOV while holding inspect (Tab / View). Yields to throw kicks and sprint widen.
/// </summary>
[DefaultExecutionOrder(45)]
public class InspectZoomController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float inspectFovDelta = -8f;
    [SerializeField] private float zoomLerpSpeed = 12f;

    private ThrowCameraKick throwKick;
    private SprintCameraFeel sprintFeel;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        throwKick = GetComponent<ThrowCameraKick>();
        sprintFeel = GetComponent<SprintCameraFeel>();
    }

    private void LateUpdate()
    {
        if (targetCamera == null || throwKick != null && throwKick.IsKickActive)
        {
            return;
        }

        if (ShouldBlock())
        {
            return;
        }

        if (!MuseumInput.IsInspecting())
        {
            return;
        }

        float baseFov = PlayerSettingsStore.FieldOfView;
        if (sprintFeel != null && sprintFeel.IsSprintFovActive)
        {
            baseFov += 4f;
        }

        float target = baseFov + inspectFovDelta;
        targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, target, Time.deltaTime * zoomLerpSpeed);
    }

    private static bool ShouldBlock()
    {
        return MainMenuController.Instance != null && MainMenuController.Instance.IsMainMenuOpen
            || PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused
            || MuseumJournalController.Instance != null && MuseumJournalController.Instance.IsOpen
            || ControlsHelpController.Instance != null && ControlsHelpController.Instance.IsOpen;
    }
}
