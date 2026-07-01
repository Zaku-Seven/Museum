using UnityEngine;

/// <summary>
/// Marks walkable colliders for surface-aware footstep tone selection.
/// </summary>
public class FootstepSurface : MonoBehaviour
{
    public enum SurfaceKind
    {
        Stone,
        Wood,
        Carpet
    }

    [SerializeField] private SurfaceKind kind = SurfaceKind.Stone;

    public SurfaceKind Kind => kind;

#if UNITY_EDITOR
    public void Configure(SurfaceKind surfaceKind)
    {
        kind = surfaceKind;
    }
#endif

    public static MuseumProceduralSfx.SfxKind ResolveProceduralKind(SurfaceKind surface)
    {
        return surface switch
        {
            SurfaceKind.Wood => MuseumProceduralSfx.SfxKind.FootstepWood,
            SurfaceKind.Carpet => MuseumProceduralSfx.SfxKind.FootstepCarpet,
            _ => MuseumProceduralSfx.SfxKind.Footstep
        };
    }
}
