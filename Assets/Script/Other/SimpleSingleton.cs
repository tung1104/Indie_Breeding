using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class SimpleSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _current;
    static public T Current
    {
        get
        {
            if (_current == null)
                _current = MonoBehaviour.FindObjectOfType<T>(true);
            return _current;
        }
    }
}
