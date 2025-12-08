using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Pooling Settings")]
    [SerializeField] protected GameObject prefab;
    [SerializeField] protected int initialSize = 10;

    protected List<GameObject> pool = new List<GameObject>();

    protected virtual void Awake()
    {
        // Pre-instantiate objects and deactivate them
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject();
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    // 🔹 Create a new pooled object
    protected virtual GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(transform);
        return obj;
    }

    // 🔹 Get an available object from the pool
    public virtual GameObject Get(Vector3 position, Quaternion rotation)
    {
        foreach (var obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
                return obj;
            }
        }

        // If none available, create a new one
        GameObject newObj = CreateNewObject();
        newObj.transform.SetPositionAndRotation(position, rotation);
        newObj.SetActive(true);
        pool.Add(newObj);
        return newObj;
    }

    // 🔹 Return an object to the pool
    public virtual void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
}
