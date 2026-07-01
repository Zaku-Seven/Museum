using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Toggles pause with Escape: unlocks the cursor, shows an overlay, and disables player
/// movement / pickup without modifying <see cref="FirstPersonController"/> source.
/// Works in standalone builds (Editor also frees the cursor on Esc, but the overlay
/// makes pause obvious during testing).
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Text pauseMessageText;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private ArtPickup artPickup;
    [SerializeField] private MountAimHighlighter mountAimHighlighter;

    [Header("Copy")]
    [SerializeField] private string pauseMessage = "PAUSED\nPress Esc to resume";

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        ResolveReferences();
        SetPaused(false, force: true);
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        SetPaused(!IsPaused);
    }

    private void ResolveReferences()
    {
        if (firstPersonController == null)
        {
            firstPersonController = GetComponent<FirstPersonController>();
        }

        if (artPickup == null || mountAimHighlighter == null)
        {
            Transform cameraTransform = transform.Find("PlayerCamera");
            if (cameraTransform != null)
            {
                if (artPickup == null)
                {
                    artPickup = cameraTransform.GetComponent<ArtPickup>();
                }

                if (mountAimHighlighter == null)
                {
                    mountAimHighlighter = cameraTransform.GetComponent<MountAimHighlighter>();
                }
            }
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void SetPaused(bool paused, bool force = false)
    {
        if (!force && paused == IsPaused)
        {
            return;
        }

        IsPaused = paused;

        if (firstPersonController != null)
        {
            firstPersonController.enabled = !paused;
        }

        if (artPickup != null)
        {
            artPickup.enabled = !paused;
        }

        if (mountAimHighlighter != null)
        {
            mountAimHighlighter.enabled = !paused;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(paused);
        }

        if (pauseMessageText != null)
        {
            pauseMessageText.text = pauseMessage;
        }

        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }
}
