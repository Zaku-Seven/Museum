using UnityEngine;

/// <summary>
/// Brief FOV punch on throw and on rejected placements (wrong wing / wrong slot).
/// </summary>
public class ThrowCameraKick : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float throwFovDelta = 4f;
    [SerializeField] private float rejectFovDelta = -2.5f;
    [SerializeField] private float kickDuration = 0.12f;

    private float kickTimer;
    private float activeDelta;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }
    }

    private void OnEnable()
    {
        MuseumGameEvents.PaintingThrown += HandleThrow;
        MuseumGameEvents.WrongWingRejected += HandleReject;
        MuseumGameEvents.WrongSlotRejected += HandleReject;
    }

    private void OnDisable()
    {
        MuseumGameEvents.PaintingThrown -= HandleThrow;
        MuseumGameEvents.WrongWingRejected -= HandleReject;
        MuseumGameEvents.WrongSlotRejected -= HandleReject;
    }

    private void Update()
    {
        if (targetCamera == null || kickTimer <= 0f)
        {
            return;
        }

        kickTimer -= Time.unscaledDeltaTime;
        float t = kickDuration > 0f ? Mathf.Clamp01(kickTimer / kickDuration) : 0f;
        targetCamera.fieldOfView = PlayerSettingsStore.FieldOfView + activeDelta * t;

        if (kickTimer <= 0f)
        {
            targetCamera.fieldOfView = PlayerSettingsStore.FieldOfView;
        }
    }

    private void HandleThrow(InteractablePainting painting)
    {
        StartKick(throwFovDelta);
    }

    private void HandleReject()
    {
        StartKick(rejectFovDelta);
    }

    private void StartKick(float delta)
    {
        if (targetCamera == null)
        {
            return;
        }

        activeDelta = delta;
        kickTimer = kickDuration;
        targetCamera.fieldOfView = PlayerSettingsStore.FieldOfView + delta;
    }
}
