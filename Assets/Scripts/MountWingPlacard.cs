using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space label on a mount frame showing wing (and optional reserved painting title).
/// Billboards toward the player camera when nearby.
/// </summary>
public class MountWingPlacard : MonoBehaviour
{
    [SerializeField] private GalleryWing requiredWing = GalleryWing.Modern;
    [SerializeField] private Text labelText;
    [SerializeField] private float billboardDistance = 24f;

    private PaintingMount sourceMount;

    public void Configure(PaintingMount mount)
    {
        sourceMount = mount;
        if (mount != null)
        {
            requiredWing = mount.RequiredWing;
        }

        RefreshLabel();
    }

    public void Configure(GalleryWing wing)
    {
        sourceMount = null;
        requiredWing = wing;
        RefreshLabel();
    }

    private void Awake()
    {
        RefreshLabel();
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        float distance = Vector3.Distance(cam.transform.position, transform.position);
        if (distance > billboardDistance)
        {
            return;
        }

        Vector3 toCamera = cam.transform.position - transform.position;
        toCamera.y = 0f;
        if (toCamera.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(-toCamera.normalized, Vector3.up);
        }
    }

    private void RefreshLabel()
    {
        if (labelText == null)
        {
            return;
        }

        if (sourceMount != null && sourceMount.HasSpecificSlot)
        {
            labelText.text = $"{FormatWingLabel(requiredWing)} · \"{sourceMount.SlotDisplayTitle}\"";
            return;
        }

        labelText.text = FormatWingLabel(requiredWing);
    }

    private static string FormatWingLabel(GalleryWing wing)
    {
        return wing switch
        {
            GalleryWing.Modern => "Modern gallery",
            GalleryWing.Classical => "Classical hall",
            GalleryWing.Impressionist => "Impressionist wing",
            _ => $"{wing} wing"
        };
    }
}
