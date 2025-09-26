using UnityEngine;

public class PathFollower : MonoBehaviour
{
    [Tooltip("The path this car will follow (Prefab Instance)")]
    public Transform pathParent;   // Drag the Path prefab instance here
    public float speed = 3f;

    private Transform[] waypoints;
    private int currentIndex = 0;
    private bool reachedEnd = false;

    void Start()
    {
        if (pathParent == null)
        {
            Debug.LogError("Path Parent not assigned!");
            return;
        }

        // Collect all children of the path as waypoints
        int count = pathParent.childCount;
        waypoints = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            waypoints[i] = pathParent.GetChild(i);
        }

        // Start from the first waypoint
        transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0 || reachedEnd) return;

        Transform target = waypoints[currentIndex];

        // Move towards the target waypoint
        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // Check if reached target
        if (Vector2.Distance(transform.position, target.position) < 0.05f)
        {
            if (currentIndex < waypoints.Length - 1)
            {
                currentIndex++;
            }
            else
            {
                reachedEnd = true; // Stop at the last point
            }
        }
    }
}
