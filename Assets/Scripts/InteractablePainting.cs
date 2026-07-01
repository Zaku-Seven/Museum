using UnityEngine;

/// <summary>
/// Marks a GameObject as a museum painting the player can pick up and hang.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class InteractablePainting : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private string paintingTitle = "Untitled";

    [Header("Sorting")]
    [Tooltip("Which gallery wing this painting belongs to. Mounts only accept a matching wing.")]
    [SerializeField] private GalleryWing wing = GalleryWing.Modern;

    public string PaintingTitle => paintingTitle;
    public GalleryWing Wing => wing;
    public PaintingMount CurrentMount { get; private set; }

    public void SetMount(PaintingMount mount)
    {
        if (CurrentMount != null && CurrentMount != mount)
        {
            CurrentMount.ClearOccupant();
        }

        CurrentMount = mount;
    }

    public void ClearMount()
    {
        CurrentMount = null;
    }
}
