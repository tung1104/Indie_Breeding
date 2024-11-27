using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Serialization;

public class ProjectileManager : StandardSingleton<ProjectileManager>
{
    [Serializable]
    public struct ProjectileBaseData
    {
        public string id;
        public string projectileName;
        public string muzzleName;
        public string impactName;
        public float moveSpeed;
        public float colliderRadius;
        public int bounces;
        public int throughTargets;
        public DamageInfo damageInfo; // DamageInfo field included
    }

    public ObjectPoolManager fxPool;
    public ProjectileBaseData[] projectileDatas;

    private NativeList<ProjectileInstance> projectiles;
    private NativeList<RaycastCommand> raycastCommands;
    private NativeList<RaycastHit> raycastHits;
    private TransformAccessArray projectileTransforms;

    private struct ProjectileInstance
    {
        public float3 startPosition;
        public float3 position;
        public float3 direction;
        public float moveSpeed;
        public float colliderRadius;
        public int bounces;
        public int throughTargets;
        public int impactNameId;
        public bool isFinished;
        public DamageInfo damageInfo;
    }

    private Action<Component, Component, DamageInfo> onHit;
    Type priorityType;

    public void SubscribeOnHit<T>(Action<T, T, DamageInfo> callback) where T : Component
    {
        priorityType = typeof(T);
        onHit = (sender, receiver, info) =>
        {
            if (sender is T senderT && receiver is T receiverT)
            {
                callback.Invoke(senderT, receiverT, info);
            }
        };
    }

    private void Start()
    {
        const int bufferSize = 500;
        projectiles = new NativeList<ProjectileInstance>(bufferSize, Allocator.Persistent);
        raycastCommands = new NativeList<RaycastCommand>(bufferSize, Allocator.Persistent);
        raycastHits = new NativeList<RaycastHit>(bufferSize, Allocator.Persistent);
        projectileTransforms = new TransformAccessArray(bufferSize);
    }

    public void SpawnProjectile(string id, Vector3 position, Vector3 direction)
    {
        ProjectileBaseData data = Array.Find(projectileDatas, x => x.id == id);
        if (string.IsNullOrEmpty(data.id))
        {
            Debug.LogWarning($"ProjectileManager: Projectile with id {id} not found");
            return;
        }

        var rotation = Quaternion.LookRotation(direction);
        fxPool.TrySpawnInstance(data.muzzleName, position, rotation, out ParticleFX muzzle);

        fxPool.TrySpawnInstance(data.projectileName, position, rotation, out ParticleFX projectile);

        var projectileInstance = new ProjectileInstance
        {
            startPosition = position,
            position = position,
            direction = direction.normalized,
            moveSpeed = data.moveSpeed,
            colliderRadius = data.colliderRadius,
            bounces = data.bounces,
            throughTargets = data.throughTargets,
            impactNameId = fxPool.GetPrefabId(data.impactName),
            damageInfo = data.damageInfo // Assign damageInfo from projectile data
        };

        projectiles.Add(projectileInstance);
        projectileTransforms.Add(projectile.transform);
        raycastHits.Add(new RaycastHit());
    }

    private void Update()
    {
        if (projectiles.Length == 0) return;

        // Prepare raycast commands after ensuring the projectile job is complete
        raycastCommands.Clear();
        for (int i = 0; i < projectiles.Length; i++)
        {
            var projectile = projectiles[i];

            // Check if the projectile if moving too far
            if (projectile.isFinished)
            {
                projectileTransforms[i].gameObject.SetActive(false);
                projectileTransforms.RemoveAtSwapBack(i);
                projectiles.RemoveAtSwapBack(i);
                raycastHits.RemoveAtSwapBack(i);
                continue;
            }

            if (projectile.colliderRadius > 0)
            {
                var perpDirection = Vector3.Cross(projectile.direction, Vector3.up).normalized;
                var rayPosition = (Vector3)projectile.position - perpDirection * projectile.colliderRadius;
                raycastCommands.Add(new RaycastCommand(rayPosition, perpDirection, projectile.colliderRadius * 2));
            }
            else
            {
                // Increase ray length to account for fast-moving projectiles
                float rayLength = projectile.moveSpeed * Time.deltaTime;
                raycastCommands.Add(new RaycastCommand(projectile.position, projectile.direction, rayLength));
            }
        }

        // Schedule and complete raycast commands
        var raycastHandle = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, 1);
        raycastHandle.Complete();

        // Process raycast results
        for (int i = 0; i < raycastHits.Length; i++)
        {
            if (raycastHits[i].collider != null)
            {
                HandleHit(i, raycastHits[i]);
            }
        }

        // Schedule the MoveProjectilesJob
        var projectileJob = new MoveProjectilesJob
        {
            deltaTime = Time.deltaTime,
            projectiles = projectiles
        };
        var moveHandle = projectileJob.Schedule(projectileTransforms);

        // Complete the moveHandle before reading from projectiles
        moveHandle.Complete();
    }

    private void HandleHit(int projectileIndex, RaycastHit hit)
    {
        if (hit.collider.isTrigger) return;

        var projectile = projectiles[projectileIndex];
        if (projectile.isFinished) return;

        // Check if the projectile hit an object that can receive damage
        if (hit.collider.TryGetComponent(priorityType, out var receiver))
        {
            // Invoke the hit event
            var dmgInfo = projectile.damageInfo;
            dmgInfo.impactDirection = projectile.direction;
            onHit?.Invoke(receiver, receiver, dmgInfo);
            fxPool.TrySpawnInstance(projectile.impactNameId, hit.point, Quaternion.LookRotation(projectile.direction),
                out var impactFX);

            projectile.throughTargets--;
            projectile.direction = Vector3.ProjectOnPlane(projectile.direction, Vector3.up).normalized;
            // projectile.position = hit.point;
            if (projectile.throughTargets <= 0)
                projectile.isFinished = true;
        }
        else if (projectile.bounces > 0)
        {
            // Decrement the bounce count and deactivate if no bounces remain
            projectile.bounces--;
            // Reflect the projectile's direction based on the hit normal
            projectile.direction = Vector3.Reflect(projectile.direction, hit.normal).normalized;
            projectileTransforms[projectileIndex].SetPositionAndRotation(hit.point,
                Quaternion.LookRotation(projectile.direction));
        }
        else
        {
            // Optionally spawn impact FX at the hit point
            fxPool.TrySpawnInstance(projectile.impactNameId, hit.point, Quaternion.LookRotation(projectile.direction),
                out var impactFX);
            projectile.isFinished = true;
        }

        // Update the projectile in the array with the new direction and bounce count
        projectiles[projectileIndex] = projectile;
    }


    [BurstCompile]
    private struct MoveProjectilesJob : IJobParallelForTransform
    {
        public float deltaTime;
        public NativeArray<ProjectileInstance> projectiles;

        public void Execute(int index, TransformAccess transform)
        {
            var projectile = projectiles[index];
            if (projectile.isFinished)
                return;
            projectile.position += deltaTime * projectile.moveSpeed * projectile.direction;
            transform.position = projectile.position;
            if (projectile.position.x < projectile.startPosition.x - 50 ||
                projectile.position.x > projectile.startPosition.x + 50 ||
                projectile.position.y < projectile.startPosition.y - 50 ||
                projectile.position.y > projectile.startPosition.y + 50 ||
                projectile.position.z < projectile.startPosition.z - 50 ||
                projectile.position.z > projectile.startPosition.z + 50)
                projectile.isFinished = true;
            projectiles[index] = projectile;
        }
    }

    private void OnDestroy()
    {
        if (projectiles.IsCreated) projectiles.Dispose();
        if (raycastCommands.IsCreated) raycastCommands.Dispose();
        if (raycastHits.IsCreated) raycastHits.Dispose();
        if (projectileTransforms.isCreated) projectileTransforms.Dispose();
    }
}