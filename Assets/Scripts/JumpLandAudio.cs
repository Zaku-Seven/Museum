using UnityEngine;

/// <summary>
/// Optional jump / land one-shots on the player.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class JumpLandAudio : MonoBehaviour
{
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private float landClipMinFallSpeed = 4f;

    private CharacterController characterController;
    private AudioSource audioSource;
    private bool wasGrounded = true;

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
        if (characterController == null || ShouldBlock())
        {
            return;
        }

        bool grounded = characterController.isGrounded;
        float verticalVelocity = characterController.velocity.y;

        if (!wasGrounded && grounded && verticalVelocity <= -landClipMinFallSpeed)
        {
            PlayOneShot(landClip, 0.45f);
        }

        if (wasGrounded && MuseumInput.JumpPressedThisFrame())
        {
            PlayOneShot(jumpClip, 0.55f);
        }

        wasGrounded = grounded;
    }

    private static bool ShouldBlock()
    {
        return MuseumTimeScale.IsFrozen
            || (MainMenuController.Instance != null && MainMenuController.Instance.IsMainMenuOpen)
            || (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused);
    }

    private void PlayOneShot(AudioClip clip, float volumeScale)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, PlayerSettingsStore.MasterVolume * volumeScale);
    }
}
