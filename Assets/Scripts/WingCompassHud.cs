using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD arrow pointing toward the nearest mount that accepts the carried painting,
/// or away from a wrong-wing mount the player is aiming at.
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
    [SerializeField] private Color validNeedleColor = new Color(0.75f, 0.95f, 0.8f);
    [SerializeField] private Color wrongWingNeedleColor = new Color(1f, 0.45f, 0.4f);

    private Text needleText;

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

        if (needleRect != null)
        {
            needleText = needleRect.GetComponent<Text>();
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

        InteractablePainting held = artPickup.HeldPainting;

        if (TryShowWrongWingHint(held))
        {
            return;
        }

        if (!MuseumNearestMountFinder.TryFindNearestAcceptingMount(
                held,
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

        ShowNeedleTowardMount(mount, distance, validNeedleColor, $"→ {mount.RequiredWing}{FormatSlot(mount)} · {distance:0}m");
    }

    private bool TryShowWrongWingHint(InteractablePainting held)
    {
        if (!artPickup.TryGetCenterRayHit(out RaycastHit hit))
        {
            return false;
        }

        PaintingMount aimedMount = hit.collider.GetComponentInParent<PaintingMount>();
        if (aimedMount == null || aimedMount.IsOccupied || aimedMount.CanAccept(held))
        {
            return false;
        }

        if (aimedMount.RequiredWing == held.Wing)
        {
            return false;
        }

        if (!MuseumNearestMountFinder.TryFindNearestAcceptingMount(
                held,
                playerBody.position,
                out PaintingMount correctMount,
                out float distance))
        {
            SetVisible(true);
            SetNeedleColor(wrongWingNeedleColor);
            if (compassLabel != null)
            {
                compassLabel.text = $"Wrong wing — need {held.Wing}, not {aimedMount.RequiredWing}";
            }

            return true;
        }

        ShowNeedleTowardMount(
            correctMount,
            distance,
            wrongWingNeedleColor,
            $"← {held.Wing} gallery · not {aimedMount.RequiredWing} · {distance:0}m");
        return true;
    }

    private void ShowNeedleTowardMount(PaintingMount mount, float distance, Color color, string label)
    {
        TutorialHints.TryShowCompassHint();
        SetVisible(true);
        SetNeedleColor(color);

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
            compassLabel.text = label;
        }
    }

    private static string FormatSlot(PaintingMount mount)
    {
        return mount.HasSpecificSlot ? $" \"{mount.SlotDisplayTitle}\"" : string.Empty;
    }

    private void SetNeedleColor(Color color)
    {
        if (needleText != null)
        {
            needleText.color = color;
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
