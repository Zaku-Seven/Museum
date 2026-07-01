using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates interactable paintings and fossil pieces with varied primitive silhouettes.
/// </summary>
public static class MuseumArtFactory
{
    public enum ShapeKind
    {
        StandardCanvas,
        PortraitTall,
        LandscapeWide,
        Miniature,
        OversizedSquare,
        RoundMedallion,
        BoneSegment,
        SkullBlock,
        ShellFossil,
        StatuePlinth
    }

    public readonly struct ArtSpec
    {
        public string Title;
        public GalleryWing Wing;
        public Color Color;
        public Vector3 Position;
        public ShapeKind Shape;
        public string EntityId;

        public ArtSpec(string title, GalleryWing wing, Color color, Vector3 position, ShapeKind shape, string entityId = null)
        {
            Title = title;
            Wing = wing;
            Color = color;
            Position = position;
            Shape = shape;
            EntityId = entityId;
        }
    }

    public static GameObject CreateFloorPiece(Transform parent, string objectName, ArtSpec spec, int interactableLayer)
    {
        if (GameObject.Find(objectName) != null)
        {
            return GameObject.Find(objectName);
        }

        (PrimitiveType primitive, Vector3 scale, Quaternion rotation) = ResolveShape(spec.Shape);
        GameObject piece = GameObject.CreatePrimitive(primitive);
        piece.name = objectName;
        piece.transform.SetParent(parent, false);
        piece.layer = interactableLayer;
        piece.transform.position = spec.Position;
        piece.transform.localScale = scale;
        piece.transform.rotation = rotation;

        float mass = Mathf.Clamp(scale.x * scale.y * scale.z * 8f, 0.5f, 12f);
        Rigidbody body = piece.GetComponent<Rigidbody>() ?? piece.AddComponent<Rigidbody>();
        body.mass = mass;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        InteractablePainting interactable = piece.AddComponent<InteractablePainting>();
        SerializedObject serialized = new SerializedObject(interactable);
        SetString(serialized, "paintingTitle", spec.Title);
        SetEnum(serialized, "wing", (int)spec.Wing);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        ApplyColorMaterial(piece, spec.Color);
        string entityId = string.IsNullOrEmpty(spec.EntityId) ? BuildEntityId(spec.Title) : spec.EntityId;
        MuseumEntityIdUtility.EnsureEntityId(piece, entityId);
        return piece;
    }

    public static GameObject CreateFossilMount(
        Transform parent,
        string mountName,
        Vector3 position,
        Quaternion rotation,
        GalleryWing wing,
        int interactableLayer,
        string slotPaintingId,
        string slotDisplayTitle,
        Color frameColor,
        Vector3 frameScale)
    {
        Transform existing = parent.Find(mountName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject mountRoot = new GameObject(mountName);
        mountRoot.transform.SetParent(parent, false);
        mountRoot.transform.position = position;
        mountRoot.transform.rotation = rotation;
        mountRoot.layer = interactableLayer;

        GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "Frame";
        frame.transform.SetParent(mountRoot.transform, false);
        frame.transform.localScale = frameScale;
        frame.layer = interactableLayer;
        Object.DestroyImmediate(frame.GetComponent<Rigidbody>());
        ApplyColorMaterial(frame, frameColor);

        GameObject snapPoint = new GameObject("SnapPoint");
        snapPoint.transform.SetParent(mountRoot.transform, false);
        snapPoint.transform.localPosition = new Vector3(0f, 0f, -0.04f);

        PaintingMount mount = mountRoot.AddComponent<PaintingMount>();
        SerializedObject serializedMount = new SerializedObject(mount);
        SetObjectReference(serializedMount, "snapPoint", snapPoint.transform);
        SetEnum(serializedMount, "requiredWing", (int)wing);
        SetString(serializedMount, "requiredPaintingId", slotPaintingId);
        SetString(serializedMount, "slotDisplayTitle", slotDisplayTitle);
        serializedMount.ApplyModifiedPropertiesWithoutUndo();

        BoxCollider collider = mountRoot.AddComponent<BoxCollider>();
        collider.size = new Vector3(frameScale.x, frameScale.y, frameScale.z + 0.08f);

        MuseumEntityIdUtility.EnsureEntityId(mountRoot, $"mount_{Sanitize(mountName)}");
        MountPlacardSetup.EnsurePlacard(mount);
        return mountRoot;
    }

    public static string BuildEntityId(string title) => "painting_" + Sanitize(title).ToLowerInvariant();

    public static string BuildFossilEntityId(string title) => "fossil_" + Sanitize(title).ToLowerInvariant();

    private static (PrimitiveType primitive, Vector3 scale, Quaternion rotation) ResolveShape(ShapeKind shape)
    {
        return shape switch
        {
            ShapeKind.PortraitTall => (PrimitiveType.Cube, new Vector3(0.32f, 0.05f, 0.88f), Quaternion.identity),
            ShapeKind.LandscapeWide => (PrimitiveType.Cube, new Vector3(1.15f, 0.05f, 0.52f), Quaternion.identity),
            ShapeKind.Miniature => (PrimitiveType.Cube, new Vector3(0.22f, 0.03f, 0.18f), Quaternion.identity),
            ShapeKind.OversizedSquare => (PrimitiveType.Cube, new Vector3(0.95f, 0.06f, 0.95f), Quaternion.identity),
            ShapeKind.RoundMedallion => (PrimitiveType.Cylinder, new Vector3(0.42f, 0.04f, 0.42f), Quaternion.Euler(90f, 0f, 0f)),
            ShapeKind.BoneSegment => (PrimitiveType.Capsule, new Vector3(0.18f, 0.35f, 0.18f), Quaternion.Euler(0f, 0f, 90f)),
            ShapeKind.SkullBlock => (PrimitiveType.Cube, new Vector3(0.45f, 0.38f, 0.52f), Quaternion.identity),
            ShapeKind.ShellFossil => (PrimitiveType.Sphere, new Vector3(0.38f, 0.22f, 0.38f), Quaternion.identity),
            ShapeKind.StatuePlinth => (PrimitiveType.Cylinder, new Vector3(0.28f, 0.55f, 0.28f), Quaternion.identity),
            _ => (PrimitiveType.Cube, new Vector3(0.6f, 0.05f, 0.4f), Quaternion.identity)
        };
    }

    private static void ApplyColorMaterial(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        renderer.sharedMaterial = new Material(shader) { color = color };
    }

    private static string Sanitize(string value) => value.Replace(" ", string.Empty);

    private static void SetObjectReference(SerializedObject obj, string propertyName, Object value)
    {
        SerializedProperty property = obj.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetString(SerializedObject obj, string propertyName, string value)
    {
        SerializedProperty property = obj.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
        }
    }

    private static void SetEnum(SerializedObject obj, string propertyName, int enumIndex)
    {
        SerializedProperty property = obj.FindProperty(propertyName);
        if (property != null)
        {
            property.enumValueIndex = enumIndex;
        }
    }
}
