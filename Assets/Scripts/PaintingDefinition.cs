using UnityEngine;

/// <summary>
/// Authoring asset for a museum painting. Swap art by changing the prefab or color here.
/// </summary>
[CreateAssetMenu(fileName = "PaintingDefinition", menuName = "Museum/Painting Definition")]
public class PaintingDefinition : ScriptableObject
{
    [SerializeField] private string paintingId;
    [SerializeField] private string title = "Untitled";
    [SerializeField] private GalleryWing wing = GalleryWing.Modern;
    [SerializeField] private Color displayColor = Color.gray;
    [SerializeField] private GameObject prefab;

    public string PaintingId => string.IsNullOrEmpty(paintingId) ? name : paintingId;
    public string Title => title;
    public GalleryWing Wing => wing;
    public Color DisplayColor => displayColor;
    public GameObject Prefab => prefab;
}
