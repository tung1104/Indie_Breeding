using System.Collections.Generic;
using UnityEngine;

public class WildAreaController : MonoBehaviour
{
    public static WildAreaController Current;

    public List<WildArea> WildAreas = new();

    private void Awake()
    {
        Current = this;
    }
}
