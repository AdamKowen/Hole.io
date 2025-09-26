using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Swallowable : MonoBehaviour
{
    [Tooltip("Approximate size of the object (radius-equivalent) used for swallow eligibility.")]
    public float size = 1f;

    [HideInInspector] public bool IsBeingSwallowed = false;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        // Ensure collider is not trigger; we want hole's trigger to detect us.
        if (col != null) col.isTrigger = false;
    }
}