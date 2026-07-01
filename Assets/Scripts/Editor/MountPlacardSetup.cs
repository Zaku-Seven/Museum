#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adds or upgrades world-space wing placards on every mount in the scene.
/// </summary>
public static class MountPlacardSetup
{
    [MenuItem("Game/Add Mount Wing Placards")]
    public static void AddPlacardsFromMenu()
    {
        PaintingMount[] mounts = Object.FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        int created = 0;

        for (int i = 0; i < mounts.Length; i++)
        {
            if (EnsurePlacard(mounts[i]))
            {
                created++;
            }
        }

        Debug.Log($"MountPlacardSetup: ensured placards on {created} mount(s).");
    }

    public static bool EnsurePlacard(PaintingMount mount)
    {
        if (mount == null)
        {
            return false;
        }

        MountWingPlacard placard = mount.GetComponentInChildren<MountWingPlacard>(true);
        if (placard == null)
        {
            placard = CreatePlacardObject(mount.transform).GetComponent<MountWingPlacard>();
        }

        placard.Configure(mount.RequiredWing);
        EditorUtility.SetDirty(mount.gameObject);
        return true;
    }

    private static GameObject CreatePlacardObject(Transform mountRoot)
    {
        GameObject placardRoot = new GameObject("WingPlacard");
        placardRoot.transform.SetParent(mountRoot, false);
        placardRoot.transform.localPosition = new Vector3(0f, 0.55f, -0.06f);
        placardRoot.transform.localRotation = Quaternion.identity;
        placardRoot.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        Canvas canvas = placardRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = placardRoot.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(180f, 40f);

        GameObject textObject = new GameObject("Label");
        textObject.transform.SetParent(placardRoot.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.95f, 0.92f, 0.85f);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        MountWingPlacard placard = placardRoot.AddComponent<MountWingPlacard>();
        SerializedObject serialized = new SerializedObject(placard);
        SerializedProperty labelProperty = serialized.FindProperty("labelText");
        if (labelProperty != null)
        {
            labelProperty.objectReferenceValue = text;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        return placardRoot;
    }
}
#endif
