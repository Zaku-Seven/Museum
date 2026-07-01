using UnityEngine;

/// <summary>
/// Highlights what the player is aiming at: mount frames tint green/red when carrying a
/// painting, and floor paintings get a subtle pulse when viewed empty-handed.
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
    [SerializeField] private float mountHighlightIntensity = 1.4f;
    [SerializeField] private float wrongPulseSpeed = 6f;

    [Header("Floor painting (empty-handed)")]
    [SerializeField] private Color floorHighlightColor = new Color(1f, 1f, 1f);
    [SerializeField] private float floorHighlightIntensity = 0.35f;
    [SerializeField] private float floorPulseSpeed = 4f;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
    private Renderer highlightedRenderer;
    private HighlightMode activeMode;

    private enum HighlightMode
    {
        None,
        MountValid,
        MountWrong,
        MountOccupied,
        FloorPainting
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
            ClearHighlight();
            return;
        }

        if (!artPickup.TryGetCenterRayHit(out RaycastHit hit))
        {
            ClearHighlight();
            return;
        }

        if (artPickup.IsHolding)
        {
            UpdateMountHighlight(hit);
            return;
        }

        UpdateFloorPaintingHighlight(hit);
    }

    private void UpdateMountHighlight(RaycastHit hit)
    {
        PaintingMount mount = hit.collider.GetComponentInParent<PaintingMount>();
        if (mount == null)
        {
            ClearHighlight();
            return;
        }

        Renderer frameRenderer = mount.GetComponentInChildren<Renderer>();
        if (frameRenderer == null)
        {
            ClearHighlight();
            return;
        }

        if (mount.IsOccupied)
        {
            ApplyHighlight(frameRenderer, HighlightMode.MountOccupied, occupiedMountColor, pulse: false);
            return;
        }

        if (mount.CanAccept(artPickup.HeldPainting))
        {
            ApplyHighlight(frameRenderer, HighlightMode.MountValid, validMountColor, pulse: false);
            return;
        }

        float pulse = 0.55f + 0.45f * Mathf.Sin(Time.time * wrongPulseSpeed);
        ApplyHighlight(frameRenderer, HighlightMode.MountWrong, wrongMountColor * pulse, pulse: true);
    }

    private void UpdateFloorPaintingHighlight(RaycastHit hit)
    {
        InteractablePainting painting = hit.collider.GetComponentInParent<InteractablePainting>();
        if (painting == null || painting.CurrentMount != null)
        {
            ClearHighlight();
            return;
        }

        Renderer canvasRenderer = painting.GetComponent<Renderer>();
        if (canvasRenderer == null)
        {
            ClearHighlight();
            return;
        }

        float pulse = 0.65f + 0.35f * Mathf.Sin(Time.time * floorPulseSpeed);
        ApplyHighlight(canvasRenderer, HighlightMode.FloorPainting, floorHighlightColor * pulse, pulse: true);
    }

    private void ApplyHighlight(Renderer renderer, HighlightMode mode, Color color, bool pulse)
    {
        if (highlightedRenderer != renderer || activeMode != mode)
        {
            ClearHighlight();
            highlightedRenderer = renderer;
            activeMode = mode;
        }

        Color emission = mode == HighlightMode.FloorPainting
            ? color * floorHighlightIntensity
            : color * (pulse ? mountHighlightIntensity : mountHighlightIntensity * 0.85f);
        propertyBlock.SetColor(EmissionColorId, emission);
        renderer.SetPropertyBlock(propertyBlock);
    }

    private void ClearHighlight()
    {
        if (highlightedRenderer != null)
        {
            highlightedRenderer.SetPropertyBlock(null);
            highlightedRenderer = null;
        }

        activeMode = HighlightMode.None;
    }

    private void OnDisable()
    {
        ClearHighlight();
    }
}
