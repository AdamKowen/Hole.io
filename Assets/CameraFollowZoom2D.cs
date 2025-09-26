using UnityEngine;

// Attach this to the Main Camera.
// Comments in English only (as requested).
[RequireComponent(typeof(Camera))]
public class CameraFollowZoom2D : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;                 // Hole transform
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float followSmoothTime = 0.20f;   // lower = snappier, higher = smoother

    [Header("Zoom")]
    [Tooltip("Base orthographic size when the hole is tiny.")]
    public float baseOrthoSize = 3.0f;
    [Tooltip("How much to add per 'unit' of hole radius.")]
    public float zoomPerUnitRadius = 1.5f;
    [Tooltip("Smoothing for zoom changes.")]
    public float zoomSmoothTime = 0.25f;

    [Header("World Bounds (optional)")]
    [Tooltip("SpriteRenderer that defines the playable map bounds. Leave null to disable clamping.")]
    public SpriteRenderer worldBounds;

    private Camera _cam;
    private Vector3 _vel;           // for SmoothDamp position
    private float _zoomVel;         // for SmoothDamp zoom

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // --- Follow ---
        Vector3 targetPos = target.position + offset;
        if (worldBounds != null)
        {
            // Clamp camera center so that edges don't go outside the map
            Bounds b = worldBounds.bounds;
            float halfHeight = _cam.orthographicSize;
            float halfWidth = halfHeight * _cam.aspect;

            float minX = b.min.x + halfWidth;
            float maxX = b.max.x - halfWidth;
            float minY = b.min.y + halfHeight;
            float maxY = b.max.y - halfHeight;

            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }

        Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPos, ref _vel, followSmoothTime);
        transform.position = newPos;

        // --- Zoom based on hole size ---
        float holeRadius = GetHoleRadius(target);
        float desiredSize = Mathf.Max(0.5f, baseOrthoSize + zoomPerUnitRadius * holeRadius);
        float newSize = Mathf.SmoothDamp(_cam.orthographicSize, desiredSize, ref _zoomVel, zoomSmoothTime);
        _cam.orthographicSize = newSize;
    }

    // Uses collider bounds to stay robust with any collider type (Circle/Box/Polygon).
    float GetHoleRadius(Transform t)
    {
        var col = t.GetComponent<Collider2D>();
        if (col != null)
        {
            // half of the largest dimension (world units)
            return Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
        }
        // Fallback: derive from scale if no collider found
        return Mathf.Max(0.1f, t.localScale.x * 0.5f);
    }
}
