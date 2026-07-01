using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime settings panel nested under the pause overlay.
/// </summary>
public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider gamepadLookSlider;
    [SerializeField] private Toggle invertYToggle;
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Toggle synthesizedSfxToggle;
    [SerializeField] private Toggle reduceMotionToggle;
    [SerializeField] private Text sensitivityValueText;
    [SerializeField] private Text gamepadLookValueText;
    [SerializeField] private Text fovValueText;
    [SerializeField] private Text volumeValueText;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private MuseumAudioDirector audioDirector;

    public bool IsSettingsOpen => settingsPanel != null && settingsPanel.activeSelf;

    private void Awake()
    {
        if (firstPersonController == null)
        {
            firstPersonController = GetComponent<FirstPersonController>();
        }

        if (audioDirector == null)
        {
            audioDirector = GetComponent<MuseumAudioDirector>();
        }

        BindUi();
        LoadValuesIntoUi();
        CloseSettings();
    }

    private void BindUi()
    {
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        if (gamepadLookSlider != null)
        {
            gamepadLookSlider.onValueChanged.AddListener(OnGamepadLookChanged);
        }

        if (invertYToggle != null)
        {
            invertYToggle.onValueChanged.AddListener(OnInvertYChanged);
        }

        if (fovSlider != null)
        {
            fovSlider.onValueChanged.AddListener(OnFovChanged);
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (synthesizedSfxToggle != null)
        {
            synthesizedSfxToggle.onValueChanged.AddListener(OnSynthesizedSfxChanged);
        }

        if (reduceMotionToggle != null)
        {
            reduceMotionToggle.onValueChanged.AddListener(OnReduceMotionChanged);
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        LoadValuesIntoUi();
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void ResetToDefaults()
    {
        PlayerSettingsStore.ResetToDefaults();
        LoadValuesIntoUi();
        firstPersonController?.ApplyPlayerSettings();
        audioDirector?.ApplyVolume();
        FindFirstObjectByType<MuseumAmbienceController>()?.ApplyVolume();
    }

    private void LoadValuesIntoUi()
    {
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.SetValueWithoutNotify(PlayerSettingsStore.MouseSensitivity);
        }

        if (gamepadLookSlider != null)
        {
            gamepadLookSlider.SetValueWithoutNotify(PlayerSettingsStore.GamepadLookSensitivity);
        }

        if (invertYToggle != null)
        {
            invertYToggle.SetIsOnWithoutNotify(PlayerSettingsStore.InvertY);
        }

        if (fovSlider != null)
        {
            fovSlider.SetValueWithoutNotify(PlayerSettingsStore.FieldOfView);
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(PlayerSettingsStore.MasterVolume);
        }

        if (synthesizedSfxToggle != null)
        {
            synthesizedSfxToggle.SetIsOnWithoutNotify(PlayerSettingsStore.UseSynthesizedSfx);
        }

        if (reduceMotionToggle != null)
        {
            reduceMotionToggle.SetIsOnWithoutNotify(PlayerSettingsStore.ReduceMotion);
        }

        RefreshValueLabels();
    }

    private void OnSensitivityChanged(float value)
    {
        PlayerSettingsStore.MouseSensitivity = value;
        firstPersonController?.ApplyPlayerSettings();
        RefreshValueLabels();
    }

    private void OnGamepadLookChanged(float value)
    {
        PlayerSettingsStore.GamepadLookSensitivity = value;
        RefreshValueLabels();
    }

    private void OnInvertYChanged(bool value)
    {
        PlayerSettingsStore.InvertY = value;
        firstPersonController?.ApplyPlayerSettings();
    }

    private void OnFovChanged(float value)
    {
        PlayerSettingsStore.FieldOfView = value;
        firstPersonController?.ApplyPlayerSettings();
        RefreshValueLabels();
    }

    private void OnVolumeChanged(float value)
    {
        PlayerSettingsStore.MasterVolume = value;
        audioDirector?.ApplyVolume();
        FindFirstObjectByType<MuseumAmbienceController>()?.ApplyVolume();
        RefreshValueLabels();
    }

    private void OnSynthesizedSfxChanged(bool value)
    {
        PlayerSettingsStore.UseSynthesizedSfx = value;
    }

    private void OnReduceMotionChanged(bool value)
    {
        PlayerSettingsStore.ReduceMotion = value;
    }

    private void RefreshValueLabels()
    {
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = PlayerSettingsStore.MouseSensitivity.ToString("0.0");
        }

        if (gamepadLookValueText != null)
        {
            gamepadLookValueText.text = PlayerSettingsStore.GamepadLookSensitivity.ToString("0.0");
        }

        if (fovValueText != null)
        {
            fovValueText.text = $"{PlayerSettingsStore.FieldOfView:0}°";
        }

        if (volumeValueText != null)
        {
            volumeValueText.text = $"{Mathf.RoundToInt(PlayerSettingsStore.MasterVolume * 100f)}%";
        }
    }
}
