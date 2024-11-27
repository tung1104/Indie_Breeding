using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectPoolSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnItem
    {
        public string prefabName;
        public int spawnRate;
        public int maxConcurrent;
        public int maxTotal;
        public bool active;
    }

    [System.Serializable]
    public class SpawnState
    {
        public int totalSpawned;
        public int concurrentSpawned;
        public float spawnCountdown;
        public float accumulationTime;
    }

    [FormerlySerializedAs("poolSystem")] public ObjectPoolManager poolManager;
    public SpawnItem[] spawnItems;
    public float spawnPointRadius = 0.1f;
    public SpawnState[] spawnStates;

    private void Awake()
    {
        spawnStates = new SpawnState[spawnItems.Length];
        for (int i = 0; i < spawnItems.Length; i++)
        {
            spawnStates[i] = new SpawnState();
        }
    }

    private void Update()
    {
        for (int i = 0; i < spawnItems.Length; i++)
        {
            var item = spawnItems[i];
            var state = spawnStates[i];

            state.accumulationTime += Time.deltaTime;
            float fixedDeltaTime = 1.0f / Mathf.Max(item.spawnRate, 0.1f);

            while (state.accumulationTime >= fixedDeltaTime)
            {
                if ((item.maxConcurrent <= 0 || state.concurrentSpawned < item.maxConcurrent) &&
                    (item.maxTotal <= 0 || state.totalSpawned < item.maxTotal) && item.active)
                {
                    var rndUnitInsideCirce = Random.insideUnitCircle;
                    var rndPosition = transform.position + transform.rotation *
                        new Vector3(rndUnitInsideCirce.x, 0, rndUnitInsideCirce.y) * spawnPointRadius;
                    poolManager.TrySpawnInstance(item.prefabName, rndPosition, Quaternion.identity, out var obj);
                    state.concurrentSpawned++;
                    state.totalSpawned++;
                    obj.CallbackOnRelease = () => state.concurrentSpawned--;
                }

                state.accumulationTime -= fixedDeltaTime;
            }
        }
    }
}