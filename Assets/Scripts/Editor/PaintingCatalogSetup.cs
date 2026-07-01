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

        CreateDefinition("painting_sunset_study", "Sunset Study", GalleryWing.Modern, new Color(0.85f, 0.55f, 0.2f),
            "Warm geometric study — belongs on the north Modern wall.");
        CreateDefinition("painting_amber_grid", "Amber Grid", GalleryWing.Modern, new Color(0.9f, 0.65f, 0.15f),
            "Golden lattice — north Modern demo painting.");
        CreateDefinition("painting_blue_horizon", "Blue Horizon", GalleryWing.Classical, new Color(0.2f, 0.35f, 0.75f),
            "Cool classical seascape — wrong wing for Modern mounts.");
        CreateDefinition("painting_cobalt_field", "Cobalt Field", GalleryWing.Modern, new Color(0.15f, 0.3f, 0.8f),
            "Deep blue abstract — South gallery, left placard.");
        CreateDefinition("painting_steel_lines", "Steel Lines", GalleryWing.Modern, new Color(0.4f, 0.45f, 0.55f),
            "Industrial minimalism — South gallery, center placard.");
        CreateDefinition("painting_neon_dusk", "Neon Dusk", GalleryWing.Modern, new Color(0.1f, 0.6f, 0.7f),
            "Teal city glow — South gallery, right placard.");
        CreateDefinition("painting_gilded_saints", "Gilded Saints", GalleryWing.Classical, new Color(0.75f, 0.6f, 0.2f),
            "Gold-leaf devotional panel — East gallery.");
        CreateDefinition("painting_marble_study", "Marble Study", GalleryWing.Classical, new Color(0.85f, 0.82f, 0.7f),
            "Pale stone figure study — East gallery.");
        CreateDefinition("painting_old_masters", "Old Masters", GalleryWing.Classical, new Color(0.4f, 0.28f, 0.18f),
            "Dark Renaissance composition — East gallery.");
        CreateDefinition("painting_garden_light", "Garden Light", GalleryWing.Impressionist, new Color(0.55f, 0.85f, 0.45f),
            "Sun-dappled garden — West gallery.");
        CreateDefinition("painting_rose_morning", "Rose Morning", GalleryWing.Impressionist, new Color(0.95f, 0.55f, 0.65f),
            "Soft pink blooms at dawn — West gallery.");
        CreateDefinition("painting_water_lilies", "Water Lilies", GalleryWing.Impressionist, new Color(0.35f, 0.65f, 0.85f),
            "Pond reflections in pastel blues — West gallery.");
        CreateDefinition("painting_twilight_arch", "Twilight Arch", GalleryWing.Modern, new Color(0.55f, 0.35f, 0.65f),
            "Violet structural study — stretch content for future north expansion.");
        CreateDefinition("painting_bronze_portrait", "Bronze Portrait", GalleryWing.Classical, new Color(0.62f, 0.42f, 0.28f),
            "Patina classical bust study — east alcove overflow.");
        CreateDefinition("painting_ivory_column", "Ivory Column", GalleryWing.Classical, new Color(0.88f, 0.86f, 0.78f),
            "Marble column study — south floor overflow.");
        CreateDefinition("painting_vermillion_block", "Vermillion Block", GalleryWing.Modern, new Color(0.85f, 0.25f, 0.2f),
            "Bold red modern block — south floor overflow.");
        CreateDefinition("painting_carbon_study", "Carbon Study", GalleryWing.Modern, new Color(0.12f, 0.12f, 0.14f),
            "Dark graphite modern — north archive slot.");
        CreateDefinition("painting_neon_grid", "Neon Grid", GalleryWing.Modern, new Color(0.2f, 0.75f, 0.85f),
            "Cyan lattice — north archive slot.");
        CreateDefinition("painting_misty_shore", "Misty Shore", GalleryWing.Impressionist, new Color(0.5f, 0.72f, 0.78f),
            "Coastal mist — west alcove overflow.");
        CreateDefinition("painting_lilac_field", "Lilac Field", GalleryWing.Impressionist, new Color(0.72f, 0.58f, 0.82f),
            "Purple meadow — south floor overflow.");

        CreateDefinition("painting_crimson_portrait", "Crimson Portrait", GalleryWing.Classical, new Color(0.72f, 0.18f, 0.2f),
            "Tall portrait — unsorted pile.");
        CreateDefinition("painting_harbor_panorama", "Harbor Panorama", GalleryWing.Impressionist, new Color(0.35f, 0.55f, 0.78f),
            "Wide seascape — unsorted pile.");
        CreateDefinition("painting_grand_still_life", "Grand Still Life", GalleryWing.Classical, new Color(0.55f, 0.32f, 0.22f),
            "Oversized square canvas.");

        CreateDefinition("fossil_sauropodskull", "Sauropod Skull", GalleryWing.Fossil, new Color(0.68f, 0.64f, 0.58f),
            "Hero skull for fossil dais mount.");
        CreateDefinition("fossil_megafaunafemur", "Megafauna Femur", GalleryWing.Fossil, new Color(0.62f, 0.58f, 0.52f),
            "Long bone segment.");
        CreateDefinition("fossil_ribarch", "Rib Arch", GalleryWing.Fossil, new Color(0.58f, 0.55f, 0.5f),
            "Curved rib cage section.");
        CreateDefinition("fossil_ammonitespiral", "Ammonite Spiral", GalleryWing.Fossil, new Color(0.55f, 0.52f, 0.48f),
            "Spiral shell fossil.");
        CreateDefinition("fossil_trilobiteplate", "Trilobite Plate", GalleryWing.Fossil, new Color(0.42f, 0.48f, 0.46f),
            "Small trilobite slab.");
        CreateDefinition("fossil_mammothtusk", "Mammoth Tusk", GalleryWing.Fossil, new Color(0.78f, 0.74f, 0.68f),
            "Ivory tusk specimen.");
        CreateDefinition("fossil_archaeopteryxslab", "Archaeopteryx Slab", GalleryWing.Fossil, new Color(0.36f, 0.34f, 0.32f),
            "Feathered imprint slab.");
        CreateDefinition("fossil_bronzeidol", "Bronze Idol", GalleryWing.Fossil, new Color(0.52f, 0.38f, 0.22f),
            "Antiquity statuette for fossil wing.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Painting definitions created under {CatalogFolder}. Assign them on paintings when swapping to real art.");
    }

    private static void CreateDefinition(string id, string title, GalleryWing wing, Color color, string description = "")
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
        SerializedProperty descriptionProperty = serialized.FindProperty("description");
        if (descriptionProperty != null)
        {
            descriptionProperty.stringValue = description;
        }

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
