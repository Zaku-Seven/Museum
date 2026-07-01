using UnityEngine;
using UnityEngine.UI;
{
    [SerializeField] private GalleryWing requiredWing = GalleryWing.Modern;
    [SerializeField] private Text labelText;
    [SerializeField] private float billboardDistance = 24f;

    public void Configure(GalleryWing wing)
    {
        requiredWing = wing;
        RefreshLabel();
    }

    private void Awake()
    {
        RefreshLabel();
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        float distance = Vector3.Distance(cam.transform.position, transform.position);
        if (distance > billboardDistance)
        {
            return;
        }

        Vector3 toCamera = cam.transform.position - transform.position;
        toCamera.y = 0f;
        if (toCamera.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(-toCamera.normalized, Vector3.up);
        }
    }

    private void RefreshLabel()
    {
        if (labelText != null)
        {
            labelText.text = $"{requiredWing} wing";
        }
    }
}
