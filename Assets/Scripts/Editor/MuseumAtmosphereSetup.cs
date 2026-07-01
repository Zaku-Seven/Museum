using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates a global URP Volume with a tunable museum mood profile.
/// </summary>
public static class MuseumAtmosphereSetup
{
    private const string ProfilePath = "Assets/Settings/MuseumAtmosphereProfile.asset";
    private const string VolumeObjectName = "MuseumAtmosphereVolume";

    [MenuItem("Game/Setup Museum Atmosphere")]
    public static void SetupFromMenu()
    {
        EnsureAtmosphereVolume();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Museum atmosphere volume ready — tweak Assets/Settings/MuseumAtmosphereProfile.asset in Inspector.");
    }

    public static void EnsureAtmosphereVolume()
    {
        EnsureSettingsFolder();

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            ConfigureProfile(profile);
            AssetDatabase.CreateAsset(profile, ProfilePath);
            AssetDatabase.SaveAssets();
        }

        GameObject volumeObject = GameObject.Find(VolumeObjectName);
        if (volumeObject == null)
        {
            volumeObject = new GameObject(VolumeObjectName);
        }

        Volume volume = volumeObject.GetComponent<Volume>();
        if (volume == null)
        {
            volume = volumeObject.AddComponent<Volume>();
        }

        volume.isGlobal = true;
        volume.priority = 5f;
        volume.weight = 1f;
        volume.sharedProfile = profile;
    }

    private static void ConfigureProfile(VolumeProfile profile)
    {
        if (!profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments = profile.Add<ColorAdjustments>(true);
        }

        colorAdjustments.active = true;
        colorAdjustments.saturation.Override(-8f);
        colorAdjustments.contrast.Override(6f);

        if (!profile.TryGet(out Vignette vignette))
        {
            vignette = profile.Add<Vignette>(true);
        }

        vignette.active = true;
        vignette.intensity.Override(0.22f);
        vignette.smoothness.Override(0.45f);

        if (!profile.TryGet(out Bloom bloom))
        {
            bloom = profile.Add<Bloom>(true);
        }

        bloom.active = true;
        bloom.intensity.Override(0.15f);
        bloom.threshold.Override(1.1f);
    }

    private static void EnsureSettingsFolder()
    {
        if (AssetDatabase.IsValidFolder("Assets/Settings"))
        {
            return;
        }

        AssetDatabase.CreateFolder("Assets", "Settings");
    }
}
