using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stable string id for save/load and reset. Registered in <see cref="Registry"/> on enable.
/// </summary>
[DisallowMultipleComponent]
public class MuseumEntityId : MonoBehaviour
{
    [SerializeField] private string entityId;

    public string EntityId => entityId;

    public static class Registry
    {
        private static readonly Dictionary<string, MuseumEntityId> ById = new Dictionary<string, MuseumEntityId>();

        public static void Register(MuseumEntityId entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.entityId))
            {
                return;
            }

            ById[entity.entityId] = entity;
        }

        public static void Unregister(MuseumEntityId entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.entityId))
            {
                return;
            }

            if (ById.TryGetValue(entity.entityId, out MuseumEntityId existing) && existing == entity)
            {
                ById.Remove(entity.entityId);
            }
        }

        public static MuseumEntityId Find(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            ById.TryGetValue(id, out MuseumEntityId entity);
            return entity;
        }

        public static void ClearForTests()
        {
            ById.Clear();
        }
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(entityId))
        {
            entityId = gameObject.name;
        }
    }

    private void OnEnable()
    {
        Registry.Register(this);
    }

    private void OnDisable()
    {
        Registry.Unregister(this);
    }

#if UNITY_EDITOR
    public void SetEntityId(string id)
    {
        entityId = id;
    }
#endif
}
