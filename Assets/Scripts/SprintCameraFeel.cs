using UnityEngine;

/// <summary>
/// Subtle FOV widen while sprinting; yields to <see cref="ThrowCameraKick"/> during reject/throw punches.
/// </summary>
[DefaultExecutionOrder(50)]
public class SprintCameraFeel : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float sprintFovBoost = 4f;
    [SerializeField] private float fovLerpSpeed = 10f;

    private ThrowCameraKick throwKick;
    private CharacterController characterController;

    public bool IsSprintFovActive { get; private set; }

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        throwKick = GetComponent<ThrowCameraKick>();
        characterController = GetComponentInParent<CharacterController>();
    }

    private void LateUpdate()
    {
        if (targetCamera == null || throwKick != null && throwKick.IsKickActive)
        {
            return;
        }

        if (ShouldBlock())
        {
            targetCamera.fieldOfView = PlayerSettingsStore.FieldOfView;
            return;
        }

        float baseFov = PlayerSettingsStore.FieldOfView;
        float targetFov = baseFov;
        if (IsSprintMoving())
        {
            targetFov += sprintFovBoost;
        }

        IsSprintFovActive = targetFov > baseFov + 0.01f;

        if (MuseumMotionSettings.ReduceMotion)
        {
            targetCamera.fieldOfView = baseFov;
            IsSprintFovActive = false;
            return;
        }

        targetCamera.fieldOfView = Mathf.Lerp(
            targetCamera.fieldOfView,
            targetFov,
            Time.deltaTime * fovLerpSpeed);
    }

    private bool IsSprintMoving()
    {
        if (characterController == null || !characterController.isGrounded)
        {
            return false;
        }

        if (!MuseumInput.IsSprinting())
        {
            return false;
        }

        return MuseumInput.MoveInput().sqrMagnitude > 0.05f;
    }

    private static bool ShouldBlock()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused)
        {
            return true;
        }

        if (MainMenuController.Instance != null && MainMenuController.Instance.IsMainMenuOpen)
        {
            return true;
        }

        if (MuseumGameFlowController.Instance != null && MuseumGameFlowController.Instance.BlockGameplayInput)
        {
            return true;
        }

        if (ControlsHelpController.Instance != null && ControlsHelpController.Instance.IsOpen)
        {
            return true;
        }

        if (MuseumJournalController.Instance != null && MuseumJournalController.Instance.IsOpen)
        {
            return true;
        }

        return false;
    }
}
