using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small world-space label on a mount frame showing which wing belongs here.
/// </summary>
public class MountWingPlacard : MonoBehaviour
{
    [SerializeField] private GalleryWing requiredWing = GalleryWing.Modern;
    [SerializeField] private Text labelText;

    public void Configure(GalleryWing wing)
    {
        requiredWing = wing;
        RefreshLabel();
    }

    private void Awake()
    {
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (labelText != null)
        {
            labelText.text = $"{requiredWing} wing";
        }
    }
}
