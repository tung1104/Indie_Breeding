using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;
using Random = UnityEngine.Random;

public class PerceptionHandler : MonoBehaviour
{
    public byte team;

    [HideInInspector] public float capsuleRadius = 0.5f;
    [HideInInspector] public float capsuleHeight = 2.0f;
    public float maxMoveSpeed = 1;

    [Tooltip("Field of view angle in degrees")]
    public float fieldOfViewAngle = 200f;

    [Tooltip("Maximum view distance")] public float viewDistance = 10f;

    [Tooltip("The position from which the object is looking (usually the head)")]
    public Transform eyesTransform;

    public Transform heartTransform;
    public bool boostUpdateNeighbours;
    public Vector3 pathDestination;

    [HideInInspector] public RecastGraph recastGraph;

    float sphereCastRadius;
    Collider[] sphereCastResults;

    RVOController rvoController;
    List<PerceptionHandler> enemies, friends;
    PerceptionHandler nearestEnemy, nearestFriend, priorityTarget;

    Seeker seeker;
    Path path;
    int currentWaypointIndex;
    bool bypassPathfindingThisFrame;

    Action<PerceptionHandler> targetChangedCallback;

    public Vector3 DecidedMoveDelta { get; private set; }

    void Awake()
    {
        const int bufferCapacity = 10;
        sphereCastResults = new Collider[bufferCapacity];

        rvoController = gameObject.AddComponent<RVOController>();
        rvoController.radius = capsuleRadius;
        rvoController.height = capsuleHeight;
        rvoController.center = capsuleHeight / 2f;
        rvoController.agentTimeHorizon = 0.5f;

        seeker = gameObject.AddComponent<Seeker>();
        seeker.startEndModifier.exactStartPoint = StartEndModifier.Exactness.Original;
        seeker.startEndModifier.exactEndPoint = StartEndModifier.Exactness.ClosestOnNode;
        //seeker.postProcessPath += OnPostProcessPath;
        var smoothModifier = gameObject.AddComponent<SimpleSmoothModifier>();
        var funnelModifier = gameObject.AddComponent<FunnelModifier>();
        var radiusModifier = gameObject.AddComponent<RadiusModifier>();
        radiusModifier.radius = capsuleRadius;
        radiusModifier.detail = 6;
        smoothModifier.uniformLength = false;
        smoothModifier.subdivisions = 1;
        smoothModifier.iterations = 1;

        enemies = new List<PerceptionHandler>(bufferCapacity);
        friends = new List<PerceptionHandler>(bufferCapacity);

        gameObject.layer = PerceptionSystem.Instance.characterLayer;

        eyesTransform = new GameObject("Eyes").transform;
        eyesTransform.SetParent(transform);
        eyesTransform.SetLocalPositionAndRotation(Vector3.up * (capsuleHeight - capsuleRadius), Quaternion.identity);

        heartTransform = new GameObject("Heart").transform;
        heartTransform.SetParent(transform);
        heartTransform.SetLocalPositionAndRotation(Vector3.up * capsuleHeight / 2f, Quaternion.identity);
    }

    void OnEnable()
    {
        PerceptionSystem.Instance.Register(this);
        rvoController.enabled = true;
    }

    void OnDisable()
    {
        if (PerceptionSystem.Instance != null)
            PerceptionSystem.Instance.Unregister(this);
        rvoController.enabled = false;

        ReleaseCurrentPath();
        DecidedMoveDelta = Vector3.zero;

        enemies.Clear();
        friends.Clear();
        nearestEnemy = null;
        nearestFriend = null;
        priorityTarget = null;
        boostUpdateNeighbours = false;
    }

    public Vector3 GetRandomPoint(float radius = 0, bool useConstantPath = false)
    {
        Vector3 randomPoint = Vector3.zero;
        if (useConstantPath)
        {
            int searchLength = Mathf.RoundToInt(radius * 2);
            ConstantPath path = ConstantPath.Construct(transform.position, searchLength);

            AstarPath.StartPath(path);
            path.BlockUntilCalculated();
            //var multipleRandomPoints = PathUtilities.GetPointsOnNodes(path.allNodes, 100);
            randomPoint = PathUtilities.GetPointsOnNodes(path.allNodes, 1)[0];
        }
        else if (radius <= 0)
        {
            // Random point in the whole graph
            var sample = AstarPath.active.graphs[0].RandomPointOnSurface(NNConstraint.Walkable);
            randomPoint = sample.position;
        }
        else
        {
            // Random point inside a circle
            var rndInsideUnitCirce = Random.insideUnitCircle * radius;
            randomPoint = transform.position + new Vector3(rndInsideUnitCirce.x, 0, rndInsideUnitCirce.y);
        }

        return randomPoint;
    }

    public void ForceSetVelocity(Vector3 velocity)
    {
        if (!rvoController.enabled) return;
        rvoController.velocity = velocity;
        bypassPathfindingThisFrame = true;
    }

    void OnPathComplete(Path p)
    {
        // Release the previous path back to the pool
        path?.Release(this);
        path = p as ABPath;
        // Claim the new path
        path.Claim(this);
        //Debug.Log($"Path calculated. Path length: {path.vectorPath.Count}");
        pathDestination = path.vectorPath[^1];
    }

    public bool HasPassedPoint(Vector3 from, Vector3 to, Vector3 minePosition)
    {
        // Calculate direction vector and squared length of the segment
        Vector3 direction = to - from;
        float segmentLengthSquared = direction.sqrMagnitude;
        // Calculate vector from 'from' to 'minePosition' and project it onto 'direction'
        Vector3 mineDirection = minePosition - from;
        float projectionLengthSquared = Vector3.Dot(mineDirection, direction);
        // Check if minePosition has passed 'to' along the 'from' -> 'to' direction
        return projectionLengthSquared > segmentLengthSquared;
    }

    public void ReleaseCurrentPath()
    {
        seeker.CancelCurrentPathRequest();
        currentWaypointIndex = 0;
        pathDestination = default;
        if (path != null)
        {
            path.Release(this);
            path = null;
        }

        rvoController.SetTarget(transform.position, 0, maxMoveSpeed,
            new Vector3(float.NaN, float.NaN, float.NaN));
    }

    void UpdatePathfinding(float deltaTime)
    {
        var isLocked = maxMoveSpeed < 0;
        if (rvoController.locked != isLocked)
            rvoController.locked = isLocked;

        // Pathfinding
        var skipDistance = rvoController.radius;
        if (path != null && path.vectorPath != null)
        {
            var pathIsValid = currentWaypointIndex < path.vectorPath.Count - 1 && pathDestination != default &&
                              Vector3.Distance(pathDestination, path.vectorPath[^1]) <= skipDistance;

            if (pathIsValid)
            {
                var from = path.vectorPath[currentWaypointIndex];
                var to = path.vectorPath[currentWaypointIndex + 1];
                Debug.DrawLine(from + transform.right * .05f, to + transform.right * .05f, Color.green);
                Debug.DrawRay(to, transform.right * .05f + Vector3.up * .1f, Color.green);
                var fromToDir = to - from;
                var distToNextPoint = fromToDir.magnitude;
                var hasPassed = HasPassedPoint(from, to, transform.position);
                if (distToNextPoint <= skipDistance || hasPassed)
                    currentWaypointIndex++;
                else
                {
                    var distToDestination = Vector3.Distance(transform.position, pathDestination);
                    rvoController.SetTarget(to + fromToDir.normalized, Mathf.Min(maxMoveSpeed, distToDestination),
                        maxMoveSpeed,
                        path.vectorPath[^1]);
                }
            }
            else
            {
                //Debug.Log($"End of path reached: {path.vectorPath.Count}");
                ReleaseCurrentPath();
            }
        }
        else if (pathDestination != default && seeker.IsDone() &&
                 Vector3.Distance(transform.position, pathDestination) > skipDistance)
        {
            seeker.StartPath(transform.position, pathDestination, OnPathComplete);
        }

        // Calculate how much to move during this frame
        // This information is based on movement commands from earlier frames
        // as local avoidance is calculated globally at regular intervals by the RVOSimulator component
        DecidedMoveDelta = rvoController.CalculateMovementDelta(transform.position, 1);
    }

    public void MangedUpdate(float deltaTime)
    {
        if (bypassPathfindingThisFrame)
            bypassPathfindingThisFrame = false;
        else
            UpdatePathfinding(deltaTime);

        if (boostUpdateNeighbours)
            UpdateNeighbors(deltaTime);
    }

    public void BalancedUpdate(float deltaTime)
    {
        if (!boostUpdateNeighbours)
            UpdateNeighbors(deltaTime);
    }

    public void SubscribeTargetChanged<T>(Action<T> callback) where T : MonoBehaviour
    {
        targetChangedCallback = (obj) =>
        {
            if (obj && obj.TryGetComponent(out T component))
                callback.Invoke(component);
            else
            {
                priorityTarget = null; // Ensure the target is null
                callback.Invoke(null);
            }
        };
    }

    void UpdateNeighbors(float deltaTime)
    {
        // Find the nearest enemy and friend
        PerceptionHandler priorityTarget = this.priorityTarget;
        nearestEnemy = null;
        nearestFriend = null;
        float distToEnemy = viewDistance, distToFriend = viewDistance;

        enemies.Clear();
        friends.Clear();

        //Debug.DrawRay(transform.position, Vector3.up * 10, Color.magenta);
        int hitsCount = Physics.OverlapSphereNonAlloc(eyesTransform.position, sphereCastRadius, sphereCastResults,
            1 << PerceptionSystem.Instance.characterLayer);
        for (int i = 0; i < hitsCount; i++)
        {
            var hit = sphereCastResults[i];
            var perceivable = hit.GetComponentInParent<PerceptionHandler>();
            if (perceivable && perceivable != this && CheckTargetIsInVision(perceivable))
            {
                float distToHit = Vector3.Distance(eyesTransform.position, perceivable.transform.position);
                if (team == 0 || perceivable.team == 0 || perceivable.team != team)
                {
                    // Is enemy
                    if (distToHit < distToEnemy)
                    {
                        distToEnemy = distToHit;
                        nearestEnemy = perceivable;

                        enemies.Insert(0, perceivable);
                    }
                    else
                        enemies.Add(perceivable);
                }
                else
                {
                    // Is friend
                    if (distToHit < distToFriend)
                    {
                        distToFriend = distToHit;
                        nearestFriend = perceivable;

                        friends.Insert(0, perceivable);
                    }
                    else
                        friends.Add(perceivable);
                }
            }
        }

        if (nearestEnemy)
        {
            sphereCastRadius = Mathf.Min(distToEnemy, viewDistance);
        }
        else
        {
            if (sphereCastRadius < viewDistance)
                sphereCastRadius = boostUpdateNeighbours
                    ? viewDistance
                    : Mathf.Min(sphereCastRadius + deltaTime * viewDistance, viewDistance);
        }

        if (nearestEnemy)
            priorityTarget = nearestEnemy;
        else if (priorityTarget && !CheckTargetIsInVision(priorityTarget))
            priorityTarget = null;

        if (this.priorityTarget != priorityTarget)
        {
            this.priorityTarget = priorityTarget;
            targetChangedCallback?.Invoke(priorityTarget);
        }
    }

    bool CheckTargetIsInVision(PerceptionHandler target)
    {
        if (!target.enabled || !target.gameObject.activeSelf) return false;

        // Check the distance between the object and the target
        Vector3 directionToTarget = target.heartTransform.position - eyesTransform.position;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget > viewDistance)
            return false; // The target is out of view distance

        // Check the angle between the forward direction and the direction to the target
        Vector3 forward = eyesTransform.forward; // The direction the object is looking
        //directionToTarget.Normalize();

        float angleToTarget = Vector3.Angle(forward, directionToTarget);

        if (!boostUpdateNeighbours && angleToTarget > fieldOfViewAngle / 2f)
            return false; // The target is outside the field of view

        // Check for obstacles using a Raycast
        if (Physics.Raycast(eyesTransform.position, directionToTarget, out RaycastHit hit, distanceToTarget,
                PerceptionSystem.Instance.obstacleLayerMask))
            return false; // There is an obstacle blocking the view of the target

        return true; // There is no obstacle blocking the view of the target
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            // Draw the field of view
            var eyePosition = eyesTransform ? eyesTransform.position : transform.position + Vector3.up;
            var eyeForward = eyesTransform ? eyesTransform.forward : transform.forward;
            Gizmos.color = Color.yellow;
            //GizmosExtensions.DrawWireFieldOfView(eyePosition, eyeForward, fieldOfViewAngle, viewDistance);

            return;
        }

        // Gizmos.color = Target != null ? Color.red : Color.green;
        // //GizmosExtensions.DrawWireFieldOfView(eyesTransform.position, eyesTransform.forward, fieldOfViewAngle, sphereCastRadius);

        // if (Target != null)
        // {
        //     //GizmosExtensions.DrawArrow(eyesTransform.position, Target.eyesTransform.position);
        // }

        // Gizmos.color = Color.red * .5f;
        // for (int i = 0; i < enemies.Count; i++)
        // {
        //     //GizmosExtensions.DrawArrow(transform.position, enemies[i].transform.position);
        // }

        // Gizmos.color = Color.green * .5f;
        // for (int i = 0; i < friends.Count; i++)
        // {
        //     //GizmosExtensions.DrawArrow(transform.position, friends[i].transform.position);
        // }
    }
}