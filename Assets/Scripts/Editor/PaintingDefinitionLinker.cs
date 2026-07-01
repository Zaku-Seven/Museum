#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Links scene paintings to <see cref="PaintingDefinition"/> assets when catalog exists.
/// </summary>
public static class PaintingDefinitionLinker
{
    private const string CatalogFolder = "Assets/Data/Paintings";

    private static readonly Dictionary<string, string> EntityIdToCatalogId = new Dictionary<string, string>
    {
        { "painting_testpainting", "painting_sunset_study" },
        { "painting_testpainting_2", "painting_blue_horizon" },
        { "painting_testpainting_3", "painting_amber_grid" },
        { "painting_cobalt_field", "painting_cobalt_field" },
        { "painting_steel_lines", "painting_steel_lines" },
        { "painting_neon_dusk", "painting_neon_dusk" },
        { "painting_gilded_saints", "painting_gilded_saints" },
        { "painting_marble_study", "painting_marble_study" },
        { "painting_old_masters", "painting_old_masters" },
        { "painting_garden_light", "painting_garden_light" },
        { "painting_rose_morning", "painting_rose_morning" },
        { "painting_water_lilies", "painting_water_lilies" },
        { "painting_twilight_arch", "painting_twilight_arch" },
        { "painting_bronze_portrait", "painting_bronze_portrait" },
        { "painting_ivory_column", "painting_ivory_column" },
        { "painting_vermillion_block", "painting_vermillion_block" },
        { "painting_carbon_study", "painting_carbon_study" },
        { "painting_neon_grid", "painting_neon_grid" },
        { "painting_misty_shore", "painting_misty_shore" },
        { "painting_lilac_field", "painting_lilac_field" }
    };

    [MenuItem("Game/Link Painting Definitions To Scene")]
    public static void LinkScenePaintings()
    {
        InteractablePainting[] paintings = Object.FindObjectsByType<InteractablePainting>(FindObjectsSortMode.None);
        int linked = 0;

        for (int i = 0; i < paintings.Length; i++)
        {
            InteractablePainting painting = paintings[i];
            if (painting == null)
            {
                continue;
            }

            MuseumEntityId entityId = painting.GetComponent<MuseumEntityId>();
            string id = entityId != null ? entityId.EntityId : painting.gameObject.name;
            PaintingDefinition definition = LoadDefinitionForEntity(id);
            if (definition == null)
            {
                definition = FindDefinitionByTitle(painting.PaintingTitle);
            }

            if (definition == null)
            {
                continue;
            }

            painting.SetDefinition(definition);
            EditorUtility.SetDirty(painting);
            linked++;
        }

        Debug.Log($"PaintingDefinitionLinker: linked {linked} painting(s). Run 'Create Painting Definition Assets' first if zero.");
    }

    private static PaintingDefinition LoadDefinitionForEntity(string entityId)
    {
        if (EntityIdToCatalogId.TryGetValue(entityId, out string catalogId))
        {
            PaintingDefinition aliased = AssetDatabase.LoadAssetAtPath<PaintingDefinition>($"{CatalogFolder}/{catalogId}.asset");
            if (aliased != null)
            {
                return aliased;
            }
        }

        return AssetDatabase.LoadAssetAtPath<PaintingDefinition>($"{CatalogFolder}/{entityId}.asset");
    }

    private static PaintingDefinition FindDefinitionByTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return null;
        }

        string[] guids = AssetDatabase.FindAssets("t:PaintingDefinition", new[] { CatalogFolder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            PaintingDefinition definition = AssetDatabase.LoadAssetAtPath<PaintingDefinition>(path);
            if (definition != null && definition.Title == title)
            {
                return definition;
            }
        }

        return null;
    }
}
#endif
