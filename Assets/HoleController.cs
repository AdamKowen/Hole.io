using UnityEngine;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class HoleController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Swallowing")]
    [Tooltip("How big the hole must be relative to target size to allow swallowing (>= factor * target.size).")]
    public float swallowFactor = 1.05f;
    [Tooltip("How much scale to add per swallowed object (can be multiplied by target size).")]
    public float growthPerObject = 0.12f;
    [Tooltip("Seconds for the swallow animation.")]
    public float swallowDuration = 0.35f;

    private Rigidbody2D _rb;
    private Vector2 _input;

    // Cache approximate base radius from collider at scale=1 for size comparisons.
    private float _baseRadius = 0.5f; // default; will be updated from CircleCollider2D if present

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        var circle = GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            // CircleCollider2D radius is already scale-independent; we'll use it as base.
            _baseRadius = circle.radius;
        }
    }

    void Update()
    {
        // Basic WASD/Arrows movement
        _input.x = Input.GetAxisRaw("Horizontal");
        _input.y = Input.GetAxisRaw("Vertical");
        _input = _input.normalized;
    }

    void FixedUpdate()
    {
        // Kinematic rb: MovePosition for smooth movement
        Vector2 targetPos = _rb.position + _input * moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(targetPos);
    }

    // Current effective hole size (radius) considering scale.
    float CurrentHoleRadius()
    {
        return _baseRadius * transform.localScale.x;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var sw = other.GetComponent<Swallowable>();
        if (sw == null || sw.IsBeingSwallowed) return;

        float holeR = CurrentHoleRadius();
        // Allow swallow if hole is modestly larger than the object's declared size.
        if (holeR >= swallowFactor * sw.size)
        {
            StartCoroutine(SwallowRoutine(sw));
        }
        // else: too big for now → do nothing
    }

    IEnumerator SwallowRoutine(Swallowable sw)
    {
        sw.IsBeingSwallowed = true;

        // Disable physics so it won't interfere while animating into the hole.
        if (sw.rb != null) { sw.rb.linearVelocity = Vector2.zero; sw.rb.isKinematic = true; }
        if (sw.col != null) sw.col.enabled = false;

        Vector3 startPos = sw.transform.position;
        Vector3 endPos = transform.position; // pull to hole center
        Vector3 startScale = sw.transform.localScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, swallowDuration);
            // Ease-in
            float k = t * t;
            sw.transform.position = Vector3.Lerp(startPos, endPos, k);
            sw.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, k);
            yield return null;
        }

        // Destroy swallowed object
        Destroy(sw.gameObject);

        // Grow hole a bit (proportional to object size so bigger objects grow more)
        float grow = growthPerObject * Mathf.Max(0.4f, sw.size);
        transform.localScale += new Vector3(grow, grow, 0f);
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        var sw = other.GetComponent<Swallowable>();
        if (sw == null || sw.IsBeingSwallowed) return;

        float holeR = CurrentHoleRadius();
        if (holeR >= swallowFactor * sw.size)
        {
            // Pull gently toward center while inside trigger,
            // so it won't get stuck on edges even before the coroutine starts.
            Vector3 dir = (transform.position - sw.transform.position);
            float pull = 6f; // mild pull strength
            sw.transform.position += dir.normalized * pull * Time.deltaTime;

            // Kick off swallow once it's pretty close to center
            if (dir.sqrMagnitude < 0.25f) StartCoroutine(SwallowRoutine(sw));
        }
    }

}
