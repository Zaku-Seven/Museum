using UnityEditor;
using UnityEditor.Events;
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
        CreateFloorPainting("TestPainting_3", "Amber Grid", new Vector3(-3f, 0.025f, 5f), new Color(0.9f, 0.65f, 0.15f), GalleryWing.Modern, interactableLayer);
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

        MuseumEntityIdUtility.EnsureEntityId(mountRoot, $"mount_{mountName.ToLowerInvariant()}");

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
        MuseumEntityIdUtility.EnsureEntityId(painting, $"painting_{SanitizeId(objectName)}");
    }

    private static string SanitizeId(string value)
    {
        return value.Replace(" ", "_").ToLowerInvariant();
    }

    private static void ConfigurePainting(GameObject painting, string title, Color color, GalleryWing wing, int interactableLayer)
    {
        if (painting == null)
        {
            return;
        }

        painting.layer = interactableLayer;
        ConfigurePaintingComponents(painting, title, color, wing);
        MuseumEntityIdUtility.EnsureEntityId(painting, $"painting_{SanitizeId(painting.name)}");
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

        holdPoint.localPosition = new Vector3(0.42f, -0.2f, 0.58f);
        holdPoint.localRotation = Quaternion.identity;
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

        Text stackText = EnsureUiText(canvasTransform, "StackLabel", defaultFont, 17, TextAnchor.LowerCenter,
            anchorMin: new Vector2(0.5f, 0f), anchorMax: new Vector2(0.5f, 0f), pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: new Vector2(0f, 118f), sizeDelta: new Vector2(700f, 28f));
        stackText.color = new Color(0.9f, 0.95f, 1f);
        stackText.enabled = false;

        Text stagingText = EnsureUiText(canvasTransform, "StagingLabel", defaultFont, 16, TextAnchor.LowerRight,
            anchorMin: new Vector2(1f, 0f), anchorMax: new Vector2(1f, 0f), pivot: new Vector2(1f, 0f),
            anchoredPosition: new Vector2(-24f, 24f), sizeDelta: new Vector2(320f, 26f));
        stagingText.color = new Color(0.8f, 0.85f, 0.95f);
        stagingText.enabled = false;

        Text tutorialText = EnsureUiText(canvasTransform, "TutorialTip", defaultFont, 17, TextAnchor.UpperCenter,
            anchorMin: new Vector2(0.5f, 1f), anchorMax: new Vector2(0.5f, 1f), pivot: new Vector2(0.5f, 1f),
            anchoredPosition: new Vector2(0f, -52f), sizeDelta: new Vector2(760f, 48f));
        tutorialText.color = new Color(1f, 0.95f, 0.75f);
        tutorialText.enabled = false;

        GameObject winPanel = EnsureWinPanel(canvasTransform, defaultFont, out Text winText);
        GameObject pausePanel = EnsurePausePanel(canvasTransform, defaultFont, out GameObject settingsPanel);
        MuseumUiActions uiActions = EnsureUiActions(canvasTransform);

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
        SetObjectReference(serializedHud, "stackText", stackText);
        SetObjectReference(serializedHud, "stagingText", stagingText);
        SetObjectReference(serializedHud, "tutorialText", tutorialText);
        SetObjectReference(serializedHud, "winPanel", winPanel);
        SetObjectReference(serializedHud, "winText", winText);
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
            SettingsMenuController settingsController = playerObject.GetComponent<SettingsMenuController>();
            if (settingsController == null)
            {
                settingsController = playerObject.AddComponent<SettingsMenuController>();
            }

            MuseumGameFlowController flowController = playerObject.GetComponent<MuseumGameFlowController>();
            if (flowController == null)
            {
                flowController = playerObject.AddComponent<MuseumGameFlowController>();
            }

            if (playerObject.GetComponent<MuseumAudioDirector>() == null)
            {
                playerObject.AddComponent<MuseumAudioDirector>();
            }

            SerializedObject serializedFlow = new SerializedObject(flowController);
            SetObjectReference(serializedFlow, "winPanel", winPanel);
            serializedFlow.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedSettings = new SerializedObject(settingsController);
            SetObjectReference(serializedSettings, "settingsPanel", settingsPanel);
            SetObjectReference(serializedSettings, "firstPersonController", fps);
            SetObjectReference(serializedSettings, "audioDirector", playerObject.GetComponent<MuseumAudioDirector>());
            WireSettingsSliders(serializedSettings, settingsPanel, defaultFont);
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedPause = new SerializedObject(pauseController);
            SetObjectReference(serializedPause, "pausePanel", pausePanel);
            SetObjectReference(serializedPause, "pauseMessageText", pausePanel.transform.Find("PauseMessage")?.GetComponent<Text>());
            SetObjectReference(serializedPause, "firstPersonController", fps);
            SetObjectReference(serializedPause, "artPickup", artPickup);
            SetObjectReference(serializedPause, "mountAimHighlighter", cameraObject.GetComponent<MountAimHighlighter>());
            SetObjectReference(serializedPause, "settingsMenu", settingsController);
            serializedPause.ApplyModifiedPropertiesWithoutUndo();

            WirePauseButtons(pausePanel, uiActions);
            WireWinButtons(winPanel, uiActions);
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

    private static GameObject EnsurePausePanel(Transform canvas, Font font, out GameObject settingsPanel)
    {
        Transform existing = canvas.Find("PausePanel");
        GameObject panelObject = existing != null ? existing.gameObject : CreatePausePanelRoot(canvas, font);

        settingsPanel = EnsureSettingsPanel(panelObject.transform, font);
        EnsurePauseButtons(panelObject.transform, font);
        panelObject.SetActive(false);
        return panelObject;
    }

    private static GameObject CreatePausePanelRoot(Transform canvas, Font font)
    {
        GameObject panelObject = new GameObject("PausePanel");
        panelObject.transform.SetParent(canvas, false);
        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image backdrop = panelObject.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject messageObject = CreateUiText(panelObject.transform, "PauseMessage", font, 28, TextAnchor.UpperCenter);
        RectTransform messageRect = messageObject.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.5f, 0.5f);
        messageRect.anchorMax = new Vector2(0.5f, 0.5f);
        messageRect.pivot = new Vector2(0.5f, 0.5f);
        messageRect.anchoredPosition = new Vector2(0f, 80f);
        messageRect.sizeDelta = new Vector2(600f, 80f);
        Text messageText = messageObject.GetComponent<Text>();
        messageText.text = "PAUSED";
        messageText.color = Color.white;

        return panelObject;
    }

    private static void EnsurePauseButtons(Transform pausePanel, Font font)
    {
        Transform buttonRow = pausePanel.Find("PauseButtons");
        if (buttonRow == null)
        {
            GameObject row = new GameObject("PauseButtons");
            row.transform.SetParent(pausePanel, false);
            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0.5f);
            rowRect.anchorMax = new Vector2(0.5f, 0.5f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = new Vector2(0f, -20f);
            rowRect.sizeDelta = new Vector2(520f, 160f);
            buttonRow = row.transform;
        }

        CreateMenuButton(buttonRow, "ResumeButton", "Resume", font, new Vector2(0f, 50f));
        CreateMenuButton(buttonRow, "SettingsButton", "Settings", font, new Vector2(0f, 0f));
        CreateMenuButton(buttonRow, "NewGameButton", "New Game", font, new Vector2(0f, -50f));
    }

    private static GameObject EnsureSettingsPanel(Transform pausePanel, Font font)
    {
        Transform existing = pausePanel.Find("SettingsPanel");
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject panel = new GameObject("SettingsPanel");
        panel.transform.SetParent(pausePanel, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(420f, 320f);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.1f, 0.14f, 0.95f);

        CreateSettingRow(panel.transform, "MouseSensitivityRow", "Mouse sensitivity", font, new Vector2(0f, 100f), out Slider sensSlider, out Text sensValue);
        CreateSettingRow(panel.transform, "FovRow", "Field of view", font, new Vector2(0f, 40f), out Slider fovSlider, out Text fovValue);
        CreateSettingRow(panel.transform, "VolumeRow", "Master volume", font, new Vector2(0f, -20f), out Slider volSlider, out Text volValue);

        GameObject invertRow = CreateUiText(panel.transform, "InvertYLabel", font, 18, TextAnchor.MiddleLeft);
        RectTransform invertLabelRect = invertRow.GetComponent<RectTransform>();
        invertLabelRect.anchorMin = new Vector2(0f, 0.5f);
        invertLabelRect.anchorMax = new Vector2(0f, 0.5f);
        invertLabelRect.pivot = new Vector2(0f, 0.5f);
        invertLabelRect.anchoredPosition = new Vector2(24f, -70f);
        invertLabelRect.sizeDelta = new Vector2(200f, 30f);
        invertRow.GetComponent<Text>().text = "Invert Y";
        invertRow.GetComponent<Text>().color = Color.white;

        GameObject invertToggleObject = new GameObject("InvertYToggle");
        invertToggleObject.transform.SetParent(panel.transform, false);
        RectTransform toggleRect = invertToggleObject.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(1f, 0.5f);
        toggleRect.anchorMax = new Vector2(1f, 0.5f);
        toggleRect.pivot = new Vector2(1f, 0.5f);
        toggleRect.anchoredPosition = new Vector2(-24f, -70f);
        toggleRect.sizeDelta = new Vector2(40f, 40f);
        Toggle invertToggle = invertToggleObject.AddComponent<Toggle>();

        CreateMenuButton(panel.transform, "CloseSettingsButton", "Back", font, new Vector2(0f, -130f));

        sensSlider.minValue = 0.25f;
        sensSlider.maxValue = 8f;
        fovSlider.minValue = 55f;
        fovSlider.maxValue = 100f;
        volSlider.minValue = 0f;
        volSlider.maxValue = 1f;

        panel.SetActive(false);
        return panel;
    }

    private static void CreateSettingRow(Transform parent, string rowName, string label, Font font, Vector2 position, out Slider slider, out Text valueText)
    {
        GameObject row = new GameObject(rowName);
        row.transform.SetParent(parent, false);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0.5f);
        rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.anchoredPosition = position;
        rowRect.sizeDelta = new Vector2(380f, 36f);

        GameObject labelObject = CreateUiText(row.transform, "Label", font, 16, TextAnchor.MiddleLeft);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(0f, 0f);
        labelRect.sizeDelta = new Vector2(160f, 30f);
        labelObject.GetComponent<Text>().text = label;
        labelObject.GetComponent<Text>().color = Color.white;

        GameObject sliderObject = new GameObject("Slider");
        sliderObject.transform.SetParent(row.transform, false);
        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 0.5f);
        sliderRect.anchorMax = new Vector2(1f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(20f, 0f);
        sliderRect.sizeDelta = new Vector2(-120f, 20f);
        slider = sliderObject.AddComponent<Slider>();

        GameObject valueObject = CreateUiText(row.transform, "Value", font, 16, TextAnchor.MiddleRight);
        RectTransform valueRect = valueObject.GetComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(1f, 0.5f);
        valueRect.anchorMax = new Vector2(1f, 0.5f);
        valueRect.pivot = new Vector2(1f, 0.5f);
        valueRect.anchoredPosition = Vector2.zero;
        valueRect.sizeDelta = new Vector2(60f, 30f);
        valueText = valueObject.GetComponent<Text>();
        valueText.color = new Color(0.85f, 0.9f, 1f);
    }

    private static MuseumUiActions EnsureUiActions(Transform canvas)
    {
        MuseumUiActions actions = canvas.GetComponent<MuseumUiActions>();
        if (actions == null)
        {
            actions = canvas.gameObject.AddComponent<MuseumUiActions>();
        }

        return actions;
    }

    private static void WirePauseButtons(GameObject pausePanel, MuseumUiActions actions)
    {
        WireButton(pausePanel.transform, "PauseButtons/ResumeButton", actions, nameof(MuseumUiActions.ResumeFromPause));
        WireButton(pausePanel.transform, "PauseButtons/SettingsButton", actions, nameof(MuseumUiActions.OpenSettingsFromPause));
        WireButton(pausePanel.transform, "PauseButtons/NewGameButton", actions, nameof(MuseumUiActions.NewGameFromPause));
        WireButton(pausePanel.transform, "SettingsPanel/CloseSettingsButton", actions, nameof(MuseumUiActions.CloseSettings));
    }

    private static void WireWinButtons(GameObject winPanel, MuseumUiActions actions)
    {
        WireButton(winPanel.transform, "WinButtons/ContinueButton", actions, nameof(MuseumUiActions.ContinueExploringAfterWin));
        WireButton(winPanel.transform, "WinButtons/NewGameButton", actions, nameof(MuseumUiActions.StartNewGame));
    }

    private static void WireSettingsSliders(SerializedObject settings, GameObject settingsPanel, Font font)
    {
        if (settingsPanel == null)
        {
            return;
        }

        SetObjectReference(settings, "mouseSensitivitySlider", settingsPanel.transform.Find("MouseSensitivityRow/Slider")?.GetComponent<Slider>());
        SetObjectReference(settings, "fovSlider", settingsPanel.transform.Find("FovRow/Slider")?.GetComponent<Slider>());
        SetObjectReference(settings, "masterVolumeSlider", settingsPanel.transform.Find("VolumeRow/Slider")?.GetComponent<Slider>());
        SetObjectReference(settings, "invertYToggle", settingsPanel.transform.Find("InvertYToggle")?.GetComponent<Toggle>());
        SetObjectReference(settings, "sensitivityValueText", settingsPanel.transform.Find("MouseSensitivityRow/Value")?.GetComponent<Text>());
        SetObjectReference(settings, "fovValueText", settingsPanel.transform.Find("FovRow/Value")?.GetComponent<Text>());
        SetObjectReference(settings, "volumeValueText", settingsPanel.transform.Find("VolumeRow/Value")?.GetComponent<Text>());
    }

    private static void WireButton(Transform root, string path, MuseumUiActions actions, string methodName)
    {
        Transform buttonTransform = root.Find(path);
        if (buttonTransform == null || actions == null)
        {
            return;
        }

        Button button = buttonTransform.GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();

        System.Reflection.MethodInfo method = typeof(MuseumUiActions).GetMethod(methodName);
        if (method != null)
        {
            UnityEventTools.AddPersistentListener(button.onClick, (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), actions, method));
        }
    }

    private static GameObject CreateMenuButton(Transform parent, string name, string label, Font font, Vector2 anchoredPosition)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(220f, 40f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.28f, 0.38f, 0.95f);
        Button button = buttonObject.AddComponent<Button>();

        GameObject textObject = CreateUiText(buttonObject.transform, "Text", font, 18, TextAnchor.MiddleCenter);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        Text text = textObject.GetComponent<Text>();
        text.text = label;
        text.color = Color.white;

        return buttonObject;
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

        GameObject playerObject = GameObject.Find("Player");
        if (playerObject == null)
        {
            return;
        }

        if (playerObject.GetComponent<MuseumProgress>() == null)
        {
            playerObject.AddComponent<MuseumProgress>();
        }

        if (playerObject.GetComponent<MuseumSaveManager>() == null)
        {
            playerObject.AddComponent<MuseumSaveManager>();
        }

        if (playerObject.GetComponent<MuseumGameFlowController>() == null)
        {
            playerObject.AddComponent<MuseumGameFlowController>();
        }

        if (playerObject.GetComponent<MuseumAudioDirector>() == null)
        {
            playerObject.AddComponent<MuseumAudioDirector>();
        }

        if (playerObject.GetComponent<SettingsMenuController>() == null)
        {
            playerObject.AddComponent<SettingsMenuController>();
        }
    }

    private static GameObject EnsureWinPanel(Transform canvas, Font font, out Text winText)
    {
        Transform existing = canvas.Find("WinPanel");
        if (existing != null)
        {
            winText = existing.Find("WinMessage")?.GetComponent<Text>();
            EnsureWinButtons(existing, font);
            return existing.gameObject;
        }

        GameObject panelObject = new GameObject("WinPanel");
        panelObject.transform.SetParent(canvas, false);
        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image backdrop = panelObject.AddComponent<Image>();
        backdrop.color = new Color(0.05f, 0.08f, 0.12f, 0.82f);

        GameObject messageObject = CreateUiText(panelObject.transform, "WinMessage", font, 34, TextAnchor.MiddleCenter);
        RectTransform messageRect = messageObject.GetComponent<RectTransform>();
        messageRect.anchorMin = Vector2.zero;
        messageRect.anchorMax = Vector2.one;
        messageRect.offsetMin = Vector2.zero;
        messageRect.offsetMax = Vector2.zero;
        winText = messageObject.GetComponent<Text>();
        winText.text = "Museum complete!\nEvery wing is hung. Cozy work.";
        winText.color = new Color(0.85f, 0.95f, 1f);

        GameObject buttonRow = new GameObject("WinButtons");
        buttonRow.transform.SetParent(panelObject.transform, false);
        RectTransform rowRect = buttonRow.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0.5f);
        rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.anchoredPosition = new Vector2(0f, -120f);
        rowRect.sizeDelta = new Vector2(520f, 100f);
        CreateMenuButton(buttonRow.transform, "ContinueButton", "Continue exploring", font, new Vector2(0f, 20f));
        CreateMenuButton(buttonRow.transform, "NewGameButton", "New game", font, new Vector2(0f, -30f));

        panelObject.SetActive(false);
        return panelObject;
    }

    private static void EnsureWinButtons(Transform winPanel, Font font)
    {
        Transform buttonRow = winPanel.Find("WinButtons");
        if (buttonRow == null)
        {
            GameObject row = new GameObject("WinButtons");
            row.transform.SetParent(winPanel, false);
            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0.5f);
            rowRect.anchorMax = new Vector2(0.5f, 0.5f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = new Vector2(0f, -120f);
            rowRect.sizeDelta = new Vector2(520f, 100f);
            buttonRow = row.transform;
        }

        CreateMenuButton(buttonRow, "ContinueButton", "Continue exploring", font, new Vector2(0f, 20f));
        CreateMenuButton(buttonRow, "NewGameButton", "New game", font, new Vector2(0f, -30f));
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
