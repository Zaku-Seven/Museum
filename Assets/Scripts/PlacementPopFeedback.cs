using System.Collections;
using UnityEngine;

/// <summary>
/// Brief scale pop when a painting snaps onto a wall mount — a tiny "satisfying click"
/// moment without audio. Added at runtime by <see cref="PaintingMount.PlacePainting"/>.
/// </summary>
public class PlacementPopFeedback : MonoBehaviour
{
    [SerializeField] private float popPeakScale = 1.08f;
    [SerializeField] private float duration = 0.22f;

    private Coroutine activeRoutine;
    private Vector3 baseLocalScale;

    /// <summary>Starts (or restarts) the pop animation on <paramref name="target"/>.</summary>
    public static void Play(Transform target)
    {
        if (target == null)
        {
            return;
        }

        PlacementPopFeedback pop = target.GetComponent<PlacementPopFeedback>();
        if (pop == null)
        {
            pop = target.gameObject.AddComponent<PlacementPopFeedback>();
        }

        pop.StartPop();
    }

    private void Awake()
    {
        baseLocalScale = transform.localScale;
    }

    private void StartPop()
    {
        baseLocalScale = transform.localScale;

        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Quick ease-out bump: 1 → peak at ~40% → 1
            float scaleFactor = t < 0.4f
                ? Mathf.Lerp(1f, popPeakScale, t / 0.4f)
                : Mathf.Lerp(popPeakScale, 1f, (t - 0.4f) / 0.6f);
            transform.localScale = baseLocalScale * scaleFactor;
            yield return null;
        }

        transform.localScale = baseLocalScale;
        activeRoutine = null;
    }
}
