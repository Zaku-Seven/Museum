using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds accent spot/point lights above each gallery wing and the north archive.
/// </summary>
public static class MuseumLightingSetup
{
    private const string RootName = "GalleryLighting";

    public static void EnsureGalleryLighting(Transform expansionRoot)
    {
        Transform parent = expansionRoot != null ? expansionRoot : GameObject.Find("Environment")?.transform;
        if (parent == null)
        {
            return;
        }

        Transform root = parent.Find(RootName);
        if (root == null)
        {
            GameObject lightingRoot = new GameObject(RootName);
            lightingRoot.transform.SetParent(parent, false);
            root = lightingRoot.transform;
        }

        EnsureSpot(root, "Light_SouthGallery", new Vector3(0f, 2.6f, -20f), new Vector3(55f, 0f, 0f), new Color(0.95f, 0.92f, 0.85f), 1.4f);
        EnsureSpot(root, "Light_EastGallery", new Vector3(20f, 2.6f, 0f), new Vector3(55f, -90f, 0f), new Color(1f, 0.88f, 0.72f), 1.3f);
        EnsureSpot(root, "Light_WestGallery", new Vector3(-20f, 2.6f, 0f), new Vector3(55f, 90f, 0f), new Color(0.85f, 0.95f, 0.88f), 1.3f);
        EnsureSpot(root, "Light_NorthGallery", new Vector3(0f, 2.6f, 22f), new Vector3(55f, 180f, 0f), new Color(0.88f, 0.9f, 1f), 1.2f);
        EnsureSpot(root, "Light_Archive", new Vector3(0f, 2.6f, 38f), new Vector3(55f, 180f, 0f), new Color(0.82f, 0.86f, 0.98f), 1.5f);
        EnsurePoint(root, "Light_Atrium", new Vector3(0f, 2.8f, 4f), new Color(1f, 0.96f, 0.9f), 1.1f, 18f);

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                string name = $"Light_Ceiling_{x + 1}_{z + 1}";
                if (root.Find(name) != null)
                {
                    continue;
                }

                EnsurePoint(root, name, new Vector3(x * 12f, 2.7f, z * 12f), new Color(0.92f, 0.94f, 0.98f), 0.45f, 14f);
            }
        }
    }

    private static void EnsureSpot(Transform parent, string name, Vector3 position, Vector3 euler, Color color, float intensity)
    {
        Transform existing = parent.Find(name);
        GameObject lightObject = existing != null ? existing.gameObject : new GameObject(name);
        if (existing == null)
        {
            lightObject.transform.SetParent(parent, false);
        }

        lightObject.transform.position = position;
        lightObject.transform.rotation = Quaternion.Euler(euler);

        Light light = lightObject.GetComponent<Light>();
        if (light == null)
        {
            light = lightObject.AddComponent<Light>();
        }

        light.type = LightType.Spot;
        light.color = color;
        light.intensity = intensity;
        light.range = 28f;
        light.spotAngle = 65f;
        light.shadows = LightShadows.Soft;
    }

    private static void EnsurePoint(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
    {
        Transform existing = parent.Find(name);
        GameObject lightObject = existing != null ? existing.gameObject : new GameObject(name);
        if (existing == null)
        {
            lightObject.transform.SetParent(parent, false);
        }

        lightObject.transform.position = position;

        Light light = lightObject.GetComponent<Light>();
        if (light == null)
        {
            light = lightObject.AddComponent<Light>();
        }

        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
    }
}
