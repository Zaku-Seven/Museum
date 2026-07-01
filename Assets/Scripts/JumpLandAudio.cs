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
    [SerializeField] private bool useSynthesizedFallback = true;

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
            PlayOneShot(ResolveClip(landClip, MuseumProceduralSfx.SfxKind.Land), 0.45f);
        }

        if (wasGrounded && MuseumInput.JumpPressedThisFrame())
        {
            PlayOneShot(ResolveClip(jumpClip, MuseumProceduralSfx.SfxKind.Jump), 0.55f);
        }

        wasGrounded = grounded;
    }

    private static bool ShouldBlock()
    {
        return MuseumTimeScale.IsFrozen
            || (MainMenuController.Instance != null && MainMenuController.Instance.IsMainMenuOpen)
            || (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused);
    }

    private AudioClip ResolveClip(AudioClip clip, MuseumProceduralSfx.SfxKind kind)
    {
        if (clip != null)
        {
            return clip;
        }

        return useSynthesizedFallback ? MuseumProceduralSfx.ResolveOrFallback(null, kind) : null;
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
