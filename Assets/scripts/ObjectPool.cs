using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Prefabs (pick randomly on create)")]
    public List<GameObject> prefabs = new List<GameObject>();

    [Header("Pool Settings")]
    public int initialSize = 16;
    public bool expandable = true;

    readonly Queue<GameObject> _q = new();

    void Awake()
    {
        if (prefabs == null || prefabs.Count == 0) return;

        for (int i = 0; i < Mathf.Max(0, initialSize); i++)
        {
            var go = Instantiate(PickRandomPrefab(), transform);
            go.SetActive(false);
            _q.Enqueue(go);
        }
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        if (prefabs == null || prefabs.Count == 0) return null;

        GameObject go = _q.Count > 0 ? _q.Dequeue()
                                     : (expandable ? Instantiate(PickRandomPrefab(), transform) : null);
        if (!go) return null;

        go.transform.SetPositionAndRotation(position, rotation);
        go.SetActive(true);
        return go;
    }

    public void Despawn(GameObject go)
    {
        if (!go) return;
        go.SetActive(false);
        go.transform.SetParent(transform, false);
        _q.Enqueue(go);
    }

    GameObject PickRandomPrefab()
    {
        int idx = Random.Range(0, prefabs.Count);
        return prefabs[idx];
    }
}
