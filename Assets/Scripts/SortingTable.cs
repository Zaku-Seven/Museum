using UnityEngine;

/// <summary>
/// Marks a floor staging zone where unsorted paintings are piled before being hung.
/// For now this is a visual + trigger marker only (no scoring); it gives the room a
/// readable "sorting in progress" staging area. The collider is a trigger so paintings
/// dropped inside are never physically blocked.
///
/// Kept in the global namespace to match the rest of the project's scripts.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class SortingTable : MonoBehaviour
{
    [SerializeField] private string zoneLabel = "Sorting Table";

    public string ZoneLabel => zoneLabel;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    /// <summary>True if the given world position lies within this zone's collider bounds.</summary>
    public bool Contains(Vector3 worldPosition)
    {
        BoxCollider box = GetComponent<BoxCollider>();
        return box != null && box.bounds.Contains(worldPosition);
    }
}
