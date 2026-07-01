using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Groups a set of <see cref="PaintingMount"/>s that share a wing. When every mount in the
/// section holds a correctly-sorted painting, the section becomes "complete": its frames glow
/// with a pulsing emission and <see cref="OnSectionCompleted"/> fires. Progress is exposed for
/// the HUD via <see cref="AllSections"/>.
///
/// Kept in the global namespace to match the rest of the project's scripts.
/// </summary>
public class GallerySection : MonoBehaviour
{
    [Header("Section")]
    [Tooltip("The wing this section represents. Mounts should require this same wing.")]
    [SerializeField] private GalleryWing sectionWing = GalleryWing.Modern;

    [Tooltip("Mounts that belong to this section. Wired by GalleryContentSetup.")]
    [SerializeField] private List<PaintingMount> mounts = new List<PaintingMount>();

    [Tooltip("Optional label for the HUD progress line (e.g. \"Modern (North)\"). Empty = wing name.")]
    [SerializeField] private string sectionDisplayName = string.Empty;

    [Header("Completion Glow")]
    [Tooltip("Renderers pulsed when the section completes. If empty, each mount's child renderer is used.")]
    [SerializeField] private List<Renderer> glowRenderers = new List<Renderer>();
    [SerializeField] private Color glowColor = new Color(0.25f, 0.55f, 1f);
    [SerializeField] private float glowIntensity = 2.5f;
    [SerializeField] private float pulseSpeed = 3f;

    /// <summary>All currently-enabled sections. The HUD reads this to build the progress line.</summary>
    public static readonly List<GallerySection> AllSections = new List<GallerySection>();

    /// <summary>Raised on the rising edge each time any section becomes complete.</summary>
    public static event Action<GallerySection> OnSectionCompleted;

    public GalleryWing SectionWing => sectionWing;
    public string DisplayName => string.IsNullOrEmpty(sectionDisplayName) ? sectionWing.ToString() : sectionDisplayName;
    public bool IsComplete { get; private set; }
    public int MountCount => mounts != null ? mounts.Count : 0;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private readonly List<Renderer> resolvedRenderers = new List<Renderer>();
    private bool renderersResolved;

    private void OnEnable()
    {
        if (!AllSections.Contains(this))
        {
            AllSections.Add(this);
        }

        BindMounts();
        CheckComplete();
    }

    private void OnDisable()
    {
        AllSections.Remove(this);
    }

    private void Update()
    {
        if (!IsComplete)
        {
            return;
        }

        // Pulse the emission between a dim and bright value for a satisfying "wing complete" glow.
        float t = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
        float intensity = Mathf.Lerp(glowIntensity * 0.35f, glowIntensity, t);
        ApplyEmission(glowColor * intensity, true);
    }

    /// <summary>
    /// Number of mounts currently holding a painting whose wing matches this section.
    /// </summary>
    public int CorrectlyFilledCount()
    {
        if (mounts == null)
        {
            return 0;
        }

        int count = 0;
        foreach (PaintingMount mount in mounts)
        {
            if (mount != null && mount.IsOccupied && mount.Occupant != null && mount.Occupant.Wing == sectionWing)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Recomputes completion. Called by member mounts whenever their occupancy changes.
    /// Toggles the glow and fires <see cref="OnSectionCompleted"/> on the rising edge.
    /// </summary>
    public bool CheckComplete()
    {
        bool complete = MountCount > 0 && CorrectlyFilledCount() == MountCount;

        if (complete && !IsComplete)
        {
            IsComplete = true;
            ApplyEmission(glowColor * glowIntensity, true);
            OnSectionCompleted?.Invoke(this);
            MuseumGameEvents.RaiseSectionCompleted(this);
        }
        else if (!complete && IsComplete)
        {
            IsComplete = false;
            ApplyEmission(Color.black, false);
        }

        return IsComplete;
    }

    /// <summary>Recomputes completion after a full scene reset (may clear glow).</summary>
    public void ForceRefreshCompletion()
    {
        bool wasComplete = IsComplete;
        IsComplete = false;
        ApplyEmission(Color.black, false);
        CheckComplete();

        if (!IsComplete && wasComplete)
        {
            // Glow already cleared above.
        }
    }

    private void BindMounts()
    {
        if (mounts == null)
        {
            return;
        }

        foreach (PaintingMount mount in mounts)
        {
            if (mount != null)
            {
                mount.SetSection(this);
            }
        }
    }

    private void ApplyEmission(Color color, bool emissionEnabled)
    {
        ResolveRenderers();

        foreach (Renderer renderer in resolvedRenderers)
        {
            if (renderer == null)
            {
                continue;
            }

            // renderer.material returns a runtime instance; only touched in play mode
            // (OnEnable/Update do not run in the editor without [ExecuteAlways]).
            Material material = renderer.material;
            if (emissionEnabled)
            {
                material.EnableKeyword("_EMISSION");
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }

            material.SetColor(EmissionColorId, color);
        }
    }

    private void ResolveRenderers()
    {
        if (renderersResolved)
        {
            return;
        }

        resolvedRenderers.Clear();

        if (glowRenderers != null && glowRenderers.Count > 0)
        {
            resolvedRenderers.AddRange(glowRenderers);
        }
        else if (mounts != null)
        {
            foreach (PaintingMount mount in mounts)
            {
                if (mount == null)
                {
                    continue;
                }

                Renderer renderer = mount.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    resolvedRenderers.Add(renderer);
                }
            }
        }

        renderersResolved = true;
    }
}
