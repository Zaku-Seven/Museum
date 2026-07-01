using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Saves which paintings are hung on which mounts (by GameObject name) via PlayerPrefs JSON.
/// </summary>
public class MuseumSaveManager : MonoBehaviour
{
    private const string SaveKey = "MuseumMountSave_v1";

    public static MuseumSaveManager Instance { get; private set; }

    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool autoSaveOnChange = true;

    private void OnEnable()
    {
        Instance = this;
        MuseumGameEvents.PaintingPlaced += HandlePaintingChanged;
        MuseumGameEvents.PlacementUndone += HandleUndone;
    }

    private void OnDisable()
    {
        MuseumGameEvents.PaintingPlaced -= HandlePaintingChanged;
        MuseumGameEvents.PlacementUndone -= HandleUndone;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (loadOnStart)
        {
            Load();
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void HandlePaintingChanged(InteractablePainting painting, PaintingMount mount)
    {
        if (autoSaveOnChange)
        {
            Save();
        }
    }

    private void HandleUndone(InteractablePainting painting, PaintingMount mount)
    {
        if (autoSaveOnChange)
        {
            Save();
        }
    }

    public void Save()
    {
        var data = new SaveData();
        PaintingMount[] mounts = FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        foreach (PaintingMount mount in mounts)
        {
            if (mount == null || !mount.IsOccupied || mount.Occupant == null)
            {
                continue;
            }

            data.entries.Add(new MountEntry
            {
                mountName = mount.gameObject.name,
                paintingName = mount.Occupant.gameObject.name
            });
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data?.entries == null)
        {
            return;
        }

        foreach (MountEntry entry in data.entries)
        {
            if (string.IsNullOrEmpty(entry.mountName) || string.IsNullOrEmpty(entry.paintingName))
            {
                continue;
            }

            GameObject mountObject = GameObject.Find(entry.mountName);
            GameObject paintingObject = GameObject.Find(entry.paintingName);
            if (mountObject == null || paintingObject == null)
            {
                continue;
            }

            PaintingMount mount = mountObject.GetComponent<PaintingMount>();
            InteractablePainting painting = paintingObject.GetComponent<InteractablePainting>();
            Rigidbody rigidbody = paintingObject.GetComponent<Rigidbody>();
            if (mount == null || painting == null || mount.IsOccupied)
            {
                continue;
            }

            if (!mount.CanAccept(painting))
            {
                continue;
            }

            mount.PlacePainting(paintingObject.transform, rigidbody);
        }
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }

    [Serializable]
    private class SaveData
    {
        public List<MountEntry> entries = new List<MountEntry>();
    }

    [Serializable]
    private class MountEntry
    {
        public string mountName;
        public string paintingName;
    }
}
