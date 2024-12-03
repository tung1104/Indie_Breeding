using System.Collections;
using System.Collections.Generic;
using DamageNumbersPro;
using UnityEngine;

public static class DamageNumberHelper
{
    static Dictionary<string, DamageNumber> damageNumberPrefabs;
    static DamageNumber textPrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        damageNumberPrefabs = new Dictionary<string, DamageNumber>();
        var prefabs = Resources.LoadAll<DamageNumber>("DamageNumbers");
        foreach (var prefab in prefabs)
        {
            damageNumberPrefabs.Add(prefab.name, prefab);
        }
        
        textPrefab = Resources.Load<DamageNumber>("DamageNumbers/CustomText");

        Application.quitting += UnInitialize;
    }

    static void UnInitialize()
    {
        Resources.UnloadUnusedAssets();

        damageNumberPrefabs.Clear();
        Application.quitting -= UnInitialize;
    }

    public static void ShowDamageNumber(string prefabName, float value, Vector3 position)
    {
        if (damageNumberPrefabs.TryGetValue(prefabName, out var prefab))
        {
            prefab.Spawn(position, value);
        }
    }
    
    public static void ShowCustomText(string text, Vector3 position)
    {
        textPrefab.Spawn(position, text);
    }
}