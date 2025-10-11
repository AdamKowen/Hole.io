using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Swallowable : MonoBehaviour
{
    [Tooltip("Hole may swallow me if its level >= this.")]
        [SerializeField, Range(1,7)] private int requiredLevel = 1;

        public int RequiredLevel => requiredLevel;

    [HideInInspector] public bool IsBeingSwallowed = false;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = false;
    }
}
