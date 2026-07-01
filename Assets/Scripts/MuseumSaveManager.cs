using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Saves mount assignments, staging, and win state using stable entity ids (PlayerPrefs JSON v2).
/// </summary>
public class MuseumSaveManager : MonoBehaviour
{
    private const string SaveKeyV2 = "MuseumMountSave_v2";
    private const string SaveKeyV1 = "MuseumMountSave_v1";

    public static MuseumSaveManager Instance { get; private set; }

    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool autoSaveOnChange = true;

    private void OnEnable()
    {
        Instance = this;
        MuseumGameEvents.PaintingPlaced += HandleAutoSave;
        MuseumGameEvents.PlacementUndone += HandleAutoSaveUndone;
        MuseumGameEvents.PaintingDropped += HandleAutoSavePainting;
        MuseumGameEvents.PaintingThrown += HandleAutoSavePainting;
        MuseumGameEvents.PaintingStaged += HandleAutoSavePainting;
        MuseumGameEvents.PaintingPickedUp += HandleAutoSavePainting;
    }

    private void OnDisable()
    {
        MuseumGameEvents.PaintingPlaced -= HandleAutoSave;
        MuseumGameEvents.PlacementUndone -= HandleAutoSaveUndone;
        MuseumGameEvents.PaintingDropped -= HandleAutoSavePainting;
        MuseumGameEvents.PaintingThrown -= HandleAutoSavePainting;
        MuseumGameEvents.PaintingStaged -= HandleAutoSavePainting;
        MuseumGameEvents.PaintingPickedUp -= HandleAutoSavePainting;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (loadOnStart && !MainMenuController.ShouldDeferSaveLoad())
        {
            Load();
        }
    }

    public static bool HasExistingSave()
    {
        return PlayerPrefs.HasKey(SaveKeyV2) || PlayerPrefs.HasKey(SaveKeyV1);
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void HandleAutoSave(InteractablePainting painting, PaintingMount mount)
    {
        RequestAutoSave();
    }

    private void HandleAutoSaveUndone(InteractablePainting painting, PaintingMount mount)
    {
        RequestAutoSave();
    }

    private void HandleAutoSavePainting(InteractablePainting painting)
    {
        RequestAutoSave();
    }

    private void RequestAutoSave()
    {
        if (autoSaveOnChange)
        {
            Save();
        }
    }

    public void Save()
    {
        var data = new SaveDataV2
        {
            version = 2,
            museumComplete = MuseumProgress.Instance != null && MuseumProgress.Instance.IsMuseumComplete
        };

        PaintingMount[] mounts = FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        for (int i = 0; i < mounts.Length; i++)
        {
            PaintingMount mount = mounts[i];
            if (mount == null || !mount.IsOccupied || mount.Occupant == null)
            {
                continue;
            }

            data.mountAssignments.Add(new MountAssignmentEntry
            {
                mountId = mount.SaveId,
                paintingId = mount.Occupant.SaveId
            });
        }

        SortingTable[] tables = FindObjectsByType<SortingTable>(FindObjectsSortMode.None);
        for (int t = 0; t < tables.Length; t++)
        {
            SortingTable table = tables[t];
            if (table == null)
            {
                continue;
            }

            IReadOnlyList<string> stagedIds = table.GetStagedSaveIds();
            for (int s = 0; s < stagedIds.Count; s++)
            {
                data.stagedPaintingIds.Add(stagedIds[s]);
            }
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKeyV2, json);
        PlayerPrefs.DeleteKey(SaveKeyV1);
        PlayerPrefs.Save();
        MuseumGameEvents.RaiseGameSaved();
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey(SaveKeyV2))
        {
            LoadV2(PlayerPrefs.GetString(SaveKeyV2));
            return;
        }

        if (PlayerPrefs.HasKey(SaveKeyV1))
        {
            LoadV1(PlayerPrefs.GetString(SaveKeyV1));
            Save();
        }
    }

    private void LoadV2(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        SaveDataV2 data = JsonUtility.FromJson<SaveDataV2>(json);
        if (data == null)
        {
            return;
        }

        ApplyMountAssignments(data.mountAssignments);
        ApplyStagedPaintings(data.stagedPaintingIds);

        if (MuseumProgress.Instance != null)
        {
            MuseumProgress.Instance.RestoreMuseumCompleteFromSave(data.museumComplete);
        }
    }

    private void LoadV1(string json)
    {
        SaveDataV1 data = JsonUtility.FromJson<SaveDataV1>(json);
        if (data?.entries == null)
        {
            return;
        }

        var assignments = new List<MountAssignmentEntry>();
        for (int i = 0; i < data.entries.Count; i++)
        {
            MountEntryV1 entry = data.entries[i];
            assignments.Add(new MountAssignmentEntry
            {
                mountId = entry.mountName,
                paintingId = entry.paintingName
            });
        }

        ApplyMountAssignments(assignments);
    }

    private static void ApplyMountAssignments(List<MountAssignmentEntry> assignments)
    {
        if (assignments == null)
        {
            return;
        }

        for (int i = 0; i < assignments.Count; i++)
        {
            MountAssignmentEntry entry = assignments[i];
            if (string.IsNullOrEmpty(entry.mountId) || string.IsNullOrEmpty(entry.paintingId))
            {
                continue;
            }

            PaintingMount mount = ResolveMount(entry.mountId);
            InteractablePainting painting = ResolvePainting(entry.paintingId);
            if (mount == null || painting == null || mount.IsOccupied)
            {
                continue;
            }

            if (!mount.CanAccept(painting))
            {
                continue;
            }

            mount.PlacePainting(painting.transform, painting.GetComponent<Rigidbody>());
        }
    }

    private static void ApplyStagedPaintings(List<string> stagedIds)
    {
        if (stagedIds == null)
        {
            return;
        }

        for (int i = 0; i < stagedIds.Count; i++)
        {
            string paintingId = stagedIds[i];
            InteractablePainting painting = ResolvePainting(paintingId);
            if (painting == null)
            {
                continue;
            }

            if (painting.CurrentMount != null)
            {
                continue;
            }

            SortingTable.TryStageOnAnyTable(painting.transform);
        }
    }

    private static PaintingMount ResolveMount(string id)
    {
        MuseumEntityId entity = MuseumEntityId.Registry.Find(id);
        if (entity != null)
        {
            return entity.GetComponent<PaintingMount>();
        }

        GameObject found = GameObject.Find(id);
        return found != null ? found.GetComponent<PaintingMount>() : null;
    }

    private static InteractablePainting ResolvePainting(string id)
    {
        MuseumEntityId entity = MuseumEntityId.Registry.Find(id);
        if (entity != null)
        {
            return entity.GetComponent<InteractablePainting>();
        }

        GameObject found = GameObject.Find(id);
        return found != null ? found.GetComponent<InteractablePainting>() : null;
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKeyV2);
        PlayerPrefs.DeleteKey(SaveKeyV1);
        PlayerPrefs.Save();
    }

    [Serializable]
    private class SaveDataV2
    {
        public int version = 2;
        public bool museumComplete;
        public List<MountAssignmentEntry> mountAssignments = new List<MountAssignmentEntry>();
        public List<string> stagedPaintingIds = new List<string>();
    }

    [Serializable]
    private class MountAssignmentEntry
    {
        public string mountId;
        public string paintingId;
    }

    [Serializable]
    private class SaveDataV1
    {
        public List<MountEntryV1> entries = new List<MountEntryV1>();
    }

    [Serializable]
    private class MountEntryV1
    {
        public string mountName;
        public string paintingName;
    }
}
