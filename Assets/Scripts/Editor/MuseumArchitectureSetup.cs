using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Greybox architecture pass: ceiling, pillars, welcome signage, wing banners.
/// </summary>
public static class MuseumArchitectureSetup
{
    private const string RootName = "MuseumArchitecture";

    [MenuItem("Game/Setup Museum Architecture")]
    public static void SetupFromMenu()
    {
        EnsureArchitecture();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Museum architecture pass complete (ceiling, pillars, signage).");
    }

    public static void EnsureArchitecture()
    {
        Transform environment = GameObject.Find("Environment")?.transform;
        if (environment == null)
        {
            Debug.LogWarning("MuseumArchitectureSetup: Environment not found — run Setup Zero-to-One first.");
            return;
        }

        Transform root = GetOrCreateChild(environment, RootName);

        EnsureCeiling(root);
        EnsurePillars(root);
        EnsureWelcomeSign(root);
        EnsureWingBanner(root, "Banner_Modern", "MODERN GALLERY", new Vector3(0f, 2.2f, -22f), new Vector3(8f, 0.8f, 0.15f), new Color(0.2f, 0.35f, 0.55f));
        EnsureWingBanner(root, "Banner_Classical", "CLASSICAL HALL", new Vector3(22f, 2.2f, 0f), new Vector3(0.15f, 0.8f, 8f), new Color(0.45f, 0.32f, 0.18f));
        EnsureWingBanner(root, "Banner_Impressionist", "IMPRESSIONIST WING", new Vector3(-22f, 2.2f, 0f), new Vector3(0.15f, 0.8f, 8f), new Color(0.22f, 0.42f, 0.32f));
        EnsureAtriumMarker(root);
    }

    private static void EnsureCeiling(Transform root)
    {
        if (root.Find("Ceiling") != null)
        {
            return;
        }

        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(root, false);
        ceiling.transform.position = new Vector3(0f, 3f, 0f);
        ceiling.transform.localScale = new Vector3(5f, 1f, 5f);
        ceiling.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        ApplyMaterial(ceiling, new Color(0.12f, 0.13f, 0.16f));
        Object.DestroyImmediate(ceiling.GetComponent<MeshCollider>());
    }

    private static void EnsurePillars(Transform root)
    {
        Vector3[] positions =
        {
            new Vector3(-10f, 1.5f, -10f),
            new Vector3(10f, 1.5f, -10f),
            new Vector3(-10f, 1.5f, 10f),
            new Vector3(10f, 1.5f, 10f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            string name = $"Pillar_{i + 1}";
            if (root.Find(name) != null)
            {
                continue;
            }

            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = name;
            pillar.transform.SetParent(root, false);
            pillar.transform.position = positions[i];
            pillar.transform.localScale = new Vector3(0.7f, 1.5f, 0.7f);
            ApplyMaterial(pillar, new Color(0.28f, 0.27f, 0.25f));
        }
    }

    private static void EnsureWelcomeSign(Transform root)
    {
        if (root.Find("WelcomeSign") != null)
        {
            return;
        }

        GameObject signRoot = new GameObject("WelcomeSign");
        signRoot.transform.SetParent(root, false);
        signRoot.transform.position = new Vector3(0f, 2f, 18f);
        signRoot.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);

        Canvas canvas = signRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform canvasRect = signRoot.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(420f, 120f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject title = new GameObject("Title");
        title.transform.SetParent(signRoot.transform, false);
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        Text titleText = title.AddComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 36;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.92f, 0.88f, 0.78f);
        titleText.text = "THE ARCANE MUSEUM\nSort each canvas into its wing";
    }

    private static void EnsureWingBanner(Transform root, string name, string label, Vector3 position, Vector3 scale, Color tint)
    {
        if (root.Find(name) != null)
        {
            return;
        }

        GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Cube);
        banner.name = name;
        banner.transform.SetParent(root, false);
        banner.transform.position = position;
        banner.transform.localScale = scale;
        ApplyMaterial(banner, tint);

        GameObject labelRoot = new GameObject("Label");
        labelRoot.transform.SetParent(banner.transform, false);
        labelRoot.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        labelRoot.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);

        Canvas canvas = labelRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        labelRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(280f, 48f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(labelRoot.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
    }

    private static void EnsureAtriumMarker(Transform root)
    {
        if (root.Find("AtriumFloorInlay") != null)
        {
            return;
        }

        GameObject inlay = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        inlay.name = "AtriumFloorInlay";
        inlay.transform.SetParent(root, false);
        inlay.transform.position = new Vector3(0f, 0.012f, 2f);
        inlay.transform.localScale = new Vector3(6f, 0.02f, 6f);
        ApplyMaterial(inlay, new Color(0.18f, 0.2f, 0.24f));
        Object.DestroyImmediate(inlay.GetComponent<Collider>());
    }

    private static void ApplyMaterial(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;
        renderer.sharedMaterial = material;
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
}
