using UnityEngine;

/// <summary>
/// Semi-transparent preview on a valid mount snap point while the player aims to place art.
/// </summary>
[RequireComponent(typeof(Camera))]
public class MountPlacementGhost : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArtPickup artPickup;

    [Header("Ghost")]
    [SerializeField] private Color ghostColor = new Color(0.45f, 0.95f, 0.55f, 0.38f);
    [SerializeField] private Vector3 ghostScale = new Vector3(0.58f, 0.05f, 0.38f);

    private GameObject ghostObject;
    private Renderer ghostRenderer;
    private PaintingMount activeMount;

    private void Awake()
    {
        if (artPickup == null)
        {
            artPickup = GetComponent<ArtPickup>();
        }

        CreateGhostObject();
    }

    private void LateUpdate()
    {
        if (artPickup == null || !artPickup.IsHolding || !artPickup.TryGetCenterRayHit(out RaycastHit hit))
        {
            HideGhost();
            return;
        }

        PaintingMount mount = hit.collider.GetComponentInParent<PaintingMount>();
        if (mount == null || !mount.CanAccept(artPickup.HeldPainting))
        {
            HideGhost();
            return;
        }

        ShowOnMount(mount);
    }

    private void ShowOnMount(PaintingMount mount)
    {
        if (ghostObject == null)
        {
            return;
        }

        activeMount = mount;
        Transform snap = mount.SnapPoint;
        ghostObject.SetActive(true);
        ghostObject.transform.SetPositionAndRotation(snap.position, snap.rotation);
    }

    private void HideGhost()
    {
        activeMount = null;
        if (ghostObject != null)
        {
            ghostObject.SetActive(false);
        }
    }

    private void CreateGhostObject()
    {
        ghostObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ghostObject.name = "PlacementGhost";
        ghostObject.hideFlags = HideFlags.HideAndDontSave;

        Collider collider = ghostObject.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        ghostRenderer = ghostObject.GetComponent<Renderer>();
        if (ghostRenderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = ghostColor;
            ConfigureTransparent(material);
            ghostRenderer.sharedMaterial = material;
        }

        ghostObject.transform.localScale = ghostScale;
        ghostObject.SetActive(false);
    }

    private static void ConfigureTransparent(Material material)
    {
        material.SetFloat("_Surface", 1f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.renderQueue = 3000;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
    }

    private void OnDisable()
    {
        HideGhost();
    }
}
