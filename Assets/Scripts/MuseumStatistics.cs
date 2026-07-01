using UnityEngine;

/// <summary>
/// Tracks cozy stats for the win screen and debug HUD.
/// </summary>
public class MuseumStatistics : MonoBehaviour
{
    public static MuseumStatistics Instance { get; private set; }

    public int PaintingsHung { get; private set; }
    public int WrongWingAttempts { get; private set; }
    public int UndosUsed { get; private set; }
    public int PaintingsThrown { get; private set; }
    public float SessionSeconds { get; private set; }

    private void OnEnable()
    {
        Instance = this;
        MuseumGameEvents.PaintingPlaced += HandlePlaced;
        MuseumGameEvents.WrongWingRejected += HandleWrongWing;
        MuseumGameEvents.PlacementUndone += HandleUndo;
        MuseumGameEvents.PaintingThrown += HandleThrow;
    }

    private void OnDisable()
    {
        MuseumGameEvents.PaintingPlaced -= HandlePlaced;
        MuseumGameEvents.WrongWingRejected -= HandleWrongWing;
        MuseumGameEvents.PlacementUndone -= HandleUndo;
        MuseumGameEvents.PaintingThrown -= HandleThrow;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        SessionSeconds += Time.deltaTime;
    }

    public void ResetStatistics()
    {
        PaintingsHung = 0;
        WrongWingAttempts = 0;
        UndosUsed = 0;
        PaintingsThrown = 0;
        SessionSeconds = 0f;
    }

    public string BuildWinStatsLine()
    {
        int minutes = Mathf.FloorToInt(SessionSeconds / 60f);
        int seconds = Mathf.FloorToInt(SessionSeconds % 60f);
        return $"{PaintingsHung} hung · {PaintingsThrown} thrown · {UndosUsed} undos · {minutes}m {seconds:D2}s";
    }

    private void HandlePlaced(InteractablePainting painting, PaintingMount mount)
    {
        PaintingsHung++;
    }

    private void HandleWrongWing()
    {
        WrongWingAttempts++;
    }

    private void HandleUndo(InteractablePainting painting, PaintingMount mount)
    {
        UndosUsed++;
        PaintingsHung = Mathf.Max(0, PaintingsHung - 1);
    }

    private void HandleThrow(InteractablePainting painting)
    {
        PaintingsThrown++;
    }
}
