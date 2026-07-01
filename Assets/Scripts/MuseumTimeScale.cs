using UnityEngine;

/// <summary>
/// Reference-counted gameplay freeze for menus and pause (Time.timeScale = 0).
/// Multiple overlays can nest safely (main menu + controls help).
/// </summary>
public static class MuseumTimeScale
{
    private static int freezeDepth;

    public static bool IsFrozen => freezeDepth > 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ResetOnSceneLoad()
    {
        freezeDepth = 0;
        Time.timeScale = 1f;
    }

    public static void PushFreeze()
    {
        freezeDepth++;
        Apply();
    }

    public static void PopFreeze()
    {
        freezeDepth = Mathf.Max(0, freezeDepth - 1);
        Apply();
    }

    public static void ForceUnfreeze()
    {
        freezeDepth = 0;
        Time.timeScale = 1f;
    }

    private static void Apply()
    {
        Time.timeScale = freezeDepth > 0 ? 0f : 1f;
    }
}
