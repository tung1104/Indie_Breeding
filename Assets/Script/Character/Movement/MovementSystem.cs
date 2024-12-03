using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : StandardSingleton<MovementSystem>
{
    public LayerMask obstacleLayerMask = 1 << 0;
    public int characterLayer = 9;
}
