using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Adds wall mounts, interaction HUD, and extra test paintings to the museum prototype.
///
/// Recommended setup run order: Setup Zero-to-One Prototype → Setup Art Pickup Test →
/// Setup Museum Gameplay (this) → Setup Gallery Wings (<see cref="GalleryContentSetup"/>).
/// This step builds the north-wall Modern mounts and the shared HUD (including the
/// per-wing progress line); Gallery Wings then adds the full 3-wing sorting content.
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
        WireNorthWallGallerySection(museumRoot);
        UpgradeExistingPaintings(interactableLayer);
        CreateFloorPainting("TestPainting_2", "Blue Horizon", new Vector3(3f, 0.025f, 5f), new Color(0.2f, 0.35f, 0.75f), GalleryWing.Classical, interactableLayer);
        SetupInteractionHud();
        SetupPlayerGameplayComponents();
        PlayerVisualSetup.EnsurePlayerBean();
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

    private static void WireNorthWallGallerySection(Transform museumRoot)
    {
        const string sectionName = "Section_ModernNorth";
        var mounts = new System.Collections.Generic.List<PaintingMount>();
        var frameRenderers = new System.Collections.Generic.List<Renderer>();

        for (int i = 1; i <= 3; i++)
        {
            GameObject mountObject = GameObject.Find($"WallMount_{i}");
            if (mountObject == null)
            {
                continue;
            }

            PaintingMount mount = mountObject.GetComponent<PaintingMount>();
            if (mount != null)
            {
                mounts.Add(mount);
            }

            Renderer frameRenderer = mountObject.GetComponentInChildren<Renderer>();
            if (frameRenderer != null)
            {
                frameRenderers.Add(frameRenderer);
            }
        }

        if (mounts.Count == 0)
        {
            return;
        }

        GameObject sectionObject = GameObject.Find(sectionName);
        if (sectionObject == null)
        {
            sectionObject = new GameObject(sectionName);
            sectionObject.transform.SetParent(museumRoot, false);
        }

        GallerySection section = sectionObject.GetComponent<GallerySection>();
        if (section == null)
        {
            section = sectionObject.AddComponent<GallerySection>();
        }

        SerializedObject serializedSection = new SerializedObject(section);
        SerializedProperty wingProperty = serializedSection.FindProperty("sectionWing");
        if (wingProperty != null)
        {
            wingProperty.enumValueIndex = (int)GalleryWing.Modern;
        }

        SerializedProperty displayNameProperty = serializedSection.FindProperty("sectionDisplayName");
        if (displayNameProperty != null)
        {
            displayNameProperty.stringValue = "Modern (North)";
        }

        AssignObjectList(serializedSection.FindProperty("mounts"), mounts.ConvertAll(m => (Object)m));
        AssignObjectList(serializedSection.FindProperty("glowRenderers"), frameRenderers.ConvertAll(r => (Object)r));
        serializedSection.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignObjectList(SerializedProperty listProperty, System.Collections.Generic.List<Object> values)
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

    private static void SetupInteractionHud()
    {
        Transform canvasTransform = GetOrCreateHudCanvas();
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Text crosshairText = EnsureUiText(canvasTransform, "Crosshair", defaultFont, 28, TextAnchor.MiddleCenter,
            anchorMin: new Vector2(0.5f, 0.5f), anchorMax: new Vector2(0.5f, 0.5f), pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: Vector2.zero, sizeDelta: new Vector2(40f, 40f));
        crosshairText.text = "+";
        crosshairText.color = new Color(1f, 1f, 1f, 0.85f);

        Text promptText = EnsureUiText(canvasTransform, "Prompt", defaultFont, 20, TextAnchor.LowerCenter,
            anchorMin: new Vector2(0.5f, 0f), anchorMax: new Vector2(0.5f, 0f), pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: new Vector2(0f, 80f), sizeDelta: new Vector2(700f, 40f));
        promptText.color = Color.white;

        Text progressText = EnsureUiText(canvasTransform, "Progress", defaultFont, 18, TextAnchor.LowerLeft,
            anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(0f, 0f), pivot: new Vector2(0f, 0f),
            anchoredPosition: new Vector2(24f, 24f), sizeDelta: new Vector2(900f, 30f));
        progressText.color = new Color(0.85f, 0.9f, 1f);

        Text hintText = EnsureUiText(canvasTransform, "Hint", defaultFont, 16, TextAnchor.UpperCenter,
            anchorMin: new Vector2(0.5f, 1f), anchorMax: new Vector2(0.5f, 1f), pivot: new Vector2(0.5f, 1f),
            anchoredPosition: new Vector2(0f, -20f), sizeDelta: new Vector2(700f, 26f));
        hintText.color = new Color(1f, 1f, 1f, 0.6f);

        Text bannerText = EnsureUiText(canvasTransform, "Banner", defaultFont, 26, TextAnchor.MiddleCenter,
            anchorMin: new Vector2(0.5f, 0.5f), anchorMax: new Vector2(0.5f, 0.5f), pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: new Vector2(0f, 120f), sizeDelta: new Vector2(800f, 48f));
        bannerText.color = new Color(0.7f, 0.9f, 1f);
        bannerText.enabled = false;

        GameObject pausePanel = EnsurePausePanel(canvasTransform, defaultFont);

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
        SetObjectReference(serializedHud, "artPickup", artPickup);
        SetObjectReference(serializedHud, "promptText", promptText);
        SetObjectReference(serializedHud, "crosshairText", crosshairText);
        SetObjectReference(serializedHud, "progressText", progressText);
        SetObjectReference(serializedHud, "hintText", hintText);
        SetObjectReference(serializedHud, "bannerText", bannerText);
        serializedHud.ApplyModifiedPropertiesWithoutUndo();

        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            PauseMenuController pauseController = playerObject.GetComponent<PauseMenuController>();
            if (pauseController == null)
            {
                pauseController = playerObject.AddComponent<PauseMenuController>();
            }

            FirstPersonController fps = playerObject.GetComponent<FirstPersonController>();
            SerializedObject serializedPause = new SerializedObject(pauseController);
            SetObjectReference(serializedPause, "pausePanel", pausePanel);
            SetObjectReference(serializedPause, "pauseMessageText", pausePanel.transform.Find("PauseMessage")?.GetComponent<Text>());
            SetObjectReference(serializedPause, "firstPersonController", fps);
            SetObjectReference(serializedPause, "artPickup", artPickup);
            SetObjectReference(serializedPause, "mountAimHighlighter", cameraObject.GetComponent<MountAimHighlighter>());
            serializedPause.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static Transform GetOrCreateHudCanvas()
    {
        GameObject canvasObject = GameObject.Find("InteractionHUD");
        if (canvasObject == null)
        {
            canvasObject = new GameObject("InteractionHUD");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        return canvasObject.transform;
    }

    private static Text EnsureUiText(
        Transform canvas,
        string objectName,
        Font font,
        int fontSize,
        TextAnchor alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        Transform existing = canvas.Find(objectName);
        GameObject textObject = existing != null ? existing.gameObject : CreateUiText(canvas, objectName, font, fontSize, alignment);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        return textObject.GetComponent<Text>();
    }

    private static GameObject EnsurePausePanel(Transform canvas, Font font)
    {
        Transform existing = canvas.Find("PausePanel");
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject panelObject = new GameObject("PausePanel");
        panelObject.transform.SetParent(canvas, false);
        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image backdrop = panelObject.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject messageObject = CreateUiText(panelObject.transform, "PauseMessage", font, 32, TextAnchor.MiddleCenter);
        RectTransform messageRect = messageObject.GetComponent<RectTransform>();
        messageRect.anchorMin = Vector2.zero;
        messageRect.anchorMax = Vector2.one;
        messageRect.offsetMin = Vector2.zero;
        messageRect.offsetMax = Vector2.zero;
        Text messageText = messageObject.GetComponent<Text>();
        messageText.text = "PAUSED\nPress Esc to resume";
        messageText.color = Color.white;

        panelObject.SetActive(false);
        return panelObject;
    }

    private static void SetupPlayerGameplayComponents()
    {
        GameObject cameraObject = GameObject.Find("PlayerCamera");
        if (cameraObject == null)
        {
            return;
        }

        ArtPickup artPickup = cameraObject.GetComponent<ArtPickup>();
        MountAimHighlighter highlighter = cameraObject.GetComponent<MountAimHighlighter>();
        if (highlighter == null)
        {
            highlighter = cameraObject.AddComponent<MountAimHighlighter>();
        }

        SerializedObject serializedHighlighter = new SerializedObject(highlighter);
        SetObjectReference(serializedHighlighter, "artPickup", artPickup);
        serializedHighlighter.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
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
