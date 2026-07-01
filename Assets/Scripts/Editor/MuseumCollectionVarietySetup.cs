using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Adds varied-size paintings, unsorted lobby piles, and a fossil display structure with mounts.
/// </summary>
public static class MuseumCollectionVarietySetup
{
    private const string InteractableLayerName = "Interactable";
    private const string VarietyRootName = "MuseumCollectionVariety";
    private const float MountHeight = 1.5f;

    [MenuItem("Game/Add Varied Paintings And Fossils")]
    public static void SetupFromMenu()
    {
        EnsureCollectionVariety();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Collection variety added — varied paintings, fossil hall structure, and loose specimens.");
    }

    public static void EnsureCollectionVariety()
    {
        int interactableLayer = LayerMask.NameToLayer(InteractableLayerName);
        if (interactableLayer < 0)
        {
            Debug.LogError("MuseumCollectionVarietySetup: Interactable layer missing — run Setup Art Pickup Test first.");
            return;
        }

        Transform root = GetOrCreateRoot();
        var counter = new Counter();

        EnsureUnsortedPaintingPile(root, interactableLayer, counter);
        EnsureFossilDisplayStructure(interactableLayer, counter);
        EnsureArtWingVariedPaintings(interactableLayer, counter);
        EnsureSortingMuseumVariedExtras(interactableLayer, counter);

        Debug.Log($"MuseumCollectionVarietySetup: created {counter.Created}, skipped {counter.Skipped}.");
    }

    private static void EnsureUnsortedPaintingPile(Transform root, int interactableLayer, Counter counter)
    {
        Transform pile = GetOrCreateChild(root, "Unsorted_Pile");

        MuseumArtFactory.ArtSpec[] specs =
        {
            new("Crimson Portrait", GalleryWing.Classical, new Color(0.72f, 0.18f, 0.2f),
                new Vector3(-6f, 0.03f, 2f), MuseumArtFactory.ShapeKind.PortraitTall),
            new("Harbor Panorama", GalleryWing.Impressionist, new Color(0.35f, 0.55f, 0.78f),
                new Vector3(-4f, 0.03f, 4f), MuseumArtFactory.ShapeKind.LandscapeWide),
            new("Study in Miniature", GalleryWing.Modern, new Color(0.85f, 0.75f, 0.35f),
                new Vector3(-2f, 0.025f, 1.5f), MuseumArtFactory.ShapeKind.Miniature),
            new("Grand Still Life", GalleryWing.Classical, new Color(0.55f, 0.32f, 0.22f),
                new Vector3(0f, 0.035f, 3f), MuseumArtFactory.ShapeKind.OversizedSquare),
            new("Moon Medallion", GalleryWing.Impressionist, new Color(0.78f, 0.82f, 0.65f),
                new Vector3(2f, 0.03f, 5f), MuseumArtFactory.ShapeKind.RoundMedallion),
            new("Teal Horizon", GalleryWing.Modern, new Color(0.12f, 0.62f, 0.58f),
                new Vector3(4f, 0.03f, 2.5f), MuseumArtFactory.ShapeKind.LandscapeWide),
            new("Ivory Miniature", GalleryWing.Classical, new Color(0.92f, 0.9f, 0.82f),
                new Vector3(6f, 0.025f, 4f), MuseumArtFactory.ShapeKind.Miniature),
            new("Violet Study", GalleryWing.Modern, new Color(0.48f, 0.22f, 0.72f),
                new Vector3(-5f, 0.03f, -2f), MuseumArtFactory.ShapeKind.PortraitTall),
            new("Pastel Meadow", GalleryWing.Impressionist, new Color(0.82f, 0.72f, 0.48f),
                new Vector3(-1f, 0.03f, -3f), MuseumArtFactory.ShapeKind.StandardCanvas),
            new("Obsidian Panel", GalleryWing.Modern, new Color(0.08f, 0.1f, 0.12f),
                new Vector3(3f, 0.035f, -1f), MuseumArtFactory.ShapeKind.OversizedSquare),
            new("Gilded Roundel", GalleryWing.Classical, new Color(0.78f, 0.62f, 0.22f),
                new Vector3(5f, 0.03f, -2.5f), MuseumArtFactory.ShapeKind.RoundMedallion),
            new("Coastal Strip", GalleryWing.Impressionist, new Color(0.42f, 0.68f, 0.82f),
                new Vector3(1f, 0.03f, 6f), MuseumArtFactory.ShapeKind.LandscapeWide)
        };

        foreach (MuseumArtFactory.ArtSpec spec in specs)
        {
            string name = $"Unsorted_{Sanitize(spec.Title)}";
            if (GameObject.Find(name) != null)
            {
                counter.Skipped++;
                continue;
            }

            MuseumArtFactory.CreateFloorPiece(pile, name, spec, interactableLayer);
            counter.Created++;
        }

        EnsureLobbyPileMarker(pile);
    }

    private static void EnsureLobbyPileMarker(Transform pile)
    {
        if (pile.Find("PileSign") != null)
        {
            return;
        }

        GameObject sign = new GameObject("PileSign");
        sign.transform.SetParent(pile, false);
        sign.transform.position = new Vector3(0f, 2.2f, 8f);
        sign.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);
        Canvas canvas = sign.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        sign.GetComponent<RectTransform>().sizeDelta = new Vector2(420f, 64f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(sign.transform, false);
        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Text label = textObject.AddComponent<Text>();
        label.font = font;
        label.fontSize = 26;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(0.92f, 0.88f, 0.78f);
        label.text = "UNSORTED COLLECTION\nPaintings & specimens — all shapes & sizes";
    }

    private static void EnsureFossilDisplayStructure(int interactableLayer, Counter counter)
    {
        Transform fossilWing = GameObject.Find("Museum_Architecture/Fossil_Wing")?.transform;
        Transform annex = GameObject.Find("Environment/MuseumExpansion/FossilAnnex")?.transform;
        Transform structureParent = fossilWing != null ? fossilWing : annex;

        if (structureParent == null)
        {
            Transform environment = GameObject.Find("Environment")?.transform;
            if (environment == null)
            {
                Debug.LogWarning("MuseumCollectionVarietySetup: No Fossil_Wing or Environment — fossil structure skipped.");
                return;
            }

            Transform expansion = GetOrCreateChild(environment, "MuseumExpansion");
            structureParent = GetOrCreateChild(expansion, "FossilAnnex");
            BuildFossilAnnexGreybox(structureParent);
        }

        Transform structure = GetOrCreateChild(structureParent, "Fossil_Display_Structure");
        Vector3 center = structureParent.position;
        if (fossilWing != null)
        {
            const float planeUnit = 10f;
            float lobbyHalf = planeUnit * 20f * 0.5f;
            float wingHalf = planeUnit * 25f * 0.5f;
            center = new Vector3(lobbyHalf + wingHalf, 0f, 0f);
        }
        else
        {
            center = new Vector3(32f, 0f, -8f);
        }

        EnsureFossilArmature(structure, center, counter);
        EnsureFossilMounts(structure, center, interactableLayer, counter);
        EnsureLooseFossils(structure, center, interactableLayer, counter);
        WireFossilSection(structure, counter);
    }

    private static void BuildFossilAnnexGreybox(Transform annex)
    {
        if (annex.Find("AnnexFloor") != null)
        {
            return;
        }

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "AnnexFloor";
        floor.transform.SetParent(annex, false);
        floor.transform.position = new Vector3(32f, -0.01f, -8f);
        floor.transform.localScale = new Vector3(18f, 0.02f, 14f);
        Object.DestroyImmediate(floor.GetComponent<BoxCollider>());

        Color slate = new Color(0.38f, 0.42f, 0.46f);
        ApplyMaterial(floor, slate);

        CreateAnnexWall(annex, "Wall_N", new Vector3(32f, 2f, -1f), new Vector3(18f, 4f, 0.4f), slate);
        CreateAnnexWall(annex, "Wall_S", new Vector3(32f, 2f, -15f), new Vector3(18f, 4f, 0.4f), slate);
        CreateAnnexWall(annex, "Wall_E", new Vector3(41f, 2f, -8f), new Vector3(0.4f, 4f, 14f), slate);
        CreateAnnexWall(annex, "Wall_W", new Vector3(23f, 2f, -8f), new Vector3(0.4f, 4f, 14f), slate);

        GameObject dais = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dais.name = "FossilDais";
        dais.transform.SetParent(annex, false);
        dais.transform.position = new Vector3(32f, 0.35f, -8f);
        dais.transform.localScale = new Vector3(6f, 0.7f, 10f);
        ApplyMaterial(dais, new Color(0.48f, 0.5f, 0.54f));
    }

    private static void CreateAnnexWall(Transform parent, string name, Vector3 pos, Vector3 scale, Color color)
    {
        if (parent.Find(name) != null)
        {
            return;
        }

        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        ApplyMaterial(wall, color);
    }

    private static void EnsureFossilArmature(Transform structure, Vector3 center, Counter counter)
    {
        if (structure.Find("Armature_Beam_A") != null)
        {
            counter.Skipped += 4;
            return;
        }

        Color beam = new Color(0.52f, 0.54f, 0.58f);
        CreateBeam(structure, "Armature_Beam_A", center + new Vector3(0f, 4f, -6f), new Vector3(10f, 0.35f, 0.35f), beam);
        CreateBeam(structure, "Armature_Beam_B", center + new Vector3(0f, 4f, 6f), new Vector3(10f, 0.35f, 0.35f), beam);
        CreateBeam(structure, "Armature_Column_L", center + new Vector3(-4.5f, 2f, 0f), new Vector3(0.4f, 4f, 0.4f), beam);
        CreateBeam(structure, "Armature_Column_R", center + new Vector3(4.5f, 2f, 0f), new Vector3(0.4f, 4f, 0.4f), beam);
        counter.Created += 4;
    }

    private static void CreateBeam(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = name;
        beam.transform.SetParent(parent, false);
        beam.transform.position = position;
        beam.transform.localScale = scale;
        ApplyMaterial(beam, color);
    }

    private static void EnsureFossilMounts(Transform structure, Vector3 center, int interactableLayer, Counter counter)
    {
        Transform mountsRoot = GetOrCreateChild(structure, "Fossil_Mounts");
        var mounts = new List<PaintingMount>();
        var renderers = new List<Renderer>();

        (string name, Vector3 offset, Vector3 frameScale, string title)[] slots =
        {
            ("FossilMount_Skull", new Vector3(0f, 2.2f, 0f), new Vector3(1.4f, 1.2f, 0.12f), "Sauropod Skull"),
            ("FossilMount_Femur", new Vector3(-2.5f, 1.6f, 3f), new Vector3(0.25f, 0.25f, 1.1f), "Megafauna Femur"),
            ("FossilMount_Rib", new Vector3(2.5f, 1.8f, -3f), new Vector3(0.9f, 0.15f, 0.5f), "Rib Arch"),
            ("FossilMount_Ammonite", new Vector3(-3.5f, 1.1f, -5f), new Vector3(0.7f, 0.7f, 0.1f), "Ammonite Spiral"),
            ("FossilMount_Trilobite", new Vector3(3.5f, 1.05f, 5f), new Vector3(0.55f, 0.55f, 0.1f), "Trilobite Plate"),
            ("FossilMount_Tusk", new Vector3(0f, 1.3f, -6f), new Vector3(0.2f, 0.2f, 0.95f), "Mammoth Tusk")
        };

        Color mountColor = new Color(0.45f, 0.48f, 0.52f);
        for (int i = 0; i < slots.Length; i++)
        {
            string entityId = MuseumArtFactory.BuildFossilEntityId(slots[i].title);
            if (mountsRoot.Find(slots[i].name) != null)
            {
                counter.Skipped++;
                CollectMount(mountsRoot.Find(slots[i].name).gameObject, mounts, renderers);
                continue;
            }

            GameObject mountObject = MuseumArtFactory.CreateFossilMount(
                mountsRoot,
                slots[i].name,
                center + slots[i].offset,
                Quaternion.Euler(0f, 180f, 0f),
                GalleryWing.Fossil,
                interactableLayer,
                entityId,
                slots[i].title,
                mountColor,
                slots[i].frameScale);
            counter.Created++;
            CollectMount(mountObject, mounts, renderers);
        }

        WireGallerySection(mounts, renderers, "Section_FossilHall", GalleryWing.Fossil, "Fossil & Antiquity");
    }

    private static void EnsureLooseFossils(Transform structure, Vector3 center, int interactableLayer, Counter counter)
    {
        Transform loose = GetOrCreateChild(structure, "Loose_Fossils");

        MuseumArtFactory.ArtSpec[] fossils =
        {
            new("Sauropod Skull", GalleryWing.Fossil, new Color(0.68f, 0.64f, 0.58f),
                center + new Vector3(-5f, 0.2f, 8f), MuseumArtFactory.ShapeKind.SkullBlock,
                MuseumArtFactory.BuildFossilEntityId("Sauropod Skull")),
            new("Megafauna Femur", GalleryWing.Fossil, new Color(0.62f, 0.58f, 0.52f),
                center + new Vector3(-3f, 0.18f, 10f), MuseumArtFactory.ShapeKind.BoneSegment,
                MuseumArtFactory.BuildFossilEntityId("Megafauna Femur")),
            new("Rib Arch", GalleryWing.Fossil, new Color(0.58f, 0.55f, 0.5f),
                center + new Vector3(0f, 0.15f, 9f), MuseumArtFactory.ShapeKind.BoneSegment,
                MuseumArtFactory.BuildFossilEntityId("Rib Arch")),
            new("Ammonite Spiral", GalleryWing.Fossil, new Color(0.55f, 0.52f, 0.48f),
                center + new Vector3(3f, 0.12f, 11f), MuseumArtFactory.ShapeKind.ShellFossil,
                MuseumArtFactory.BuildFossilEntityId("Ammonite Spiral")),
            new("Trilobite Plate", GalleryWing.Fossil, new Color(0.42f, 0.48f, 0.46f),
                center + new Vector3(5f, 0.08f, 8.5f), MuseumArtFactory.ShapeKind.ShellFossil,
                MuseumArtFactory.BuildFossilEntityId("Trilobite Plate")),
            new("Mammoth Tusk", GalleryWing.Fossil, new Color(0.78f, 0.74f, 0.68f),
                center + new Vector3(2f, 0.1f, 12f), MuseumArtFactory.ShapeKind.BoneSegment,
                MuseumArtFactory.BuildFossilEntityId("Mammoth Tusk")),
            new("Archaeopteryx Slab", GalleryWing.Fossil, new Color(0.36f, 0.34f, 0.32f),
                center + new Vector3(-1f, 0.05f, 13f), MuseumArtFactory.ShapeKind.LandscapeWide,
                MuseumArtFactory.BuildFossilEntityId("Archaeopteryx Slab")),
            new("Bronze Idol", GalleryWing.Fossil, new Color(0.52f, 0.38f, 0.22f),
                center + new Vector3(4f, 0.28f, 6f), MuseumArtFactory.ShapeKind.StatuePlinth,
                MuseumArtFactory.BuildFossilEntityId("Bronze Idol"))
        };

        foreach (MuseumArtFactory.ArtSpec spec in fossils)
        {
            string name = $"Loose_{Sanitize(spec.Title)}";
            if (GameObject.Find(name) != null)
            {
                counter.Skipped++;
                continue;
            }

            MuseumArtFactory.CreateFloorPiece(loose, name, spec, interactableLayer);
            counter.Created++;
        }
    }

    private static void EnsureArtWingVariedPaintings(int interactableLayer, Counter counter)
    {
        Transform artWing = GameObject.Find("Museum_Architecture/Art_Wing_Pre1900")?.transform;
        if (artWing == null)
        {
            return;
        }

        Transform extras = GetOrCreateChild(artWing, "Varied_Paintings");
        const float planeUnit = 10f;
        float wingCenterX = -planeUnit * 20f * 0.5f - planeUnit * 10f * 0.5f;

        MuseumArtFactory.ArtSpec[] specs =
        {
            new("Baroque Altarpiece", GalleryWing.Classical, new Color(0.58f, 0.22f, 0.18f),
                new Vector3(wingCenterX - 2f, 0.04f, -60f), MuseumArtFactory.ShapeKind.OversizedSquare),
            new("Dutch Interior", GalleryWing.Classical, new Color(0.48f, 0.36f, 0.24f),
                new Vector3(wingCenterX + 2f, 0.03f, -30f), MuseumArtFactory.ShapeKind.LandscapeWide),
            new("Saint Miniature", GalleryWing.Classical, new Color(0.72f, 0.65f, 0.42f),
                new Vector3(wingCenterX, 0.025f, 0f), MuseumArtFactory.ShapeKind.Miniature),
            new("Romantic Portrait", GalleryWing.Classical, new Color(0.32f, 0.22f, 0.28f),
                new Vector3(wingCenterX + 1.5f, 0.03f, 35f), MuseumArtFactory.ShapeKind.PortraitTall),
            new("Tapestry Roundel", GalleryWing.Classical, new Color(0.65f, 0.42f, 0.28f),
                new Vector3(wingCenterX - 1f, 0.03f, 70f), MuseumArtFactory.ShapeKind.RoundMedallion),
            new("Pastoral Wide", GalleryWing.Impressionist, new Color(0.52f, 0.72f, 0.45f),
                new Vector3(wingCenterX, 0.03f, 90f), MuseumArtFactory.ShapeKind.LandscapeWide)
        };

        foreach (MuseumArtFactory.ArtSpec spec in specs)
        {
            string name = $"ArtWing_{Sanitize(spec.Title)}";
            if (GameObject.Find(name) != null)
            {
                counter.Skipped++;
                continue;
            }

            MuseumArtFactory.CreateFloorPiece(extras, name, spec, interactableLayer);
            counter.Created++;
        }
    }

    private static void EnsureSortingMuseumVariedExtras(int interactableLayer, Counter counter)
    {
        Transform galleryWings = GameObject.Find("GalleryWings")?.transform;
        if (galleryWings == null)
        {
            return;
        }

        Transform extras = GetOrCreateChild(galleryWings, "Varied_FloorArt");
        MuseumArtFactory.ArtSpec[] specs =
        {
            new("Wide Modern Panel", GalleryWing.Modern, new Color(0.22f, 0.55f, 0.82f),
                new Vector3(-10f, 0.03f, 8f), MuseumArtFactory.ShapeKind.LandscapeWide),
            new("Tall Classical Lady", GalleryWing.Classical, new Color(0.62f, 0.48f, 0.38f),
                new Vector3(10f, 0.03f, 8f), MuseumArtFactory.ShapeKind.PortraitTall),
            new("Plein Air Mini", GalleryWing.Impressionist, new Color(0.88f, 0.72f, 0.52f),
                new Vector3(0f, 0.025f, 10f), MuseumArtFactory.ShapeKind.Miniature)
        };

        foreach (MuseumArtFactory.ArtSpec spec in specs)
        {
            string name = $"Gallery_{Sanitize(spec.Title)}";
            if (GameObject.Find(name) != null)
            {
                counter.Skipped++;
                continue;
            }

            MuseumArtFactory.CreateFloorPiece(extras, name, spec, interactableLayer);
            counter.Created++;
        }
    }

    private static void WireFossilSection(List<PaintingMount> mounts, List<Renderer> renderers, string sectionName, GalleryWing wing, string displayName)
    {
        if (mounts.Count == 0)
        {
            return;
        }

        Transform sectionParent = GameObject.Find("GalleryWings")?.transform
            ?? GameObject.Find("Museum_Architecture")?.transform
            ?? GameObject.Find("Environment")?.transform;

        GameObject sectionObject = GameObject.Find(sectionName);
        if (sectionObject == null)
        {
            sectionObject = new GameObject(sectionName);
            sectionObject.transform.SetParent(sectionParent, false);
        }

        GallerySection section = sectionObject.GetComponent<GallerySection>() ?? sectionObject.AddComponent<GallerySection>();
        SerializedObject serialized = new SerializedObject(section);
        SetEnum(serialized, "sectionWing", (int)wing);
        SetString(serialized, "sectionDisplayName", displayName);
        AssignList(serialized.FindProperty("mounts"), mounts);
        AssignRendererList(serialized.FindProperty("glowRenderers"), renderers);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireGallerySection(List<PaintingMount> mounts, List<Renderer> renderers, string sectionName, GalleryWing wing, string displayName)
    {
        WireFossilSection(mounts, renderers, sectionName, wing, displayName);
    }

    private static void CollectMount(GameObject mountObject, List<PaintingMount> mounts, List<Renderer> renderers)
    {
        PaintingMount mount = mountObject.GetComponent<PaintingMount>();
        if (mount != null && !mounts.Contains(mount))
        {
            mounts.Add(mount);
        }

        Renderer renderer = mountObject.GetComponentInChildren<Renderer>();
        if (renderer != null && !renderers.Contains(renderer))
        {
            renderers.Add(renderer);
        }
    }

    private static void ApplyMaterial(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        renderer.sharedMaterial = new Material(shader) { color = color };
    }

    private static Transform GetOrCreateRoot()
    {
        GameObject existing = GameObject.Find(VarietyRootName);
        return existing != null ? existing.transform : new GameObject(VarietyRootName).transform;
    }

    private static Transform GetOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child;
        }

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static string Sanitize(string value) => value.Replace(" ", string.Empty);

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

    private static void AssignList(SerializedProperty listProperty, List<PaintingMount> mounts)
    {
        if (listProperty == null)
        {
            return;
        }

        listProperty.arraySize = mounts.Count;
        for (int i = 0; i < mounts.Count; i++)
        {
            listProperty.GetArrayElementAtIndex(i).objectReferenceValue = mounts[i];
        }
    }

    private static void AssignRendererList(SerializedProperty listProperty, List<Renderer> renderers)
    {
        if (listProperty == null)
        {
            return;
        }

        listProperty.arraySize = renderers.Count;
        for (int i = 0; i < renderers.Count; i++)
        {
            listProperty.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
        }
    }

    private class Counter
    {
        public int Created;
        public int Skipped;
    }
}
