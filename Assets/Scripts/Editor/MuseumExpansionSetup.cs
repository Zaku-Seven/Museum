using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Literal greybox expansion: north archive wing, alcoves, furniture, lighting, overflow art.
/// Idempotent — safe to re-run from Game → Expand Museum Building or Setup Full Museum.
/// </summary>
public static class MuseumExpansionSetup
{
    private const string RootName = "MuseumExpansion";
    private const string InteractableLayerName = "Interactable";
    private const float MountHeight = 1.5f;

    [MenuItem("Game/Expand Museum Building")]
    public static void SetupFromMenu()
    {
        EnsureExpansion();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Museum expansion complete — north archive, alcoves, props, lighting, overflow paintings.");
    }

    public static void EnsureExpansion()
    {
        Transform environment = GameObject.Find("Environment")?.transform;
        if (environment == null)
        {
            Debug.LogWarning("MuseumExpansionSetup: Environment missing — run Setup Zero-to-One first.");
            return;
        }

        int interactableLayer = LayerMask.NameToLayer(InteractableLayerName);
        if (interactableLayer < 0)
        {
            Debug.LogError("MuseumExpansionSetup: Interactable layer missing.");
            return;
        }

        Transform root = GetOrCreateChild(environment, RootName);
        var counter = new Counter();

        SplitNorthWallForArchive(environment);
        BuildNorthArchiveGallery(root, interactableLayer, counter);
        BuildCorridorArchitecture(root, counter);
        BuildPerimeterWainscoting(root, counter);
        BuildEastClassicalAlcove(root, interactableLayer, counter);
        BuildWestImpressionistAlcove(root, interactableLayer, counter);
        BuildCentralCuratorArea(root, counter);
        BuildPropsAndBarriers(root, counter);
        MuseumLightingSetup.EnsureGalleryLighting(root);
        AddOverflowCollectionPaintings(interactableLayer, counter);

        Debug.Log($"MuseumExpansionSetup: created {counter.Created}, skipped {counter.Skipped}.");
    }

    private static void SplitNorthWallForArchive(Transform environment)
    {
        Transform westSegment = environment.Find("Wall_North_West");
        Transform eastSegment = environment.Find("Wall_North_East");
        if (westSegment != null && eastSegment != null)
        {
            return;
        }

        Transform legacyNorth = environment.Find("Wall_North");
        if (legacyNorth != null && westSegment == null)
        {
            legacyNorth.name = "Wall_North_West";
            legacyNorth.position = new Vector3(-14f, 1.5f, 25f);
            legacyNorth.localScale = new Vector3(22f, 3f, 1f);
        }

        if (eastSegment == null)
        {
            GameObject eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eastWall.name = "Wall_North_East";
            eastWall.transform.SetParent(environment, false);
            eastWall.transform.position = new Vector3(14f, 1.5f, 25f);
            eastWall.transform.localScale = new Vector3(22f, 3f, 1f);
            ApplyMaterial(eastWall, new Color(0.55f, 0.55f, 0.58f));
        }
    }

    private static void BuildNorthArchiveGallery(Transform root, int interactableLayer, Counter counter)
    {
        Transform annex = GetOrCreateChild(root, "NorthArchive");

        EnsureBoxRoom(annex, "ArchiveFloor", new Vector3(0f, 0f, 38f), new Vector3(22f, 0.02f, 18f), new Color(0.16f, 0.17f, 0.2f), isFloor: true);
        EnsureBoxRoom(annex, "CorridorFloor", new Vector3(0f, 0f, 27f), new Vector3(8f, 0.02f, 6f), new Color(0.14f, 0.15f, 0.18f), isFloor: true);
        EnsureBoxRoom(annex, "ArchiveCeiling", new Vector3(0f, 3f, 38f), new Vector3(22f, 0.15f, 18f), new Color(0.1f, 0.11f, 0.14f), isFloor: false);
        EnsureBoxRoom(annex, "ArchiveWall_Far", new Vector3(0f, 1.5f, 47f), new Vector3(22f, 3f, 0.4f), new Color(0.2f, 0.21f, 0.24f), isFloor: false);
        EnsureBoxRoom(annex, "ArchiveWall_West", new Vector3(-11f, 1.5f, 38f), new Vector3(0.4f, 3f, 18f), new Color(0.2f, 0.21f, 0.24f), isFloor: false);
        EnsureBoxRoom(annex, "ArchiveWall_East", new Vector3(11f, 1.5f, 38f), new Vector3(0.4f, 3f, 18f), new Color(0.2f, 0.21f, 0.24f), isFloor: false);
        EnsureDoorArch(annex, "DoorArch_North", new Vector3(0f, 2.1f, 24.6f), new Vector3(5f, 2.6f, 0.35f));
        EnsureWorldLabel(annex, "ArchiveSign", new Vector3(0f, 2.4f, 30f), "MODERN ARCHIVE\nOverflow collection");

        var mounts = new List<PaintingMount>();
        var renderers = new List<Renderer>();
        float[] xs = { -6f, 0f, 6f };
        for (int i = 0; i < xs.Length; i++)
        {
            string mountName = $"ArchiveMount_{i + 1}";
            string title = i switch
            {
                0 => "Twilight Arch",
                1 => "Carbon Study",
                _ => "Neon Grid"
            };

            GameObject mountObject = EnsureArchiveMount(
                annex,
                mountName,
                new Vector3(xs[i], MountHeight, 46.2f),
                Quaternion.Euler(0f, 180f, 0f),
                GalleryWing.Modern,
                interactableLayer,
                $"painting_{Sanitize(title)}",
                title,
                counter);
            CollectMount(mountObject, mounts, renderers);
        }

        WireGallerySection(
            GameObject.Find("GalleryWings")?.transform ?? root,
            "Section_ModernArchive",
            GalleryWing.Modern,
            "Modern (Archive)",
            mounts,
            renderers);

        Transform corridorFloor = annex.Find("CorridorFloor");
        if (corridorFloor != null && corridorFloor.GetComponent<FootstepSurface>() == null)
        {
            FootstepSurface surface = corridorFloor.gameObject.AddComponent<FootstepSurface>();
            surface.Configure(FootstepSurface.SurfaceKind.Stone);
        }

        FootstepSurface archiveFloor = annex.Find("ArchiveFloor")?.GetComponent<FootstepSurface>();
        if (archiveFloor == null && annex.Find("ArchiveFloor") != null)
        {
            FootstepSurface surface = annex.Find("ArchiveFloor").gameObject.AddComponent<FootstepSurface>();
            surface.Configure(FootstepSurface.SurfaceKind.Carpet);
        }

        EnsureArchiveSortingTable(annex, counter);
        EnsureArchivePillars(annex, counter);
    }

    private static void BuildCorridorArchitecture(Transform root, Counter counter)
    {
        Transform corridor = GetOrCreateChild(root, "NorthCorridor");
        EnsureBoxRoom(corridor, "CorridorWall_West", new Vector3(-4.2f, 1.5f, 27f), new Vector3(0.35f, 3f, 6f), new Color(0.24f, 0.25f, 0.28f), isFloor: false);
        EnsureBoxRoom(corridor, "CorridorWall_East", new Vector3(4.2f, 1.5f, 27f), new Vector3(0.35f, 3f, 6f), new Color(0.24f, 0.25f, 0.28f), isFloor: false);
        EnsureBoxRoom(corridor, "CorridorCeiling", new Vector3(0f, 3f, 27f), new Vector3(8.5f, 0.12f, 6f), new Color(0.11f, 0.12f, 0.15f), isFloor: false);

        Vector3[] columnPositions =
        {
            new Vector3(-3.5f, 1.5f, 25.5f),
            new Vector3(3.5f, 1.5f, 25.5f),
            new Vector3(-3.5f, 1.5f, 28.5f),
            new Vector3(3.5f, 1.5f, 28.5f)
        };

        for (int i = 0; i < columnPositions.Length; i++)
        {
            string name = $"CorridorColumn_{i + 1}";
            if (corridor.Find(name) != null)
            {
                counter.Skipped++;
                continue;
            }

            GameObject column = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            column.name = name;
            column.transform.SetParent(corridor, false);
            column.transform.position = columnPositions[i];
            column.transform.localScale = new Vector3(0.35f, 1.5f, 0.35f);
            ApplyMaterial(column, new Color(0.32f, 0.3f, 0.28f));
            counter.Created++;
        }
    }

    private static void BuildPerimeterWainscoting(Transform root, Counter counter)
    {
        Transform wainscot = GetOrCreateChild(root, "Wainscoting");
        (string name, Vector3 pos, Vector3 scale)[] strips =
        {
            ("Wainscot_South", new Vector3(0f, 0.45f, -24.5f), new Vector3(48f, 0.9f, 0.12f)),
            ("Wainscot_North", new Vector3(0f, 0.45f, 24.5f), new Vector3(48f, 0.9f, 0.12f)),
            ("Wainscot_East", new Vector3(24.5f, 0.45f, 0f), new Vector3(0.12f, 0.9f, 48f)),
            ("Wainscot_West", new Vector3(-24.5f, 0.45f, 0f), new Vector3(0.12f, 0.9f, 48f))
        };

        for (int i = 0; i < strips.Length; i++)
        {
            if (wainscot.Find(strips[i].name) != null)
            {
                counter.Skipped++;
                continue;
            }

            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = strips[i].name;
            strip.transform.SetParent(wainscot, false);
            strip.transform.position = strips[i].pos;
            strip.transform.localScale = strips[i].scale;
            ApplyMaterial(strip, new Color(0.38f, 0.32f, 0.26f));
            Object.DestroyImmediate(strip.GetComponent<Collider>());
            counter.Created++;
        }
    }

    private static void EnsureArchivePillars(Transform annex, Counter counter)
    {
        Vector3[] positions =
        {
            new Vector3(-7f, 1.5f, 32f),
            new Vector3(7f, 1.5f, 32f),
            new Vector3(-7f, 1.5f, 44f),
            new Vector3(7f, 1.5f, 44f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            string name = $"ArchivePillar_{i + 1}";
            if (annex.Find(name) != null)
            {
                counter.Skipped++;
                continue;
            }

            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = name;
            pillar.transform.SetParent(annex, false);
            pillar.transform.position = positions[i];
            pillar.transform.localScale = new Vector3(0.5f, 1.5f, 0.5f);
            ApplyMaterial(pillar, new Color(0.26f, 0.27f, 0.3f));
            counter.Created++;
        }
    }

    private static void EnsureArchiveSortingTable(Transform annex, Counter counter)
    {
        const string tableName = "SortingTable_Archive";
        if (GameObject.Find(tableName) != null)
        {
            counter.Skipped++;
            return;
        }

        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = tableName;
        table.transform.SetParent(annex, false);
        table.transform.position = new Vector3(0f, 0.01f, 36f);
        table.transform.localScale = new Vector3(10f, 0.02f, 6f);

        BoxCollider collider = table.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        ApplyMaterial(table, new Color(0.2f, 0.22f, 0.26f));
        table.AddComponent<SortingTable>();
        FootstepSurface surface = table.AddComponent<FootstepSurface>();
        surface.Configure(FootstepSurface.SurfaceKind.Wood);
        MuseumEntityIdUtility.EnsureEntityId(table, "sorting_table_archive");
        counter.Created++;
    }

    private static void EnsureVitrine(Transform parent, string name, Vector3 position, Color accent, Counter counter)
    {
        if (parent.Find(name) != null)
        {
            counter.Skipped++;
            return;
        }

        GameObject vitrine = new GameObject(name);
        vitrine.transform.SetParent(parent, false);
        vitrine.transform.position = position;

        GameObject basePlinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        basePlinth.name = "Plinth";
        basePlinth.transform.SetParent(vitrine.transform, false);
        basePlinth.transform.localPosition = Vector3.zero;
        basePlinth.transform.localScale = new Vector3(1.2f, 0.35f, 0.8f);
        ApplyMaterial(basePlinth, new Color(0.22f, 0.22f, 0.24f));

        GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glass.name = "Glass";
        glass.transform.SetParent(vitrine.transform, false);
        glass.transform.localPosition = new Vector3(0f, 0.75f, 0f);
        glass.transform.localScale = new Vector3(1.1f, 1.1f, 0.7f);
        ApplyMaterial(glass, new Color(0.75f, 0.85f, 0.95f, 0.35f));
        Object.DestroyImmediate(glass.GetComponent<Collider>());

        GameObject artifact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        artifact.name = "Artifact";
        artifact.transform.SetParent(vitrine.transform, false);
        artifact.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        artifact.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        ApplyMaterial(artifact, accent);
        Object.DestroyImmediate(artifact.GetComponent<Collider>());

        counter.Created++;
    }

    private static void BuildEastClassicalAlcove(Transform root, int interactableLayer, Counter counter)
    {
        Transform alcove = GetOrCreateChild(root, "EastClassicalAlcove");
        EnsureBoxRoom(alcove, "AlcoveFloor", new Vector3(28f, 0f, 0f), new Vector3(8f, 0.02f, 10f), new Color(0.22f, 0.19f, 0.16f), isFloor: true);
        EnsureBoxRoom(alcove, "AlcoveWall", new Vector3(32f, 1.5f, 0f), new Vector3(0.35f, 3f, 10f), new Color(0.28f, 0.22f, 0.16f), isFloor: false);
        EnsureBench(alcove, "Bench_East", new Vector3(27f, 0.25f, 0f), new Vector3(2.2f, 0.5f, 0.7f), new Color(0.35f, 0.22f, 0.12f), counter);
        EnsurePedestal(alcove, "Pedestal_East", new Vector3(29f, 0.4f, -2.5f), new Color(0.4f, 0.38f, 0.34f), counter);
        EnsureVitrine(alcove, "Vitrine_East", new Vector3(30.5f, 0.55f, 1.5f), new Color(0.55f, 0.48f, 0.38f), counter);
        EnsureWorldLabel(alcove, "AlcoveSign_East", new Vector3(28f, 2.2f, 4f), "Classical alcove\nStudy benches");
        EnsureFloorPainting(alcove, "Overflow_BronzePortrait", "Bronze Portrait", new Vector3(28.5f, 0.025f, 2f),
            new Color(0.62f, 0.42f, 0.28f), GalleryWing.Classical, interactableLayer, counter);
    }

    private static void BuildWestImpressionistAlcove(Transform root, int interactableLayer, Counter counter)
    {
        Transform alcove = GetOrCreateChild(root, "WestImpressionistAlcove");
        EnsureBoxRoom(alcove, "AlcoveFloor", new Vector3(-28f, 0f, 0f), new Vector3(8f, 0.02f, 10f), new Color(0.16f, 0.22f, 0.18f), isFloor: true);
        EnsureBoxRoom(alcove, "AlcoveWall", new Vector3(-32f, 1.5f, 0f), new Vector3(0.35f, 3f, 10f), new Color(0.18f, 0.26f, 0.2f), isFloor: false);
        EnsureBench(alcove, "Bench_West", new Vector3(-27f, 0.25f, 0f), new Vector3(2.2f, 0.5f, 0.7f), new Color(0.2f, 0.32f, 0.24f), counter);
        EnsurePedestal(alcove, "Pedestal_West", new Vector3(-29f, 0.4f, 2.5f), new Color(0.34f, 0.4f, 0.36f), counter);
        EnsureVitrine(alcove, "Vitrine_West", new Vector3(-30.5f, 0.55f, -1.5f), new Color(0.42f, 0.58f, 0.48f), counter);
        EnsureWorldLabel(alcove, "AlcoveSign_West", new Vector3(-28f, 2.2f, 4f), "Impressionist alcove\nGarden view");
        EnsureFloorPainting(alcove, "Overflow_MistyShore", "Misty Shore", new Vector3(-28.5f, 0.025f, -2f),
            new Color(0.5f, 0.72f, 0.78f), GalleryWing.Impressionist, interactableLayer, counter);
    }

    private static void BuildCentralCuratorArea(Transform root, Counter counter)
    {
        Transform area = GetOrCreateChild(root, "CuratorArea");
        EnsureBoxRoom(area, "CuratorDesk", new Vector3(-4f, 0.45f, 14f), new Vector3(2.4f, 0.9f, 1.1f), new Color(0.32f, 0.24f, 0.16f), isFloor: false);
        EnsureBoxRoom(area, "CuratorChair", new Vector3(-4f, 0.25f, 12.6f), new Vector3(0.7f, 0.5f, 0.7f), new Color(0.25f, 0.25f, 0.28f), isFloor: false);
        EnsureWorldLabel(area, "CuratorSign", new Vector3(-4f, 1.6f, 14f), "Curator desk\nCollection notes");
        EnsurePedestal(area, "InfoKiosk", new Vector3(4f, 0.55f, 14f), new Color(0.18f, 0.22f, 0.28f), counter);
        counter.Created++;
    }

    private static void BuildPropsAndBarriers(Transform root, Counter counter)
    {
        Transform props = GetOrCreateChild(root, "Props");
        Vector3[] stanchionPositions =
        {
            new Vector3(-5f, 0.5f, 8f),
            new Vector3(-2f, 0.5f, 8f),
            new Vector3(2f, 0.5f, 8f),
            new Vector3(5f, 0.5f, 8f)
        };

        for (int i = 0; i < stanchionPositions.Length; i++)
        {
            string name = $"Stanchion_{i + 1}";
            if (props.Find(name) != null)
            {
                counter.Skipped++;
                continue;
            }

            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = name;
            post.transform.SetParent(props, false);
            post.transform.position = stanchionPositions[i];
            post.transform.localScale = new Vector3(0.08f, 0.5f, 0.08f);
            ApplyMaterial(post, new Color(0.75f, 0.72f, 0.65f));
            counter.Created++;
        }

        if (props.Find("RopeLine") == null)
        {
            GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rope.name = "RopeLine";
            rope.transform.SetParent(props, false);
            rope.transform.position = new Vector3(0f, 0.55f, 8f);
            rope.transform.localScale = new Vector3(10f, 0.04f, 0.04f);
            ApplyMaterial(rope, new Color(0.7f, 0.15f, 0.12f));
            Object.DestroyImmediate(rope.GetComponent<Collider>());
            counter.Created++;
        }
    }

    private static void AddOverflowCollectionPaintings(int interactableLayer, Counter counter)
    {
        Transform galleryWings = GameObject.Find("GalleryWings")?.transform;
        if (galleryWings == null)
        {
            return;
        }

        EnsureFloorPainting(galleryWings, "GalleryArt_Modern_VermillionBlock", "Vermillion Block",
            new Vector3(-8f, 0.025f, 12f), new Color(0.85f, 0.25f, 0.2f), GalleryWing.Modern, interactableLayer, counter);
        EnsureFloorPainting(galleryWings, "GalleryArt_Classical_IvoryColumn", "Ivory Column",
            new Vector3(8f, 0.025f, 12f), new Color(0.88f, 0.86f, 0.78f), GalleryWing.Classical, interactableLayer, counter);
        EnsureFloorPainting(galleryWings, "GalleryArt_Impressionist_LilacField", "Lilac Field",
            new Vector3(0f, 0.025f, 13.5f), new Color(0.72f, 0.58f, 0.82f), GalleryWing.Impressionist, interactableLayer, counter);

        Transform annex = GameObject.Find("Environment")?.transform.Find($"{RootName}/NorthArchive");
        if (annex != null)
        {
            EnsureFloorPainting(annex, "ArchiveArt_TwilightArch", "Twilight Arch", new Vector3(-4f, 0.025f, 34f),
                new Color(0.55f, 0.35f, 0.65f), GalleryWing.Modern, interactableLayer, counter);
            EnsureFloorPainting(annex, "ArchiveArt_CarbonStudy", "Carbon Study", new Vector3(0f, 0.025f, 35f),
                new Color(0.12f, 0.12f, 0.14f), GalleryWing.Modern, interactableLayer, counter);
            EnsureFloorPainting(annex, "ArchiveArt_NeonGrid", "Neon Grid", new Vector3(4f, 0.025f, 34f),
                new Color(0.2f, 0.75f, 0.85f), GalleryWing.Modern, interactableLayer, counter);
        }
    }

    private static GameObject EnsureArchiveMount(
        Transform parent,
        string mountName,
        Vector3 position,
        Quaternion rotation,
        GalleryWing wing,
        int interactableLayer,
        string slotPaintingId,
        string slotDisplayTitle,
        Counter counter)
    {
        Transform existing = parent.Find(mountName);
        if (existing != null)
        {
            counter.Skipped++;
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
        frame.transform.localScale = new Vector3(0.9f, 0.7f, 0.05f);
        frame.layer = interactableLayer;
        Object.DestroyImmediate(frame.GetComponent<Rigidbody>());
        ApplyMaterial(frame, new Color(0.22f, 0.24f, 0.28f));

        GameObject snapPoint = new GameObject("SnapPoint");
        snapPoint.transform.SetParent(mountRoot.transform, false);
        snapPoint.transform.localPosition = new Vector3(0f, 0f, -0.03f);

        PaintingMount mount = mountRoot.AddComponent<PaintingMount>();
        SerializedObject serializedMount = new SerializedObject(mount);
        SetObjectReference(serializedMount, "snapPoint", snapPoint.transform);
        SetEnum(serializedMount, "requiredWing", (int)wing);
        SetString(serializedMount, "requiredPaintingId", slotPaintingId);
        SetString(serializedMount, "slotDisplayTitle", slotDisplayTitle);
        serializedMount.ApplyModifiedPropertiesWithoutUndo();

        BoxCollider collider = mountRoot.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.9f, 0.7f, 0.1f);

        MuseumEntityIdUtility.EnsureEntityId(mountRoot, $"mount_archive_{Sanitize(mountName)}");
        MountPlacardSetup.EnsurePlacard(mount);
        counter.Created++;
        return mountRoot;
    }

    private static void EnsureFloorPainting(
        Transform parent,
        string objectName,
        string title,
        Vector3 position,
        Color color,
        GalleryWing wing,
        int interactableLayer,
        Counter counter)
    {
        if (GameObject.Find(objectName) != null)
        {
            counter.Skipped++;
            return;
        }

        GameObject painting = GameObject.CreatePrimitive(PrimitiveType.Cube);
        painting.name = objectName;
        painting.transform.SetParent(parent, false);
        painting.layer = interactableLayer;
        painting.transform.position = position;
        painting.transform.localScale = new Vector3(0.6f, 0.05f, 0.4f);

        Rigidbody body = painting.AddComponent<Rigidbody>();
        body.mass = 2f;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        InteractablePainting interactable = painting.AddComponent<InteractablePainting>();
        SerializedObject serialized = new SerializedObject(interactable);
        SetString(serialized, "paintingTitle", title);
        SetEnum(serialized, "wing", (int)wing);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        ApplyMaterial(painting, color);
        MuseumEntityIdUtility.EnsureEntityId(painting, $"painting_{Sanitize(title)}");
        counter.Created++;
    }

    private static void WireGallerySection(
        Transform parent,
        string sectionName,
        GalleryWing wing,
        string displayName,
        List<PaintingMount> mounts,
        List<Renderer> frameRenderers)
    {
        if (mounts.Count == 0)
        {
            return;
        }

        Transform sectionParent = parent != null ? parent : GameObject.Find("GalleryWings")?.transform;
        if (sectionParent == null)
        {
            sectionParent = GameObject.Find("MuseumFeatures")?.transform;
        }

        GameObject sectionObject = GameObject.Find(sectionName);
        if (sectionObject == null)
        {
            sectionObject = new GameObject(sectionName);
            sectionObject.transform.SetParent(sectionParent != null ? sectionParent : null, false);
        }

        GallerySection section = sectionObject.GetComponent<GallerySection>();
        if (section == null)
        {
            section = sectionObject.AddComponent<GallerySection>();
        }

        SerializedObject serializedSection = new SerializedObject(section);
        SetEnum(serializedSection, "sectionWing", (int)wing);
        SetString(serializedSection, "sectionDisplayName", displayName);
        AssignObjectList(serializedSection.FindProperty("mounts"), mounts);
        AssignObjectList(serializedSection.FindProperty("glowRenderers"), frameRenderers);
        serializedSection.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureBoxRoom(Transform parent, string name, Vector3 position, Vector3 scale, Color color, bool isFloor)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return;
        }

        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.position = position;
        box.transform.localScale = scale;
        ApplyMaterial(box, color);

        if (isFloor)
        {
            Object.DestroyImmediate(box.GetComponent<Collider>());
        }
    }

    private static void EnsureDoorArch(Transform parent, string name, Vector3 position, Vector3 scale)
    {
        if (parent.Find(name) != null)
        {
            return;
        }

        GameObject arch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arch.name = name;
        arch.transform.SetParent(parent, false);
        arch.transform.position = position;
        arch.transform.localScale = scale;
        ApplyMaterial(arch, new Color(0.32f, 0.28f, 0.22f));
        Object.DestroyImmediate(arch.GetComponent<Collider>());
    }

    private static void EnsureBench(Transform parent, string name, Vector3 position, Vector3 scale, Color color, Counter counter)
    {
        if (parent.Find(name) != null)
        {
            counter.Skipped++;
            return;
        }

        GameObject bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bench.name = name;
        bench.transform.SetParent(parent, false);
        bench.transform.position = position;
        bench.transform.localScale = scale;
        ApplyMaterial(bench, color);
        counter.Created++;
    }

    private static void EnsurePedestal(Transform parent, string name, Vector3 position, Color color, Counter counter)
    {
        if (parent.Find(name) != null)
        {
            counter.Skipped++;
            return;
        }

        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = name;
        pedestal.transform.SetParent(parent, false);
        pedestal.transform.position = position;
        pedestal.transform.localScale = new Vector3(0.55f, 0.45f, 0.55f);
        ApplyMaterial(pedestal, color);

        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cap.name = "Cap";
        cap.transform.SetParent(pedestal.transform, false);
        cap.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        cap.transform.localScale = new Vector3(0.9f, 0.08f, 0.9f);
        ApplyMaterial(cap, color * 1.1f);
        Object.DestroyImmediate(cap.GetComponent<Collider>());
        counter.Created++;
    }

    private static void EnsureWorldLabel(Transform parent, string name, Vector3 position, string text)
    {
        if (parent.Find(name) != null)
        {
            return;
        }

        GameObject signRoot = new GameObject(name);
        signRoot.transform.SetParent(parent, false);
        signRoot.transform.position = position;
        signRoot.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        Canvas canvas = signRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        signRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(320f, 80f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(signRoot.transform, false);
        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Text label = textObject.AddComponent<Text>();
        label.font = font;
        label.fontSize = 28;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(0.9f, 0.88f, 0.82f);
        label.text = text;
    }

    private static void CollectMount(GameObject mountObject, List<PaintingMount> mounts, List<Renderer> renderers)
    {
        if (mountObject == null)
        {
            return;
        }

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

    private static Transform GetOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing;
        }

        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child.transform;
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

    private static void AssignObjectList(SerializedProperty listProperty, List<PaintingMount> mounts)
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

    private static void AssignObjectList(SerializedProperty listProperty, List<Renderer> renderers)
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
