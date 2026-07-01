using UnityEngine;

/// <summary>
/// Persists player comfort settings in PlayerPrefs.
/// </summary>
public static class PlayerSettingsStore
{
    private const string MouseSensitivityKey = "Museum_MouseSensitivity";
    private const string InvertYKey = "Museum_InvertY";
    private const string FieldOfViewKey = "Museum_FieldOfView";
    private const string MasterVolumeKey = "Museum_MasterVolume";

    public const float DefaultMouseSensitivity = 2f;
    public const float DefaultFieldOfView = 75f;
    public const float DefaultMasterVolume = 0.8f;
    public const float DefaultGamepadLookSensitivity = 2.5f;

    public static float GamepadLookSensitivity
    {
        get => PlayerPrefs.GetFloat("Museum_GamepadLook", DefaultGamepadLookSensitivity);
        set
        {
            PlayerPrefs.SetFloat("Museum_GamepadLook", Mathf.Clamp(value, 0.5f, 8f));
            PlayerPrefs.Save();
        }
    }

    public static float MouseSensitivity
    {
        get => PlayerPrefs.GetFloat(MouseSensitivityKey, DefaultMouseSensitivity);
        set
        {
            PlayerPrefs.SetFloat(MouseSensitivityKey, Mathf.Clamp(value, 0.25f, 8f));
            PlayerPrefs.Save();
        }
    }

    public static bool InvertY
    {
        get => PlayerPrefs.GetInt(InvertYKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(InvertYKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static float FieldOfView
    {
        get => PlayerPrefs.GetFloat(FieldOfViewKey, DefaultFieldOfView);
        set
        {
            PlayerPrefs.SetFloat(FieldOfViewKey, Mathf.Clamp(value, 55f, 100f));
            PlayerPrefs.Save();
        }
    }

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
        set
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }

    public static void ResetToDefaults()
    {
        MouseSensitivity = DefaultMouseSensitivity;
        GamepadLookSensitivity = DefaultGamepadLookSensitivity;
        InvertY = false;
        FieldOfView = DefaultFieldOfView;
        MasterVolume = DefaultMasterVolume;
    }
}
