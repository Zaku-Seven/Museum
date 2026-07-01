using UnityEngine;

/// <summary>
/// Highlights what the player is aiming at: mount frames tint green/red when carrying a
/// painting, floor paintings get a subtle pulse when viewed empty-handed, and the active
/// stack item gets a forward carry glow while holding multiple paintings.
/// Uses <see cref="MaterialPropertyBlock"/> so base materials are never mutated.
/// </summary>
[RequireComponent(typeof(Camera))]
public class MountAimHighlighter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArtPickup artPickup;

    [Header("Mount aim (while carrying)")]
    [SerializeField] private Color validMountColor = new Color(0.35f, 0.95f, 0.45f);
    [SerializeField] private Color wrongMountColor = new Color(0.95f, 0.3f, 0.25f);
    [SerializeField] private Color occupiedMountColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private float mountHighlightIntensity = 1.65f;
    [SerializeField] private float wrongPulseSpeed = 6f;
    [SerializeField] private float validMountPulseSpeed = 2.5f;

    [Header("Floor painting (empty-handed)")]
    [SerializeField] private Color floorHighlightColor = new Color(1f, 1f, 1f);
    [SerializeField] private Color stagedHighlightColor = new Color(0.55f, 0.85f, 0.95f);
    [SerializeField] private float floorHighlightIntensity = 0.35f;
    [SerializeField] private float stagedHighlightIntensity = 0.5f;
    [SerializeField] private float floorPulseSpeed = 4f;

    [Header("Active stack item (while carrying)")]
    [SerializeField] private Color activeCarryColor = new Color(0.85f, 0.95f, 1f);
    [SerializeField] private float activeCarryIntensity = 0.55f;
    [SerializeField] private float activeCarryPulseSpeed = 5f;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
    private readonly MaterialPropertyBlock carryPropertyBlock = new MaterialPropertyBlock();
    private Renderer highlightedRenderer;
    private Renderer carryHighlightedRenderer;
    private HighlightMode activeMode;

    private enum HighlightMode
    {
        None,
        MountValid,
        MountWrong,
        MountOccupied,
        FloorPainting,
        StagedPainting
    }

    private void Awake()
    {
        if (artPickup == null)
        {
            artPickup = GetComponent<ArtPickup>();
        }
    }

    private void LateUpdate()
    {
        if (artPickup == null)
        {
            ClearAllHighlights();
            return;
        }

        UpdateActiveCarryHighlight();

        if (!artPickup.TryGetCenterRayHit(out RaycastHit hit))
        {
            ClearAimHighlight();
            return;
        }

        if (artPickup.IsHolding)
        {
            UpdateMountHighlight(hit);
            return;
        }

        UpdateFloorPaintingHighlight(hit);
    }

    private void UpdateActiveCarryHighlight()
    {
        if (!artPickup.IsHolding || artPickup.CarryCount <= 1)
        {
            ClearCarryHighlight();
            return;
        }

        Transform activeTransform = artPickup.ActiveCarriedTransform;
        if (activeTransform == null)
        {
            ClearCarryHighlight();
            return;
        }

        Renderer canvasRenderer = activeTransform.GetComponent<Renderer>();
        if (canvasRenderer == null)
        {
            ClearCarryHighlight();
            return;
        }

        if (carryHighlightedRenderer != canvasRenderer)
        {
            ClearCarryHighlight();
            carryHighlightedRenderer = canvasRenderer;
        }

        float pulse = 0.7f + 0.3f * Mathf.Sin(Time.time * activeCarryPulseSpeed);
        carryPropertyBlock.SetColor(EmissionColorId, activeCarryColor * (activeCarryIntensity * pulse));
        canvasRenderer.SetPropertyBlock(carryPropertyBlock);
    }

    private void UpdateMountHighlight(RaycastHit hit)
    {
        PaintingMount mount = hit.collider.GetComponentInParent<PaintingMount>();
        if (mount == null)
        {
            ClearAimHighlight();
            return;
        }

        Renderer frameRenderer = mount.GetComponentInChildren<Renderer>();
        if (frameRenderer == null)
        {
            ClearAimHighlight();
            return;
        }

        if (mount.IsOccupied)
        {
            ApplyAimHighlight(frameRenderer, HighlightMode.MountOccupied, occupiedMountColor, pulse: false);
            return;
        }

        if (mount.CanAccept(artPickup.HeldPainting))
        {
            float pulse = MuseumMotionSettings.Pulse01(validMountPulseSpeed);
            ApplyAimHighlight(frameRenderer, HighlightMode.MountValid, validMountColor * pulse, pulse: true);
            return;
        }

        float wrongPulse = MuseumMotionSettings.ReduceMotion
            ? 1f
            : 0.55f + 0.45f * Mathf.Sin(Time.time * wrongPulseSpeed);
        ApplyAimHighlight(frameRenderer, HighlightMode.MountWrong, wrongMountColor * wrongPulse, pulse: true);
    }

    private void UpdateFloorPaintingHighlight(RaycastHit hit)
    {
        InteractablePainting painting = hit.collider.GetComponentInParent<InteractablePainting>();
        if (painting == null || painting.CurrentMount != null)
        {
            ClearAimHighlight();
            return;
        }

        Renderer canvasRenderer = painting.GetComponent<Renderer>();
        if (canvasRenderer == null)
        {
            ClearAimHighlight();
            return;
        }

        bool staged = SortingTable.IsStaged(painting.transform);
        float pulse = MuseumMotionSettings.Pulse01(floorPulseSpeed);
        if (staged)
        {
            ApplyAimHighlight(canvasRenderer, HighlightMode.StagedPainting, stagedHighlightColor * pulse, pulse: true);
            return;
        }

        ApplyAimHighlight(canvasRenderer, HighlightMode.FloorPainting, floorHighlightColor * pulse, pulse: true);
    }

    private void ApplyAimHighlight(Renderer renderer, HighlightMode mode, Color color, bool pulse)
    {
        if (highlightedRenderer != renderer || activeMode != mode)
        {
            ClearAimHighlight();
            highlightedRenderer = renderer;
            activeMode = mode;
        }

        Color emission = mode == HighlightMode.FloorPainting
            ? color * floorHighlightIntensity
            : mode == HighlightMode.StagedPainting
                ? color * stagedHighlightIntensity
                : color * (pulse ? mountHighlightIntensity : mountHighlightIntensity * 0.85f);
        propertyBlock.SetColor(EmissionColorId, emission);
        renderer.SetPropertyBlock(propertyBlock);
    }

    private void ClearAimHighlight()
    {
        if (highlightedRenderer != null)
        {
            highlightedRenderer.SetPropertyBlock(null);
            highlightedRenderer = null;
        }

        activeMode = HighlightMode.None;
    }

    private void ClearCarryHighlight()
    {
        if (carryHighlightedRenderer != null)
        {
            carryHighlightedRenderer.SetPropertyBlock(null);
            carryHighlightedRenderer = null;
        }
    }

    private void ClearAllHighlights()
    {
        ClearAimHighlight();
        ClearCarryHighlight();
    }

    private void OnDisable()
    {
        ClearAllHighlights();
    }
}
