using UnityEngine;

/// <summary>
/// Finds the closest wall mount that can accept a given painting.
/// </summary>
public static class MuseumNearestMountFinder
{
    public static bool TryFindNearestAcceptingMount(
        InteractablePainting painting,
        Vector3 fromPosition,
        out PaintingMount mount,
        out float distanceMeters)
    {
        mount = null;
        distanceMeters = float.MaxValue;

        if (painting == null)
        {
            return false;
        }

        PaintingMount[] mounts = Object.FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        for (int i = 0; i < mounts.Length; i++)
        {
            PaintingMount candidate = mounts[i];
            if (candidate == null || !candidate.CanAccept(painting))
            {
                continue;
            }

            float distance = Vector3.Distance(fromPosition, candidate.transform.position);
            if (distance < distanceMeters)
            {
                distanceMeters = distance;
                mount = candidate;
            }
        }

        return mount != null;
    }

    public static float SignedAngleToMount(Vector3 fromPosition, Vector3 forward, PaintingMount mount)
    {
        if (mount == null)
        {
            return 0f;
        }

        Vector3 toMount = mount.transform.position - fromPosition;
        toMount.y = 0f;
        if (toMount.sqrMagnitude < 0.001f)
        {
            return 0f;
        }

        Vector3 flatForward = forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = Vector3.forward;
        }

        return Vector3.SignedAngle(flatForward.normalized, toMount.normalized, Vector3.up);
    }
}
