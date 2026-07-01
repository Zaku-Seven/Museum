using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a crosshair, a context-sensitive interaction prompt, and a per-wing sorting
/// progress line (e.g. "Modern: 2/3 hung") for the art pickup system.
/// </summary>
public class InteractionHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArtPickup artPickup;
    [SerializeField] private Text promptText;
    [SerializeField] private Text crosshairText;
    [SerializeField] private Text progressText;
    [SerializeField] private Text hintText;

    [Header("Hints")]
    [Tooltip("Constant hint shown at the top of the screen (stretch-goal placeholder).")]
    [SerializeField] private string hintMessage = "Future: scroll to reorder stack";

    private static readonly StringBuilder ProgressBuilder = new StringBuilder();

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

        if (progressText != null)
        {
            string progress = BuildProgressText();
            progressText.text = progress;
            progressText.enabled = !string.IsNullOrEmpty(progress);
        }

        if (hintText != null)
        {
            hintText.text = hintMessage;
            hintText.enabled = !string.IsNullOrEmpty(hintMessage);
        }
    }

    /// <summary>
    /// Builds the per-wing progress line from every active <see cref="GallerySection"/>,
    /// e.g. "Modern: 2/3 hung    Classical: 3/3 hung - done".
    /// </summary>
    private static string BuildProgressText()
    {
        var sections = GallerySection.AllSections;
        if (sections == null || sections.Count == 0)
        {
            return string.Empty;
        }

        ProgressBuilder.Clear();
        for (int i = 0; i < sections.Count; i++)
        {
            GallerySection section = sections[i];
            if (section == null)
            {
                continue;
            }

            if (ProgressBuilder.Length > 0)
            {
                ProgressBuilder.Append("    ");
            }

            ProgressBuilder
                .Append(section.SectionWing)
                .Append(": ")
                .Append(section.CorrectlyFilledCount())
                .Append('/')
                .Append(section.MountCount)
                .Append(" hung");

            if (section.IsComplete)
            {
                ProgressBuilder.Append(" - done");
            }
        }

        return ProgressBuilder.ToString();
    }
}
