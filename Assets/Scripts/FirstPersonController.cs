using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First-person character controller using Unity's CharacterController (no Rigidbody).
/// Handles WASD movement, mouse look, gravity, jumping, and configurable FOV.
/// Uses the Input System package (this project's active input handler).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The camera transform used for mouse look (pitch) and movement direction.")]
    [SerializeField] private Transform playerCamera;

    [Header("Movement")]
    [Tooltip("Horizontal movement speed in units per second.")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Upward velocity applied when the player jumps.")]
    [SerializeField] private float jumpForce = 6f;

    [Tooltip("Downward acceleration while airborne or grounded.")]
    [SerializeField] private float gravity = -20f;

    [Header("Mouse Look")]
    [Tooltip("Multiplier applied to mouse delta each frame.")]
    [SerializeField] private float mouseSensitivity = 2f;

    [Tooltip("Minimum vertical look angle (looking down).")]
    [SerializeField] private float minPitch = -85f;

    [Tooltip("Maximum vertical look angle (looking up).")]
    [SerializeField] private float maxPitch = 85f;

    [Header("Camera")]
    [Tooltip("Field of view applied to the player camera on start.")]
    [SerializeField] private float fieldOfView = 75f;

    private CharacterController characterController;
    private float verticalVelocity;
    private float pitch;
    private float mouseSensitivity;
    private float fieldOfView;
    private bool invertY;

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

        ApplyPlayerSettings();

        // Lock and hide the cursor for standard FPS controls.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize pitch from the camera's current local rotation.
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
            return;
        }

        HandleMouseLook();
        HandleMovement();
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

        return false;
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Rotates the body on Y (yaw) and the camera on X (pitch), with pitch clamping.
    /// </summary>
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

        // Yaw rotates the entire player body left/right.
        transform.Rotate(Vector3.up * mouseX);

        // Pitch rotates only the camera up/down, clamped to prevent over-rotation.
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    /// <summary>
    /// Applies WASD movement relative to the camera's forward/right on the XZ plane,
    /// plus gravity and jumping via CharacterController.Move.
    /// </summary>
    private void HandleMovement()
    {
        bool isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (isGrounded && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity = jumpForce;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector2 moveInput = MuseumInput.MoveInput();
        float horizontal = moveInput.x;
        float vertical = moveInput.y;

        Vector3 forward = playerCamera.forward;
        Vector3 right = playerCamera.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized * moveSpeed;
        Vector3 velocity = moveDirection + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
