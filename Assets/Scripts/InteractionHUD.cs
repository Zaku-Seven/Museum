using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a crosshair and context-sensitive interaction prompt for the art pickup system.
/// </summary>
public class InteractionHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArtPickup artPickup;
    [SerializeField] private Text promptText;
    [SerializeField] private Text crosshairText;

    private void Awake()
    {
        if (artPickup == null)
        {
            artPickup = GetComponent<ArtPickup>();
        }
    }

    private void Update()
    {
        if (artPickup == null || promptText == null)
        {
            return;
        }

        string prompt = artPickup.GetInteractionPrompt();
        promptText.text = prompt;
        promptText.enabled = !string.IsNullOrEmpty(prompt);

        if (crosshairText != null)
        {
            crosshairText.enabled = true;
        }
    }
}
