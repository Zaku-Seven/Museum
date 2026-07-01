using UnityEngine;

/// <summary>
/// Plays optional footstep one-shots while the player walks.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FootstepController : MonoBehaviour
{
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float stepInterval = 0.42f;
    [SerializeField] private float minMoveInput = 0.15f;

    private CharacterController characterController;
    private AudioSource audioSource;
    private float stepTimer;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<MuseumAudioDirector>() != null
            ? GetComponent<AudioSource>()
            : gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        if (footstepClip == null || characterController == null || !characterController.isGrounded)
        {
            return;
        }

        if (ShouldBlockFootsteps())
        {
            return;
        }

        if (MuseumInput.MoveInput().sqrMagnitude < minMoveInput * minMoveInput)
        {
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer > 0f)
        {
            return;
        }

        stepTimer = stepInterval;
        float pitch = Random.Range(0.92f, 1.08f);
        float volume = PlayerSettingsStore.MasterVolume * 0.35f;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(footstepClip, volume);
        audioSource.pitch = 1f;
    }

    private static bool ShouldBlockFootsteps()
    {
        if (MainMenuController.Instance != null && MainMenuController.Instance.IsMainMenuOpen)
        {
            return true;
        }

        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused)
        {
            return true;
        }

        if (MuseumGameFlowController.Instance != null && MuseumGameFlowController.Instance.IsWinModalActive)
        {
            return true;
        }

        return false;
    }
}
