using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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

        PaintingMount mountComponent = mountRoot.GetComponent<PaintingMount>();
        MountPlacardSetup.EnsurePlacard(mountComponent);

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
        EnsureEventSystem();

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

        Text objectiveText = EnsureUiText(canvasTransform, "Objective", defaultFont, 16, TextAnchor.UpperLeft,
            anchorMin: new Vector2(0f, 1f), anchorMax: new Vector2(0f, 1f), pivot: new Vector2(0f, 1f),
            anchoredPosition: new Vector2(24f, -20f), sizeDelta: new Vector2(520f, 24f));
        objectiveText.color = new Color(0.8f, 0.88f, 0.95f);

        Text wingGuideText = EnsureUiText(canvasTransform, "WingGuide", defaultFont, 15, TextAnchor.UpperLeft,
            anchorMin: new Vector2(0f, 1f), anchorMax: new Vector2(0f, 1f), pivot: new Vector2(0f, 1f),
            anchoredPosition: new Vector2(24f, -46f), sizeDelta: new Vector2(560f, 22f));
        wingGuideText.color = new Color(0.72f, 0.82f, 0.72f);
        wingGuideText.enabled = false;

        Text inspectText = EnsureUiText(canvasTransform, "InspectPanel", defaultFont, 16, TextAnchor.MiddleCenter,
            anchorMin: new Vector2(0.5f, 0.5f), anchorMax: new Vector2(0.5f, 0.5f), pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: new Vector2(0f, -40f), sizeDelta: new Vector2(520f, 72f));
        inspectText.color = new Color(0.92f, 0.9f, 0.82f);
        inspectText.alignment = TextAnchor.MiddleCenter;
        inspectText.enabled = false;

        Image progressFillImage = EnsureProgressBar(canvasTransform, defaultFont);

        GameObject journalPanel = EnsureJournalPanel(canvasTransform, defaultFont);
        (Text compassLabel, RectTransform compassNeedle) = EnsureCompassHud(canvasTransform, defaultFont);

        GameObject winPanel = EnsureWinPanel(canvasTransform, defaultFont, out Text winText);
        GameObject pausePanel = EnsurePausePanel(canvasTransform, defaultFont, out GameObject settingsPanel);
        UpgradeSettingsPanel(settingsPanel, defaultFont);
        GameObject mainMenuPanel = EnsureMainMenuPanel(canvasTransform, defaultFont, out GameObject newGameConfirmPanel, out Button continueButton, out Text mainMenuSubtitle);
        GameObject controlsHelpPanel = EnsureControlsHelpPanel(canvasTransform, defaultFont);
        MuseumUiActions uiActions = EnsureUiActions(canvasTransform);
        EnsureControlsHelpController(canvasTransform, controlsHelpPanel);
        EnsureJournalController(canvasTransform, journalPanel);

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
        SetObjectReference(serializedHud, "objectiveText", objectiveText);
        SetObjectReference(serializedHud, "wingGuideText", wingGuideText);
        SetObjectReference(serializedHud, "inspectText", inspectText);
        SetObjectReference(serializedHud, "progressFillImage", progressFillImage);
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
            WireJournalPanel(journalPanel, uiActions);
            WireCompassHud(playerObject, cameraObject, artPickup, compassLabel, compassNeedle);

            WireButton(settingsPanel.transform, "ResetDefaultsButton", uiActions, nameof(MuseumUiActions.ResetSettingsToDefaults));

            MainMenuController mainMenu = playerObject.GetComponent<MainMenuController>();
            if (mainMenu == null)
            {
                mainMenu = playerObject.AddComponent<MainMenuController>();
            }

            SerializedObject serializedMainMenu = new SerializedObject(mainMenu);
            SetObjectReference(serializedMainMenu, "mainMenuPanel", mainMenuPanel);
            SetObjectReference(serializedMainMenu, "newGameConfirmPanel", newGameConfirmPanel);
            SetObjectReference(serializedMainMenu, "continueButton", continueButton);
            SetObjectReference(serializedMainMenu, "subtitleText", mainMenuSubtitle);
            serializedMainMenu.ApplyModifiedPropertiesWithoutUndo();

            WireMainMenuButtons(mainMenuPanel, newGameConfirmPanel, uiActions);
            WireControlsHelpButton(controlsHelpPanel, uiActions);
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

        CreateMenuButton(buttonRow, "ResumeButton", "Resume", font, new Vector2(0f, 80f));
        CreateMenuButton(buttonRow, "SettingsButton", "Settings", font, new Vector2(0f, 30f));
        CreateMenuButton(buttonRow, "ControlsButton", "Controls", font, new Vector2(0f, -20f));
        CreateMenuButton(buttonRow, "CollectionButton", "Collection", font, new Vector2(0f, -70f));
        CreateMenuButton(buttonRow, "NewGameButton", "New Game", font, new Vector2(0f, -120f));

        Transform buttonRowTransform = pausePanel.Find("PauseButtons");
        if (buttonRowTransform != null)
        {
            RectTransform buttonRowRect = buttonRowTransform.GetComponent<RectTransform>();
            if (buttonRowRect != null)
            {
                buttonRowRect.sizeDelta = new Vector2(520f, 220f);
            }
        }
    }

    private static void UpgradeSettingsPanel(GameObject settingsPanel, Font font)
    {
        if (settingsPanel == null)
        {
            return;
        }

        RectTransform rect = settingsPanel.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(420f, 420f);
        }

        if (settingsPanel.transform.Find("GamepadLookRow") == null)
        {
            CreateSettingRow(settingsPanel.transform, "GamepadLookRow", "Gamepad look", font, new Vector2(0f, 130f), out Slider gpSlider, out Text _);
            gpSlider.minValue = 0.5f;
            gpSlider.maxValue = 8f;
        }

        if (settingsPanel.transform.Find("ResetDefaultsButton") == null)
        {
            CreateMenuButton(settingsPanel.transform, "ResetDefaultsButton", "Reset defaults", font, new Vector2(0f, -190f));
        }

        EnsureSynthesizedSfxToggle(settingsPanel.transform, font);

        Transform resetButton = settingsPanel.transform.Find("ResetDefaultsButton");
        if (resetButton != null)
        {
            resetButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -190f);
        }

        Transform closeButton = settingsPanel.transform.Find("CloseSettingsButton");
        if (closeButton != null)
        {
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -240f);
        }

        Transform invertLabel = settingsPanel.transform.Find("InvertYLabel");
        if (invertLabel != null)
        {
            invertLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(24f, -100f);
        }

        Transform invertToggle = settingsPanel.transform.Find("InvertYToggle");
        if (invertToggle != null)
        {
            invertToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24f, -100f);
        }
    }

    private static void EnsureSynthesizedSfxToggle(Transform settingsPanel, Font font)
    {
        if (settingsPanel.Find("SynthesizedSfxToggle") != null)
        {
            return;
        }

        GameObject labelRow = CreateUiText(settingsPanel, "SynthesizedSfxLabel", font, 18, TextAnchor.MiddleLeft);
        RectTransform labelRect = labelRow.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(24f, -130f);
        labelRect.sizeDelta = new Vector2(260f, 30f);
        Text label = labelRow.GetComponent<Text>();
        label.text = "Placeholder SFX";
        label.color = Color.white;

        GameObject toggleObject = new GameObject("SynthesizedSfxToggle");
        toggleObject.transform.SetParent(settingsPanel, false);
        RectTransform toggleRect = toggleObject.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(1f, 0.5f);
        toggleRect.anchorMax = new Vector2(1f, 0.5f);
        toggleRect.pivot = new Vector2(1f, 0.5f);
        toggleRect.anchoredPosition = new Vector2(-24f, -130f);
        toggleRect.sizeDelta = new Vector2(40f, 40f);
        toggleObject.AddComponent<Toggle>();
    }

    private static GameObject EnsureControlsHelpPanel(Transform canvas, Font font)
    {
        Transform existing = canvas.Find("ControlsHelpPanel");
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject panel = new GameObject("ControlsHelpPanel");
        panel.transform.SetParent(canvas, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.03f, 0.05f, 0.08f, 0.92f);

        GameObject bodyObject = CreateUiText(panel.transform, "Body", font, 18, TextAnchor.UpperLeft);
        RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRect.pivot = new Vector2(0.5f, 0.5f);
        bodyRect.anchoredPosition = new Vector2(0f, 20f);
        bodyRect.sizeDelta = new Vector2(520f, 360f);
        Text body = bodyObject.GetComponent<Text>();
        body.color = new Color(0.9f, 0.93f, 0.98f);
        body.horizontalOverflow = HorizontalWrapMode.Wrap;
        body.verticalOverflow = VerticalWrapMode.Overflow;

        CreateMenuButton(panel.transform, "CloseControlsButton", "Back", font, new Vector2(0f, -200f));
        panel.SetActive(false);
        return panel;
    }

    private static void EnsureControlsHelpController(Transform canvas, GameObject panel)
    {
        ControlsHelpController controller = canvas.GetComponent<ControlsHelpController>();
        if (controller == null)
        {
            controller = canvas.gameObject.AddComponent<ControlsHelpController>();
        }

        SerializedObject serialized = new SerializedObject(controller);
        SetObjectReference(serialized, "panel", panel);
        SetObjectReference(serialized, "bodyText", panel.transform.Find("Body")?.GetComponent<Text>());
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireControlsHelpButton(GameObject panel, MuseumUiActions actions)
    {
        WireButton(panel.transform, "CloseControlsButton", actions, nameof(MuseumUiActions.CloseControlsHelp));
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

    private static void WireMainMenuButtons(GameObject mainMenuPanel, GameObject confirmPanel, MuseumUiActions actions)
    {
        WireButton(mainMenuPanel.transform, "MainMenuButtons/ContinueButton", actions, nameof(MuseumUiActions.MainMenuContinue));
        WireButton(mainMenuPanel.transform, "MainMenuButtons/StartButton", actions, nameof(MuseumUiActions.MainMenuStartFresh));
        WireButton(mainMenuPanel.transform, "MainMenuButtons/NewGameButton", actions, nameof(MuseumUiActions.MainMenuShowNewGameConfirm));
        WireButton(mainMenuPanel.transform, "MainMenuButtons/SettingsButton", actions, nameof(MuseumUiActions.MainMenuOpenSettings));
        WireButton(mainMenuPanel.transform, "MainMenuButtons/ControlsButton", actions, nameof(MuseumUiActions.OpenControlsHelp));

        if (confirmPanel != null)
        {
            WireButton(confirmPanel.transform, "ConfirmButtons/YesButton", actions, nameof(MuseumUiActions.MainMenuConfirmNewGame));
            WireButton(confirmPanel.transform, "ConfirmButtons/NoButton", actions, nameof(MuseumUiActions.MainMenuCancelNewGameConfirm));
        }
    }

    private static GameObject EnsureMainMenuPanel(Transform canvas, Font font, out GameObject confirmPanel, out Button continueButton, out Text subtitle)
    {
        Transform existing = canvas.Find("MainMenuPanel");
        GameObject panelObject;
        if (existing != null)
        {
            panelObject = existing.gameObject;
            subtitle = panelObject.transform.Find("Subtitle")?.GetComponent<Text>();
            continueButton = panelObject.transform.Find("MainMenuButtons/ContinueButton")?.GetComponent<Button>();
            confirmPanel = EnsureNewGameConfirmPanel(panelObject.transform, font);
            return panelObject;
        }

        panelObject = new GameObject("MainMenuPanel");
        panelObject.transform.SetParent(canvas, false);
        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image backdrop = panelObject.AddComponent<Image>();
        backdrop.color = new Color(0.04f, 0.06f, 0.1f, 0.92f);

        GameObject titleObject = CreateUiText(panelObject.transform, "Title", font, 42, TextAnchor.UpperCenter);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -80f);
        titleRect.sizeDelta = new Vector2(700f, 60f);
        titleObject.GetComponent<Text>().text = "Arcane Museum";
        titleObject.GetComponent<Text>().color = new Color(0.9f, 0.95f, 1f);

        GameObject subtitleObject = CreateUiText(panelObject.transform, "Subtitle", font, 20, TextAnchor.UpperCenter);
        RectTransform subtitleRect = subtitleObject.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 1f);
        subtitleRect.anchorMax = new Vector2(0.5f, 1f);
        subtitleRect.pivot = new Vector2(0.5f, 1f);
        subtitleRect.anchoredPosition = new Vector2(0f, -140f);
        subtitleRect.sizeDelta = new Vector2(700f, 40f);
        subtitle = subtitleObject.GetComponent<Text>();
        subtitle.color = new Color(0.75f, 0.82f, 0.95f);

        GameObject buttonRow = new GameObject("MainMenuButtons");
        buttonRow.transform.SetParent(panelObject.transform, false);
        RectTransform rowRect = buttonRow.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0.5f);
        rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.anchoredPosition = new Vector2(0f, -20f);
        rowRect.sizeDelta = new Vector2(520f, 280f);

        CreateMenuButton(buttonRow.transform, "ContinueButton", "Continue", font, new Vector2(0f, 100f));
        CreateMenuButton(buttonRow.transform, "StartButton", "Enter museum", font, new Vector2(0f, 15f));
        CreateMenuButton(buttonRow.transform, "NewGameButton", "New game", font, new Vector2(0f, -40f));
        CreateMenuButton(buttonRow.transform, "SettingsButton", "Settings", font, new Vector2(0f, -95f));
        CreateMenuButton(buttonRow.transform, "ControlsButton", "Controls", font, new Vector2(0f, -150f));

        subtitle = panelObject.transform.Find("Subtitle")?.GetComponent<Text>();
        continueButton = panelObject.transform.Find("MainMenuButtons/ContinueButton")?.GetComponent<Button>();

        confirmPanel = EnsureNewGameConfirmPanel(panelObject.transform, font);
        panelObject.SetActive(false);
        return panelObject;
    }

    private static GameObject EnsureNewGameConfirmPanel(Transform mainMenuPanel, Font font)
    {
        Transform existing = mainMenuPanel.Find("NewGameConfirmPanel");
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject panel = new GameObject("NewGameConfirmPanel");
        panel.transform.SetParent(mainMenuPanel, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);

        GameObject message = CreateUiText(panel.transform, "Message", font, 24, TextAnchor.MiddleCenter);
        RectTransform messageRect = message.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.5f, 0.5f);
        messageRect.anchorMax = new Vector2(0.5f, 0.5f);
        messageRect.pivot = new Vector2(0.5f, 0.5f);
        messageRect.anchoredPosition = new Vector2(0f, 40f);
        messageRect.sizeDelta = new Vector2(520f, 80f);
        message.GetComponent<Text>().text = "Start a new museum?\nCurrent progress will be cleared.";
        message.GetComponent<Text>().color = Color.white;

        GameObject row = new GameObject("ConfirmButtons");
        row.transform.SetParent(panel.transform, false);
        RectTransform confirmRowRect = row.AddComponent<RectTransform>();
        confirmRowRect.anchorMin = new Vector2(0.5f, 0.5f);
        confirmRowRect.anchorMax = new Vector2(0.5f, 0.5f);
        confirmRowRect.pivot = new Vector2(0.5f, 0.5f);
        confirmRowRect.anchoredPosition = new Vector2(0f, -40f);
        confirmRowRect.sizeDelta = new Vector2(400f, 50f);

        CreateMenuButton(row.transform, "YesButton", "Yes, reset", font, new Vector2(-110f, 0f));
        CreateMenuButton(row.transform, "NoButton", "Cancel", font, new Vector2(110f, 0f));

        panel.SetActive(false);
        return panel;
    }

    private static void WirePauseButtons(GameObject pausePanel, MuseumUiActions actions)
    {
        WireButton(pausePanel.transform, "PauseButtons/ResumeButton", actions, nameof(MuseumUiActions.ResumeFromPause));
        WireButton(pausePanel.transform, "PauseButtons/SettingsButton", actions, nameof(MuseumUiActions.OpenSettingsFromPause));
        WireButton(pausePanel.transform, "PauseButtons/ControlsButton", actions, nameof(MuseumUiActions.OpenControlsHelp));
        WireButton(pausePanel.transform, "PauseButtons/CollectionButton", actions, nameof(MuseumUiActions.OpenCollectionJournal));
        WireButton(pausePanel.transform, "PauseButtons/NewGameButton", actions, nameof(MuseumUiActions.NewGameFromPause));
        WireButton(pausePanel.transform, "SettingsPanel/ResetDefaultsButton", actions, nameof(MuseumUiActions.ResetSettingsToDefaults));
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
        SetObjectReference(settings, "gamepadLookSlider", settingsPanel.transform.Find("GamepadLookRow/Slider")?.GetComponent<Slider>());
        SetObjectReference(settings, "fovSlider", settingsPanel.transform.Find("FovRow/Slider")?.GetComponent<Slider>());
        SetObjectReference(settings, "masterVolumeSlider", settingsPanel.transform.Find("VolumeRow/Slider")?.GetComponent<Slider>());
        SetObjectReference(settings, "invertYToggle", settingsPanel.transform.Find("InvertYToggle")?.GetComponent<Toggle>());
        SetObjectReference(settings, "synthesizedSfxToggle", settingsPanel.transform.Find("SynthesizedSfxToggle")?.GetComponent<Toggle>());
        SetObjectReference(settings, "sensitivityValueText", settingsPanel.transform.Find("MouseSensitivityRow/Value")?.GetComponent<Text>());
        SetObjectReference(settings, "gamepadLookValueText", settingsPanel.transform.Find("GamepadLookRow/Value")?.GetComponent<Text>());
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

    private static Image EnsureProgressBar(Transform canvas, Font font)
    {
        Transform existingFill = canvas.Find("ProgressBar/Fill");
        if (existingFill != null)
        {
            return existingFill.GetComponent<Image>();
        }

        GameObject barRoot = new GameObject("ProgressBar");
        barRoot.transform.SetParent(canvas, false);
        RectTransform barRect = barRoot.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(0f, 1f);
        barRect.pivot = new Vector2(0f, 1f);
        barRect.anchoredPosition = new Vector2(24f, -72f);
        barRect.sizeDelta = new Vector2(220f, 10f);

        Image bg = barRoot.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.18f, 0.22f, 0.85f);

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(barRoot.transform, false);
        RectTransform fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fill = fillObject.AddComponent<Image>();
        fill.color = new Color(0.45f, 0.75f, 0.95f, 0.95f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 0f;
        return fill;
    }

    private static GameObject EnsureJournalPanel(Transform canvas, Font font)
    {
        Transform existing = canvas.Find("JournalPanel");
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject panel = new GameObject("JournalPanel");
        panel.transform.SetParent(canvas, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image backdrop = panel.AddComponent<Image>();
        backdrop.color = new Color(0.04f, 0.07f, 0.1f, 0.88f);

        GameObject title = CreateUiText(panel.transform, "Title", font, 28, TextAnchor.UpperCenter);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -36f);
        titleRect.sizeDelta = new Vector2(600f, 40f);
        title.GetComponent<Text>().text = "Curator Collection";
        title.GetComponent<Text>().color = new Color(0.85f, 0.92f, 1f);

        GameObject body = CreateUiText(panel.transform, "Body", font, 17, TextAnchor.UpperLeft);
        RectTransform bodyRect = body.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRect.pivot = new Vector2(0.5f, 0.5f);
        bodyRect.anchoredPosition = new Vector2(0f, -20f);
        bodyRect.sizeDelta = new Vector2(560f, 420f);
        Text bodyText = body.GetComponent<Text>();
        bodyText.color = new Color(0.82f, 0.88f, 0.95f);
        bodyText.lineSpacing = 1.1f;

        CreateMenuButton(panel.transform, "CloseJournalButton", "Close (J)", font, new Vector2(0f, -260f));
        panel.SetActive(false);
        return panel;
    }

    private static (Text label, RectTransform needle) EnsureCompassHud(Transform canvas, Font font)
    {
        Transform existing = canvas.Find("CompassHud");
        if (existing != null)
        {
            return (
                existing.Find("CompassLabel")?.GetComponent<Text>(),
                existing.Find("CompassNeedle")?.GetComponent<RectTransform>());
        }

        GameObject root = new GameObject("CompassHud");
        root.transform.SetParent(canvas, false);
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = new Vector2(0f, 120f);
        rootRect.sizeDelta = new Vector2(200f, 80f);

        GameObject needleObject = CreateUiText(root.transform, "CompassNeedle", font, 36, TextAnchor.MiddleCenter);
        RectTransform needleRect = needleObject.GetComponent<RectTransform>();
        needleRect.anchorMin = new Vector2(0.5f, 0.5f);
        needleRect.anchorMax = new Vector2(0.5f, 0.5f);
        needleRect.pivot = new Vector2(0.5f, 0.5f);
        needleRect.anchoredPosition = new Vector2(0f, 12f);
        needleRect.sizeDelta = new Vector2(40f, 40f);
        Text needleText = needleObject.GetComponent<Text>();
        needleText.text = "▲";
        needleText.color = new Color(0.55f, 0.95f, 0.65f);

        GameObject labelObject = CreateUiText(root.transform, "CompassLabel", font, 15, TextAnchor.MiddleCenter);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(0f, -22f);
        labelRect.sizeDelta = new Vector2(320f, 24f);
        Text label = labelObject.GetComponent<Text>();
        label.color = new Color(0.75f, 0.95f, 0.8f);

        root.SetActive(true);
        return (label, needleRect);
    }

    private static void EnsureJournalController(Transform canvas, GameObject journalPanel)
    {
        MuseumJournalController journal = canvas.GetComponent<MuseumJournalController>();
        if (journal == null)
        {
            journal = canvas.gameObject.AddComponent<MuseumJournalController>();
        }

        SerializedObject serialized = new SerializedObject(journal);
        SetObjectReference(serialized, "panel", journalPanel);
        SetObjectReference(serialized, "bodyText", journalPanel.transform.Find("Body")?.GetComponent<Text>());
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireJournalPanel(GameObject journalPanel, MuseumUiActions actions)
    {
        WireButton(journalPanel.transform, "CloseJournalButton", actions, nameof(MuseumUiActions.CloseJournal));
    }

    private static void WireCompassHud(
        GameObject playerObject,
        GameObject cameraObject,
        ArtPickup artPickup,
        Text compassLabel,
        RectTransform compassNeedle)
    {
        if (playerObject == null)
        {
            return;
        }

        WingCompassHud compass = playerObject.GetComponent<WingCompassHud>();
        if (compass == null)
        {
            compass = playerObject.AddComponent<WingCompassHud>();
        }

        SerializedObject serialized = new SerializedObject(compass);
        SetObjectReference(serialized, "artPickup", artPickup);
        SetObjectReference(serialized, "playerBody", playerObject.transform);
        SetObjectReference(serialized, "compassLabel", compassLabel);
        SetObjectReference(serialized, "needleRect", compassNeedle);
        serialized.ApplyModifiedPropertiesWithoutUndo();
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

        if (playerObject.GetComponent<MuseumStatistics>() == null)
        {
            playerObject.AddComponent<MuseumStatistics>();
        }

        if (playerObject.GetComponent<MuseumDevCheats>() == null)
        {
            playerObject.AddComponent<MuseumDevCheats>();
        }

        if (playerObject.GetComponent<FootstepController>() == null)
        {
            playerObject.AddComponent<FootstepController>();
        }

        if (cameraObject.GetComponent<ThrowCameraKick>() == null)
        {
            cameraObject.AddComponent<ThrowCameraKick>();
        }

        if (cameraObject.GetComponent<MountPlacementGhost>() == null)
        {
            cameraObject.AddComponent<MountPlacementGhost>();
        }

        if (playerObject.GetComponent<JumpLandAudio>() == null)
        {
            playerObject.AddComponent<JumpLandAudio>();
        }

        if (playerObject.GetComponent<WingCompassHud>() == null)
        {
            playerObject.AddComponent<WingCompassHud>();
        }

        if (playerObject.GetComponent<MuseumStartupValidator>() == null)
        {
            playerObject.AddComponent<MuseumStartupValidator>();
        }
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
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
