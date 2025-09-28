using UnityEngine;
using System.Collections;

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

    Rigidbody2D _rb;
    Vector2 _input;
    Collider2D _triggerCol;
    int _swallowedSinceLevelUp = 0;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _triggerCol = GetComponent<Collider2D>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        // חשוב: רק קוליידר אחד על החור, והוא Trigger.
        if (_triggerCol != null) _triggerCol.isTrigger = true;
    }

    void Update()
    {
        _input.x = Input.GetAxisRaw("Horizontal");
        _input.y = Input.GetAxisRaw("Vertical");
        _input.Normalize();
    }

    void FixedUpdate()
    {
        Vector2 targetPos = _rb.position + _input * moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(targetPos);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var sw = other.GetComponent<Swallowable>();
        if (sw == null || sw.IsBeingSwallowed) return;

        // תנאי יחיד: לפי Level
        if (holeLevel >= sw.requiredLevel)
            StartCoroutine(SwallowRoutine(sw));
    }

    IEnumerator SwallowRoutine(Swallowable sw)
    {
        sw.IsBeingSwallowed = true;

        if (sw.rb != null)
        {
            sw.rb.linearVelocity = Vector2.zero;
            sw.rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (sw.col != null) sw.col.enabled = false;

        Vector3 startPos = sw.transform.position;
        Vector3 endPos = transform.position;
        Vector3 startScale = sw.transform.localScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, swallowDuration);
            float k = t * t; // ease-in
            sw.transform.position = Vector3.Lerp(startPos, endPos, k);
            sw.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, k);
            yield return null;
        }

        Destroy(sw.gameObject);

        // ספר בליעה, ו-LevelUp בתדירות שבחרת
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
        float k = 1f + Mathf.Max(0f, growthPerLevel); // e.g. 0.12 => +12%
        transform.localScale = new Vector3(
            transform.localScale.x * k,
            transform.localScale.y * k,
            transform.localScale.z
        );
        // הקוליידר יתעדכן אוטומטית עם הסקייל (Circle/Polygon וכו').
    }
}
