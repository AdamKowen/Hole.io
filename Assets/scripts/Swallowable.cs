using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Swallowable : MonoBehaviour
{
    [Tooltip("Hole may swallow me if its level >= this.")]
    [Range(1,7)] public int requiredLevel = 1;

    [HideInInspector] public bool IsBeingSwallowed = false;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        // ה־Hole הוא ה־Trigger; האובייקט עצמו לא-Trigger.
        if (col != null) col.isTrigger = false;
    }
}