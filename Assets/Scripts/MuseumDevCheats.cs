using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Development-only hotkeys for rapid museum testing (stripped from release builds unless DEVELOPMENT_BUILD).
/// </summary>
public class MuseumDevCheats : MonoBehaviour
{
    [SerializeField] private bool enableCheats = true;
    [SerializeField] private float teleportStandOffMeters = 1.6f;

    private void Update()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        return;
#else
        if (!enableCheats || Keyboard.current == null)
        {
            return;
        }

        if (MainMenuController.Instance != null && MainMenuController.Instance.IsMainMenuOpen)
        {
            return;
        }

        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            TeleportToNearestMount();
        }

        if (Keyboard.current.f7Key.wasPressedThisFrame)
        {
            ToggleDebugOverlay();
        }

        if (Keyboard.current.f8Key.wasPressedThisFrame)
        {
            MuseumSaveManager.Instance?.Save();
            Debug.Log("MuseumDevCheats: manual save triggered.");
        }

        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            CompleteIncompleteSection();
        }

        if (Keyboard.current.f10Key.wasPressedThisFrame)
        {
            AutoHangAllMatchingPaintings();
        }

        if (Keyboard.current.f11Key.wasPressedThisFrame)
        {
            TriggerMuseumWin();
        }
#endif
    }

    private static void ToggleDebugOverlay()
    {
        MuseumDebugOverlay overlay = MuseumDebugOverlay.Instance;
        if (overlay == null)
        {
            overlay = Object.FindFirstObjectByType<MuseumDebugOverlay>();
        }

        overlay?.Toggle();
    }

    private void TeleportToNearestMount()
    {
        GameObject playerObject = gameObject;
        CharacterController controller = playerObject.GetComponent<CharacterController>();
        Transform cameraTransform = playerObject.transform.Find("PlayerCamera");
        ArtPickup artPickup = cameraTransform != null ? cameraTransform.GetComponent<ArtPickup>() : null;

        InteractablePainting referencePainting = artPickup != null ? artPickup.HeldPainting : null;
        if (referencePainting == null)
        {
            referencePainting = FindFirstLoosePainting();
        }

        if (referencePainting == null)
        {
            Debug.Log("MuseumDevCheats: no loose painting to guide teleport.");
            return;
        }

        if (!MuseumNearestMountFinder.TryFindNearestAcceptingMount(
                referencePainting,
                playerObject.transform.position,
                out PaintingMount mount,
                out _))
        {
            Debug.Log("MuseumDevCheats: no accepting mount found.");
            return;
        }

        Vector3 standPoint = mount.transform.position - mount.transform.forward * teleportStandOffMeters;
        standPoint.y = playerObject.transform.position.y;

        if (controller != null)
        {
            controller.enabled = false;
        }

        playerObject.transform.position = standPoint;

        if (controller != null)
        {
            controller.enabled = true;
        }

        Debug.Log($"MuseumDevCheats: teleported near mount for {referencePainting.PaintingTitle}.");
    }

    private static InteractablePainting FindFirstLoosePainting()
    {
        InteractablePainting[] paintings = Object.FindObjectsByType<InteractablePainting>(FindObjectsSortMode.None);
        for (int i = 0; i < paintings.Length; i++)
        {
            InteractablePainting painting = paintings[i];
            if (painting != null && painting.CurrentMount == null)
            {
                return painting;
            }
        }

        return null;
    }

    private static void TriggerMuseumWin()
    {
        if (MuseumProgress.Instance != null && MuseumProgress.Instance.IsMuseumComplete)
        {
            MuseumGameFlowController.Instance?.EnterWinModal();
            Debug.Log("MuseumDevCheats: museum already complete — opened win modal.");
            return;
        }

        AutoHangAllMatchingPaintings();
        int sectionCount = GallerySection.AllSections != null ? GallerySection.AllSections.Count : 0;
        for (int pass = 0; pass < sectionCount; pass++)
        {
            CompleteIncompleteSection();
        }

        AutoHangAllMatchingPaintings();

        if (MuseumProgress.AreAllSectionsComplete())
        {
            Debug.Log("MuseumDevCheats: all sections filled — win should trigger via MuseumProgress.");
            return;
        }

        Debug.Log("MuseumDevCheats: could not complete all sections (missing paintings?).");
    }

    private static void CompleteIncompleteSection()
    {
        if (GallerySection.AllSections == null)
        {
            return;
        }

        for (int i = 0; i < GallerySection.AllSections.Count; i++)
        {
            GallerySection section = GallerySection.AllSections[i];
            if (section == null || section.IsComplete)
            {
                continue;
            }

            FillSection(section);
            Debug.Log($"MuseumDevCheats: filled section {section.DisplayName}.");
            return;
        }

        Debug.Log("MuseumDevCheats: all sections already complete.");
    }

    private static void FillSection(GallerySection section)
    {
        PaintingMount[] mounts = Object.FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        InteractablePainting[] paintings = Object.FindObjectsByType<InteractablePainting>(FindObjectsSortMode.None);

        for (int m = 0; m < mounts.Length; m++)
        {
            PaintingMount mount = mounts[m];
            if (mount == null || mount.IsOccupied)
            {
                continue;
            }

            if (mount.RequiredWing != section.SectionWing)
            {
                continue;
            }

            for (int p = 0; p < paintings.Length; p++)
            {
                InteractablePainting painting = paintings[p];
                if (painting == null || painting.CurrentMount != null)
                {
                    continue;
                }

                if (!mount.CanAccept(painting))
                {
                    continue;
                }

                mount.PlacePainting(painting.transform, painting.GetComponent<Rigidbody>());
                break;
            }
        }
    }

    private static void AutoHangAllMatchingPaintings()
    {
        PaintingMount[] mounts = Object.FindObjectsByType<PaintingMount>(FindObjectsSortMode.None);
        InteractablePainting[] paintings = Object.FindObjectsByType<InteractablePainting>(FindObjectsSortMode.None);

        for (int m = 0; m < mounts.Length; m++)
        {
            PaintingMount mount = mounts[m];
            if (mount == null || mount.IsOccupied)
            {
                continue;
            }

            for (int p = 0; p < paintings.Length; p++)
            {
                InteractablePainting painting = paintings[p];
                if (painting == null || painting.CurrentMount != null)
                {
                    continue;
                }

                if (!mount.CanAccept(painting))
                {
                    continue;
                }

                mount.PlacePainting(painting.transform, painting.GetComponent<Rigidbody>());
                break;
            }
        }

        Debug.Log("MuseumDevCheats: auto-hung all available matching paintings.");
    }
}
