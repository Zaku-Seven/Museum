using UnityEngine;

/// <summary>
/// Colored floor marker showing which wing paintings belong on a nearby wall.
/// </summary>
public class WingZoneMarker : MonoBehaviour
{
    [SerializeField] private GalleryWing wing = GalleryWing.Modern;
    [SerializeField] private string zoneLabel = "Modern gallery";

    public GalleryWing Wing => wing;
    public string ZoneLabel => zoneLabel;

#if UNITY_EDITOR
    public void Configure(GalleryWing newWing, string label, Color floorTint)
    {
        wing = newWing;
        zoneLabel = label;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial.color = floorTint;
        }
    }
#endif
}
