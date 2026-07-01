using System;

/// <summary>
/// Global gameplay events for HUD, tutorials, SFX hooks, and progress systems.
/// Subscribe from audio/VFX components without tight coupling to <see cref="ArtPickup"/>.
/// </summary>
public static class MuseumGameEvents
{
    public static event Action<InteractablePainting> PaintingPickedUp;
    public static event Action<InteractablePainting, PaintingMount> PaintingPlaced;
    public static event Action<InteractablePainting> PaintingDropped;
    public static event Action<GallerySection> SectionCompleted;
    public static event Action MuseumCompleted;
    public static event Action<InteractablePainting, PaintingMount> PlacementUndone;
    public static event Action WrongWingRejected;

    public static void RaisePaintingPickedUp(InteractablePainting painting)
    {
        PaintingPickedUp?.Invoke(painting);
    }

    public static void RaisePaintingPlaced(InteractablePainting painting, PaintingMount mount)
    {
        PaintingPlaced?.Invoke(painting, mount);
    }

    public static void RaisePaintingDropped(InteractablePainting painting)
    {
        PaintingDropped?.Invoke(painting);
    }

    public static void RaiseSectionCompleted(GallerySection section)
    {
        SectionCompleted?.Invoke(section);
    }

    public static void RaiseMuseumCompleted()
    {
        MuseumCompleted?.Invoke();
    }

    public static void RaisePlacementUndone(InteractablePainting painting, PaintingMount mount)
    {
        PlacementUndone?.Invoke(painting, mount);
    }

    public static void RaiseWrongWingRejected()
    {
        WrongWingRejected?.Invoke();
    }
}
