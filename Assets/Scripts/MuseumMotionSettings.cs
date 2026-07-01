using UnityEngine;

/// <summary>
/// Accessibility helpers for reduced camera / UI motion.
/// </summary>
public static class MuseumMotionSettings
{
    public static bool ReduceMotion => PlayerSettingsStore.ReduceMotion;

    public static float Pulse01(float speed)
    {
        if (ReduceMotion)
        {
            return 1f;
        }

        return 0.5f + 0.5f * Mathf.Sin(Time.time * speed);
    }

    public static float ScaleMultiplier(float multiplier)
    {
        return ReduceMotion ? 1f : multiplier;
    }
}
