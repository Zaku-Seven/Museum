using System;
using UnityEngine;

/// <summary>
/// One-shot tutorial lines stored in <see cref="PlayerPrefs"/> so new players get gentle guidance once.
/// </summary>
public static class TutorialHints
{
    private const string Prefix = "MuseumTutorial_";

    public static event Action<string> OnShowHint;

    public static void TryShowPickupHint()
    {
        TryShowOnce("Pickup", "Tip: E picks up paintings. Stack several, then scroll to choose which one to hang.");
    }

    public static void TryShowWrongWingHint()
    {
        TryShowOnce("WrongWing", "Tip: Each wall is a gallery wing — match the painting's wing to the wall.");
    }

    public static void TryShowWrongSlotHint()
    {
        TryShowOnce("WrongSlot", "Tip: Some frames are reserved for a specific painting — read the placard label.");
    }

    public static void TryShowStackScrollHint()
    {
        TryShowOnce("StackScroll", "Tip: Scroll the mouse wheel to change which painting you're holding forward.");
    }

    public static void TryShowThrowHint()
    {
        TryShowOnce("Throw", "Tip: Press Q to throw the active painting — aim at the sorting table to catch it.");
    }

    public static void TryShowCompleteHint()
    {
        TryShowOnce("MuseumWin", "The museum is open! Every wing is hung. Cozy work.");
    }

    public static void ClearAllHints()
    {
        string[] keys = { "Pickup", "WrongWing", "WrongSlot", "StackScroll", "Throw", "MuseumWin" };
        for (int i = 0; i < keys.Length; i++)
        {
            PlayerPrefs.DeleteKey(Prefix + keys[i]);
        }

        PlayerPrefs.Save();
    }

    private static void TryShowOnce(string key, string message)
    {
        string prefKey = Prefix + key;
        if (PlayerPrefs.GetInt(prefKey, 0) == 1)
        {
            return;
        }

        PlayerPrefs.SetInt(prefKey, 1);
        PlayerPrefs.Save();
        OnShowHint?.Invoke(message);
    }
}
