using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Raycast-based pick-up, carry, wall-mount placement, and drop for museum paintings.
/// Attach to the player camera. Uses the "Interactable" layer and a HoldPoint child transform.
///
/// Carry feel (Night 1, verified defaults — see docs/MORNING_TEST.md):
///   - Immediate snap to the hold point on pickup (no pop-in lag), then a per-frame
///     LateUpdate lerp keeps the canvas glued to the hands after camera movement.
///   - Rigidbody is disabled while held so physics never fights the camera.
///   - carryFollowSharpness ~= 28 gives a tight, readable follow; carryLocalEuler (-90,0,0)
///     rotates the flat cube "canvas" to face the player.
/// Placement is wing-gated (see GalleryWing / PaintingMount.CanAccept): aiming at a
/// wrong-wing or occupied mount rejects the click but never drops the painting.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ArtPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Maximum distance from the camera center ray to pick up or place objects.")]
    [SerializeField] private float maxPickupDistance = 3.5f;

    [Tooltip("Empty transform in front of the camera representing the player's hands.")]
    [SerializeField] private Transform holdPoint;

    [Header("Carry Feel")]
    [Tooltip("Local rotation applied while carrying so the canvas faces the player naturally.")]
    [SerializeField] private Vector3 carryLocalEuler = new Vector3(-90f, 0f, 0f);

    [Tooltip("How quickly the painting catches up to the hold point. Higher = tighter to the camera.")]
    [SerializeField] private float carryFollowSharpness = 28f;

    private Camera playerCamera;
    private int interactableLayerMask;
    private Transform heldObject;
    private InteractablePainting heldPainting;
    private Rigidbody heldRigidbody;
    private Collider[] heldColliders;
    private Quaternion carryLocalRotation;

    public bool IsHolding => heldObject != null;
    public InteractablePainting HeldPainting => heldPainting;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();
        interactableLayerMask = LayerMask.GetMask("Interactable");
        carryLocalRotation = Quaternion.Euler(carryLocalEuler);

        if (interactableLayerMask == 0)
        {
            Debug.LogWarning("ArtPickup: 'Interactable' layer not found. Create it in Project Settings > Tags and Layers.");
        }

        EnsureHoldPointExists();
    }

    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (heldObject != null)
        {
            HandleHeldClick();
            return;
        }

        TryPickupFromRaycast();
    }

    /// <summary>
    /// Keeps the carried painting glued to the hold point after camera movement.
    /// Runs after FirstPersonController updates the camera rotation.
    /// </summary>
    private void LateUpdate()
    {
        if (heldObject == null || holdPoint == null)
        {
            return;
        }

        Quaternion targetRotation = holdPoint.rotation * carryLocalRotation;
        float followFactor = 1f - Mathf.Exp(-carryFollowSharpness * Time.deltaTime);
        heldObject.position = Vector3.Lerp(heldObject.position, holdPoint.position, followFactor);
        heldObject.rotation = Quaternion.Slerp(heldObject.rotation, targetRotation, followFactor);
    }

    /// <summary>
    /// Returns a context-sensitive prompt based on what the player is looking at.
    /// </summary>
    public string GetInteractionPrompt()
    {
        if (!TryGetCenterRayHit(out RaycastHit hit))
        {
            return IsHolding ? "Click to drop" : string.Empty;
        }

        if (IsHolding)
        {
            PaintingMount mount = hit.collider.GetComponentInParent<PaintingMount>();
            if (mount != null)
            {
                if (mount.IsOccupied)
                {
                    return "Gallery spot taken";
                }

                if (mount.CanAccept(heldPainting))
                {
                    return "Click to place on wall";
                }

                return $"This belongs in the {mount.RequiredWing} gallery";
            }

            return "Click to drop";
        }

        if (hit.collider.GetComponentInParent<InteractablePainting>() != null)
        {
            InteractablePainting painting = hit.collider.GetComponentInParent<InteractablePainting>();
            return $"Click to pick up \"{painting.PaintingTitle}\"";
        }

        return string.Empty;
    }

    private void EnsureHoldPointExists()
    {
        if (holdPoint == null)
        {
            Transform existing = transform.Find("HoldPoint");
            holdPoint = existing;
        }

        if (holdPoint == null)
        {
            GameObject holdPointObject = new GameObject("HoldPoint");
            holdPointObject.transform.SetParent(transform, false);
            holdPoint = holdPointObject.transform;
        }

        ApplyHoldPointDefaults();
    }

    /// <summary>
    /// Positions the hold point at chest height, arm's length, with a slight tilt toward the player.
    /// </summary>
    private void ApplyHoldPointDefaults()
    {
        holdPoint.localPosition = new Vector3(0f, -0.22f, 0.68f);
        holdPoint.localRotation = Quaternion.Euler(6f, 0f, 0f);
    }

    /// <summary>Center-screen raycast on the Interactable layer (shared with highlight + prompts).</summary>
    public bool TryGetCenterRayHit(out RaycastHit hit)
    {
        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return Physics.Raycast(centerRay, out hit, maxPickupDistance, interactableLayerMask);
    }

    private void TryPickupFromRaycast()
    {
        if (!TryGetCenterRayHit(out RaycastHit hit))
        {
            return;
        }

        InteractablePainting painting = hit.collider.GetComponentInParent<InteractablePainting>();
        if (painting == null)
        {
            return;
        }

        PickUpObject(painting.transform);
    }

    /// <summary>
    /// Handles a left-click while carrying a painting. Placing is wing-gated: aiming at a
    /// valid empty mount hangs the painting; aiming at an occupied or wrong-wing mount
    /// rejects the click and keeps the painting in hand (cozy, forgiving — never a silent
    /// or accidental drop). Clicking while not aimed at any mount drops the painting.
    /// </summary>
    private void HandleHeldClick()
    {
        if (TryGetCenterRayHit(out RaycastHit hit))
        {
            PaintingMount mount = hit.collider.GetComponentInParent<PaintingMount>();
            if (mount != null)
            {
                if (mount.CanAccept(heldPainting))
                {
                    PlaceOnMount(mount);
                }

                // Occupied or wrong wing: reject but keep holding. The HUD prompt
                // ("Wrong gallery" / "Gallery spot taken") already explains why.
                return;
            }
        }

        DropHeldObject();
    }

    private void PickUpObject(Transform target)
    {
        heldObject = target;
        heldRigidbody = heldObject.GetComponent<Rigidbody>();
        heldColliders = heldObject.GetComponentsInChildren<Collider>();

        InteractablePainting painting = heldObject.GetComponent<InteractablePainting>();
        heldPainting = painting;
        if (painting != null && painting.CurrentMount != null)
        {
            painting.CurrentMount.ClearOccupant();
            painting.ClearMount();
        }

        // Detach from any parent so the carry system fully controls the transform.
        heldObject.SetParent(null);

        foreach (Collider collider in heldColliders)
        {
            collider.enabled = false;
        }

        // Kinematic + no collisions prevents physics from fighting camera movement while carried.
        if (heldRigidbody != null)
        {
            heldRigidbody.linearVelocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.detectCollisions = false;
        }

        // Snap immediately so there is no pop-in lag on pickup.
        heldObject.SetPositionAndRotation(holdPoint.position, holdPoint.rotation * carryLocalRotation);
    }

    private void PlaceOnMount(PaintingMount mount)
    {
        Transform objectToPlace = heldObject;
        Rigidbody rigidbody = heldRigidbody;
        Collider[] colliders = heldColliders;

        heldObject = null;
        heldPainting = null;
        heldRigidbody = null;
        heldColliders = null;

        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }

        RestoreRigidbody(rigidbody, kinematic: true, useGravity: false);
        mount.PlacePainting(objectToPlace, rigidbody);
    }

    private void DropHeldObject()
    {
        Transform droppedObject = heldObject;
        Rigidbody rigidbody = heldRigidbody;
        Collider[] colliders = heldColliders;

        heldObject = null;
        heldPainting = null;
        heldRigidbody = null;
        heldColliders = null;

        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }

        RestoreRigidbody(rigidbody, kinematic: false, useGravity: true);

        // Nudge slightly forward so the painting clears the player's collider when dropped.
        if (droppedObject != null)
        {
            droppedObject.position += playerCamera.transform.forward * 0.25f;
        }
    }

    private static void RestoreRigidbody(Rigidbody rigidbody, bool kinematic, bool useGravity)
    {
        if (rigidbody == null)
        {
            return;
        }

        rigidbody.isKinematic = kinematic;
        rigidbody.useGravity = useGravity;
        rigidbody.detectCollisions = true;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }
}
