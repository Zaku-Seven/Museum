using UnityEngine;

/// <summary>
/// Tracks whether every <see cref="GallerySection"/> in the scene is complete (museum win state).
/// </summary>
public class MuseumProgress : MonoBehaviour
{
    public static MuseumProgress Instance { get; private set; }

    public bool IsMuseumComplete { get; private set; }

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (IsMuseumComplete)
        {
            return;
        }

        if (AreAllSectionsComplete())
        {
            SetMuseumComplete(true);
        }
    }

    public void ResetProgress()
    {
        SetMuseumComplete(false);
    }

    public void RestoreMuseumCompleteFromSave(bool complete)
    {
        if (complete)
        {
            SetMuseumComplete(true, fireEvent: false);
            MuseumGameFlowController.Instance?.EnterWinModal();
        }
        else
        {
            SetMuseumComplete(false);
        }
    }

    private void SetMuseumComplete(bool complete, bool fireEvent = true)
    {
        IsMuseumComplete = complete;

        if (complete && fireEvent)
        {
            MuseumGameEvents.RaiseMuseumCompleted();
        }
    }

    public static bool AreAllSectionsComplete()
    {
        if (GallerySection.AllSections == null || GallerySection.AllSections.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < GallerySection.AllSections.Count; i++)
        {
            GallerySection section = GallerySection.AllSections[i];
            if (section == null || !section.IsComplete)
            {
                return false;
            }
        }

        return true;
    }
}
