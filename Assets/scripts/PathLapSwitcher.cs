using UnityEngine;
using System.Collections;

public class PathLapSwitcher : MonoBehaviour
{
    [Header("Path")]
    public Transform pathParent;       // Path instance with child waypoints in order
    public float speed = 3f;
    public float arriveThreshold = 0.05f;

    [Header("Loop")]
    public float dwellAtStart = 2f;    // Pause at waypoint 0 each lap
    public bool snapOnLoop = true;     // Snap to first point on loop

    [Header("Sprites (optional; one change per lap)")]
    public Sprite[] sprites;           // Cycle per lap (0,1,2,...,0)
    public bool randomStartSprite = true;

    private Transform[] _wps;
    private int _i = 0;                // waypoint index
    private bool _waitingAtStart = false;

    private SpriteRenderer _sr;
    private int _spriteIndex = 0;      // current sprite index (wraps modulo length)

    void Start()
    {
        if (!pathParent) { Debug.LogError("Path Parent not assigned!"); enabled = false; return; }

        int count = pathParent.childCount;
        if (count == 0) { Debug.LogError("Path has no children/waypoints!"); enabled = false; return; }

        _wps = new Transform[count];
        for (int i = 0; i < count; i++) _wps[i] = pathParent.GetChild(i);

        transform.position = _wps[0].position;

        _sr = GetComponentInChildren<SpriteRenderer>();
        if (_sr && sprites != null && sprites.Length > 0)
        {
            _spriteIndex = randomStartSprite ? Random.Range(0, sprites.Length) : 0;
            _sr.sprite = sprites[_spriteIndex];
        }
        else
        {
            // No sprites provided — just ensure index starts at 0
            _spriteIndex = 0;
        }

        if (_wps.Length == 1) { enabled = false; return; }

        StartCoroutine(WaitAtStartThenAdvance());
    }

    void Update()
    {
        if (_waitingAtStart || _wps == null || _wps.Length < 2) return;

        Transform target = _wps[_i];
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) <= arriveThreshold)
        {
            _i++;

            // Reached end → loop to start
            if (_i >= _wps.Length)
            {
                _i = 0;

                if (snapOnLoop)
                    transform.position = _wps[0].position;

                // Switch sprite once per lap (modulo). If none, just reset index to 0.
                AdvanceSprite();

                // Dwell at start before heading to waypoint #1
                StartCoroutine(WaitAtStartThenAdvance());
            }
        }
    }

    IEnumerator WaitAtStartThenAdvance()
    {
        _waitingAtStart = true;
        yield return new WaitForSeconds(dwellAtStart);
        _i = (_wps.Length > 1) ? 1 : 0;
        _waitingAtStart = false;
    }

    void AdvanceSprite()
    {
        if (_sr && sprites != null && sprites.Length > 0)
        {
            _spriteIndex = (_spriteIndex + 1) % sprites.Length;  // modulo wrap
            _sr.sprite = sprites[_spriteIndex];
        }
        else
        {
            // No sprites: behave safely and keep index in a valid state
            _spriteIndex = 0;
        }
    }
}
