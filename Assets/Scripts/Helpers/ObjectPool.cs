using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Queue<T> pool = new Queue<T>();
    private readonly HashSet<T> active = new HashSet<T>();
    private readonly int initialSize;

    public ObjectPool(T prefab, Transform parent = null, int initialSize = 10)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.initialSize = initialSize;

        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }

    private T CreateNewObject()
    {
        T obj = Object.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }

    public T Get()
    {
        T obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = CreateNewObject();
        }

        obj.gameObject.SetActive(true);
        active.Add(obj);
        return obj;
    }

    public void Return(T obj)
    {
        if (obj == null || !active.Contains(obj))
            return;

        obj.gameObject.SetActive(false);
        active.Remove(obj);
        pool.Enqueue(obj);
    }

    public void ReturnAll()
    {
        var activeList = new List<T>(active);
        foreach (var obj in activeList)
        {
            Return(obj);
        }
    }

    public int ActiveCount => active.Count;
    public int PooledCount => pool.Count;
    public int TotalCount => ActiveCount + PooledCount;
}
