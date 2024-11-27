using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StandardSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>(true);
                if (_instance)
                {
                    //DontDestroyOnLoad(_instance.gameObject);
                    Debug.Log($"{typeof(T).Name} is becoming a singleton");
                    (_instance as StandardSingleton<T>).OnInitSingleton();
                }
            }
            return _instance;
        }
    }
    
    // protected virtual void Awake()
    // {
    //     if (_instance == null)
    //     {
    //         _instance = this as T;
    //         DontDestroyOnLoad(gameObject);
    //         OnInitSingleton();
    //     }
    //     else if (_instance != this)
    //         Destroy(gameObject);
    // }

    protected virtual void OnInitSingleton() { }
}