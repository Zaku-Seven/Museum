using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First-person carry system for museum paintings.
///
/// Design (matches common Unity FPS "equip slot" patterns):
/// - A <see cref="holdPoint"/> empty on the camera marks where the hands sit (position only, no tilt).
/// - While carried, paintings stay <b>unparented</b> (kinematic RB) and their world pose is written each
///   <see cref="LateUpdate"/> so they never lag behind movement and never fight the physics step.
/// - Canvas rotation uses <see cref="Quaternion.FromToRotation"/> so floor paintings always face the camera.
/// - Stack fans along <see cref="Camera.transform.right"/> in world space (readable deck to your right).
///
/// Input: <b>E</b> pick/add to stack · <b>Left click</b> place/drop the last picked item.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ArtPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float maxPickupDistance = 3.5f;
    [SerializeField] private Transform holdPoint;

    [Header("Carry Stack")]
    [SerializeField] private int maxStackSize = 5;
    [Tooltip("World-space spacing between stacked items along camera right.")]
    [SerializeField] private float stackSlotSpacing = 0.16f;
    [Tooltip("Slight push toward camera per stack slot so items do not z-fight.")]
    [SerializeField] private float stackForwardStep = 0.015f;

    private Camera playerCamera;
    private int interactableLayerMask;
    private readonly List<CarriedEntry> carryStack = new List<CarriedEntry>();

    public bool IsHolding => carryStack.Count > 0;
    public int CarryCount => carryStack.Count;
    public InteractablePainting HeldPainting => carryStack.Count > 0 ? carryStack[carryStack.Count - 1].Painting : null;

    private sealed class CarriedEntry
    {
        public Transform Transform;
        public InteractablePainting Painting;
        public Rigidbody Rigidbody;
        public Collider[] Colliders;
    }

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();
        interactableLayerMask = LayerMask.GetMask("Interactable");

        if (interactableLayerMask == 0)
        {
            Debug.LogWarning("ArtPickup: 'Interactable' layer not found.");
        }

        EnsureHoldPointExists();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryPickupFromRaycast();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && IsHolding)
        {
            HandleHeldClick();
        }
    }

    /// <summary>
    /// Snap carried items after <see cref="FirstPersonController"/> moves the camera this frame.
    /// </summary>
    private void LateUpdate()
    {
        ApplyCarryPoses();
    }

    public string GetInteractionPrompt()
    {
        if (!TryGetCenterRayHit(out RaycastHit hit))
        {
            return IsHolding ? BuildCarryPrompt("Click to drop") : string.Empty;
        }

        if (IsHolding)
        {
            PaintingMount mount = hit.collider.GetComponentInParent<PaintingMount>();
            InteractablePainting aimedPainting = hit.collider.GetComponentInParent<InteractablePainting>();

            if (mount != null)
            {
                if (mount.IsOccupied)
                {
                    return BuildCarryPrompt("Gallery spot taken");
                }

                if (mount.CanAccept(HeldPainting))
                {
                    return BuildCarryPrompt("Click to place on wall");
                }

                return BuildCarryPrompt($"This belongs in the {mount.RequiredWing} gallery");
            }

            if (aimedPainting != null && !IsInStack(aimedPainting.transform) && carryStack.Count < maxStackSize)
            {
                return BuildCarryPrompt($"E — add \"{aimedPainting.PaintingTitle}\" to stack");
            }

            if (aimedPainting != null && carryStack.Count >= maxStackSize)
            {
                return BuildCarryPrompt("Stack full — place or drop first");
            }

            return BuildCarryPrompt("Click to drop");
        }

        InteractablePainting painting = hit.collider.GetComponentInParent<InteractablePainting>();
        if (painting != null)
        {
            if (carryStack.Count >= maxStackSize)
            {
                return "Stack full — place or drop first";
            }

            return $"E — pick up \"{painting.PaintingTitle}\"";
        }

        return string.Empty;
    }

    private string BuildCarryPrompt(string action)
    {
        if (carryStack.Count <= 1)
        {
            return action;
        }

        string title = HeldPainting != null ? HeldPainting.PaintingTitle : "item";
        return $"{action} ({title}, stack {carryStack.Count}/{maxStackSize})";
    }

    public bool TryGetCenterRayHit(out RaycastHit hit)
    {
        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return Physics.Raycast(centerRay, out hit, maxPickupDistance, interactableLayerMask);
    }

    private void TryPickupFromRaycast()
    {
        if (carryStack.Count >= maxStackSize || !TryGetCenterRayHit(out RaycastHit hit))
        {
            return;
        }

        InteractablePainting painting = hit.collider.GetComponentInParent<InteractablePainting>();
        if (painting == null || IsInStack(painting.transform))
        {
            return;
        }

        AddToStack(painting.transform);
    }

    private void HandleHeldClick()
    {
        if (TryGetCenterRayHit(out RaycastHit hit))
        {
            PaintingMount mount = hit.collider.GetComponentInParent<PaintingMount>();
            if (mount != null)
            {
                if (mount.CanAccept(HeldPainting))
                {
                    PlaceActiveOnMount(mount);
                }

                return;
            }
        }

        DropActiveObject();
    }

    private void AddToStack(Transform target)
    {
        var entry = new CarriedEntry
        {
            Transform = target,
            Rigidbody = target.GetComponent<Rigidbody>(),
            Colliders = target.GetComponentsInChildren<Collider>(),
            Painting = target.GetComponent<InteractablePainting>()
        };

        if (entry.Painting != null && entry.Painting.CurrentMount != null)
        {
            entry.Painting.CurrentMount.ClearOccupant();
            entry.Painting.ClearMount();
        }

        // Unparent so carry pose is driven entirely by this script (standard FPS equip slot pattern).
        target.SetParent(null, true);

        foreach (Collider collider in entry.Colliders)
        {
            collider.enabled = false;
        }

        SuspendRigidbody(entry.Rigidbody);
        carryStack.Add(entry);
        ApplyCarryPoses();
    }

    /// <summary>
    /// Positions each stack slot in front of the hold point, fanned along camera right.
    /// </summary>
    private void ApplyCarryPoses()
    {
        if (carryStack.Count == 0 || holdPoint == null)
        {
            return;
        }

        Vector3 anchor = holdPoint.position;
        Vector3 right = playerCamera.transform.right;
        Vector3 forward = playerCamera.transform.forward;
        Quaternion faceCamera = GetCarryWorldRotation();

        for (int i = 0; i < carryStack.Count; i++)
        {
            Transform item = carryStack[i].Transform;
            if (item == null)
            {
                continue;
            }

            item.SetPositionAndRotation(
                anchor + right * (stackSlotSpacing * i) + forward * (stackForwardStep * i),
                faceCamera);
        }
    }

    /// <summary>
    /// Floor/wall paintings use Y as the thin axis when on the ground; map that axis toward the camera
    /// so the canvas faces the player (not edge-on).
    /// </summary>
    private Quaternion GetCarryWorldRotation()
    {
        Vector3 viewForward = playerCamera.transform.forward;
        if (viewForward.sqrMagnitude < 0.0001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.FromToRotation(Vector3.up, viewForward);
    }

    private void PlaceActiveOnMount(PaintingMount mount)
    {
        if (carryStack.Count == 0)
        {
            return;
        }

        CarriedEntry entry = carryStack[carryStack.Count - 1];
        carryStack.RemoveAt(carryStack.Count - 1);

        foreach (Collider collider in entry.Colliders)
        {
            collider.enabled = true;
        }

        RestoreRigidbody(entry.Rigidbody, kinematic: true, useGravity: false);
        mount.PlacePainting(entry.Transform, entry.Rigidbody);
        ApplyCarryPoses();
    }

    private void DropActiveObject()
    {
        if (carryStack.Count == 0)
        {
            return;
        }

        CarriedEntry entry = carryStack[carryStack.Count - 1];
        carryStack.RemoveAt(carryStack.Count - 1);

        foreach (Collider collider in entry.Colliders)
        {
            collider.enabled = true;
        }

        RestoreRigidbody(entry.Rigidbody, kinematic: false, useGravity: true);
        entry.Transform.position += playerCamera.transform.forward * 0.25f;
        ApplyCarryPoses();
    }

    private bool IsInStack(Transform target)
    {
        for (int i = 0; i < carryStack.Count; i++)
        {
            if (carryStack[i].Transform == target)
            {
                return true;
            }
        }

        return false;
    }

    private void EnsureHoldPointExists()
    {
        if (holdPoint == null)
        {
            holdPoint = transform.Find("HoldPoint");
        }

        if (holdPoint == null)
        {
            GameObject holdPointObject = new GameObject("HoldPoint");
            holdPointObject.transform.SetParent(transform, false);
            holdPoint = holdPointObject.transform;
        }

        // Position-only rig: identity rotation keeps stack offsets aligned with the camera.
        holdPoint.localPosition = new Vector3(0.12f, -0.18f, 0.62f);
        holdPoint.localRotation = Quaternion.identity;
    }

    private static void SuspendRigidbody(Rigidbody rigidbody)
    {
        if (rigidbody == null)
        {
            return;
        }

        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        rigidbody.detectCollisions = false;
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
