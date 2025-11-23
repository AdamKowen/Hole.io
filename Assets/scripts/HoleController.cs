using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class HoleController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    
    [Header("Movement Bounds")]
    [SerializeField] private SpriteRenderer movementBounds;
    [Tooltip("Extra padding from the bounds (world units).")]
    [SerializeField] private float boundsPadding = 0f;

    [Header("Levels (simple)")]
    [Tooltip("Current hole level (1..7).")]
    [SerializeField, Range(1,7)] private int holeLevel = 1;

    [Tooltip("How much to grow (scale multiplier) on each level-up. 0.12 = +12%.")]
    [SerializeField] private float growthPerLevel = 0.12f;

    [Header("Swallowing FX")]
    [Tooltip("Seconds for the swallow animation.")]
    [SerializeField] private float swallowDuration = 0.35f;

    [Header("Blocked Visual (targets)")]
    [Tooltip("Opacity for targets we’re touching but can’t swallow yet.")]
    [SerializeField, Range(0f,1f)] private float blockedOpacity = 0.5f;

    [Header("Scoring")]
    [SerializeField] private LevelPointsTable pointsTable;
    
    [Header("Progression")]
    [SerializeField] private int maxLevel = 7;
    [SerializeField] private int[] nextLevelCostByLevel = new int[] { 0, 10, 15, 20, 25, 35, 50 };
    
    
    [Header("Floating Score")]
    [SerializeField] private ObjectPool floatingTextPool;
    [SerializeField] private Vector3 floatingTextOffset = new Vector3(0f, 0.25f, 0f);
    
    
    [Header("Control")]
    [SerializeField] private bool isFrozen = false;

    // *** NEW: Touch Control (minimal) ***
    [Header("Touch Control")]
    [SerializeField] private bool useTouchMovement = true;
    [SerializeField] private Camera gameplayCamera;

    Rigidbody2D _rb;
    Vector2 _input;
    Collider2D _triggerCol;

    // NEW: touch target data
    Vector2 _touchTarget;
    bool _hasTouchTarget = false;

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

        // camera fallback
        if (!gameplayCamera)
            gameplayCamera = Camera.main;
    }

    void Update()
    {
        if (isFrozen) return;

        // Touch input 
        if (useTouchMovement && Input.touchCount > 0 && gameplayCamera != null)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began ||
                touch.phase == TouchPhase.Moved ||
                touch.phase == TouchPhase.Stationary)
            {
                Vector3 worldPos = gameplayCamera.ScreenToWorldPoint(touch.position);
                _touchTarget = new Vector2(worldPos.x, worldPos.y);
                _hasTouchTarget = true;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                _hasTouchTarget = false;
            }
        }
        else
        {
            // No touch (or disabled) → normal keyboard input
            _hasTouchTarget = false;

            _input.x = Input.GetAxisRaw("Horizontal");
            _input.y = Input.GetAxisRaw("Vertical");
            _input.Normalize();
        }
    }

    void FixedUpdate()
    {
        if (isFrozen) return;

        Vector2 targetPos;

        // if we have a touch target, move toward it
        if (useTouchMovement && _hasTouchTarget)
        {
            targetPos = Vector2.MoveTowards(
                _rb.position,
                _touchTarget,
                moveSpeed * Time.fixedDeltaTime
            );
        }
        else
        {
            // keyboard movement
            targetPos = _rb.position + _input * (moveSpeed * Time.fixedDeltaTime);
        }

        // Clamp to movementBounds so the hole gets "stuck" at the edge
        if (movementBounds)
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

        if (holeLevel >= sw.RequiredLevel)
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

    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator SwallowRoutine(Swallowable sw)
    {
        sw.IsBeingSwallowed = true;

        SetSpriteOpacity(sw, 1f);
        _faded.Remove(sw);

        if (sw.rb)
        {
            sw.rb.linearVelocity = Vector2.zero;
            sw.rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (sw.col) sw.col.enabled = false;

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
            awardedPoints = pointsTable.GetPoints(sw.RequiredLevel);
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
        if (!_triggerCol) return Vector2.zero;
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
        if (ft && ft.Tmp != null)
        {
            // Font size relative to hole, but clamped to a max
            float baseFont = 20f;
            float scaleFactor = Mathf.Clamp(transform.localScale.x, 0.5f, 3f); // limit growth
            ft.Tmp.fontSize = baseFont * scaleFactor;

            // Adjust RectTransform to fit new font size (prevent line breaks)
            RectTransform rect = ft.Tmp.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(ft.Tmp.fontSize * 2f, ft.Tmp.fontSize * 1.2f);

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
