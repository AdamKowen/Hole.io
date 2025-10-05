using UnityEngine;
using System.Collections;
using System.Collections.Generic; // keeping track of who we faded

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class HoleController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    
    [Header("Movement Bounds")]
    public SpriteRenderer movementBounds;
    [Tooltip("Extra padding from the bounds (world units).")]
    public float boundsPadding = 0f;

    [Header("Levels (simple)")]
    [Tooltip("Current hole level (1..7).")]
    [Range(1,7)] public int holeLevel = 1;

    [Tooltip("How much to grow (scale multiplier) on each level-up. 0.12 = +12%.")]
    public float growthPerLevel = 0.12f;

    [Header("Swallowing FX")]
    [Tooltip("Seconds for the swallow animation.")]
    public float swallowDuration = 0.35f;

    [Header("Blocked Visual (targets)")]
    [Tooltip("Opacity for targets we’re touching but can’t swallow yet.")]
    [Range(0f,1f)] public float blockedOpacity = 0.5f;

    [Header("Scoring")]
    public LevelPointsTable pointsTable;
    
    [Header("Progression")]
    public int maxLevel = 7;
    public int[] nextLevelCostByLevel = new int[] { 0, 10, 15, 20, 25, 35, 50 };
    
    
    [Header("Floating Score")]
    public ObjectPool floatingTextPool;
    public Vector3 floatingTextOffset = new Vector3(0f, 0.25f, 0f);
    
    
    [Header("Control")]
    public bool isFrozen = false;

    Rigidbody2D _rb;
    Vector2 _input;
    Collider2D _triggerCol;

    // Points accumulated since last level-up (level up every +10 points)
    int _pointsSinceLevelUp = 0;

    // Who's currently faded (so we can bring them back on exit)
    readonly HashSet<Swallowable> _faded = new HashSet<Swallowable>();

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _triggerCol = GetComponent<Collider2D>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        if (_triggerCol != null) _triggerCol.isTrigger = true;
    }

    void Update()
    {
        if (isFrozen) return;
        _input.x = Input.GetAxisRaw("Horizontal");
        _input.y = Input.GetAxisRaw("Vertical");
        _input.Normalize();
    }

    void FixedUpdate()
    {
        if (isFrozen) return;
        Vector2 targetPos = _rb.position + _input * moveSpeed * Time.fixedDeltaTime;

        // NEW: clamp to movementBounds so the hole gets "stuck" at the edge
        if (movementBounds != null)
        {
            Bounds b = movementBounds.bounds;
            Vector2 half = GetHalfSizeWorld() + Vector2.one * boundsPadding;

            float minX = b.min.x + half.x;
            float maxX = b.max.x - half.x;
            float minY = b.min.y + half.y;
            float maxY = b.max.y - half.y;

            // In case the hole becomes larger than the bounds in any axis
            if (minX > maxX) { float midX = (b.min.x + b.max.x) * 0.5f; minX = maxX = midX; }
            if (minY > maxY) { float midY = (b.min.y + b.max.y) * 0.5f; minY = maxY = midY; }

            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }

        _rb.MovePosition(targetPos);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        var sw = GetSwallowableFromCollider(other);
        if (sw == null || sw.IsBeingSwallowed) return;

        if (holeLevel >= sw.requiredLevel)
        {
            SetSpriteOpacity(sw, 1f);
            _faded.Remove(sw);
            StartCoroutine(SwallowRoutine(sw));
        }
        else
        {
            SetSpriteOpacity(sw, blockedOpacity);
            _faded.Add(sw);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var sw = GetSwallowableFromCollider(other);
        if (sw == null) return;

        if (_faded.Remove(sw))
            SetSpriteOpacity(sw, 1f);
    }

    IEnumerator SwallowRoutine(Swallowable sw)
    {
        sw.IsBeingSwallowed = true;

        SetSpriteOpacity(sw, 1f);
        _faded.Remove(sw);

        if (sw.rb != null)
        {
            sw.rb.linearVelocity = Vector2.zero;
            sw.rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (sw.col != null) sw.col.enabled = false;

        Vector3 startPos   = sw.transform.position;
        Vector3 endPos     = transform.position;
        Vector3 startScale = sw.transform.localScale;

        float t = 0f;
        float dur = Mathf.Max(0.01f, swallowDuration);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float k = t * t;
            sw.transform.position = Vector3.Lerp(startPos, endPos, k);
            sw.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, k);
            yield return null;
        }

        int awardedPoints = 0;
        if (ScoreManager.Instance != null && pointsTable != null)
        {
            awardedPoints = pointsTable.GetPoints(sw.requiredLevel);
            ScoreManager.Instance.AddPoints(awardedPoints);
            SpawnFloatingScore(awardedPoints);
        }

        Destroy(sw.gameObject);

        _pointsSinceLevelUp += awardedPoints;
        TryLevelUps();
    }

    
    void TryLevelUps()
    {
        while (holeLevel < maxLevel)
        {
            int cost = GetCostForLevel(holeLevel);
            if (_pointsSinceLevelUp < cost) break;
            _pointsSinceLevelUp -= cost;
            LevelUp();
        }
    }

    int GetCostForLevel(int lvl)
    {
        if (nextLevelCostByLevel == null || lvl <= 0 || lvl >= nextLevelCostByLevel.Length)
            return 10; // fallback
        return Mathf.Max(1, nextLevelCostByLevel[lvl]);
    }

    
    
    void LevelUp()
    {
        holeLevel = Mathf.Min(holeLevel + 1, maxLevel);
        float k = 1f + Mathf.Max(0f, growthPerLevel);
        transform.localScale = new Vector3(
            transform.localScale.x * k,
            transform.localScale.y * k,
            transform.localScale.z
        );
    }


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
    
    // Helper to get half-size of the hole in world units (uses collider bounds)
    Vector2 GetHalfSizeWorld()
    {
        if (_triggerCol == null) return Vector2.zero;
        var e = _triggerCol.bounds.extents; // world-space half-size
        return new Vector2(e.x, e.y);
    }
    
    
    void SpawnFloatingScore(int awardedPoints)
    {
        if (!floatingTextPool) return;

        Vector3 spawnPos = transform.position + floatingTextOffset;
        var go = floatingTextPool.Spawn(spawnPos, Quaternion.identity);
        if (!go) return;

        var ft = go.GetComponent<FloatingText>();
        if (ft && ft.tmp != null)
        {
            // Font size relative to hole, but clamped to a max
            float baseFont = 20f;
            float scaleFactor = Mathf.Clamp(transform.localScale.x, 0.5f, 3f); // limit growth
            ft.tmp.fontSize = baseFont * scaleFactor;

            // Adjust RectTransform to fit new font size (prevent line breaks)
            RectTransform rect = ft.tmp.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(ft.tmp.fontSize * 2f, ft.tmp.fontSize * 1.2f);

            // Pass hole transform to make text follow it slightly
            ft.Play("+" + awardedPoints.ToString(), spawnPos, floatingTextPool, transform);
        }
        
    }

    
    public void Freeze(bool value)
    {
        isFrozen = value;

        // kill any residual input / velocity immediately
        _input = Vector2.zero;
        if (_rb) _rb.linearVelocity = Vector2.zero;
    }


}
