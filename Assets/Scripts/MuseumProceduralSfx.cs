using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tiny synthesized one-shots when real AudioClips are not assigned yet.
/// Lets Play mode feel responsive before the visual/audio polish pass.
/// </summary>
public static class MuseumProceduralSfx
{
    public enum SfxKind
    {
        Pickup,
        Place,
        Drop,
        Wrong,
        SectionComplete,
        MuseumWin,
        Undo,
        Throw,
        Stage,
        UiClick,
        Footstep,
        FootstepB,
        FootstepC,
        FootstepWood,
        FootstepCarpet,
        Jump,
        Land,
        SprintStart,
        AmbienceLoop
    }

    private static readonly Dictionary<SfxKind, AudioClip> Cache = new Dictionary<SfxKind, AudioClip>();
    private static AudioClip ambienceLoopClip;

    public static AudioClip ResolveOrFallback(AudioClip clip, SfxKind kind)
    {
        if (clip != null)
        {
            return clip;
        }

        return PlayerSettingsStore.UseSynthesizedSfx ? Get(kind) : null;
    }

    public static AudioClip GetRandomFootstepVariant()
    {
        int pick = Random.Range(0, 3);
        SfxKind kind = pick switch
        {
            1 => SfxKind.FootstepB,
            2 => SfxKind.FootstepC,
            _ => SfxKind.Footstep
        };

        return Get(kind);
    }

    public static AudioClip GetAmbienceLoop()
    {
        if (ambienceLoopClip != null)
        {
            return ambienceLoopClip;
        }

        ambienceLoopClip = CreateAmbienceLoop();
        return ambienceLoopClip;
    }

    public static float ComputeSprintStepInterval(float baseInterval, float sprintCadenceMultiplier, bool isSprinting)
    {
        if (baseInterval <= 0f)
        {
            return baseInterval;
        }

        return isSprinting ? baseInterval / Mathf.Max(1f, sprintCadenceMultiplier) : baseInterval;
    }

    public static AudioClip Get(SfxKind kind)
    {
        if (Cache.TryGetValue(kind, out AudioClip cached) && cached != null)
        {
            return cached;
        }

        AudioClip clip = kind switch
        {
            SfxKind.Pickup => CreateTone("sfx_pickup", 620f, 0.07f, 0.22f),
            SfxKind.Place => CreateTone("sfx_place", 420f, 0.11f, 0.28f, secondFreq: 840f),
            SfxKind.Drop => CreateTone("sfx_drop", 280f, 0.09f, 0.2f),
            SfxKind.Wrong => CreateTone("sfx_wrong", 140f, 0.14f, 0.35f, wave: Wave.Square),
            SfxKind.SectionComplete => CreateTone("sfx_section", 523f, 0.22f, 0.25f, secondFreq: 784f),
            SfxKind.MuseumWin => CreateTone("sfx_win", 440f, 0.35f, 0.3f, secondFreq: 660f, thirdFreq: 880f),
            SfxKind.Undo => CreateTone("sfx_undo", 360f, 0.08f, 0.18f, pitchSlide: -80f),
            SfxKind.Throw => CreateTone("sfx_throw", 300f, 0.06f, 0.16f, pitchSlide: 120f),
            SfxKind.Stage => CreateTone("sfx_stage", 500f, 0.1f, 0.22f, secondFreq: 750f),
            SfxKind.UiClick => CreateTone("sfx_ui", 880f, 0.04f, 0.15f),
            SfxKind.Footstep => CreateTone("sfx_step", 90f, 0.05f, 0.12f, wave: Wave.Noise),
            SfxKind.FootstepB => CreateTone("sfx_step_b", 75f, 0.055f, 0.11f, wave: Wave.Noise, pitchSlide: -15f),
            SfxKind.FootstepC => CreateTone("sfx_step_c", 105f, 0.045f, 0.13f, wave: Wave.Noise, pitchSlide: 20f),
            SfxKind.FootstepWood => CreateTone("sfx_step_wood", 65f, 0.06f, 0.14f, wave: Wave.Noise, secondFreq: 130f),
            SfxKind.FootstepCarpet => CreateTone("sfx_step_carpet", 45f, 0.07f, 0.09f, wave: Wave.Noise),
            SfxKind.Jump => CreateTone("sfx_jump", 520f, 0.06f, 0.14f, pitchSlide: 100f),
            SfxKind.Land => CreateTone("sfx_land", 110f, 0.05f, 0.16f, wave: Wave.Noise),
            SfxKind.SprintStart => CreateTone("sfx_sprint", 180f, 0.05f, 0.1f, wave: Wave.Noise, pitchSlide: 60f),
            _ => CreateTone("sfx_default", 440f, 0.05f, 0.1f)
        };

        Cache[kind] = clip;
        return clip;
    }

    private enum Wave
    {
        Sine,
        Square,
        Noise
    }

    private static AudioClip CreateTone(
        string name,
        float frequency,
        float durationSeconds,
        float volume,
        float secondFreq = 0f,
        float thirdFreq = 0f,
        float pitchSlide = 0f,
        Wave wave = Wave.Sine)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSeconds));
        var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        var data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float progress = i / (float)sampleCount;
            float freq = frequency + pitchSlide * progress;
            float sample = wave switch
            {
                Wave.Square => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * 0.55f,
                Wave.Noise => Random.Range(-1f, 1f) * 0.35f,
                _ => Mathf.Sin(2f * Mathf.PI * freq * t)
            };

            if (secondFreq > 0f)
            {
                sample += Mathf.Sin(2f * Mathf.PI * secondFreq * t) * 0.35f;
            }

            if (thirdFreq > 0f)
            {
                sample += Mathf.Sin(2f * Mathf.PI * thirdFreq * t) * 0.2f;
            }

            float envelope = Mathf.Clamp01(1f - progress);
            data[i] = sample * volume * envelope;
        }

        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateAmbienceLoop()
    {
        const int sampleRate = 44100;
        const float durationSeconds = 12f;
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSeconds));
        var clip = AudioClip.Create("sfx_ambience_loop", sampleCount, 1, sampleRate, false);
        var data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float cycle = t / durationSeconds * Mathf.PI * 2f;
            float hum = Mathf.Sin(cycle * 0.5f) * 0.06f + Mathf.Sin(cycle * 1.7f) * 0.03f;
            float air = (Mathf.PerlinNoise(t * 0.25f, 0.12f) - 0.5f) * 0.04f;
            data[i] = hum + air;
        }

        clip.SetData(data, 0);
        return clip;
    }
}
