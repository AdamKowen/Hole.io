using UnityEngine;

public class PathFollower : MonoBehaviour
{
    [Tooltip("The path this object will follow (Prefab Instance)")]
    [SerializeField] private Transform pathParent;      // Drag the Path prefab instance here
    [SerializeField] private float speed = 3f;
    [SerializeField] private float arriveThreshold = 0.05f;

    [Header("Visual")]
    [Tooltip("Flip horizontally when moving backward (mirror on vertical axis).")]
    [SerializeField] private bool flipOnBacktrack = true;

    private Transform[] waypoints;
    private int index = 0;            // current waypoint
    private int dir = 1;              // +1 forward, -1 backward

    // For flipping
    SpriteRenderer _sr;
    Vector3 _baseScale;

    void Start()
    {
        if (!pathParent)
        {
            Debug.LogError("Path Parent not assigned!");
            enabled = false;
            return;
        }

        int count = pathParent.childCount;
        if (count == 0)
        {
            Debug.LogError("Path has no children/waypoints!");
            enabled = false;
            return;
        }

        waypoints = new Transform[count];
        for (int i = 0; i < count; i++)
            waypoints[i] = pathParent.GetChild(i);

        // start at first point
        transform.position = waypoints[0].position;

        // cache visual refs
        _sr = GetComponentInChildren<SpriteRenderer>();
        _baseScale = transform.localScale;

        ApplyFacing(dir);

        // if only one point, nothing to do
        if (waypoints.Length == 1) enabled = false;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Transform target = waypoints[index];

        // move toward current target
        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // arrived?
        if (Vector2.Distance(transform.position, target.position) <= arriveThreshold)
        {
            // advance along current direction
            index += dir;

            // if we hit an end, flip direction and step to the next valid point
            if (index >= waypoints.Length)
            {
                dir = -1;
                index = waypoints.Length - 2; // bounce to previous
                ApplyFacing(dir); // flipped: now moving backward
            }
            else if (index < 0)
            {
                dir = 1;
                index = 1; // bounce to next
                ApplyFacing(dir); // unflip: now moving forward
            }
        }
    }

    void ApplyFacing(int direction)
    {
        if (!flipOnBacktrack) return;

        // direction: +1 forward (no mirror), -1 backward (mirror on vertical axis)
        if (_sr)
        {
            // flip sprite horizontally when going backward
            _sr.flipX = (direction == -1);
        }
        else
        {
            // fallback: mirror by scale X
            var s = _baseScale;
            s.x = Mathf.Abs(s.x) * (direction == -1 ? -1f : 1f);
            transform.localScale = s;
        }
    }
}
