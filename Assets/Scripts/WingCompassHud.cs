using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD arrow pointing toward the nearest mount that accepts the carried painting.
/// </summary>
public class WingCompassHud : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArtPickup artPickup;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Text compassLabel;
    [SerializeField] private RectTransform needleRect;

    [Header("Display")]
    [SerializeField] private float minShowDistance = 2.5f;
    [SerializeField] private float maxShowDistance = 40f;

    private void Awake()
    {
        if (artPickup == null)
        {
            artPickup = GetComponent<ArtPickup>();
        }

        if (playerBody == null)
        {
            FirstPersonController fps = GetComponentInParent<FirstPersonController>();
            if (fps != null)
            {
                playerBody = fps.transform;
            }
        }

        SetVisible(false);
    }

    private void Update()
    {
        if (artPickup == null || playerBody == null || !artPickup.IsHolding || artPickup.HeldPainting == null)
        {
            SetVisible(false);
            return;
        }

        if (!MuseumNearestMountFinder.TryFindNearestAcceptingMount(
                artPickup.HeldPainting,
                playerBody.position,
                out PaintingMount mount,
                out float distance))
        {
            SetVisible(false);
            return;
        }

        if (distance < minShowDistance || distance > maxShowDistance)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        if (needleRect != null)
        {
            float angle = MuseumNearestMountFinder.SignedAngleToMount(
                playerBody.position,
                playerBody.forward,
                mount);
            needleRect.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }

        if (compassLabel != null)
        {
            string slot = mount.HasSpecificSlot ? $" \"{mount.SlotDisplayTitle}\"" : string.Empty;
            compassLabel.text = $"→ {mount.RequiredWing}{slot} · {distance:0}m";
        }
    }

    private void SetVisible(bool visible)
    {
        if (compassLabel != null)
        {
            compassLabel.enabled = visible;
        }

        if (needleRect != null)
        {
            needleRect.gameObject.SetActive(visible);
        }
    }
}
