using UnityEngine;

/// <summary>
/// Loops optional museum ambience; uses a procedural hum when no clip is assigned.
/// </summary>
public class MuseumAmbienceController : MonoBehaviour
{
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private float ambienceVolume = 0.22f;
    [SerializeField] private bool useSynthesizedFallback = true;

    private AudioSource ambienceSource;

    private void Awake()
    {
        ambienceSource = CreateAmbienceSource();
    }

    private void Update()
    {
        if (ambienceSource == null)
        {
            return;
        }

        bool shouldPlay = !ShouldBlockAmbience();
        if (shouldPlay)
        {
            EnsurePlaying();
        }
        else if (ambienceSource.isPlaying)
        {
            ambienceSource.Stop();
        }

        ApplyVolume();
    }

    public void ApplyVolume()
    {
        if (ambienceSource == null)
        {
            return;
        }

        if (!PlayerSettingsStore.UseSynthesizedSfx && ambientClip == null)
        {
            ambienceSource.volume = 0f;
            return;
        }

        ambienceSource.volume = PlayerSettingsStore.MasterVolume * ambienceVolume;
    }

    private void EnsurePlaying()
    {
        AudioClip clip = ResolveAmbienceClip();
        if (clip == null)
        {
            return;
        }

        if (ambienceSource.clip != clip)
        {
            ambienceSource.clip = clip;
        }

        if (!ambienceSource.isPlaying)
        {
            ambienceSource.loop = true;
            ambienceSource.Play();
        }
    }

    private AudioClip ResolveAmbienceClip()
    {
        if (ambientClip != null)
        {
            return ambientClip;
        }

        if (!useSynthesizedFallback || !PlayerSettingsStore.UseSynthesizedSfx)
        {
            return null;
        }

        return MuseumProceduralSfx.GetAmbienceLoop();
    }

    private AudioSource CreateAmbienceSource()
    {
        Transform existing = transform.Find("AmbienceAudio");
        if (existing != null)
        {
            AudioSource existingSource = existing.GetComponent<AudioSource>();
            if (existingSource != null)
            {
                existingSource.playOnAwake = false;
                existingSource.spatialBlend = 0f;
                existingSource.loop = true;
                return existingSource;
            }
        }

        GameObject child = new GameObject("AmbienceAudio");
        child.transform.SetParent(transform, false);
        AudioSource source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.loop = true;
        return source;
    }

    private static bool ShouldBlockAmbience()
    {
        if (MuseumTimeScale.IsFrozen)
        {
            return true;
        }

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
