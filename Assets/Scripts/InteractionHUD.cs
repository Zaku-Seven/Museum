using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a crosshair, context-sensitive prompt, per-wing progress line, completion banner,
/// and optional hint text for the art pickup system.
/// </summary>
public class InteractionHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArtPickup artPickup;
    [SerializeField] private Text promptText;
    [SerializeField] private Text crosshairText;
    [SerializeField] private Text progressText;
    [SerializeField] private Text hintText;
    [SerializeField] private Text bannerText;

    [Header("Hints")]
    [Tooltip("Constant hint shown at the top of the screen.")]
    [SerializeField] private string hintMessage = "E — pick up / add to stack    ·    Click — place or drop    ·    Esc — pause";

    [Header("Completion banner")]
    [SerializeField] private float bannerDuration = 4f;

    private static readonly StringBuilder ProgressBuilder = new StringBuilder();
    private float bannerTimer;

    private void Awake()
    {
        if (artPickup == null)
        {
            artPickup = GetComponent<ArtPickup>();
        }
    }

    private void OnEnable()
    {
        GallerySection.OnSectionCompleted += HandleSectionCompleted;
    }

    private void OnDisable()
    {
        GallerySection.OnSectionCompleted -= HandleSectionCompleted;
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

        UpdateBanner();
    }

    private void HandleSectionCompleted(GallerySection section)
    {
        if (section == null || bannerText == null)
        {
            return;
        }

        bannerText.text = $"{section.DisplayName} wing complete!";
        bannerText.enabled = true;
        bannerTimer = bannerDuration;
    }

    private void UpdateBanner()
    {
        if (bannerText == null || bannerTimer <= 0f)
        {
            if (bannerText != null && bannerTimer <= 0f)
            {
                bannerText.enabled = false;
            }

            return;
        }

        bannerTimer -= Time.deltaTime;
        if (bannerTimer <= 0f)
        {
            bannerText.enabled = false;
        }
    }

    /// <summary>
    /// Builds the per-wing progress line from every active <see cref="GallerySection"/>.
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
                .Append(section.DisplayName)
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
