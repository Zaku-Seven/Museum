using UnityEngine;

/// <summary>
/// Wall mount that accepts a carried painting and snaps it into place.
/// </summary>
public class PaintingMount : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform snapPoint;

    [Header("Sorting")]
    [Tooltip("Only a painting whose wing matches this value can be hung here.")]
    [SerializeField] private GalleryWing requiredWing = GalleryWing.Modern;

    [Tooltip("Optional: only this painting (by id) may hang here. Empty = any wing match.")]
    [SerializeField] private string requiredPaintingId = string.Empty;

    [Tooltip("HUD/placard label when a specific painting is reserved for this mount.")]
    [SerializeField] private string slotDisplayTitle = string.Empty;

    [Header("State")]
    [SerializeField] private bool isOccupied;

    public bool IsOccupied => isOccupied;
    public GalleryWing RequiredWing => requiredWing;
    public string RequiredPaintingId => requiredPaintingId;
    public bool HasSpecificSlot => !string.IsNullOrEmpty(requiredPaintingId);
    public string SlotDisplayTitle => string.IsNullOrEmpty(slotDisplayTitle) ? requiredPaintingId : slotDisplayTitle;
    public Transform SnapPoint => snapPoint != null ? snapPoint : transform;

    /// <summary>The painting currently hung on this mount, or null when empty.</summary>
    public InteractablePainting Occupant { get; private set; }

    public string SaveId => GetComponent<MuseumEntityId>()?.EntityId ?? gameObject.name;

    private GallerySection section;

    /// <summary>
    /// Called by <see cref="GallerySection"/> so the mount can report placement/removal
    /// back to its owning section for completion checks.
    /// </summary>
    public void SetSection(GallerySection owningSection)
    {
        section = owningSection;
    }

    /// <summary>Editor/setup helper to reserve a mount for one painting.</summary>
    public void ConfigureSlot(string paintingId, string displayTitle)
    {
        requiredPaintingId = paintingId ?? string.Empty;
        slotDisplayTitle = displayTitle ?? string.Empty;
    }

    private void Awake()
    {
        if (snapPoint == null)
        {
            Transform existing = transform.Find("SnapPoint");
            snapPoint = existing != null ? existing : transform;
        }
    }

    /// <summary>
    /// True when this mount is empty and the painting matches wing (and optional slot id).
    /// </summary>
    public bool CanAccept(InteractablePainting painting)
    {
        if (isOccupied || painting == null || painting.Wing != requiredWing)
        {
            return false;
        }

        if (HasSpecificSlot && !PaintingIdsMatch(requiredPaintingId, painting))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// True when occupied by a painting that satisfies wing and optional slot rules.
    /// </summary>
    public bool IsCorrectlyOccupied()
    {
        if (!isOccupied || Occupant == null || Occupant.Wing != requiredWing)
        {
            return false;
        }

        if (HasSpecificSlot && !PaintingIdsMatch(requiredPaintingId, Occupant))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Parents and snaps a painting onto this mount.
    /// </summary>
    public void PlacePainting(Transform painting, Rigidbody rigidbody)
    {
        if (isOccupied || painting == null)
        {
            return;
        }

        painting.SetParent(SnapPoint, false);
        painting.localPosition = Vector3.zero;
        painting.localRotation = Quaternion.identity;

        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            rigidbody.detectCollisions = true;
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        InteractablePainting interactablePainting = painting.GetComponent<InteractablePainting>();
        if (interactablePainting != null)
        {
            interactablePainting.SetMount(this);
        }

        Occupant = interactablePainting;
        isOccupied = true;

        PlacementPopFeedback.Play(painting);
        NotifySectionChanged();

        if (interactablePainting != null)
        {
            MuseumGameEvents.RaisePaintingPlaced(interactablePainting, this);
        }
    }

    /// <summary>
    /// Removes the hung painting for undo / take-down. Returns the painting, or null if empty.
    /// </summary>
    public InteractablePainting TakeDownPainting()
    {
        if (!isOccupied || Occupant == null)
        {
            return null;
        }

        InteractablePainting painting = Occupant;
        Transform paintingTransform = painting.transform;
        paintingTransform.SetParent(null, true);
        painting.ClearMount();
        ClearOccupant();

        Rigidbody rb = paintingTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.detectCollisions = true;
        }

        return painting;
    }

    /// <summary>
    /// Clears occupancy when a painting is picked up off this mount.
    /// </summary>
    public void ClearOccupant()
    {
        isOccupied = false;
        Occupant = null;

        NotifySectionChanged();
    }

    private void NotifySectionChanged()
    {
        if (section != null)
        {
            section.CheckComplete();
        }
    }

    private static bool PaintingIdsMatch(string requiredId, InteractablePainting painting)
    {
        if (string.IsNullOrEmpty(requiredId) || painting == null)
        {
            return true;
        }

        return string.Equals(requiredId, painting.PaintingId, System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(requiredId, painting.SaveId, System.StringComparison.OrdinalIgnoreCase);
    }
}
