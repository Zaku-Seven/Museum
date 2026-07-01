using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds the cozy curation "Whitebox Museum" layout in the active scene:
/// Grand Lobby hub, Pre-1900 Art wing (left), Fossil &amp; Antiquity wing (right).
/// Idempotent — re-run Game → Build Whitebox Museum to refresh geometry.
/// </summary>
public static class WhiteboxMuseumSetup
{
    private const string RootName = "Museum_Architecture";
    private const string LobbyName = "Grand_Lobby";
    private const string ArtWingName = "Art_Wing_Pre1900";
    private const string FossilWingName = "Fossil_Wing";

    // Unity plane mesh is 10×10; scale multiplies footprint.
    private const float PlaneUnit = 10f;

    [MenuItem("Game/Build Whitebox Museum", priority = 2)]
    public static void BuildFromMenu()
    {
        BuildWhiteboxMuseum();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Whitebox Museum ready — press Play and walk through the left/right archways.");
    }

    public static void BuildWhiteboxMuseum()
    {
        Transform root = EnsureRoot();
        Transform lobby = EnsureChild(root, LobbyName);
        Transform artWing = EnsureChild(root, ArtWingName);
        Transform fossilWing = EnsureChild(root, FossilWingName);

        ClearChildren(lobby);
        ClearChildren(artWing);
        ClearChildren(fossilWing);

        Material lobbyStone = CreateMaterial("Whitebox_LobbyStone", new Color(0.72f, 0.7f, 0.66f));
        Material artWarm = CreateMaterial("Whitebox_ArtWarm", new Color(0.62f, 0.38f, 0.32f));
        Material artFloor = CreateMaterial("Whitebox_ArtFloor", new Color(0.48f, 0.32f, 0.28f));
        Material fossilSlate = CreateMaterial("Whitebox_FossilSlate", new Color(0.42f, 0.46f, 0.5f));
        Material fossilFloor = CreateMaterial("Whitebox_FossilFloor", new Color(0.36f, 0.4f, 0.44f));
        Material pillarMat = CreateMaterial("Whitebox_Pillar", new Color(0.58f, 0.56f, 0.52f));
        Material partitionMat = CreateMaterial("Whitebox_Partition", new Color(0.55f, 0.34f, 0.3f));
        Material daisMat = CreateMaterial("Whitebox_Dais", new Color(0.5f, 0.52f, 0.56f));

        BuildGrandLobby(lobby, lobbyStone, pillarMat);
        BuildArtWing(artWing, artWarm, artFloor, partitionMat);
        BuildFossilWing(fossilWing, fossilSlate, fossilFloor, daisMat);
        SetupMuseumLighting(lobby, artWing);
        PlacePlayerInLobby();
    }

    private static void BuildGrandLobby(Transform lobby, Material floorMat, Material pillarMat)
    {
        Vector3 floorScale = new Vector3(20f, 1f, 20f);
        float halfX = PlaneUnit * floorScale.x * 0.5f;
        float halfZ = PlaneUnit * floorScale.z * 0.5f;
        const float wallHeight = 10f;
        const float wallThickness = 1f;
        const float archWidth = 4f;
        float wallCenterY = wallHeight * 0.5f;

        CreateFloorPlane(lobby, "Floor", Vector3.zero, floorScale, floorMat);

        // North / south — solid enclosure.
        CreateWallCube(lobby, "Wall_North", new Vector3(0f, wallCenterY, halfZ + wallThickness * 0.5f),
            new Vector3(halfX * 2f + wallThickness * 2f, wallHeight, wallThickness), floorMat);
        CreateWallCube(lobby, "Wall_South", new Vector3(0f, wallCenterY, -halfZ - wallThickness * 0.5f),
            new Vector3(halfX * 2f + wallThickness * 2f, wallHeight, wallThickness), floorMat);

        // West / east — 4-unit archways centered on Z = 0 (connects to wings).
        CreateWallWithCenterOpening(lobby, "Wall_West", -halfX - wallThickness * 0.5f, halfZ * 2f, wallHeight,
            wallThickness, archWidth, floorMat, alongZ: true);
        CreateWallWithCenterOpening(lobby, "Wall_East", halfX + wallThickness * 0.5f, halfZ * 2f, wallHeight,
            wallThickness, archWidth, floorMat, alongZ: true);

        Vector3[] pillarPositions =
        {
            new Vector3(-12f, wallHeight * 0.5f, -12f),
            new Vector3(12f, wallHeight * 0.5f, -12f),
            new Vector3(-12f, wallHeight * 0.5f, 12f),
            new Vector3(12f, wallHeight * 0.5f, 12f)
        };

        for (int i = 0; i < pillarPositions.Length; i++)
        {
            CreatePillar(lobby, $"Pillar_{i + 1}", pillarPositions[i], new Vector3(1f, 10f, 1f), pillarMat);
        }
    }

    private static void BuildArtWing(Transform wing, Material wallMat, Material floorMat, Material partitionMat)
    {
        // Long gallery to the left (−X) of the lobby; hallway runs along Z.
        Vector3 floorScale = new Vector3(10f, 1f, 30f);
        float halfX = PlaneUnit * floorScale.x * 0.5f;
        float halfZ = PlaneUnit * floorScale.z * 0.5f;
        const float wallHeight = 8f;
        const float wallThickness = 1f;
        float wallCenterY = wallHeight * 0.5f;

        float lobbyHalfX = PlaneUnit * 20f * 0.5f;
        float wingCenterX = -lobbyHalfX - halfX;
        Vector3 wingCenter = new Vector3(wingCenterX, 0f, 0f);

        CreateFloorPlane(wing, "Floor", wingCenter, floorScale, floorMat);

        float outerX = wingCenterX - halfX - wallThickness * 0.5f;
        float innerX = wingCenterX + halfX + wallThickness * 0.5f;

        CreateWallCube(wing, "Wall_Outer", new Vector3(outerX, wallCenterY, 0f),
            new Vector3(wallThickness, wallHeight, halfZ * 2f), wallMat);
        CreateWallCube(wing, "Wall_Inner", new Vector3(innerX, wallCenterY, 0f),
            new Vector3(wallThickness, wallHeight, halfZ * 2f), wallMat);
        CreateWallCube(wing, "Wall_North", new Vector3(wingCenterX, wallCenterY, halfZ + wallThickness * 0.5f),
            new Vector3(halfX * 2f, wallHeight, wallThickness), wallMat);
        CreateWallCube(wing, "Wall_South", new Vector3(wingCenterX, wallCenterY, -halfZ - wallThickness * 0.5f),
            new Vector3(halfX * 2f, wallHeight, wallThickness), wallMat);

        // Freestanding partition walls down the gallery spine (painting shadow cutouts later).
        float[] partitionZs = { -90f, -45f, 0f, 45f, 90f };
        for (int i = 0; i < partitionZs.Length; i++)
        {
            CreateWallCube(wing, $"Partition_{i + 1}", new Vector3(wingCenterX, wallHeight * 0.45f, partitionZs[i]),
                new Vector3(0.35f, wallHeight * 0.9f, 8f), partitionMat);
        }
    }

    private static void BuildFossilWing(Transform wing, Material wallMat, Material floorMat, Material daisMat)
    {
        Vector3 floorScale = new Vector3(25f, 1f, 25f);
        float halfX = PlaneUnit * floorScale.x * 0.5f;
        float halfZ = PlaneUnit * floorScale.z * 0.5f;
        const float wallHeight = 15f;
        const float wallThickness = 1f;
        float wallCenterY = wallHeight * 0.5f;

        float lobbyHalfX = PlaneUnit * 20f * 0.5f;
        float wingCenterX = lobbyHalfX + halfX;
        Vector3 wingCenter = new Vector3(wingCenterX, 0f, 0f);

        CreateFloorPlane(wing, "Floor", wingCenter, floorScale, floorMat);

        float outerX = wingCenterX + halfX + wallThickness * 0.5f;
        float innerX = wingCenterX - halfX - wallThickness * 0.5f;

        CreateWallCube(wing, "Wall_Outer", new Vector3(outerX, wallCenterY, 0f),
            new Vector3(wallThickness, wallHeight, halfZ * 2f), wallMat);
        CreateWallCube(wing, "Wall_Inner", new Vector3(innerX, wallCenterY, 0f),
            new Vector3(wallThickness, wallHeight, halfZ * 2f), wallMat);
        CreateWallCube(wing, "Wall_North", new Vector3(wingCenterX, wallCenterY, halfZ + wallThickness * 0.5f),
            new Vector3(halfX * 2f, wallHeight, wallThickness), wallMat);
        CreateWallCube(wing, "Wall_South", new Vector3(wingCenterX, wallCenterY, -halfZ - wallThickness * 0.5f),
            new Vector3(halfX * 2f, wallHeight, wallThickness), wallMat);

        // Central raised dais for hero fossils / statues.
        Vector3 daisScale = new Vector3(8f, 1f, 15f);
        float daisTopY = daisScale.y * 0.5f;
        CreateWallCube(wing, "Central_Dais", new Vector3(wingCenterX, daisTopY, 0f), daisScale, daisMat);

        CreateRamp(wing, "Ramp_North", new Vector3(wingCenterX, daisTopY * 0.5f, daisScale.z * 0.5f + 2f),
            new Vector3(daisScale.x * 0.9f, daisTopY, 4f), wallMat, pitchDegrees: -18f);
        CreateRamp(wing, "Ramp_South", new Vector3(wingCenterX, daisTopY * 0.5f, -daisScale.z * 0.5f - 2f),
            new Vector3(daisScale.x * 0.9f, daisTopY, 4f), wallMat, pitchDegrees: 18f);
    }

    private static void SetupMuseumLighting(Transform lobby, Transform artWing)
    {
        Light sun = FindOrCreateDirectionalLight();
        sun.transform.rotation = Quaternion.Euler(42f, 125f, 0f);
        sun.intensity = 1.15f;
        sun.shadows = LightShadows.Soft;
        sun.color = new Color(1f, 0.96f, 0.88f);
        RenderSettings.sun = sun;

        Transform lightingRoot = EnsureChild(lobby.parent, "Whitebox_Lighting");

        EnsurePointLight(lightingRoot, "Lobby_Chandelier", Vector3.zero, new Color(1f, 0.88f, 0.62f), 2.4f, 45f);

        Transform artLights = EnsureChild(lightingRoot, "ArtWing_Spots");
        float wingCenterX = -PlaneUnit * 20f * 0.5f - PlaneUnit * 10f * 0.5f;
        float[] spotZs = { -90f, -45f, 0f, 45f, 90f };
        for (int i = 0; i < spotZs.Length; i++)
        {
            EnsureSpotLight(
                artLights,
                $"GallerySpot_{i + 1}",
                new Vector3(wingCenterX, 7.5f, spotZs[i]),
                Quaternion.Euler(58f, 0f, 0f),
                new Color(1f, 0.82f, 0.72f),
                1.6f);
        }
    }

    private static void PlacePlayerInLobby()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("WhiteboxMuseumSetup: No Player found — run Setup Zero-to-One Prototype first.");
            return;
        }

        player.transform.position = new Vector3(0f, 1f, 0f);
        player.transform.rotation = Quaternion.identity;
        EditorUtility.SetDirty(player);
    }

    private static Transform EnsureRoot()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            return existing.transform;
        }

        GameObject root = new GameObject(RootName);
        root.transform.position = Vector3.zero;
        return root.transform;
    }

    private static Transform EnsureChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child;
        }

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        return go.transform;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static void CreateFloorPlane(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = name;
        floor.transform.SetParent(parent, false);
        floor.transform.position = position;
        floor.transform.localScale = scale;
        ApplyMaterial(floor, material);
        EnsureSolidCollider(floor, isFloor: true);
    }

    private static void CreateWallCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        ApplyMaterial(wall, material);
        EnsureSolidCollider(wall, isFloor: false);
    }

    private static void CreateWallWithCenterOpening(
        Transform parent,
        string baseName,
        float wallCenterXOrZ,
        float totalLength,
        float wallHeight,
        float thickness,
        float openingWidth,
        Material material,
        bool alongZ)
    {
        float wallCenterY = wallHeight * 0.5f;
        float segmentLength = (totalLength - openingWidth) * 0.5f;
        float segmentCenterOffset = (openingWidth + segmentLength) * 0.5f;

        if (alongZ)
        {
            CreateWallCube(parent, $"{baseName}_North", new Vector3(wallCenterXOrZ, wallCenterY, segmentCenterOffset),
                new Vector3(thickness, wallHeight, segmentLength), material);
            CreateWallCube(parent, $"{baseName}_South", new Vector3(wallCenterXOrZ, wallCenterY, -segmentCenterOffset),
                new Vector3(thickness, wallHeight, segmentLength), material);
        }
        else
        {
            CreateWallCube(parent, $"{baseName}_East", new Vector3(segmentCenterOffset, wallCenterY, wallCenterXOrZ),
                new Vector3(segmentLength, wallHeight, thickness), material);
            CreateWallCube(parent, $"{baseName}_West", new Vector3(-segmentCenterOffset, wallCenterY, wallCenterXOrZ),
                new Vector3(segmentLength, wallHeight, thickness), material);
        }
    }

    private static void CreatePillar(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = name;
        pillar.transform.SetParent(parent, false);
        pillar.transform.position = position;
        pillar.transform.localScale = scale;
        ApplyMaterial(pillar, material);
        EnsureSolidCollider(pillar, isFloor: false);
    }

    private static void CreateRamp(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 scale,
        Material material,
        float pitchDegrees)
    {
        GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = name;
        ramp.transform.SetParent(parent, false);
        ramp.transform.position = position;
        ramp.transform.localScale = scale;
        ramp.transform.rotation = Quaternion.Euler(pitchDegrees, 0f, 0f);
        ApplyMaterial(ramp, material);
        EnsureSolidCollider(ramp, isFloor: false);
    }

    private static void EnsureSolidCollider(GameObject target, bool isFloor)
    {
        MeshCollider meshCollider = target.GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            Object.DestroyImmediate(meshCollider);
        }

        if (target.GetComponent<BoxCollider>() == null)
        {
            target.AddComponent<BoxCollider>();
        }

        if (isFloor)
        {
            BoxCollider box = target.GetComponent<BoxCollider>();
            box.size = new Vector3(10f, 0.05f, 10f);
            box.center = Vector3.zero;
        }
    }

    private static Light FindOrCreateDirectionalLight()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null && lights[i].type == LightType.Directional)
            {
                return lights[i];
            }
        }

        GameObject sunObject = GameObject.Find("Main Sun") ?? new GameObject("Directional Light");
        Light light = sunObject.GetComponent<Light>() ?? sunObject.AddComponent<Light>();
        light.type = LightType.Directional;
        return light;
    }

    private static void EnsurePointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
    {
        Transform existing = parent.Find(name);
        GameObject lightObject = existing != null ? existing.gameObject : new GameObject(name);
        if (existing == null)
        {
            lightObject.transform.SetParent(parent, false);
        }

        lightObject.transform.position = position;
        Light light = lightObject.GetComponent<Light>() ?? lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
    }

    private static void EnsureSpotLight(
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        Color color,
        float intensity)
    {
        Transform existing = parent.Find(name);
        GameObject lightObject = existing != null ? existing.gameObject : new GameObject(name);
        if (existing == null)
        {
            lightObject.transform.SetParent(parent, false);
        }

        lightObject.transform.position = position;
        lightObject.transform.rotation = rotation;

        Light light = lightObject.GetComponent<Light>() ?? lightObject.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = color;
        light.intensity = intensity;
        light.range = 22f;
        light.spotAngle = 55f;
        light.shadows = LightShadows.Soft;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.color = color;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material material = new Material(shader) { color = color };
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void ApplyMaterial(GameObject target, Material material)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }
    }
}
