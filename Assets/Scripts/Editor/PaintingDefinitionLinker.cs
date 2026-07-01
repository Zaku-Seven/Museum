#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Links scene paintings to <see cref="PaintingDefinition"/> assets when catalog exists.
/// </summary>
public static class PaintingDefinitionLinker
{
    private const string CatalogFolder = "Assets/Data/Paintings";

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
            string assetPath = $"{CatalogFolder}/{id}.asset";
            PaintingDefinition definition = AssetDatabase.LoadAssetAtPath<PaintingDefinition>(assetPath);
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
}
#endif
