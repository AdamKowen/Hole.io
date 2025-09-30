using UnityEngine;

public class PathFollower : MonoBehaviour
{
    [Tooltip("The path this object will follow (Prefab Instance)")]
    public Transform pathParent;      // Drag the Path prefab instance here
    public float speed = 3f;
    public float arriveThreshold = 0.05f;

    private Transform[] waypoints;
    private int index = 0;            // current waypoint
    private int dir = 1;              // +1 forward, -1 backward

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
            }
            else if (index < 0)
            {
                dir = 1;
                index = 1; // bounce to next
            }
        }
    }
}
