using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private ObjectPool pool;         
    [SerializeField] private float spawnInterval = 2f; 
    [SerializeField] private bool spawnOnStart = true; 

    float _t;

    void Start()
    {
        if (spawnOnStart) SpawnOne();
    }

    void Update()
    {
        if (!pool) return;

        _t += Time.deltaTime;
        if (_t >= spawnInterval)
        {
            _t = 0f;
            SpawnOne();
        }
    }

    public void SpawnOne()
    {
        if (!pool) return;
        pool.Spawn(transform.position, transform.rotation);
    }
}
