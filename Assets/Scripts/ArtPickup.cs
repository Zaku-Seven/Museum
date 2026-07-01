using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First-person carry: E adds to stack, scroll selects primary item, click place/drop, right-click undo last hang.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ArtPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float maxPickupDistance = 3.5f;
    [SerializeField] private Transform holdPoint;

    [Header("Carry Stack")]
    [SerializeField] private int maxStackSize = 5;
    [SerializeField] private float stackSlotSpacing = 0.16f;
    [SerializeField] private float stackForwardStep = 0.015f;
    [SerializeField] private float activeItemForwardBoost = 0.08f;
    [SerializeField] private float activeItemScaleBoost = 1.06f;

    [Header("Throw")]
    [SerializeField] private float throwForce = 9f;
    [SerializeField] private float throwUpwardBoost = 1.5f;
    [SerializeField] private float throwSpin = 4f;

    private Camera playerCamera;
    private int interactableLayerMask;
    private readonly List<CarriedEntry> carryStack = new List<CarriedEntry>();
    private readonly Stack<UndoRecord> placementUndoStack = new Stack<UndoRecord>();
    private int activeStackIndex;

    public bool IsHolding => carryStack.Count > 0;
    public int CarryCount => carryStack.Count;
    public int ActiveStackIndex => activeStackIndex;

    public InteractablePainting HeldPainting =>
        carryStack.Count > 0 ? carryStack[activeStackIndex].Painting : null;

    public Transform ActiveCarriedTransform =>
        carryStack.Count > 0 ? carryStack[activeStackIndex].Transform : null;

    private struct UndoRecord
    {
        public PaintingMount Mount;
        public InteractablePainting Painting;
    }

    private sealed class CarriedEntry
    {
        public Transform Transform;
        public InteractablePainting Painting;
        public Rigidbody Rigidbody;
        public Collider[] Colliders;
        public Vector3 BaseLocalScale;
    }

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();
        interactableLayerMask = LayerMask.GetMask("Interactable");
        EnsureHoldPointExists();
    }

    private void Update()
    {
        if (MuseumProgress.Instance != null && MuseumProgress.Instance.IsMuseumComplete)
        {
            return;
        }

        if (MuseumInput.InteractPressedThisFrame())
        {
            TryPickupFromRaycast();
        }

        HandleScrollStackSelection();

        if (MuseumInput.PlacePressedThisFrame() && IsHolding)
        {
            HandleHeldClick();
        }

        if (MuseumInput.UndoPressedThisFrame())
        {
            TryUndoLastPlacement();
        }

        if (MuseumInput.ThrowPressedThisFrame() && IsHolding)
        {
            ThrowActiveObject();
        }
    }

    private void LateUpdate()
    {
        ApplyCarryPoses();
    }

    private void HandleScrollStackSelection()
    {
        if (carryStack.Count <= 1 || Mouse.current == null && Gamepad.current == null)
        {
            return;
        }

        float scroll = MuseumInput.StackScrollDelta();
        if (scroll > 0.05f)
        {
            activeStackIndex = (activeStackIndex + 1) % carryStack.Count;
            TutorialHints.TryShowStackScrollHint();
        }
        else if (scroll < -0.05f)
        {
            activeStackIndex = (activeStackIndex - 1 + carryStack.Count) % carryStack.Count;
            TutorialHints.TryShowStackScrollHint();
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
                    string title = HeldPainting != null ? HeldPainting.PaintingTitle : "painting";
                    return BuildCarryPrompt($"Click to hang \"{title}\" ({mount.RequiredWing})");
                }

                if (HeldPainting != null && mount.RequiredWing == HeldPainting.Wing && mount.HasSpecificSlot)
                {
                    return BuildCarryPrompt($"Spot reserved for \"{mount.SlotDisplayTitle}\"");
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

        if (hit.collider.GetComponentInParent<WingZoneMarker>() is WingZoneMarker zone)
        {
            return $"Floor zone: {zone.ZoneLabel}";
        }

        PaintingMount emptyMount = hit.collider.GetComponentInParent<PaintingMount>();
        if (emptyMount != null)
        {
            if (emptyMount.IsOccupied && emptyMount.Occupant != null)
            {
                return $"Hung: \"{emptyMount.Occupant.PaintingTitle}\" ({emptyMount.RequiredWing})";
            }

            if (emptyMount.HasSpecificSlot)
            {
                return $"{emptyMount.RequiredWing} · \"{emptyMount.SlotDisplayTitle}\" — empty";
            }

            return $"{emptyMount.RequiredWing} wing mount — empty";
        }

        InteractablePainting painting = hit.collider.GetComponentInParent<InteractablePainting>();
        if (painting != null && SortingTable.IsStaged(painting.transform))
        {
            return $"Staged: \"{painting.PaintingTitle}\" — E to pick up";
        }

        if (painting != null)
        {
            if (painting.CurrentMount != null)
            {
                if (carryStack.Count >= maxStackSize)
                {
                    return "Stack full — place or drop first";
                }

                return $"E — take down \"{painting.PaintingTitle}\"";
            }

            if (carryStack.Count >= maxStackSize)
            {
                return "Stack full — place or drop first";
            }

            return $"E — pick up \"{painting.PaintingTitle}\"";
        }

        return string.Empty;
    }

    public string GetActiveStackLabel()
    {
        if (!IsHolding || HeldPainting == null)
        {
            return string.Empty;
        }

        if (carryStack.Count <= 1)
        {
            return string.Empty;
        }

        return $"Placing: \"{HeldPainting.PaintingTitle}\" ({activeStackIndex + 1}/{carryStack.Count}) · scroll to change";
    }

    /// <summary>HUD hint for where to hang the active painting.</summary>
    public string GetWingGuidanceLine()
    {
        if (!IsHolding || HeldPainting == null)
        {
            return string.Empty;
        }

        PaintingMount[] mounts = FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        return MuseumWingGuide.BuildGuidance(HeldPainting, mounts);
    }

    private string BuildCarryPrompt(string action)
    {
        if (carryStack.Count <= 1)
        {
            return $"{action} · Q throw";
        }

        string title = HeldPainting != null ? HeldPainting.PaintingTitle : "item";
        return $"{action} ({title}, {activeStackIndex + 1}/{carryStack.Count}) · Q throw";
    }

    public bool TryGetCenterRayHit(out RaycastHit hit)
    {
        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return Physics.Raycast(centerRay, out hit, maxPickupDistance, interactableLayerMask);
    }

    /// <summary>Clears carry stack, undo history, and restores held colliders.</summary>
    public void ClearSession()
    {
        for (int i = carryStack.Count - 1; i >= 0; i--)
        {
            CarriedEntry entry = carryStack[i];
            if (entry.Transform == null)
            {
                continue;
            }

            foreach (Collider collider in entry.Colliders)
            {
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }

            entry.Transform.localScale = entry.BaseLocalScale;
            RestoreRigidbody(entry.Rigidbody, kinematic: false, useGravity: true);
        }

        carryStack.Clear();
        placementUndoStack.Clear();
        activeStackIndex = 0;
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
                else if (HeldPainting != null)
                {
                    if (mount.RequiredWing != HeldPainting.Wing)
                    {
                        TutorialHints.TryShowWrongWingHint();
                        MuseumGameEvents.RaiseWrongWingRejected();
                    }
                    else if (mount.HasSpecificSlot)
                    {
                        TutorialHints.TryShowWrongSlotHint();
                        MuseumGameEvents.RaiseWrongSlotRejected();
                    }
                }

                return;
            }
        }

        DropActiveObject();
    }

    private void AddToStack(Transform target)
    {
        SortingTable.UnregisterIfPresent(target);

        var entry = new CarriedEntry
        {
            Transform = target,
            Rigidbody = target.GetComponent<Rigidbody>(),
            Colliders = target.GetComponentsInChildren<Collider>(),
            Painting = target.GetComponent<InteractablePainting>(),
            BaseLocalScale = target.localScale
        };

        if (entry.Painting != null && entry.Painting.CurrentMount != null)
        {
            entry.Painting.CurrentMount.ClearOccupant();
            entry.Painting.ClearMount();
        }

        target.SetParent(null, true);

        foreach (Collider collider in entry.Colliders)
        {
            collider.enabled = false;
        }

        SuspendRigidbody(entry.Rigidbody);
        carryStack.Add(entry);
        activeStackIndex = carryStack.Count - 1;
        ApplyCarryPoses();

        if (entry.Painting != null)
        {
            MuseumGameEvents.RaisePaintingPickedUp(entry.Painting);
            TutorialHints.TryShowPickupHint();
            if (carryStack.Count > 1)
            {
                TutorialHints.TryShowStackScrollHint();
            }
        }
    }

    private void ApplyCarryPoses()
    {
        if (carryStack.Count == 0 || holdPoint == null)
        {
            return;
        }

        activeStackIndex = Mathf.Clamp(activeStackIndex, 0, carryStack.Count - 1);

        Vector3 anchor = holdPoint.position;
        Vector3 right = playerCamera.transform.right;
        Vector3 forward = playerCamera.transform.forward;
        Quaternion faceCamera = GetCarryWorldRotation();

        for (int i = 0; i < carryStack.Count; i++)
        {
            CarriedEntry entry = carryStack[i];
            if (entry.Transform == null)
            {
                continue;
            }

            bool isActive = i == activeStackIndex;
            float forwardExtra = isActive ? activeItemForwardBoost : 0f;
            float scaleMul = isActive ? activeItemScaleBoost : 1f;

            entry.Transform.SetPositionAndRotation(
                anchor + right * (stackSlotSpacing * i) + forward * (stackForwardStep * i + forwardExtra),
                faceCamera);
            entry.Transform.localScale = entry.BaseLocalScale * scaleMul;
        }
    }

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

        CarriedEntry entry = carryStack[activeStackIndex];
        carryStack.RemoveAt(activeStackIndex);

        foreach (Collider collider in entry.Colliders)
        {
            collider.enabled = true;
        }

        entry.Transform.localScale = entry.BaseLocalScale;
        RestoreRigidbody(entry.Rigidbody, kinematic: true, useGravity: false);
        mount.PlacePainting(entry.Transform, entry.Rigidbody);

        if (entry.Painting != null)
        {
            placementUndoStack.Push(new UndoRecord { Mount = mount, Painting = entry.Painting });
        }

        ClampActiveIndex();
        ApplyCarryPoses();
    }

    private void DropActiveObject()
    {
        if (carryStack.Count == 0)
        {
            return;
        }

        CarriedEntry entry = carryStack[activeStackIndex];
        carryStack.RemoveAt(activeStackIndex);

        foreach (Collider collider in entry.Colliders)
        {
            collider.enabled = true;
        }

        entry.Transform.localScale = entry.BaseLocalScale;
        RestoreRigidbody(entry.Rigidbody, kinematic: false, useGravity: true);
        entry.Transform.position += playerCamera.transform.forward * 0.25f;

        if (!SortingTable.TryStageOnAnyTable(entry.Transform) && entry.Painting != null)
        {
            MuseumGameEvents.RaisePaintingDropped(entry.Painting);
        }

        ClampActiveIndex();
        ApplyCarryPoses();
    }

    private void ThrowActiveObject()
    {
        if (carryStack.Count == 0)
        {
            return;
        }

        CarriedEntry entry = carryStack[activeStackIndex];
        carryStack.RemoveAt(activeStackIndex);

        foreach (Collider collider in entry.Colliders)
        {
            collider.enabled = true;
        }

        entry.Transform.localScale = entry.BaseLocalScale;
        entry.Transform.position = holdPoint != null ? holdPoint.position : entry.Transform.position;

        Vector3 throwDirection = playerCamera.transform.forward + Vector3.up * (throwUpwardBoost / Mathf.Max(throwForce, 0.01f));
        throwDirection.Normalize();

        if (entry.Rigidbody != null)
        {
            entry.Rigidbody.isKinematic = false;
            entry.Rigidbody.useGravity = true;
            entry.Rigidbody.detectCollisions = true;
            entry.Rigidbody.linearVelocity = throwDirection * throwForce;
            entry.Rigidbody.angularVelocity = playerCamera.transform.right * throwSpin;
        }

        if (entry.Painting != null)
        {
            MuseumGameEvents.RaisePaintingThrown(entry.Painting);
            TutorialHints.TryShowThrowHint();
        }

        ClampActiveIndex();
        ApplyCarryPoses();
    }

    private void TryUndoLastPlacement()
    {
        if (placementUndoStack.Count == 0 || carryStack.Count >= maxStackSize)
        {
            return;
        }

        UndoRecord record = placementUndoStack.Pop();
        if (record.Mount == null || record.Painting == null || !record.Mount.IsOccupied)
        {
            return;
        }

        InteractablePainting takenDown = record.Mount.TakeDownPainting();
        if (takenDown == null)
        {
            return;
        }

        AddToStack(takenDown.transform);
        MuseumGameEvents.RaisePlacementUndone(takenDown, record.Mount);
    }

    private void ClampActiveIndex()
    {
        if (carryStack.Count == 0)
        {
            activeStackIndex = 0;
            return;
        }

        activeStackIndex = Mathf.Clamp(activeStackIndex, 0, carryStack.Count - 1);
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

        holdPoint.localPosition = new Vector3(0.42f, -0.2f, 0.58f);
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
