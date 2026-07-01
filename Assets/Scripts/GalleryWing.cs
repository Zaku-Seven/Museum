/// <summary>
/// Themed museum wings used to sort paintings. A painting belongs to exactly one wing,
/// and a <see cref="PaintingMount"/> only accepts a painting whose wing matches its
/// <c>requiredWing</c>. Kept in the global namespace to match the rest of the scripts.
/// </summary>
public enum GalleryWing
{
    Modern,
    Classical,
    Impressionist,
    Fossil
}
