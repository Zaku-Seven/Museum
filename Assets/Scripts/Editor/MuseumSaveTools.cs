using UnityEditor;
using UnityEngine;

public static class MuseumSaveTools
{
    [MenuItem("Game/Clear Museum Save")]
    public static void ClearMuseumSave()
    {
        PlayerPrefs.DeleteKey("MuseumMountSave_v2");
        PlayerPrefs.DeleteKey("MuseumMountSave_v1");
        PlayerPrefs.Save();
        Debug.Log("Museum save cleared from PlayerPrefs.");
    }
}
