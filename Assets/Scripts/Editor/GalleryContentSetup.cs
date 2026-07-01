using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds the full "arcane museum" sorting content: three themed wings, each with its own
/// wall (3 mounts) and a small floor pile of matching paintings, grouped under a
/// <see cref="GallerySection"/>. Run via <b>Game → Setup Gallery Wings</b>.
///
/// Layout (50x50 room, walls at ±25):
///   Modern        → South wall (z = -24), floor pile front-left
///   Classical     → East wall  (x = +24), floor pile front-right
///   Impressionist → West wall  (x = -24), floor pile far-center
/// The North wall is left to the existing "Setup Museum Gameplay" mounts.
///
/// Idempotent: every object is guarded by name, so re-running only creates what is missing.
/// </summary>
public static class GalleryContentSetup
{
    private const string SetupCompleteKey = "GalleryWingsSetupComplete";
    private const string InteractableLayerName = "Interactable";
    private const string RootName = "GalleryWings";
    private const float MountHeight = 1.5f;

    private struct Art
    {
        public string Title;
        public Color Color;
        public Vector3 FloorPosition;

        public Art(string title, Color color, Vector3 floorPosition)
        {
            Title = title;
            Color = color;
            FloorPosition = floorPosition;
        }
    }

    [InitializeOnLoadMethod]
    private static void AutoSetupOnCompile()
    {
        EditorApplication.delayCall += TryAutoSetup;
    }

    [MenuItem("Game/Setup Gallery Wings")]
    public static void SetupFromMenu()
    {
        EditorPrefs.DeleteKey(SetupCompleteKey);
        RunSetup();
    }

    private static void TryAutoSetup()
    {
        if (EditorPrefs.GetBool(SetupCompleteKey, false))
        {
            return;
        }

        // Only auto-run once the base room + player exist so mounts land on real walls.
        if (GameObject.Find("Environment") == null || GameObject.Find("PlayerCamera") == null)
        {
            return;
        }

        RunSetup();
    }

    private static void RunSetup()
    {
        EnsureInteractableLayerExists();
        int interactableLayer = LayerMask.NameToLayer(InteractableLayerName);
        if (interactableLayer < 0)
        {
            Debug.LogError("GalleryContentSetup: Interactable layer not available; run 'Setup Art Pickup Test' first.");
            return;
        }

        Transform root = GetOrCreateRoot(RootName);
        var counter = new Counter();

        BuildWing(
            root, "Modern", GalleryWing.Modern, interactableLayer, counter,
            new[]
            {
                new Vector3(-8f, MountHeight, -24f),
                new Vector3(0f, MountHeight, -24f),
                new Vector3(8f, MountHeight, -24f)
            },
            Quaternion.Euler(0f, 0f, 0f),
            new Color(0.28f, 0.24f, 0.18f),
            new[]
            {
                new Art("Cobalt Field", new Color(0.15f, 0.3f, 0.8f), new Vector3(-6f, 0.025f, 3f)),
                new Art("Steel Lines", new Color(0.4f, 0.45f, 0.55f), new Vector3(-6f, 0.025f, 6f)),
                new Art("Neon Dusk", new Color(0.1f, 0.6f, 0.7f), new Vector3(-6f, 0.025f, 9f))
            });

        BuildWing(
            root, "Classical", GalleryWing.Classical, interactableLayer, counter,
            new[]
            {
                new Vector3(24f, MountHeight, -8f),
                new Vector3(24f, MountHeight, 0f),
                new Vector3(24f, MountHeight, 8f)
            },
            Quaternion.Euler(0f, 270f, 0f),
            new Color(0.3f, 0.24f, 0.14f),
            new[]
            {
                new Art("Gilded Saints", new Color(0.75f, 0.6f, 0.2f), new Vector3(6f, 0.025f, 3f)),
                new Art("Marble Study", new Color(0.85f, 0.82f, 0.7f), new Vector3(6f, 0.025f, 6f)),
                new Art("Old Masters", new Color(0.4f, 0.28f, 0.18f), new Vector3(6f, 0.025f, 9f))
            });

        BuildWing(
            root, "Impressionist", GalleryWing.Impressionist, interactableLayer, counter,
            new[]
            {
                new Vector3(-24f, MountHeight, -8f),
                new Vector3(-24f, MountHeight, 0f),
                new Vector3(-24f, MountHeight, 8f)
            },
            Quaternion.Euler(0f, 90f, 0f),
            new Color(0.22f, 0.28f, 0.2f),
            new[]
            {
                new Art("Garden Light", new Color(0.55f, 0.8f, 0.5f), new Vector3(-1.5f, 0.025f, 10f)),
                new Art("Rose Morning", new Color(0.9f, 0.6f, 0.7f), new Vector3(0f, 0.025f, 11.5f)),
                new Art("Water Lilies", new Color(0.4f, 0.75f, 0.7f), new Vector3(1.5f, 0.025f, 10f))
            });

        CreateWingFloorZones(root);
        CreateSortingTable(root, counter);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        EditorPrefs.SetBool(SetupCompleteKey, true);

        Debug.Log($"Gallery Wings setup complete. Created {counter.Created} new object(s), skipped {counter.Skipped} existing. " +
                  "Sort the floor paintings onto their matching wing's wall.");
    }

    private static void BuildWing(
        Transform root,
        string wingName,
        GalleryWing wing,
        int interactableLayer,
        Counter counter,
        Vector3[] mountPositions,
        Quaternion mountRotation,
        Color frameColor,
        Art[] artworks)
    {
        Transform wingRoot = GetOrCreateChild(root, $"Wing_{wingName}");

        var mounts = new List<PaintingMount>();
        var frameRenderers = new List<Renderer>();

        for (int i = 0; i < mountPositions.Length; i++)
        {
            string mountName = $"GalleryMount_{wingName}_{i + 1}";
            GameObject existing = GameObject.Find(mountName);
            if (existing != null)
            {
                counter.Skipped++;
                CollectMount(existing, mounts, frameRenderers);
                continue;
            }

            GameObject mountObject = CreateMount(wingRoot, mountName, mountPositions[i], mountRotation, wing, frameColor, interactableLayer);
            counter.Created++;
            CollectMount(mountObject, mounts, frameRenderers);
        }

        foreach (Art art in artworks)
        {
            string artName = $"GalleryArt_{wingName}_{SanitizeName(art.Title)}";
            if (GameObject.Find(artName) != null)
            {
                counter.Skipped++;
                continue;
            }

            CreateFloorPainting(wingRoot, artName, art, wing, interactableLayer);
            counter.Created++;
        }

        string sectionName = $"Section_{wingName}";
        GameObject sectionObject = GameObject.Find(sectionName);
        if (sectionObject == null)
        {
            sectionObject = new GameObject(sectionName);
            sectionObject.transform.SetParent(wingRoot, false);
            counter.Created++;
        }

        GallerySection section = sectionObject.GetComponent<GallerySection>();
        if (section == null)
        {
            section = sectionObject.AddComponent<GallerySection>();
        }

        WireSection(section, wing, GetSectionDisplayName(wingName), mounts, frameRenderers);
    }

    private static string GetSectionDisplayName(string wingName)
    {
        switch (wingName)
        {
            case "Modern":
                return "Modern (South)";
            case "Classical":
                return "Classical (East)";
            case "Impressionist":
                return "Impressionist (West)";
            default:
                return wingName;
        }
    }

    private static void CreateWingFloorZones(Transform root)
    {
        CreateWingZone(root, "Zone_Modern", GalleryWing.Modern, "Modern paintings → South wall",
            new Vector3(-6f, 0.011f, 6f), new Vector3(4f, 0.01f, 8f), new Color(0.12f, 0.18f, 0.32f, 0.45f));
        CreateWingZone(root, "Zone_Classical", GalleryWing.Classical, "Classical paintings → East wall",
            new Vector3(6f, 0.011f, 6f), new Vector3(4f, 0.01f, 8f), new Color(0.32f, 0.24f, 0.12f, 0.45f));
        CreateWingZone(root, "Zone_Impressionist", GalleryWing.Impressionist, "Impressionist paintings → West wall",
            new Vector3(0.75f, 0.011f, 10.75f), new Vector3(5f, 0.01f, 4f), new Color(0.12f, 0.28f, 0.22f, 0.45f));
    }

    private static void CreateWingZone(Transform parent, string name, GalleryWing wing, string label, Vector3 position, Vector3 scale, Color tint)
    {
        if (GameObject.Find(name) != null)
        {
            return;
        }

        GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.name = name;
        zone.transform.SetParent(parent, false);
        zone.transform.position = position;
        zone.transform.localScale = scale;

        Collider collider = zone.GetComponent<Collider>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }

        WingZoneMarker marker = zone.AddComponent<WingZoneMarker>();
        marker.Configure(wing, label, tint);
    }

    private static void CreateSortingTable(Transform parent, Counter counter)
    {
        const string tableName = "SortingTable";
        if (GameObject.Find(tableName) != null)
        {
            counter.Skipped++;
            return;
        }

        // A flat, muted slab on the floor marking the staging area. Left on the Default layer
        // so the pickup raycast (Interactable-only) ignores it; its collider is a trigger.
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = tableName;
        table.transform.SetParent(parent, false);
        table.transform.position = new Vector3(0f, 0.01f, 6.5f);
        table.transform.localScale = new Vector3(14f, 0.02f, 9f);

        BoxCollider collider = table.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        Renderer renderer = table.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = CreateColorMaterial(new Color(0.22f, 0.22f, 0.24f));
        }

        table.AddComponent<SortingTable>();
        MuseumEntityIdUtility.EnsureEntityId(table, "sorting_table_main");
        counter.Created++;
    }

    private static void CollectMount(GameObject mountObject, List<PaintingMount> mounts, List<Renderer> frameRenderers)
    {
        PaintingMount mount = mountObject.GetComponent<PaintingMount>();
        if (mount != null && !mounts.Contains(mount))
        {
            mounts.Add(mount);
        }

        Renderer renderer = mountObject.GetComponentInChildren<Renderer>();
        if (renderer != null && !frameRenderers.Contains(renderer))
        {
            frameRenderers.Add(renderer);
        }
    }

    private static GameObject CreateMount(
        Transform parent,
        string mountName,
        Vector3 position,
        Quaternion rotation,
        GalleryWing wing,
        Color frameColor,
        int interactableLayer)
    {
        GameObject mountRoot = new GameObject(mountName);
        mountRoot.transform.SetParent(parent, false);
        mountRoot.transform.position = position;
        mountRoot.transform.rotation = rotation;
        mountRoot.layer = interactableLayer;

        GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "Frame";
        frame.transform.SetParent(mountRoot.transform, false);
        frame.transform.localPosition = Vector3.zero;
        frame.transform.localScale = new Vector3(0.9f, 0.7f, 0.05f);
        frame.layer = interactableLayer;
        Object.DestroyImmediate(frame.GetComponent<Rigidbody>());

        Renderer frameRenderer = frame.GetComponent<Renderer>();
        if (frameRenderer != null)
        {
            // Distinct material instance per frame so the completion glow does not bleed
            // across mounts sharing the default material.
            frameRenderer.sharedMaterial = CreateColorMaterial(frameColor);
        }

        GameObject snapPoint = new GameObject("SnapPoint");
        snapPoint.transform.SetParent(mountRoot.transform, false);
        snapPoint.transform.localPosition = new Vector3(0f, 0f, -0.03f);

        PaintingMount mount = mountRoot.AddComponent<PaintingMount>();
        SerializedObject serializedMount = new SerializedObject(mount);
        SerializedProperty snapProperty = serializedMount.FindProperty("snapPoint");
        if (snapProperty != null)
        {
            snapProperty.objectReferenceValue = snapPoint.transform;
        }

        SerializedProperty wingProperty = serializedMount.FindProperty("requiredWing");
        if (wingProperty != null)
        {
            wingProperty.enumValueIndex = (int)wing;
        }

        serializedMount.ApplyModifiedPropertiesWithoutUndo();

        BoxCollider mountCollider = mountRoot.AddComponent<BoxCollider>();
        mountCollider.size = new Vector3(0.9f, 0.7f, 0.1f);
        mountCollider.center = Vector3.zero;

        MuseumEntityIdUtility.EnsureEntityId(mountRoot, BuildMountEntityId(wing, mountName));

        return mountRoot;
    }

    private static string BuildMountEntityId(GalleryWing wing, string mountName)
    {
        return $"mount_{wing.ToString().ToLowerInvariant()}_{SanitizeName(mountName)}";
    }

    private static void CreateFloorPainting(Transform parent, string objectName, Art art, GalleryWing wing, int interactableLayer)
    {
        GameObject painting = GameObject.CreatePrimitive(PrimitiveType.Cube);
        painting.name = objectName;
        painting.transform.SetParent(parent, false);
        painting.layer = interactableLayer;
        painting.transform.position = art.FloorPosition;
        painting.transform.localScale = new Vector3(0.6f, 0.05f, 0.4f);

        Rigidbody rigidbody = painting.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = painting.AddComponent<Rigidbody>();
        }

        rigidbody.mass = 2f;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        InteractablePainting interactablePainting = painting.AddComponent<InteractablePainting>();
        SerializedObject serializedPainting = new SerializedObject(interactablePainting);
        SerializedProperty titleProperty = serializedPainting.FindProperty("paintingTitle");
        if (titleProperty != null)
        {
            titleProperty.stringValue = art.Title;
        }

        SerializedProperty wingProperty = serializedPainting.FindProperty("wing");
        if (wingProperty != null)
        {
            wingProperty.enumValueIndex = (int)wing;
        }

        serializedPainting.ApplyModifiedPropertiesWithoutUndo();

        Renderer renderer = painting.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = CreateColorMaterial(art.Color);
        }

        MuseumEntityIdUtility.EnsureEntityId(painting, BuildPaintingEntityId(art.Title));
    }

    private static string BuildPaintingEntityId(string title)
    {
        return "painting_" + SanitizeName(title).ToLowerInvariant();
    }

    private static void WireSection(GallerySection section, GalleryWing wing, string displayName, List<PaintingMount> mounts, List<Renderer> frameRenderers)
    {
        SerializedObject serializedSection = new SerializedObject(section);

        SerializedProperty wingProperty = serializedSection.FindProperty("sectionWing");
        if (wingProperty != null)
        {
            wingProperty.enumValueIndex = (int)wing;
        }

        SerializedProperty displayNameProperty = serializedSection.FindProperty("sectionDisplayName");
        if (displayNameProperty != null)
        {
            displayNameProperty.stringValue = displayName;
        }

        AssignObjectList(serializedSection.FindProperty("mounts"), mounts.ConvertAll(m => (Object)m));
        AssignObjectList(serializedSection.FindProperty("glowRenderers"), frameRenderers.ConvertAll(r => (Object)r));

        serializedSection.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignObjectList(SerializedProperty listProperty, List<Object> values)
    {
        if (listProperty == null)
        {
            return;
        }

        listProperty.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++)
        {
            listProperty.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static Material CreateColorMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader) { color = color };
        return material;
    }

    private static Transform GetOrCreateRoot(string rootName)
    {
        GameObject existing = GameObject.Find(rootName);
        return existing != null ? existing.transform : new GameObject(rootName).transform;
    }

    private static Transform GetOrCreateChild(Transform parent, string childName)
    {
        Transform existing = parent.Find(childName);
        if (existing != null)
        {
            return existing;
        }

        GameObject child = new GameObject(childName);
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private static string SanitizeName(string value)
    {
        return value.Replace(" ", string.Empty);
    }

    private static void EnsureInteractableLayerExists()
    {
        if (LayerMask.NameToLayer(InteractableLayerName) >= 0)
        {
            return;
        }

        Object[] tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (tagManagerAssets == null || tagManagerAssets.Length == 0)
        {
            Debug.LogError("GalleryContentSetup: Could not load TagManager.asset.");
            return;
        }

        SerializedObject tagManager = new SerializedObject(tagManagerAssets[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 8; i < layers.arraySize; i++)
        {
            if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
            {
                layers.GetArrayElementAtIndex(i).stringValue = InteractableLayerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"GalleryContentSetup: Added '{InteractableLayerName}' layer at index {i}.");
                return;
            }
        }

        Debug.LogError("GalleryContentSetup: No empty layer slot available for Interactable.");
    }

    private class Counter
    {
        public int Created;
        public int Skipped;
    }
}
