using Pathfinding;
using UnityEngine;

public class AstarPathController : MonoBehaviour
{
    public AstarPath astarPath;

    public void Init()
    {
    }

    public Vector3 RandomNode()
    {
        return astarPath.graphs[0].RandomPointOnSurface(NNConstraint.Walkable).position;
    }
}
