using UnityEngine;

/// <summary>
/// Subscribes to <see cref="MuseumGameEvents"/> and plays clips or synthesized fallbacks.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MuseumAudioDirector : MonoBehaviour
{
    [Header("Clips (optional — leave empty to use synthesized placeholders)")]
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip placeClip;
    [SerializeField] private AudioClip dropClip;
    [SerializeField] private AudioClip wrongWingClip;
    [SerializeField] private AudioClip wrongSlotClip;
    [SerializeField] private AudioClip sectionCompleteClip;
    [SerializeField] private AudioClip museumWinClip;
    [SerializeField] private AudioClip undoClip;
    [SerializeField] private AudioClip throwClip;
    [SerializeField] private AudioClip stageClip;
    [SerializeField] private AudioClip uiClickClip;

    [Header("Mix")]
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private bool useSynthesizedFallback = true;

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
        MuseumGameEvents.WrongSlotRejected += HandleWrongSlot;
        MuseumGameEvents.PaintingThrown += HandleThrow;
        MuseumGameEvents.PaintingStaged += HandleStaged;
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
        MuseumGameEvents.WrongSlotRejected -= HandleWrongSlot;
        MuseumGameEvents.PaintingThrown -= HandleThrow;
        MuseumGameEvents.PaintingStaged -= HandleStaged;
    }

    public void ApplyVolume()
    {
        float master = PlayerSettingsStore.MasterVolume;
        audioSource.volume = Mathf.Clamp01(sfxVolume * master);
    }

    private void HandlePickup(InteractablePainting painting) => Play(pickupClip, MuseumProceduralSfx.SfxKind.Pickup);
    private void HandlePlace(InteractablePainting painting, PaintingMount mount) => Play(placeClip, MuseumProceduralSfx.SfxKind.Place);
    private void HandleDrop(InteractablePainting painting) => Play(dropClip, MuseumProceduralSfx.SfxKind.Drop);
    private void HandleSectionComplete(GallerySection section) => Play(sectionCompleteClip, MuseumProceduralSfx.SfxKind.SectionComplete);
    private void HandleMuseumWin() => Play(museumWinClip, MuseumProceduralSfx.SfxKind.MuseumWin);
    private void HandleUndo(InteractablePainting painting, PaintingMount mount) => Play(undoClip, MuseumProceduralSfx.SfxKind.Undo);
    private void HandleWrongWing() => Play(wrongWingClip, MuseumProceduralSfx.SfxKind.Wrong);
    private void HandleWrongSlot() => Play(wrongSlotClip != null ? wrongSlotClip : wrongWingClip, MuseumProceduralSfx.SfxKind.Wrong);
    private void HandleThrow(InteractablePainting painting) => Play(throwClip, MuseumProceduralSfx.SfxKind.Throw);
    private void HandleStaged(InteractablePainting painting) => Play(stageClip, MuseumProceduralSfx.SfxKind.Stage);

    public void PlayUiClick() => Play(uiClickClip, MuseumProceduralSfx.SfxKind.UiClick);

    private void Play(AudioClip clip, MuseumProceduralSfx.SfxKind fallbackKind)
    {
        if (audioSource == null)
        {
            return;
        }

        if (clip == null && useSynthesizedFallback)
        {
            clip = MuseumProceduralSfx.ResolveOrFallback(null, fallbackKind);
        }

        if (clip == null)
        {
            return;
        }

        ApplyVolume();
        audioSource.PlayOneShot(clip);
    }
}
