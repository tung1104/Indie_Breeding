using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static IndexedDictionary<string, ObjectPoolManager> Instances { get; private set; } = new();

    internal class Pool
    {
        public string name;
        public GameObject prefab;
        public List<PoolObject> activeInstances;
        public List<PoolObject> reserveInstances;
    }

    [SerializeField] GameObject[] prefabs;
    [SerializeField] string resourcePath;

    IndexedDictionary<string, Pool> pools;
    List<PoolObject> allInstances;

    private void Awake()
    {
        Instances.Add(name, this);

        pools = new IndexedDictionary<string, Pool>();
        allInstances = new List<PoolObject>();

        var prefabList = new List<GameObject>();

        for (var i = 0; i < transform.childCount; i++)
            prefabList.Add(transform.GetChild(i).gameObject);

        if (!string.IsNullOrEmpty(resourcePath))
            foreach (var obj in Resources.LoadAll<GameObject>(resourcePath))
            {
                if (!prefabList.Exists(x => x.name == obj.name))
                    prefabList.Add(obj);
            }

        foreach (var prefab in prefabList)
        {
            if (prefab.scene.rootCount > 0)
                prefab.SetActive(false);

            var pool = new Pool()
            {
                name = prefab.name,
                prefab = prefab,
                activeInstances = new List<PoolObject>(),
                reserveInstances = new List<PoolObject>(),
            };

            pools.Add(pool.name, pool);
        }
    }

    public int GetPrefabId(string name)
    {
        return pools.IndexOfKey(name);
    }

    public bool TryGetReserveOf(string name, out PoolObject instance)
    {
        if (pools.TryGetValue(name, out Pool pool))
        {
            if (pool.reserveInstances.Count > 0)
            {
                instance = pool.reserveInstances[0];
            }
            else
            {
                var obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                instance = obj.AddComponent<PoolObject>();
                instance.pool = pool;
                pool.reserveInstances.Add(instance);

                instance.index = allInstances.Count;
                allInstances.Add(instance);
            }

            return true;
        }

        instance = null;
        return false;
    }

    public PoolObject GetInstanceByIndex(int index)
    {
        return index > -1 && index < allInstances.Count ? allInstances[index] : null;
    }

    public bool TryGetReserveOf<T>(string name, out T instance) where T : MonoBehaviour
    {
        if (TryGetReserveOf(name, out PoolObject poolObject))
            return poolObject.TryGetComponent(out instance);
        instance = null;
        return false;
    }

    public bool TryGetReserveOf(int index, out PoolObject instance)
    {
        instance = null;
        return pools.TryGetKeyByIndex(index, out var key) && TryGetReserveOf(key, out instance);
    }

    public bool TryGetReserveOf<T>(int index, out T instance) where T : MonoBehaviour
    {
        if (TryGetReserveOf(index, out PoolObject poolObject))
            return poolObject.TryGetComponent(out instance);
        instance = null;
        return false;
    }

    public bool TrySpawnInstance(int index, Vector3 position, Quaternion rotation, out PoolObject instance)
    {
        instance = null;
        return pools.TryGetKeyByIndex(index, out var key) && TrySpawnInstance(key, position, rotation, out instance);
    }

    public bool TrySpawnInstance<T>(int index, Vector3 position, Quaternion rotation, out T instance)
        where T : MonoBehaviour
    {
        if (TrySpawnInstance(index, position, rotation, out PoolObject poolObject))
            return poolObject.TryGetComponent(out instance);
        instance = null;
        return false;
    }

    public bool TrySpawnInstance(string name, Vector3 position, Quaternion rotation, out PoolObject instance)
    {
        if (TryGetReserveOf(name, out instance))
        {
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            return true;
        }

        return false;
    }

    public bool TrySpawnInstance<T>(string name, Vector3 position, Quaternion rotation, out T instance)
        where T : MonoBehaviour
    {
        if (TrySpawnInstance(name, position, rotation, out PoolObject poolObject))
            return poolObject.TryGetComponent(out instance);
        instance = null;
        return false;
    }

    [ContextMenu("Release All Instances")]
    public void ReleaseAllInstances()
    {
        foreach (var pool in pools.Values)
        {
            for (int i = pool.activeInstances.Count - 1; i >= 0; i--)
                pool.activeInstances[i].gameObject.SetActive(false);
        }
    }

    public void ReleaseAllInstances(string name)
    {
        if (pools.TryGetValue(name, out Pool pool))
        {
            for (int i = pool.activeInstances.Count - 1; i >= 0; i--)
                pool.activeInstances[i].gameObject.SetActive(false);
        }
    }
}

public class PoolObject : MonoBehaviour
{
    internal ObjectPoolManager.Pool pool;
    internal int index;
    internal int activeIndex;

    public Action CallbackOnRelease { get; set; }

    private void OnEnable()
    {
        pool.activeInstances.Add(this);
        pool.reserveInstances.Remove(this);
    }

    private void OnDisable()
    {
        pool.activeInstances.Remove(this);
        pool.reserveInstances.Add(this);
        CallbackOnRelease?.Invoke();
        CallbackOnRelease = null;
    }
}