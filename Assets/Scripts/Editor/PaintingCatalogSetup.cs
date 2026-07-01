using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates <see cref="PaintingDefinition"/> assets for the default gallery catalog.
/// </summary>
public static class PaintingCatalogSetup
{
    private const string CatalogFolder = "Assets/Data/Paintings";

    [MenuItem("Game/Create Painting Definition Assets")]
    public static void CreateDefaultCatalog()
    {
        EnsureFolder(CatalogFolder);

        CreateDefinition("painting_sunset_study", "Sunset Study", GalleryWing.Modern, new Color(0.85f, 0.55f, 0.2f));
        CreateDefinition("painting_blue_horizon", "Blue Horizon", GalleryWing.Classical, new Color(0.2f, 0.35f, 0.75f));
        CreateDefinition("painting_cobalt_field", "Cobalt Field", GalleryWing.Modern, new Color(0.15f, 0.3f, 0.8f));
        CreateDefinition("painting_steel_lines", "Steel Lines", GalleryWing.Modern, new Color(0.4f, 0.45f, 0.55f));
        CreateDefinition("painting_neon_dusk", "Neon Dusk", GalleryWing.Modern, new Color(0.1f, 0.6f, 0.7f));
        CreateDefinition("painting_gilded_saints", "Gilded Saints", GalleryWing.Classical, new Color(0.75f, 0.6f, 0.2f));
        CreateDefinition("painting_marble_study", "Marble Study", GalleryWing.Classical, new Color(0.85f, 0.82f, 0.7f));
        CreateDefinition("painting_old_masters", "Old Masters", GalleryWing.Classical, new Color(0.4f, 0.28f, 0.18f));
        CreateDefinition("painting_garden_light", "Garden Light", GalleryWing.Impressionist, new Color(0.55f, 0.85f, 0.45f));
        CreateDefinition("painting_rose_morning", "Rose Morning", GalleryWing.Impressionist, new Color(0.95f, 0.55f, 0.65f));
        CreateDefinition("painting_water_lilies", "Water Lilies", GalleryWing.Impressionist, new Color(0.35f, 0.65f, 0.85f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Painting definitions created under {CatalogFolder}. Assign them on paintings when swapping to real art.");
    }

    private static void CreateDefinition(string id, string title, GalleryWing wing, Color color)
    {
        string path = $"{CatalogFolder}/{id}.asset";
        PaintingDefinition existing = AssetDatabase.LoadAssetAtPath<PaintingDefinition>(path);
        if (existing != null)
        {
            return;
        }

        PaintingDefinition definition = ScriptableObject.CreateInstance<PaintingDefinition>();
        SerializedObject serialized = new SerializedObject(definition);
        serialized.FindProperty("paintingId").stringValue = id;
        serialized.FindProperty("title").stringValue = title;
        serialized.FindProperty("wing").enumValueIndex = (int)wing;
        serialized.FindProperty("displayColor").colorValue = color;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(definition, path);
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
        string leaf = Path.GetFileName(folderPath);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent ?? "Assets", leaf);
    }
}
