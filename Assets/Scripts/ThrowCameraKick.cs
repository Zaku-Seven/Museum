using UnityEngine;

/// <summary>
/// Brief FOV kick when throwing a painting.
/// </summary>
public class ThrowCameraKick : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float kickFovDelta = 4f;
    [SerializeField] private float kickDuration = 0.12f;

    private float kickTimer;
    private float baseFov;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (targetCamera != null)
        {
            baseFov = targetCamera.fieldOfView;
        }
    }

    private void OnEnable()
    {
        MuseumGameEvents.PaintingThrown += HandleThrow;
    }

    private void OnDisable()
    {
        MuseumGameEvents.PaintingThrown -= HandleThrow;
    }

    private void Update()
    {
        if (targetCamera == null || kickTimer <= 0f)
        {
            return;
        }

        kickTimer -= Time.deltaTime;
        float t = Mathf.Clamp01(kickTimer / kickDuration);
        targetCamera.fieldOfView = PlayerSettingsStore.FieldOfView + kickFovDelta * t;
    }

    private void HandleThrow(InteractablePainting painting)
    {
        if (targetCamera == null)
        {
            return;
        }

        baseFov = PlayerSettingsStore.FieldOfView;
        kickTimer = kickDuration;
        targetCamera.fieldOfView = baseFov + kickFovDelta;
    }
}
