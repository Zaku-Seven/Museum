using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Configures the Interactable layer, ArtPickup on PlayerCamera, HoldPoint, and TestPainting.
/// </summary>
public static class ArtPickupSceneSetup
{
    private const string SetupCompleteKey = "ArtPickupSceneSetupComplete";
    private const string InteractableLayerName = "Interactable";

    [InitializeOnLoadMethod]
    private static void AutoSetupOnCompile()
    {
        EditorApplication.delayCall += TryAutoSetup;
    }

    [MenuItem("Game/Setup Art Pickup Test")]
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

        if (GameObject.Find("PlayerCamera") == null)
        {
            return;
        }

        RunSetup();
    }

    private static void RunSetup()
    {
        EnsureInteractableLayerExists();
        SetupPlayerCamera();
        CreateTestPainting();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        EditorPrefs.SetBool(SetupCompleteKey, true);

        Debug.Log("Art pickup setup complete. Press Play, look at TestPainting, and left-click to pick it up.");
    }

    private static void EnsureInteractableLayerExists()
    {
        Object[] tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (tagManagerAssets == null || tagManagerAssets.Length == 0)
        {
            Debug.LogError("ArtPickupSceneSetup: Could not load TagManager.asset.");
            return;
        }

        SerializedObject tagManager = new SerializedObject(tagManagerAssets[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 0; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).stringValue == InteractableLayerName)
            {
                return;
            }
        }

        for (int i = 8; i < layers.arraySize; i++)
        {
            if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
            {
                layers.GetArrayElementAtIndex(i).stringValue = InteractableLayerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"ArtPickupSceneSetup: Added '{InteractableLayerName}' layer at index {i}.");
                return;
            }
        }

        Debug.LogError("ArtPickupSceneSetup: No empty layer slot available for Interactable.");
    }

    private static void SetupPlayerCamera()
    {
        GameObject cameraObject = GameObject.Find("PlayerCamera");
        if (cameraObject == null)
        {
            Debug.LogError("ArtPickupSceneSetup: PlayerCamera not found in scene.");
            return;
        }

        Transform holdPoint = cameraObject.transform.Find("HoldPoint");
        if (holdPoint == null)
        {
            GameObject holdPointObject = new GameObject("HoldPoint");
            holdPointObject.transform.SetParent(cameraObject.transform, false);
            holdPointObject.transform.localPosition = new Vector3(0f, -0.22f, 0.68f);
            holdPointObject.transform.localRotation = Quaternion.Euler(6f, 0f, 0f);
            holdPoint = holdPointObject.transform;
        }
        else
        {
            holdPoint.localPosition = new Vector3(0f, -0.22f, 0.68f);
            holdPoint.localRotation = Quaternion.Euler(6f, 0f, 0f);
        }

        ArtPickup artPickup = cameraObject.GetComponent<ArtPickup>();
        if (artPickup == null)
        {
            artPickup = cameraObject.AddComponent<ArtPickup>();
        }

        SerializedObject serializedPickup = new SerializedObject(artPickup);
        SerializedProperty holdPointProperty = serializedPickup.FindProperty("holdPoint");
        if (holdPointProperty != null)
        {
            holdPointProperty.objectReferenceValue = holdPoint;
            serializedPickup.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void CreateTestPainting()
    {
        if (GameObject.Find("TestPainting") != null)
        {
            return;
        }

        int interactableLayer = LayerMask.NameToLayer(InteractableLayerName);
        if (interactableLayer < 0)
        {
            Debug.LogError("ArtPickupSceneSetup: Interactable layer is not available.");
            return;
        }

        GameObject testPainting = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testPainting.name = "TestPainting";
        testPainting.layer = interactableLayer;
        testPainting.transform.position = new Vector3(0f, 0.025f, 4f);
        testPainting.transform.localScale = new Vector3(0.6f, 0.05f, 0.4f);

        Rigidbody rigidbody = testPainting.AddComponent<Rigidbody>();
        rigidbody.mass = 2f;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        InteractablePainting interactablePainting = testPainting.AddComponent<InteractablePainting>();
        SerializedObject serializedPainting = new SerializedObject(interactablePainting);
        SerializedProperty titleProperty = serializedPainting.FindProperty("paintingTitle");
        if (titleProperty != null)
        {
            titleProperty.stringValue = "Sunset Study";
            serializedPainting.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
