using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowZoom2D : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float followSmoothTime = 0.20f;

    [Header("Zoom")]
    [SerializeField] private float baseOrthoSize = 3.0f;
    [SerializeField] private float zoomPerUnitRadius = 1.5f;
    [SerializeField] private float zoomSmoothTime = 0.20f;

    [Header("World Bounds (optional)")]
    [SerializeField] private SpriteRenderer worldBounds;

    [Header("Menu/Intro")]
    [SerializeField] private bool menuMode = false;
    [SerializeField] private float menuOrthoSize = 12f;
    [SerializeField] private float menuZoomSmoothTime = 0.35f;
    
    [Header("Zoom Limits")]
    [SerializeField] private bool limitZoom = true;
    [SerializeField] private float minOrthoSize = 2.0f;  
    [SerializeField] private float maxOrthoSize = 12.0f;  

    [Header("Manual Lock (optional)")]
    [SerializeField] private bool lockZoom = false;       
    [SerializeField] private float lockedSize = 10f;       


    // don't move at all while menu is up
    [SerializeField] private bool freezePositionInMenu = true;

    private Camera _cam;
    private Vector3 _vel;
    private float _zoomVel;

    private void Awake() { _cam = GetComponent<Camera>(); }

    private void LateUpdate()
    {
        if (!_cam || !target) return;

        // Decide desired size for this frame
        float desiredSize = menuMode ? menuOrthoSize : GetDesiredSizeByTarget(target);

        // NEW: allow locking zoom entirely (you decide when to stop)
        if (lockZoom)
            desiredSize = lockedSize;

        // NEW: clamp zoom range
        if (limitZoom)
            desiredSize = Mathf.Clamp(desiredSize, minOrthoSize, maxOrthoSize);

        // If menu is up and we freeze position — skip all follow/clamp logic.
        if (!(menuMode && freezePositionInMenu))
        {

            Vector3 desiredPos = target.position + offset;

            if (worldBounds)
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

        // never go below epsilon
        _cam.orthographicSize = Mathf.Max(0.01f, nextSize);
    }


    private float GetDesiredSizeByTarget(Transform t)
    {
        if (!t) return baseOrthoSize;
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
