using UnityEngine;

/// <summary>
/// Marks a GameObject as a museum painting the player can pick up and hang.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class InteractablePainting : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private string paintingTitle = "Untitled";

    [Header("Sorting")]
    [Tooltip("Which gallery wing this painting belongs to. Mounts only accept a matching wing.")]
    [SerializeField] private GalleryWing wing = GalleryWing.Modern;

    [Header("Data (optional)")]
    [SerializeField] private PaintingDefinition definition;

    private Vector3 defaultWorldPosition;
    private Quaternion defaultWorldRotation;
    private Transform defaultParent;
    private bool spawnPoseCaptured;

    public string PaintingTitle => definition != null ? definition.Title : paintingTitle;
    public GalleryWing Wing => definition != null ? definition.Wing : wing;
    public string PaintingId => definition != null ? definition.PaintingId : SaveId;
    public string InspectDescription => definition != null && !string.IsNullOrWhiteSpace(definition.Description)
        ? definition.Description
        : $"Curator note: belongs in the {Wing} gallery wing.";
    public PaintingMount CurrentMount { get; private set; }
    public string SaveId => GetComponent<MuseumEntityId>()?.EntityId ?? gameObject.name;

    private void Awake()
    {
        ApplyDefinitionIfPresent();
        CaptureDefaultSpawnIfNeeded();
    }

    public void ApplyDefinitionIfPresent()
    {
        if (definition == null)
        {
            return;
        }

        paintingTitle = definition.Title;
        wing = definition.Wing;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial.color = definition.DisplayColor;
        }
    }

    public void SetDefinition(PaintingDefinition newDefinition)
    {
        definition = newDefinition;
        ApplyDefinitionIfPresent();
    }

    public void CaptureDefaultSpawnIfNeeded()
    {
        if (spawnPoseCaptured || CurrentMount != null)
        {
            return;
        }

        defaultWorldPosition = transform.position;
        defaultWorldRotation = transform.rotation;
        defaultParent = transform.parent;
        spawnPoseCaptured = true;
    }

    public void ResetToDefaultSpawn()
    {
        if (CurrentMount != null)
        {
            CurrentMount.TakeDownPainting();
        }

        SortingTable.UnregisterIfPresent(transform);

        if (!spawnPoseCaptured)
        {
            CaptureDefaultSpawnIfNeeded();
        }

        transform.SetParent(defaultParent, true);
        transform.SetPositionAndRotation(defaultWorldPosition, defaultWorldRotation);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.detectCollisions = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = true;
        }
    }

    public void SetMount(PaintingMount mount)
    {
        if (CurrentMount != null && CurrentMount != mount)
        {
            CurrentMount.ClearOccupant();
        }

        CurrentMount = mount;
    }

    public void ClearMount()
    {
        CurrentMount = null;
    }
}
