using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowZoom2D : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float followSmoothTime = 0.20f;

    [Header("Zoom")]
    public float baseOrthoSize = 3.0f;
    public float zoomPerUnitRadius = 1.5f;
    public float zoomSmoothTime = 0.20f;

    [Header("World Bounds (optional)")]
    public SpriteRenderer worldBounds;

    [Header("Menu/Intro")]
    public bool menuMode = false;
    public float menuOrthoSize = 12f;
    public float menuZoomSmoothTime = 0.35f;

    // NEW: don't move at all while menu is up
    public bool freezePositionInMenu = true;

    private Camera _cam;
    private Vector3 _vel;
    private float _zoomVel;

    private void Awake() { _cam = GetComponent<Camera>(); }

    private void LateUpdate()
    {
        if (_cam == null) return;

        // Decide desired size for this frame
        float desiredSize = menuMode ? menuOrthoSize : GetDesiredSizeByTarget(target);

        // If menu is up and we freeze position — skip all follow/clamp logic.
        if (!(menuMode && freezePositionInMenu))
        {
            if (target == null) return;

            Vector3 desiredPos = target.position + offset;

            if (worldBounds != null)
            {
                // clamp using current camera size (or desiredSize, either is fine here)
                float halfH = desiredSize;
                float halfW = halfH * _cam.aspect;
                Bounds b = worldBounds.bounds;
                desiredPos.x = Mathf.Clamp(desiredPos.x, b.min.x + halfW, b.max.x - halfW);
                desiredPos.y = Mathf.Clamp(desiredPos.y, b.min.y + halfH, b.max.y - halfH);
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _vel, followSmoothTime);
        }

        // Smooth zoom (works also in menu)
        float nextSize = Mathf.SmoothDamp(
            _cam.orthographicSize,
            desiredSize,
            ref _zoomVel,
            menuMode ? menuZoomSmoothTime : zoomSmoothTime
        );
        _cam.orthographicSize = Mathf.Max(0.01f, nextSize);
    }

    private float GetDesiredSizeByTarget(Transform t)
    {
        if (t == null) return baseOrthoSize;
        float r = Mathf.Max(t.localScale.x, t.localScale.y) * 0.5f;
        return baseOrthoSize + r * zoomPerUnitRadius;
    }

    public void EnterMenuMode(float size, bool snap = false)
    {
        menuMode = true;
        menuOrthoSize = size;
        if (snap && _cam != null)
            _cam.orthographicSize = menuOrthoSize; // snap zoom immediately
    }

    public void ExitMenuMode()
    {
        menuMode = false;
    }
}
