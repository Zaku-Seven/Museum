using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Floor staging zone: dropped paintings snap to a neat grid on the table.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class SortingTable : MonoBehaviour
{
    private static readonly List<SortingTable> ActiveTables = new List<SortingTable>();

    [SerializeField] private string zoneLabel = "Sorting Table";
    [SerializeField] private int gridColumns = 5;
    [SerializeField] private float cellSpacingX = 0.65f;
    [SerializeField] private float cellSpacingZ = 0.5f;
    [SerializeField] private float surfaceHeight = 0.03f;

    private readonly List<Transform> stagedItems = new List<Transform>();

    public string ZoneLabel => zoneLabel;
    public int StagedCount => stagedItems.Count;

    public IReadOnlyList<string> GetStagedSaveIds()
    {
        var ids = new List<string>(stagedItems.Count);
        for (int i = 0; i < stagedItems.Count; i++)
        {
            Transform item = stagedItems[i];
            if (item == null)
            {
                continue;
            }

            InteractablePainting painting = item.GetComponent<InteractablePainting>();
            ids.Add(painting != null ? painting.SaveId : item.name);
        }

        return ids;
    }

    public static int TotalStagedCount
    {
        get
        {
            int total = 0;
            for (int i = 0; i < ActiveTables.Count; i++)
            {
                if (ActiveTables[i] != null)
                {
                    total += ActiveTables[i].StagedCount;
                }
            }

            return total;
        }
    }

    private void OnEnable()
    {
        if (!ActiveTables.Contains(this))
        {
            ActiveTables.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveTables.Remove(this);
    }

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCatchLoosePainting(other.transform);
    }

    /// <summary>
    /// Thrown or dropped paintings that enter the trigger at low speed snap to the grid.
    /// </summary>
    private void TryCatchLoosePainting(Transform candidate)
    {
        InteractablePainting painting = candidate.GetComponentInParent<InteractablePainting>();
        if (painting == null || painting.CurrentMount != null)
        {
            return;
        }

        for (int i = 0; i < stagedItems.Count; i++)
        {
            if (stagedItems[i] == painting.transform)
            {
                return;
            }
        }

        Rigidbody rb = painting.GetComponent<Rigidbody>();
        if (rb != null && rb.linearVelocity.magnitude > 3.5f)
        {
            return;
        }

        TryStagePainting(painting.transform);
    }

    public bool Contains(Vector3 worldPosition)
    {
        BoxCollider box = GetComponent<BoxCollider>();
        return box != null && box.bounds.Contains(worldPosition);
    }

    /// <summary>
    /// If the painting is over this table, snap it to the next grid cell and lay it flat.
    /// </summary>
    public bool TryStagePainting(Transform painting)
    {
        if (painting == null || !Contains(painting.position))
        {
            return false;
        }

        UnregisterIfPresent(painting);
        stagedItems.Add(painting);

        painting.SetParent(transform, true);
        int index = stagedItems.Count - 1;
        painting.localPosition = GetGridLocalPosition(index);
        painting.localRotation = Quaternion.identity;

        Rigidbody rb = painting.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.detectCollisions = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        InteractablePainting interactable = painting.GetComponent<InteractablePainting>();
        if (interactable != null)
        {
            MuseumGameEvents.RaisePaintingStaged(interactable);
            TutorialHints.TryShowStagingHint();
            PlacementPopFeedback.Play(painting);
        }

        return true;
    }

    public static bool TryStageOnAnyTable(Transform painting)
    {
        for (int i = 0; i < ActiveTables.Count; i++)
        {
            SortingTable table = ActiveTables[i];
            if (table != null && table.TryStagePainting(painting))
            {
                return true;
            }
        }

        return false;
    }

    public static void UnregisterIfPresent(Transform painting)
    {
        for (int i = 0; i < ActiveTables.Count; i++)
        {
            ActiveTables[i]?.RemoveStaged(painting);
        }
    }

    public static bool IsStaged(Transform painting)
    {
        if (painting == null)
        {
            return false;
        }

        for (int i = 0; i < ActiveTables.Count; i++)
        {
            SortingTable table = ActiveTables[i];
            if (table != null && table.ContainsStaged(painting))
            {
                return true;
            }
        }

        return false;
    }

    public bool ContainsStaged(Transform painting)
    {
        return painting != null && stagedItems.Contains(painting);
    }

    public void ClearAllStaged()
    {
        for (int i = stagedItems.Count - 1; i >= 0; i--)
        {
            Transform item = stagedItems[i];
            if (item != null)
            {
                item.SetParent(null, true);
            }
        }

        stagedItems.Clear();
    }

    private void RemoveStaged(Transform painting)
    {
        if (painting == null)
        {
            return;
        }

        int index = stagedItems.IndexOf(painting);
        if (index < 0)
        {
            return;
        }

        stagedItems.RemoveAt(index);
        ReflowGrid();
    }

    private void ReflowGrid()
    {
        for (int i = 0; i < stagedItems.Count; i++)
        {
            Transform item = stagedItems[i];
            if (item == null)
            {
                continue;
            }

            item.SetParent(transform, true);
            item.localPosition = GetGridLocalPosition(i);
            item.localRotation = Quaternion.identity;
        }
    }

    private Vector3 GetGridLocalPosition(int index)
    {
        int col = index % gridColumns;
        int row = index / gridColumns;
        float startX = -(gridColumns - 1) * cellSpacingX * 0.5f;
        return new Vector3(startX + col * cellSpacingX, surfaceHeight, row * cellSpacingZ);
    }
}
