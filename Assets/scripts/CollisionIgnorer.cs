using UnityEngine;

public class CollisionIgnorer : MonoBehaviour
{
    public Collider2D[] colliders; 

    void Start()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            for (int j = i + 1; j < colliders.Length; j++)
            {
                Physics2D.IgnoreCollision(colliders[i], colliders[j]);
            }
        }
    }
}
