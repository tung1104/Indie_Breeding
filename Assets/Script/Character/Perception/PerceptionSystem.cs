using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;

[RequireComponent(typeof(RVOSimulator), typeof(AstarPath))]
public class PerceptionSystem : StandardSingleton<PerceptionSystem>
{
    [SerializeField] AstarPath astarPath;

    [Tooltip("How many perception handlers should be updated per second")] [Range(1, 60)] [SerializeField]
    int balancedUpdateRate = 60;

    public LayerMask obstacleLayerMask = 1 << 0;
    public int characterLayer = 9;

    List<PerceptionHandler> perceptionHandlers;
    float accumulationTime;
    int balancedUpdateIndex;

    RecastGraph recastGraph;

    public bool IsInitialized { get; private set; }

    protected override void OnInitSingleton()
    {
        perceptionHandlers = new List<PerceptionHandler>();
    }

    public IEnumerator Initialize()
    {
        // Creating graph during runtime
        // This holds all graph data
        var data = astarPath.data;
        // This creates a Grid Graph
        var rg = (RecastGraph)data.AddGraph(typeof(RecastGraph));
        // Setup a grid graph with some values
        rg.characterRadius = 0.5f;
        rg.walkableHeight = 2.0f;
        rg.walkableClimb = 0.5f;
        rg.cellSize = .1f;
        rg.collectionSettings.rasterizeTerrain = false;
        rg.collectionSettings.rasterizeMeshes = false;
        rg.collectionSettings.rasterizeColliders = true;
        rg.collectionSettings.layerMask = obstacleLayerMask;
        rg.SnapBoundsToScene();
        rg.forcedBoundsSize = rg.bounds.size * 1.1f;
        // Scans all graphs
        foreach (var progress in astarPath.ScanAsync())
        {
            Debug.Log("Scanning... " + progress.ToString());
            yield return null;
        }

        yield return new WaitForEndOfFrame();
        IsInitialized = true;
    }

    public void Register(PerceptionHandler perceptionHandler)
    {
        // Add the perceptionHandler to the list of registered perceptionHandlers
        perceptionHandlers.Add(perceptionHandler);
        perceptionHandler.recastGraph = recastGraph;
    }

    public void Unregister(PerceptionHandler perceptionHandler)
    {
        // Remove the perceptionHandler from the list of registered perceptionHandlers
        perceptionHandlers.Remove(perceptionHandler);
        perceptionHandler.recastGraph = null;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!astarPath && TryGetComponent(out astarPath))
        {
            astarPath.scanOnStartup = false;
            UnityEditor.EditorUtility.SetDirty(astarPath);
        }
    }
#endif

    void Update()
    {
        if (!IsInitialized) return;

        // Add to the accumulator
        accumulationTime += Time.deltaTime;
        float fixedDeltaTime = 1.0f / balancedUpdateRate * 2.0f;

        // While enough time has passed for an update.
        while (accumulationTime >= fixedDeltaTime)
        {
            if (perceptionHandlers.Count == 0)
                return;
            if (balancedUpdateIndex >= perceptionHandlers.Count)
                balancedUpdateIndex = 0;
            perceptionHandlers[balancedUpdateIndex].BalancedUpdate(fixedDeltaTime);
            balancedUpdateIndex++;
            accumulationTime -= fixedDeltaTime;
        }

        // Update the perception handlers
        for (int i = 0; i < perceptionHandlers.Count; i++)
        {
            perceptionHandlers[i].MangedUpdate(Time.deltaTime);
        }
    }
}