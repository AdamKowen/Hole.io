using UnityEngine;
using System.Collections;
using System.Collections.Generic; // keeping track of who we faded

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class HoleController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Levels (simple)")]
    [Tooltip("Current hole level (1..7).")]
    [Range(1,7)] public int holeLevel = 1;

    [Tooltip("How many swallows before we grow once.")]
    public int eatsPerLevel = 1;

    [Tooltip("How much to grow (scale multiplier) on each level-up. 0.12 = +12%.")]
    public float growthPerLevel = 0.12f;

    [Header("Swallowing FX")]
    [Tooltip("Seconds for the swallow animation.")]
    public float swallowDuration = 0.35f;

    [Header("Blocked Visual (targets)")]
    [Tooltip("Opacity for targets we’re touching but can’t swallow yet.")]
    [Range(0f,1f)] public float blockedOpacity = 0.5f;

    Rigidbody2D _rb;
    Vector2 _input;
    Collider2D _triggerCol;
    int _swallowedSinceLevelUp = 0;

    // Who’s currently faded (so we can bring them back on exit)
    readonly HashSet<Swallowable> _faded = new HashSet<Swallowable>();

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _triggerCol = GetComponent<Collider2D>();

        // Top-down: no gravity, no rotation
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        // The hole’s collider must be a Trigger
        if (_triggerCol != null) _triggerCol.isTrigger = true;
    }

    void Update()
    {
        // Simple WASD/Arrows input
        _input.x = Input.GetAxisRaw("Horizontal");
        _input.y = Input.GetAxisRaw("Vertical");
        _input.Normalize();
    }

    void FixedUpdate()
    {
        // Smooth physics movement
        Vector2 targetPos = _rb.position + _input * moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(targetPos);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var sw = GetSwallowableFromCollider(other);
        if (sw == null || sw.IsBeingSwallowed) return;

        // Big enough? Eat. Otherwise, fade until we leave.
        if (holeLevel >= sw.requiredLevel)
        {
            SetSpriteOpacity(sw, 1f); // make sure it’s not faded while eating
            _faded.Remove(sw);
            StartCoroutine(SwallowRoutine(sw));
        }
        else
        {
            SetSpriteOpacity(sw, blockedOpacity); // “too big” hint
            _faded.Add(sw);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var sw = GetSwallowableFromCollider(other);
        if (sw == null) return;

        // No longer touching → put its opacity back
        if (_faded.Remove(sw))
            SetSpriteOpacity(sw, 1f);
    }

    IEnumerator SwallowRoutine(Swallowable sw)
    {
        sw.IsBeingSwallowed = true;

        // Full opacity during the swallow animation
        SetSpriteOpacity(sw, 1f);
        _faded.Remove(sw);

        // Stop physics from fighting the animation
        if (sw.rb != null)
        {
            sw.rb.linearVelocity = Vector2.zero;      
            sw.rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (sw.col != null) sw.col.enabled = false;

        // Pull to center + shrink
        Vector3 startPos   = sw.transform.position;
        Vector3 endPos     = transform.position;
        Vector3 startScale = sw.transform.localScale;

        float t = 0f;
        float dur = Mathf.Max(0.01f, swallowDuration);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float k = t * t; // easy ease-in
            sw.transform.position = Vector3.Lerp(startPos, endPos, k);
            sw.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, k);
            yield return null;
        }

        Destroy(sw.gameObject);

        // Track progress and maybe level up
        _swallowedSinceLevelUp++;
        if (_swallowedSinceLevelUp >= Mathf.Max(1, eatsPerLevel))
        {
            _swallowedSinceLevelUp = 0;
            LevelUp();
        }
    }

    void LevelUp()
    {
        holeLevel = Mathf.Min(holeLevel + 1, 7);

        // Grow the hole a bit each level (e.g., +12%)
        float k = 1f + Mathf.Max(0f, growthPerLevel);
        transform.localScale = new Vector3(
            transform.localScale.x * k,
            transform.localScale.y * k,
            transform.localScale.z
        );
        // The collider scales with the transform automatically.
    }

    // Works even if the collider we hit is on a child
    static Swallowable GetSwallowableFromCollider(Collider2D col) =>
        col.GetComponentInParent<Swallowable>();

    static void SetSpriteOpacity(Swallowable sw, float a)
    {
        var sr = sw.GetComponent<SpriteRenderer>() ?? sw.GetComponentInChildren<SpriteRenderer>(true);
        if (sr == null) return;

        var c = sr.color;
        c.a = Mathf.Clamp01(a);
        sr.color = c;
    }
}
