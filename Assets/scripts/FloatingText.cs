using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text tmp; // assigned in prefab

    public TMP_Text Tmp => tmp;

    [Header("Motion")]
    [SerializeField] private float riseDistance = 0.6f; 
    [SerializeField] private float duration = 0.6f;     
    [SerializeField] private Transform followTarget;    // optional (e.g., the hole transform)
    [SerializeField] private Vector3 followOffset;      // offset above target

    private Vector3 startPos;
    private Vector3 endPos;
    private float t;
    private ObjectPool pool;

    void OnEnable() => t = 0f;

    public void Play(string text, Vector3 worldPos, ObjectPool ownerPool, Transform target = null)
    {
        pool = ownerPool;
        if (tmp) tmp.text = text;
        followTarget = target;
        followOffset = worldPos - (target ? target.position : Vector3.zero);

        startPos = worldPos;
        endPos = startPos + Vector3.up * riseDistance;

        // Reset Canvas scale each time (prevents "+↵1" line breaks)
        transform.localScale = Vector3.one * 0.01f;

        SetAlpha(1f);
    }

    void Update()
    {
        t += Time.deltaTime;
        float normalized = Mathf.Clamp01(t / duration);

        // Base position: either follow the target (the hole) or stay fixed
        Vector3 basePos = followTarget ? followTarget.position + followOffset : startPos;

        // Scale factor based on hole size (same logic as font scaling)
        float scaleFactor = followTarget ? Mathf.Clamp(followTarget.localScale.x, 0.5f, 3f) : 1f;

        // Adjust rise distance according to hole size
        float rise = riseDistance * scaleFactor;

        // Smooth upward movement using cubic easing
        Vector3 currentPos = Vector3.Lerp(basePos, basePos + Vector3.up * rise, EaseOutCubic(normalized));
        transform.position = currentPos;

        // Fade out over time
        SetAlpha(1f - normalized);

        // When animation completes, return object to the pool or disable it
        if (normalized >= 1f)
        {
            if (pool) pool.Despawn(gameObject);
            else gameObject.SetActive(false);
        }
    }


    void SetAlpha(float a)
    {
        if (!tmp) return;
        var c = tmp.color;
        c.a = a;
        tmp.color = c;
    }

    float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
}