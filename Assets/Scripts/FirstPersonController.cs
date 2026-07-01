using UnityEngine;

/// <summary>
/// First-person character controller using Unity's CharacterController (no Rigidbody).
/// Handles WASD movement, mouse look, gravity, jumping, sprint, and subtle head bob.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The camera transform used for mouse look (pitch) and movement direction.")]
    [SerializeField] private Transform playerCamera;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.45f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float gravity = -20f;

    [Header("Mouse Look")]
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    [Header("Head Bob")]
    [SerializeField] private float headBobAmount = 0.035f;
    [SerializeField] private float headBobFrequency = 11f;
    [SerializeField] private float sprintHeadBobMultiplier = 1.35f;

    private CharacterController characterController;
    private ArtPickup artPickup;
    private float verticalVelocity;
    private float pitch;
    private float mouseSensitivity;
    private float fieldOfView;
    private bool invertY;
    private Vector3 cameraRestLocalPos;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        if (playerCamera == null)
        {
            Debug.LogError("FirstPersonController: Player Camera reference is not assigned.", this);
            enabled = false;
            return;
        }

        artPickup = playerCamera.GetComponent<ArtPickup>();
        cameraRestLocalPos = playerCamera.localPosition;
        ApplyPlayerSettings();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pitch = playerCamera.localEulerAngles.x;
        if (pitch > 180f)
        {
            pitch -= 360f;
        }
    }

    public void ApplyPlayerSettings()
    {
        mouseSensitivity = PlayerSettingsStore.MouseSensitivity;
        fieldOfView = PlayerSettingsStore.FieldOfView;
        invertY = PlayerSettingsStore.InvertY;

        Camera cam = playerCamera != null ? playerCamera.GetComponent<Camera>() : null;
        if (cam != null)
        {
            cam.fieldOfView = fieldOfView;
        }
    }

    private void Update()
    {
        if (ShouldBlockGameplay())
        {
            ResetHeadBob();
            return;
        }

        HandleMouseLook();
        HandleMovement();
    }

    private void LateUpdate()
    {
        if (ShouldBlockGameplay())
        {
            return;
        }

        if (MuseumProgress.Instance != null && MuseumProgress.Instance.IsMuseumComplete
            && MuseumGameFlowController.Instance != null && MuseumGameFlowController.Instance.IsWinModalActive)
        {
            return;
        }

        ApplyHeadBob();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private bool ShouldBlockGameplay()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused)
        {
            return true;
        }

        if (MuseumGameFlowController.Instance != null && MuseumGameFlowController.Instance.BlockGameplayInput)
        {
            return true;
        }

        if (MainMenuController.Instance != null && MainMenuController.Instance.IsMainMenuOpen)
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

    private void HandleMouseLook()
    {
        Vector2 lookDelta = MuseumInput.LookDelta();
        if (lookDelta.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float mouseX = lookDelta.x * mouseSensitivity * 0.1f;
        float mouseY = lookDelta.y * mouseSensitivity * 0.1f;
        if (invertY)
        {
            mouseY = -mouseY;
        }

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        bool isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (isGrounded && MuseumInput.JumpPressedThisFrame())
        {
            verticalVelocity = jumpForce;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector2 moveInput = MuseumInput.MoveInput();
        float speed = moveSpeed;
        if (MuseumInput.IsSprinting() && moveInput.sqrMagnitude > 0.01f && isGrounded)
        {
            speed *= sprintMultiplier;
        }

        Vector3 forward = playerCamera.forward;
        Vector3 right = playerCamera.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized * speed;
        Vector3 velocity = moveDirection + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void ApplyHeadBob()
    {
        if (playerCamera == null)
        {
            return;
        }

        if (artPickup != null && artPickup.IsHolding)
        {
            ResetHeadBob();
            return;
        }

        if (!characterController.isGrounded)
        {
            ResetHeadBob();
            return;
        }

        Vector2 moveInput = MuseumInput.MoveInput();
        if (moveInput.sqrMagnitude < 0.05f)
        {
            ResetHeadBob();
            return;
        }

        float bobFrequency = headBobFrequency;
        float bobAmount = headBobAmount;
        if (MuseumInput.IsSprinting() && characterController.isGrounded)
        {
            bobFrequency *= sprintHeadBobMultiplier;
            bobAmount *= 1.12f;
        }

        float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmount;
        playerCamera.localPosition = cameraRestLocalPos + Vector3.up * bob;
    }

    private void ResetHeadBob()
    {
        if (playerCamera != null)
        {
            playerCamera.localPosition = cameraRestLocalPos;
        }
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        MuseumTimeScale.ForceUnfreeze();
    }
}
