using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility that builds the Zero-to-One prototype scene:
/// environment room, lighting, and first-person player hierarchy.
/// </summary>
public static class PrototypeSceneSetup
{
    private const string SetupCompleteKey = "ZeroToOnePrototypeSetupComplete";

    [InitializeOnLoadMethod]
    private static void SaveSceneAfterSetup()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorPrefs.GetBool(SetupCompleteKey, false) || GameObject.Find("Environment") == null)
            {
                return;
            }

            if (SceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveOpenScenes();
            }
        };
    }

    [InitializeOnLoadMethod]
    private static void AutoSetupOnCompile()
    {
        EditorApplication.delayCall += TryAutoSetup;
    }

    [MenuItem("Game/Setup Zero-to-One Prototype")]
    public static void SetupFromMenu()
    {
        EditorPrefs.DeleteKey(SetupCompleteKey);
        RunSetup();
    }

    public static void ExecuteFromCommandLine()
    {
        RunSetup();
        EditorApplication.Exit(0);
    }

    private static void TryAutoSetup()
    {
        if (EditorPrefs.GetBool(SetupCompleteKey, false))
        {
            return;
        }

        if (GameObject.Find("Environment") != null)
        {
            EditorPrefs.SetBool(SetupCompleteKey, true);
            return;
        }

        RunSetup();
    }

    private static void RunSetup()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.isLoaded)
        {
            Debug.LogError("PrototypeSceneSetup: No active scene loaded.");
            return;
        }

        RemoveExistingPrototypeObjects();

        GameObject environment = new GameObject("Environment");

        CreateFloor(environment.transform);
        CreateWalls(environment.transform);
        SetupLighting();
        SetupPlayer();

        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveOpenScenes();
        EditorPrefs.SetBool(SetupCompleteKey, true);

        Debug.Log("Zero-to-One prototype setup complete. Press Play to test the first-person controller.");
    }

    private static void RemoveExistingPrototypeObjects()
    {
        DestroyIfExists("Environment");
        DestroyIfExists("Player");
        DestroyIfExists("Main Sun");

        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            Object.DestroyImmediate(mainCamera);
        }
    }

    private static void DestroyIfExists(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }
    }

    private static void CreateFloor(Transform parent)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(parent, false);
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(5f, 1f, 5f);

        // Planes ship with a MeshCollider; swap to BoxCollider per spec.
        Object.DestroyImmediate(floor.GetComponent<MeshCollider>());
        BoxCollider floorCollider = floor.AddComponent<BoxCollider>();
        floorCollider.size = new Vector3(10f, 0.01f, 10f);
    }

    private static void CreateWalls(Transform parent)
    {
        // Default plane is 10x10 units; scale (5,1,5) yields a 50x50 floor centered at origin.
        const float halfExtent = 25f;
        const float wallHeight = 3f;
        const float wallThickness = 1f;
        float wallCenterY = wallHeight * 0.5f;

        CreateWall(parent, "Wall_North", new Vector3(0f, wallCenterY, halfExtent),
            new Vector3(50f, wallHeight, wallThickness));
        CreateWall(parent, "Wall_South", new Vector3(0f, wallCenterY, -halfExtent),
            new Vector3(50f, wallHeight, wallThickness));
        CreateWall(parent, "Wall_East", new Vector3(halfExtent, wallCenterY, 0f),
            new Vector3(wallThickness, wallHeight, 50f));
        CreateWall(parent, "Wall_West", new Vector3(-halfExtent, wallCenterY, 0f),
            new Vector3(wallThickness, wallHeight, 50f));
    }

    private static void CreateWall(Transform parent, string wallName, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = wallName;
        wall.transform.SetParent(parent, false);
        wall.transform.position = position;
        wall.transform.localScale = scale;

        // Cubes include BoxCollider by default; ensure one is present.
        if (wall.GetComponent<BoxCollider>() == null)
        {
            wall.AddComponent<BoxCollider>();
        }
    }

    private static void SetupLighting()
    {
        GameObject sun = new GameObject("Main Sun");
        Light light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Link the sun to render settings for ambient/skybox integration.
        RenderSettings.sun = light;
    }

    private static void SetupPlayer()
    {
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 1f, 0f);

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        controller.center = new Vector3(0f, 0f, 0f);

        FirstPersonController fpsController = player.AddComponent<FirstPersonController>();

        GameObject cameraObject = new GameObject("PlayerCamera");
        cameraObject.transform.SetParent(player.transform, false);
        cameraObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        cameraObject.AddComponent<AudioListener>();

        // Wire the camera reference into the controller via SerializedObject (inspector link).
        SerializedObject serializedController = new SerializedObject(fpsController);
        SerializedProperty cameraProperty = serializedController.FindProperty("playerCamera");
        if (cameraProperty != null)
        {
            cameraProperty.objectReferenceValue = cameraObject.transform;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning("PrototypeSceneSetup: Could not find playerCamera field on FirstPersonController.");
        }
    }
}
