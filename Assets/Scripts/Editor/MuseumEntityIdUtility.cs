using UnityEditor;
using UnityEngine;

/// <summary>
/// Ensures <see cref="MuseumEntityId"/> components exist with stable ids on museum content.
/// </summary>
public static class MuseumEntityIdUtility
{
    public static void EnsureEntityId(GameObject target, string entityId)
    {
        if (target == null || string.IsNullOrEmpty(entityId))
        {
            return;
        }

        MuseumEntityId component = target.GetComponent<MuseumEntityId>();
        if (component == null)
        {
            component = target.AddComponent<MuseumEntityId>();
        }

        component.SetEntityId(entityId);
        if (Application.isPlaying)
        {
            MuseumEntityId.Registry.Register(component);
        }

        EditorUtility.SetDirty(target);
    }
}
