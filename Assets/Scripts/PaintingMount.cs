using UnityEngine;

/// <summary>
/// Wall mount that accepts a carried painting and snaps it into place.
/// </summary>
public class PaintingMount : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform snapPoint;

    [Header("State")]
    [SerializeField] private bool isOccupied;

    public bool IsOccupied => isOccupied;
    public Transform SnapPoint => snapPoint != null ? snapPoint : transform;

    private void Awake()
    {
        if (snapPoint == null)
        {
            Transform existing = transform.Find("SnapPoint");
            snapPoint = existing != null ? existing : transform;
        }
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

        isOccupied = true;
    }

    /// <summary>
    /// Clears occupancy when a painting is picked up off this mount.
    /// </summary>
    public void ClearOccupant()
    {
        isOccupied = false;
    }
}
