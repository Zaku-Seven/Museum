using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Raycast pick-up, multi-item carry stack, wall placement, and drop for museum paintings.
/// <b>E</b> adds a painting to the stack (offset to the right of the hold point).
/// <b>Left click</b> places or drops the front-most stacked item (rightmost / last picked).
/// Items are parented to the hold point so they move with zero lag.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ArtPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float maxPickupDistance = 3.5f;
    [SerializeField] private Transform holdPoint;

    [Header("Carry Stack")]
    [SerializeField] private int maxStackSize = 5;
    [Tooltip("Local-space offset per stack slot along the hold point's right axis.")]
    [SerializeField] private float stackSlotSpacing = 0.14f;
    [SerializeField] private Vector3 carryLocalEuler = new Vector3(-90f, 0f, 0f);

    private Camera playerCamera;
    private int interactableLayerMask;
    private readonly List<CarriedEntry> carryStack = new List<CarriedEntry>();
    private Quaternion carryLocalRotation;

    public bool IsHolding => carryStack.Count > 0;
    public int CarryCount => carryStack.Count;

    /// <summary>The painting currently offered for placement (last picked / rightmost in stack).</summary>
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
        carryLocalRotation = Quaternion.Euler(carryLocalEuler);

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
        if (carryStack.Count >= maxStackSize)
        {
            return;
        }

        if (!TryGetCenterRayHit(out RaycastHit hit))
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

        foreach (Collider collider in entry.Colliders)
        {
            collider.enabled = false;
        }

        if (entry.Rigidbody != null)
        {
            entry.Rigidbody.linearVelocity = Vector3.zero;
            entry.Rigidbody.angularVelocity = Vector3.zero;
            entry.Rigidbody.isKinematic = true;
            entry.Rigidbody.useGravity = false;
            entry.Rigidbody.detectCollisions = false;
        }

        carryStack.Add(entry);
        RefreshStackLayout();
    }

    private void RefreshStackLayout()
    {
        for (int i = 0; i < carryStack.Count; i++)
        {
            CarriedEntry entry = carryStack[i];
            entry.Transform.SetParent(holdPoint, false);
            entry.Transform.localPosition = GetStackLocalOffset(i);
            entry.Transform.localRotation = carryLocalRotation;
        }
    }

    private Vector3 GetStackLocalOffset(int stackIndex)
    {
        return new Vector3(stackSlotSpacing * stackIndex, 0f, 0f);
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

        entry.Transform.SetParent(null, true);
        RestoreRigidbody(entry.Rigidbody, kinematic: true, useGravity: false);
        mount.PlacePainting(entry.Transform, entry.Rigidbody);
        RefreshStackLayout();
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

        entry.Transform.SetParent(null, true);
        RestoreRigidbody(entry.Rigidbody, kinematic: false, useGravity: true);
        entry.Transform.position += playerCamera.transform.forward * 0.25f;
        RefreshStackLayout();
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

        holdPoint.localPosition = new Vector3(0f, -0.22f, 0.68f);
        holdPoint.localRotation = Quaternion.Euler(6f, 0f, 0f);
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
