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

    [Header("State")]
    [SerializeField] private bool isOccupied;

    public bool IsOccupied => isOccupied;
    public GalleryWing RequiredWing => requiredWing;
    public Transform SnapPoint => snapPoint != null ? snapPoint : transform;

    /// <summary>The painting currently hung on this mount, or null when empty.</summary>
    public InteractablePainting Occupant { get; private set; }

    private GallerySection section;

    /// <summary>
    /// Called by <see cref="GallerySection"/> so the mount can report placement/removal
    /// back to its owning section for completion checks.
    /// </summary>
    public void SetSection(GallerySection owningSection)
    {
        section = owningSection;
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
    /// True when this mount is empty and the painting's wing matches <see cref="RequiredWing"/>.
    /// </summary>
    public bool CanAccept(InteractablePainting painting)
    {
        return !isOccupied && painting != null && painting.Wing == requiredWing;
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
            rigidbody.enabled = true;
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
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

        NotifySectionChanged();
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
}
