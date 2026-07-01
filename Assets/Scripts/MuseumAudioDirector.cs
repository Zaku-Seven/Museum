using UnityEngine;

/// <summary>
/// Subscribes to <see cref="MuseumGameEvents"/> and plays optional one-shot clips (null-safe).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MuseumAudioDirector : MonoBehaviour
{
    [Header("Clips (optional)")]
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip placeClip;
    [SerializeField] private AudioClip dropClip;
    [SerializeField] private AudioClip wrongWingClip;
    [SerializeField] private AudioClip sectionCompleteClip;
    [SerializeField] private AudioClip museumWinClip;
    [SerializeField] private AudioClip undoClip;
    [SerializeField] private AudioClip throwClip;

    [Header("Mix")]
    [SerializeField] private float sfxVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        ApplyVolume();
    }

    private void OnEnable()
    {
        MuseumGameEvents.PaintingPickedUp += HandlePickup;
        MuseumGameEvents.PaintingPlaced += HandlePlace;
        MuseumGameEvents.PaintingDropped += HandleDrop;
        MuseumGameEvents.SectionCompleted += HandleSectionComplete;
        MuseumGameEvents.MuseumCompleted += HandleMuseumWin;
        MuseumGameEvents.PlacementUndone += HandleUndo;
        MuseumGameEvents.WrongWingRejected += HandleWrongWing;
        MuseumGameEvents.PaintingThrown += HandleThrow;
    }

    private void OnDisable()
    {
        MuseumGameEvents.PaintingPickedUp -= HandlePickup;
        MuseumGameEvents.PaintingPlaced -= HandlePlace;
        MuseumGameEvents.PaintingDropped -= HandleDrop;
        MuseumGameEvents.SectionCompleted -= HandleSectionComplete;
        MuseumGameEvents.MuseumCompleted -= HandleMuseumWin;
        MuseumGameEvents.PlacementUndone -= HandleUndo;
        MuseumGameEvents.WrongWingRejected -= HandleWrongWing;
        MuseumGameEvents.PaintingThrown -= HandleThrow;
    }

    public void ApplyVolume()
    {
        float master = PlayerSettingsStore.MasterVolume;
        audioSource.volume = Mathf.Clamp01(sfxVolume * master);
    }

    private void HandlePickup(InteractablePainting painting)
    {
        PlayOneShot(pickupClip);
    }

    private void HandlePlace(InteractablePainting painting, PaintingMount mount)
    {
        PlayOneShot(placeClip);
    }

    private void HandleDrop(InteractablePainting painting)
    {
        PlayOneShot(dropClip);
    }

    private void HandleSectionComplete(GallerySection section)
    {
        PlayOneShot(sectionCompleteClip);
    }

    private void HandleMuseumWin()
    {
        PlayOneShot(museumWinClip);
    }

    private void HandleUndo(InteractablePainting painting, PaintingMount mount)
    {
        PlayOneShot(undoClip);
    }

    private void HandleWrongWing()
    {
        PlayOneShot(wrongWingClip);
    }

    private void HandleThrow(InteractablePainting painting)
    {
        PlayOneShot(throwClip);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        ApplyVolume();
        audioSource.PlayOneShot(clip);
    }
}
