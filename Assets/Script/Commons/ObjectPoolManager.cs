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
        public PoolObject prefab;
        public List<PoolObject> instances;
        public List<PoolObject> reserveInstances;
    }

    [SerializeField] GameObject[] prefabs;
    [SerializeField] string resourcePath;

    IndexedDictionary<string, Pool> pools;

    private void Awake()
    {
        Instances.Add(name, this);

        pools = new IndexedDictionary<string, Pool>();

        var prefabList = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
            prefabList.Add(transform.GetChild(i).gameObject);

        if (!string.IsNullOrEmpty(resourcePath))
            foreach (var obj in Resources.LoadAll<GameObject>(resourcePath))
            {
                if (!prefabList.Exists(x => x.name == obj.name))
                    prefabList.Add(obj);
            }

        prefabs = prefabList.ToArray();

        for (int i = 0; i < prefabs.Length; i++)
        {
            var prefab = prefabs[i];
            prefab.SetActive(false);

            var pool = new Pool()
            {
                name = prefabs[i].name,
                prefab = prefabs[i].AddComponent<PoolObject>(),
                instances = new List<PoolObject>(),
                reserveInstances = new List<PoolObject>()
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
                return true;
            }
            else
            {
                instance = Instantiate(pool.prefab, transform);
                pool.reserveInstances.Add(instance);
                instance.Pool = pool;
                return true;
            }
        }

        instance = null;
        return false;
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
            for (int i = pool.instances.Count - 1; i >= 0; i--)
                pool.instances[i].gameObject.SetActive(false);
        }
    }

    public void ReleaseAllInstances(string name)
    {
        if (pools.TryGetValue(name, out Pool pool))
        {
            for (int i = pool.instances.Count - 1; i >= 0; i--)
                pool.instances[i].gameObject.SetActive(false);
        }
    }
}

public class PoolObject : MonoBehaviour
{
    internal ObjectPoolManager.Pool Pool;

    public Action CallbackOnRelease;

    private void OnEnable()
    {
        Pool.instances.Add(this);
        Pool.reserveInstances.Remove(this);
    }

    private void OnDisable()
    {
        Pool.instances.Remove(this);
        Pool.reserveInstances.Add(this);
        CallbackOnRelease?.Invoke();
        CallbackOnRelease = null;
    }
}