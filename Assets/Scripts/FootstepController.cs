using UnityEngine;

/// <summary>
/// Plays optional footstep one-shots while the player walks (dedicated AudioSource).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FootstepController : MonoBehaviour
{
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float stepInterval = 0.42f;
    [SerializeField] private float minMoveInput = 0.15f;
    [SerializeField] private bool useSynthesizedFallback = true;

    private CharacterController characterController;
    private AudioSource footstepSource;
    private float stepTimer;

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
            return;
        }

        AudioClip clip = ResolveFootstepClip();
        if (clip == null)
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
        footstepSource.pitch = pitch;
        footstepSource.PlayOneShot(clip, volume);
        footstepSource.pitch = 1f;
    }

    private AudioClip ResolveFootstepClip()
    {
        if (footstepClip != null)
        {
            return footstepClip;
        }

        return useSynthesizedFallback
            ? MuseumProceduralSfx.ResolveOrFallback(null, MuseumProceduralSfx.SfxKind.Footstep)
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

        return false;
    }
}
