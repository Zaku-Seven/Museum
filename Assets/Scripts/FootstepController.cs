using UnityEngine;

/// <summary>
/// Plays optional footstep one-shots while the player walks (dedicated AudioSource).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FootstepController : MonoBehaviour
{
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip[] footstepVariants;
    [SerializeField] private float stepInterval = 0.42f;
    [SerializeField] private float sprintCadenceMultiplier = 1.45f;
    [SerializeField] private float minMoveInput = 0.15f;
    [SerializeField] private bool useSynthesizedFallback = true;

    private CharacterController characterController;
    private AudioSource footstepSource;
    private float stepTimer;
    private bool wasSprinting;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        footstepSource = CreateFootstepSource();
    }

    private AudioSource CreateFootstepSource()
    {
        Transform existing = transform.Find("FootstepAudio");
        if (existing != null)
        {
            AudioSource existingSource = existing.GetComponent<AudioSource>();
            if (existingSource != null)
            {
                return existingSource;
            }
        }

        GameObject child = new GameObject("FootstepAudio");
        child.transform.SetParent(transform, false);
        AudioSource source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        return source;
    }

    private void Update()
    {
        if (characterController == null || !characterController.isGrounded)
        {
            wasSprinting = false;
            return;
        }

        if (ShouldBlockFootsteps())
        {
            wasSprinting = false;
            return;
        }

        Vector2 moveInput = MuseumInput.MoveInput();
        if (moveInput.sqrMagnitude < minMoveInput * minMoveInput)
        {
            wasSprinting = false;
            return;
        }

        bool isSprinting = MuseumInput.IsSprinting();
        if (isSprinting && !wasSprinting)
        {
            PlaySprintStart();
        }

        wasSprinting = isSprinting;

        AudioClip clip = ResolveFootstepClip();
        if (clip == null)
        {
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer > 0f)
        {
            return;
        }

        stepTimer = MuseumProceduralSfx.ComputeSprintStepInterval(stepInterval, sprintCadenceMultiplier, isSprinting);
        float pitchMin = isSprinting ? 0.88f : 0.92f;
        float pitchMax = isSprinting ? 1.12f : 1.08f;
        float pitch = Random.Range(pitchMin, pitchMax);
        float volumeScale = isSprinting ? 0.48f : 0.35f;
        float volume = PlayerSettingsStore.MasterVolume * volumeScale;
        footstepSource.pitch = pitch;
        footstepSource.PlayOneShot(clip, volume);
        footstepSource.pitch = 1f;
    }

    private void PlaySprintStart()
    {
        if (footstepSource == null || !useSynthesizedFallback || !PlayerSettingsStore.UseSynthesizedSfx)
        {
            return;
        }

        AudioClip clip = MuseumProceduralSfx.ResolveOrFallback(null, MuseumProceduralSfx.SfxKind.SprintStart);
        if (clip == null)
        {
            return;
        }

        footstepSource.PlayOneShot(clip, PlayerSettingsStore.MasterVolume * 0.25f);
    }

    private AudioClip ResolveFootstepClip()
    {
        if (footstepVariants != null && footstepVariants.Length > 0)
        {
            for (int attempt = 0; attempt < footstepVariants.Length; attempt++)
            {
                AudioClip candidate = footstepVariants[Random.Range(0, footstepVariants.Length)];
                if (candidate != null)
                {
                    return candidate;
                }
            }
        }

        if (footstepClip != null)
        {
            return footstepClip;
        }

        return useSynthesizedFallback && PlayerSettingsStore.UseSynthesizedSfx
            ? MuseumProceduralSfx.GetRandomFootstepVariant()
            : null;
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

        if (MuseumJournalController.Instance != null && MuseumJournalController.Instance.IsOpen)
        {
            return true;
        }

        if (ControlsHelpController.Instance != null && ControlsHelpController.Instance.IsOpen)
        {
            return true;
        }

        return false;
    }
}
