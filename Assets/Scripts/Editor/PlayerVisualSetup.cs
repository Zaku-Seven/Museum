using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Adds a simple capsule "bean" body under the player so you see a character when looking down.
/// </summary>
public static class PlayerVisualSetup
{
    private const string BeanName = "PlayerBean";

    [MenuItem("Game/Add Player Bean Visual")]
    public static void AddFromMenu()
    {
        if (EnsurePlayerBean())
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("Player bean visual added (or already present).");
        }
    }

    /// <summary>Idempotent — returns false if Player object is missing.</summary>
    public static bool EnsurePlayerBean()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("PlayerVisualSetup: Player not found in scene.");
            return false;
        }

        if (player.transform.Find(BeanName) != null)
        {
            return true;
        }

        GameObject bean = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bean.name = BeanName;
        bean.transform.SetParent(player.transform, false);
        bean.transform.localPosition = new Vector3(0f, -0.05f, 0.18f);
        bean.transform.localScale = new Vector3(0.52f, 0.38f, 0.48f);

        Object.DestroyImmediate(bean.GetComponent<CapsuleCollider>());

        Renderer renderer = bean.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = new Color(0.92f, 0.72f, 0.55f);
            renderer.sharedMaterial = material;
        }

        return true;
    }
}
