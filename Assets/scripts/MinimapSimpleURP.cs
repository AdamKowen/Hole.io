using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class MinimapSimpleURP : MonoBehaviour
{
    [Header("Zoom (Orthographic Size)")]
    [Tooltip("Smaller = closer")]
    [SerializeField] private float zoom = 30f;

    Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void Update()
    {
        if (!cam) cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(0.001f, zoom);

        // Always look straight down (north-up)
        transform.rotation = Quaternion.identity;
    }
}
