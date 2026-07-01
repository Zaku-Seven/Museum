using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a crosshair, context-sensitive prompt, per-wing progress line, completion banner,
/// stack/staging info, one-shot tutorial tips, and the museum win overlay.
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
    [SerializeField] private Text stackText;
    [SerializeField] private Text stagingText;
    [SerializeField] private Text tutorialText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Text winText;

    [Header("Hints")]
    [Tooltip("Constant hint shown at the top of the screen.")]
    [SerializeField] private string hintMessage =
        "E — pick up / stack  ·  Scroll — active item  ·  Click — place/drop  ·  Q — throw  ·  Right-click — undo  ·  Esc — pause";

    [Header("Completion banner")]
    [SerializeField] private float bannerDuration = 4f;

    [Header("Tutorial tips")]
    [SerializeField] private float tutorialDuration = 5f;

    [Header("Win overlay")]
    [SerializeField] private string winMessage = "Museum complete!\nEvery wing is hung. Cozy work.";

    private static readonly StringBuilder ProgressBuilder = new StringBuilder();
    private float bannerTimer;
    private float tutorialTimer;

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
        MuseumGameEvents.MuseumCompleted += HandleMuseumCompleted;
        TutorialHints.OnShowHint += HandleTutorialHint;
    }

    private void OnDisable()
    {
        GallerySection.OnSectionCompleted -= HandleSectionCompleted;
        MuseumGameEvents.MuseumCompleted -= HandleMuseumCompleted;
        TutorialHints.OnShowHint -= HandleTutorialHint;
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
            bool hideCrosshair = MuseumGameFlowController.Instance != null
                && MuseumGameFlowController.Instance.IsWinModalActive;
            crosshairText.enabled = !hideCrosshair;
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

        if (stackText != null)
        {
            string stackLabel = artPickup.GetActiveStackLabel();
            stackText.text = stackLabel;
            stackText.enabled = !string.IsNullOrEmpty(stackLabel);
        }

        if (stagingText != null)
        {
            int staged = SortingTable.TotalStagedCount;
            stagingText.text = staged > 0 ? $"Sorting table: {staged} staged" : string.Empty;
            stagingText.enabled = staged > 0;
        }

        UpdateBanner();
        UpdateTutorialBanner();
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

    private void HandleMuseumCompleted()
    {
        TutorialHints.TryShowCompleteHint();

        if (winText != null)
        {
            string stats = MuseumStatistics.Instance != null
                ? MuseumStatistics.Instance.BuildWinStatsLine()
                : string.Empty;
            winText.text = string.IsNullOrEmpty(stats)
                ? winMessage
                : $"{winMessage}\n\n{stats}";
        }

        // Win modal + input blocking handled by MuseumGameFlowController.
    }

    private void HandleTutorialHint(string message)
    {
        if (tutorialText == null || string.IsNullOrEmpty(message))
        {
            return;
        }

        tutorialText.text = message;
        tutorialText.enabled = true;
        tutorialTimer = tutorialDuration;
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

    private void UpdateTutorialBanner()
    {
        if (tutorialText == null || tutorialTimer <= 0f)
        {
            if (tutorialText != null && tutorialTimer <= 0f)
            {
                tutorialText.enabled = false;
            }

            return;
        }

        tutorialTimer -= Time.deltaTime;
        if (tutorialTimer <= 0f)
        {
            tutorialText.enabled = false;
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
