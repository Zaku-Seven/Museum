using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Adds wall mounts, interaction HUD, and extra test paintings to the museum prototype.
/// </summary>
public static class MuseumGameplaySetup
{
    private const string SetupCompleteKey = "MuseumGameplaySetupComplete";
    private const string InteractableLayerName = "Interactable";

    [InitializeOnLoadMethod]
    private static void AutoSetupOnCompile()
    {
        EditorApplication.delayCall += TryAutoSetup;
    }

    [MenuItem("Game/Setup Museum Gameplay")]
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

        if (GameObject.Find("PlayerCamera") == null || GameObject.Find("Environment") == null)
        {
            return;
        }

        RunSetup();
    }

    private static void RunSetup()
    {
        int interactableLayer = LayerMask.NameToLayer(InteractableLayerName);
        if (interactableLayer < 0)
        {
            Debug.LogError("MuseumGameplaySetup: Interactable layer not found.");
            return;
        }

        Transform museumRoot = GetOrCreateRoot("MuseumFeatures");
        // The three north-wall mounts form a small Modern wall so the existing menu already
        // demonstrates wing sorting (TestPainting = Modern fits; TestPainting_2 = Classical is rejected).
        CreateWallMounts(museumRoot, interactableLayer, GalleryWing.Modern);
        UpgradeExistingPaintings(interactableLayer);
        CreateFloorPainting("TestPainting_2", "Blue Horizon", new Vector3(3f, 0.025f, 5f), new Color(0.2f, 0.35f, 0.75f), GalleryWing.Classical, interactableLayer);
        SetupInteractionHud();
        FixExistingHoldPoint();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        EditorPrefs.SetBool(SetupCompleteKey, true);

        Debug.Log("Museum gameplay setup complete. Pick up floor paintings and hang them on the north wall mounts.");
    }

    private static Transform GetOrCreateRoot(string rootName)
    {
        GameObject existing = GameObject.Find(rootName);
        if (existing != null)
        {
            return existing.transform;
        }

        return new GameObject(rootName).transform;
    }

    private static void CreateWallMounts(Transform parent, int interactableLayer, GalleryWing wing)
    {
        float[] mountPositionsX = { -8f, 0f, 8f };
        for (int i = 0; i < mountPositionsX.Length; i++)
        {
            string mountName = $"WallMount_{i + 1}";
            if (GameObject.Find(mountName) != null)
            {
                continue;
            }

            CreateWallMount(parent, mountName, new Vector3(mountPositionsX[i], 1.5f, 24f), wing, interactableLayer);
        }
    }

    private static void CreateWallMount(Transform parent, string mountName, Vector3 position, GalleryWing wing, int interactableLayer)
    {
        GameObject mountRoot = new GameObject(mountName);
        mountRoot.transform.SetParent(parent, false);
        mountRoot.transform.position = position;
        mountRoot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        mountRoot.layer = interactableLayer;

        GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "Frame";
        frame.transform.SetParent(mountRoot.transform, false);
        frame.transform.localPosition = Vector3.zero;
        frame.transform.localScale = new Vector3(0.9f, 0.7f, 0.05f);
        frame.layer = interactableLayer;

        Object.DestroyImmediate(frame.GetComponent<Rigidbody>());

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

        Renderer frameRenderer = frame.GetComponent<Renderer>();
        if (frameRenderer != null)
        {
            frameRenderer.sharedMaterial.color = new Color(0.25f, 0.2f, 0.15f);
        }
    }

    private static void UpgradeExistingPaintings(int interactableLayer)
    {
        ConfigurePainting(GameObject.Find("TestPainting"), "Sunset Study", new Color(0.85f, 0.55f, 0.2f), GalleryWing.Modern, interactableLayer);
    }

    private static void CreateFloorPainting(string objectName, string title, Vector3 position, Color color, GalleryWing wing, int interactableLayer)
    {
        if (GameObject.Find(objectName) != null)
        {
            return;
        }

        GameObject painting = GameObject.CreatePrimitive(PrimitiveType.Cube);
        painting.name = objectName;
        painting.layer = interactableLayer;
        painting.transform.position = position;
        painting.transform.localScale = new Vector3(0.6f, 0.05f, 0.4f);

        ConfigurePaintingComponents(painting, title, color, wing);
    }

    private static void ConfigurePainting(GameObject painting, string title, Color color, GalleryWing wing, int interactableLayer)
    {
        if (painting == null)
        {
            return;
        }

        painting.layer = interactableLayer;
        ConfigurePaintingComponents(painting, title, color, wing);
    }

    private static void ConfigurePaintingComponents(GameObject painting, string title, Color color, GalleryWing wing)
    {
        Rigidbody rigidbody = painting.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = painting.AddComponent<Rigidbody>();
        }

        rigidbody.mass = 2f;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        InteractablePainting interactablePainting = painting.GetComponent<InteractablePainting>();
        if (interactablePainting == null)
        {
            interactablePainting = painting.AddComponent<InteractablePainting>();
        }

        SerializedObject serializedPainting = new SerializedObject(interactablePainting);
        SerializedProperty titleProperty = serializedPainting.FindProperty("paintingTitle");
        if (titleProperty != null)
        {
            titleProperty.stringValue = title;
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
            renderer.sharedMaterial.color = color;
        }
    }

    [MenuItem("Game/Fix Carry Settings")]
    public static void FixCarrySettingsFromMenu()
    {
        FixExistingHoldPoint();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Carry hold point updated to chest-height, arm's length position.");
    }

    private static void FixExistingHoldPoint()
    {
        GameObject cameraObject = GameObject.Find("PlayerCamera");
        if (cameraObject == null)
        {
            return;
        }

        Transform holdPoint = cameraObject.transform.Find("HoldPoint");
        if (holdPoint == null)
        {
            return;
        }

        holdPoint.localPosition = new Vector3(0f, -0.22f, 0.68f);
        holdPoint.localRotation = Quaternion.Euler(6f, 0f, 0f);
    }

    private static void SetupInteractionHud()
    {
        if (GameObject.Find("InteractionHUD") != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("InteractionHUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject crosshairObject = CreateUiText(canvasObject.transform, "Crosshair", defaultFont, 28, TextAnchor.MiddleCenter);
        RectTransform crosshairRect = crosshairObject.GetComponent<RectTransform>();
        crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRect.anchoredPosition = Vector2.zero;
        crosshairRect.sizeDelta = new Vector2(40f, 40f);
        Text crosshairText = crosshairObject.GetComponent<Text>();
        crosshairText.text = "+";
        crosshairText.color = new Color(1f, 1f, 1f, 0.85f);

        GameObject promptObject = CreateUiText(canvasObject.transform, "Prompt", defaultFont, 20, TextAnchor.LowerCenter);
        RectTransform promptRect = promptObject.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0f);
        promptRect.anchorMax = new Vector2(0.5f, 0f);
        promptRect.anchoredPosition = new Vector2(0f, 80f);
        promptRect.sizeDelta = new Vector2(700f, 40f);
        Text promptText = promptObject.GetComponent<Text>();
        promptText.color = Color.white;

        GameObject progressObject = CreateUiText(canvasObject.transform, "Progress", defaultFont, 18, TextAnchor.LowerLeft);
        RectTransform progressRect = progressObject.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0f, 0f);
        progressRect.anchorMax = new Vector2(0f, 0f);
        progressRect.pivot = new Vector2(0f, 0f);
        progressRect.anchoredPosition = new Vector2(24f, 24f);
        progressRect.sizeDelta = new Vector2(900f, 30f);
        Text progressText = progressObject.GetComponent<Text>();
        progressText.color = new Color(0.85f, 0.9f, 1f);

        GameObject cameraObject = GameObject.Find("PlayerCamera");
        if (cameraObject == null)
        {
            Debug.LogWarning("MuseumGameplaySetup: PlayerCamera not found; HUD created but not linked.");
            return;
        }

        InteractionHUD hud = cameraObject.GetComponent<InteractionHUD>();
        if (hud == null)
        {
            hud = cameraObject.AddComponent<InteractionHUD>();
        }

        ArtPickup artPickup = cameraObject.GetComponent<ArtPickup>();
        SerializedObject serializedHud = new SerializedObject(hud);
        SerializedProperty pickupProperty = serializedHud.FindProperty("artPickup");
        SerializedProperty promptProperty = serializedHud.FindProperty("promptText");
        SerializedProperty crosshairProperty = serializedHud.FindProperty("crosshairText");
        SerializedProperty progressProperty = serializedHud.FindProperty("progressText");

        if (pickupProperty != null)
        {
            pickupProperty.objectReferenceValue = artPickup;
        }

        if (promptProperty != null)
        {
            promptProperty.objectReferenceValue = promptText;
        }

        if (crosshairProperty != null)
        {
            crosshairProperty.objectReferenceValue = crosshairText;
        }

        if (progressProperty != null)
        {
            progressProperty.objectReferenceValue = progressText;
        }

        serializedHud.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateUiText(Transform parent, string objectName, Font font, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        return textObject;
    }
}
