using UnityEngine;

// Attach this to the Main Camera.
[RequireComponent(typeof(Camera))]
public class CameraFollowZoom2D : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;                       // Hole transform
    public Vector3 followOffset = new Vector3(0f, 0f, -10f);
    [Tooltip("Lower = snappier, higher = smoother")]
    public float followSmoothTime = 0.20f;

    [Header("Zoom (by hole level)")]
    [Tooltip("Orthographic size at level 1 (more = zoomed out).")]
    public float startSize = 3.0f;
    [Tooltip("How much to add per level-up (level 2 adds once, 3 adds twice, etc.).")]
    public float sizePerLevel = 0.75f;
    [Tooltip("Smoothing for zoom changes.")]
    public float zoomSmoothTime = 0.25f;

    [Header("World Bounds (optional)")]
    [Tooltip("SpriteRenderer that defines the map bounds. Leave null to disable clamping.")]
    public SpriteRenderer worldBounds;

    private Camera _cam;
    private Vector3 _followVel;   // for SmoothDamp (position)
    private float _zoomVel;       // for SmoothDamp (size)

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // --- Desired zoom by level ---
        float desiredSize = GetDesiredSizeByLevel(target);
        float nextSize = Mathf.SmoothDamp(_cam.orthographicSize, desiredSize, ref _zoomVel, zoomSmoothTime);
        _cam.orthographicSize = Mathf.Max(0.1f, nextSize);

        // --- Follow (with optional bounds clamp) ---
        Vector3 targetPos = target.position + followOffset;

        if (worldBounds != null)
        {
            Bounds b = worldBounds.bounds;
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            float minX = b.min.x + halfW;
            float maxX = b.max.x - halfW;
            float minY = b.min.y + halfH;
            float maxY = b.max.y - halfH;

            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }

        Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPos, ref _followVel, followSmoothTime);
        transform.position = newPos;
    }

    // Simple: size = startSize + sizePerLevel * (holeLevel - 1).
    float GetDesiredSizeByLevel(Transform t)
    {
        var hc = t.GetComponent<HoleController>();
        if (hc != null)
        {
            int lvl = Mathf.Max(1, hc.holeLevel);
            return Mathf.Max(0.1f, startSize + sizePerLevel * (lvl - 1));
        }
        // Fallback if no HoleController on target
        return Mathf.Max(0.1f, startSize);
    }
}
