using System.Collections.Generic;
using UnityEngine;

public class MonoPool<T> where T : MonoBehaviour
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly Queue<T> _pool = new();

    public int CountInactive => _pool.Count;

    public MonoPool(T prefab, Transform parent, int prewarm = 0)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < prewarm; i++)
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        T obj = _pool.Count > 0 ? _pool.Dequeue() : Object.Instantiate(_prefab, _parent);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Release(T obj)
    {
        if (obj == null) return;
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
