using UnityEngine;

/// <summary>
/// Short procedural particle bursts when a wing section or the full museum completes.
/// </summary>
public class MuseumCelebrationFx : MonoBehaviour
{
    [SerializeField] private Color sectionBurstColor = new Color(0.45f, 0.75f, 1f);
    [SerializeField] private Color museumWinBurstColor = new Color(1f, 0.85f, 0.35f);
    [SerializeField] private float burstHeight = 1.4f;

    private Transform burstAnchor;

    private void Awake()
    {
        burstAnchor = transform;
    }

    private void OnEnable()
    {
        MuseumGameEvents.SectionCompleted += HandleSectionCompleted;
        MuseumGameEvents.MuseumCompleted += HandleMuseumCompleted;
    }

    private void OnDisable()
    {
        MuseumGameEvents.SectionCompleted -= HandleSectionCompleted;
        MuseumGameEvents.MuseumCompleted -= HandleMuseumCompleted;
    }

    private void HandleSectionCompleted(GallerySection section)
    {
        if (section == null || MuseumMotionSettings.ReduceMotion)
        {
            return;
        }

        Vector3 position = section.GetCelebrationPosition() + Vector3.up * burstHeight;
        SpawnBurst(position, sectionBurstColor, particleCount: 28, speed: 2.2f, size: 0.07f);
    }

    private void HandleMuseumCompleted()
    {
        if (MuseumMotionSettings.ReduceMotion)
        {
            return;
        }

        Vector3 position = burstAnchor != null
            ? burstAnchor.position + Vector3.up * (burstHeight + 0.4f)
            : Vector3.up * burstHeight;

        SpawnBurst(position, museumWinBurstColor, particleCount: 48, speed: 3f, size: 0.09f);
    }

    private static void SpawnBurst(Vector3 position, Color color, int particleCount, float speed, float size)
    {
        GameObject burstObject = new GameObject("CelebrationBurst");
        burstObject.transform.position = position;

        ParticleSystem particles = burstObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.55f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 0.65f;
        main.startSpeed = speed;
        main.startSize = size;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = color;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)particleCount) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(Color.white, 0.35f),
                new GradientColorKey(color * 0.6f, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.85f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        ParticleSystemRenderer renderer = burstObject.GetComponent<ParticleSystemRenderer>();
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null)
        {
            particleShader = Shader.Find("Particles/Standard Unlit");
        }

        if (particleShader != null)
        {
            renderer.material = new Material(particleShader);
        }

        particles.Play();
        Object.Destroy(burstObject, 2f);
    }
}
