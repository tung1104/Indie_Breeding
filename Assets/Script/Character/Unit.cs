using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public enum DamageType
{
    Physical,
    Critical,
    Magical,
    Pure,
}

[Serializable]
public struct DamageInfo
{
    public float damage;
    public float impactForce;
    public DamageType damageType;

    [HideInInspector] public float3 impactDirection;
    [HideInInspector] public float3 impactPoint;
}

public abstract class Unit : MonoBehaviour
{
    public Bounds bounds;

    [Header("Default Stats, Abilities")] public byte team;

    public byte level;

    public RuntimeStatsData defaultStatsData = new()
    {
        healthPoint = new RuntimeStatsCapacity(100),
        physicalDamage = 1,
        attackSpeed = 1,
        attackRange = 1,
    };

    [SerializeField] private AbilityProfile[] abilityProfiles;

    [Header("Runtime Variables")] public RuntimeStatsHandler runtimeStats;
    public bool attackTrigger;
    public int castTrigger = -1;

    SampleHealthBar healthBar;
    MeshRenderer[] meshRenderers;
    SkinnedMeshRenderer[] skinnedMeshRenderers;

    protected AbilityInstanceStorage abilityInstances;

    protected float attackCooldown, stunCooldown, silenceCooldown;
    protected float deactivateTimer;

    protected ParticleFX stunFx, silenceFx;

    public bool CheckIsEnemy(Unit target)
    {
        return CheckIsEnemy(target.team);
    }

    public bool CheckIsEnemy(byte targetTeam)
    {
        return team == 0 || targetTeam == 0 || targetTeam != team;
    }

    protected virtual void Awake()
    {
        healthBar = Instantiate(Resources.Load<SampleHealthBar>("SampleHealthBar"), transform.position,
            transform.rotation, transform);
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        if (bounds == default)
        {
            var pose = new Pose(transform.position, transform.rotation);
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
                bounds.Encapsulate(skinnedMeshRenderer.bounds);
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        healthBar.transform.localPosition = Vector3.up * (bounds.center.y + bounds.extents.y + 1);

        runtimeStats = gameObject.AddComponent<RuntimeStatsHandler>();
        runtimeStats.data = defaultStatsData.Clone(level);
    }

    protected virtual void Start()
    {
        // Register abilities
        AbilityManager.Instance.RegisterAbilities(this, abilityProfiles, out abilityInstances);
    }

    protected virtual void OnEnable()
    {
        // Reset runtime stats
        runtimeStats.data.healthPoint.value = runtimeStats.data.healthPoint.maxValue;
        runtimeStats.isAlive = true;
    }

    protected virtual void OnDisable()
    {
        attackCooldown = stunCooldown = silenceCooldown = 0;
    }

    protected virtual void Update()
    {
        runtimeStats.ManualUpdate(Time.deltaTime);
        if (!Mathf.Approximately(healthBar.value, runtimeStats.data.healthPoint.Normalized))
        {
            healthBar.value = runtimeStats.data.healthPoint.Normalized;
            healthBar.showTrigger = true;
        }

        if (deactivateTimer > 0)
        {
            deactivateTimer -= Time.deltaTime;
            attackCooldown = stunCooldown = silenceCooldown = 0;

            if (deactivateTimer <= 0)
            {
                gameObject.SetActive(false);
            }
        }
        else if (runtimeStats.isAlive)
        {
            if (transform.position.y < -1.5f)
            {
                Debug.LogWarning($"Object {name} is die coz underwater");
                runtimeStats.data.healthPoint.value = 0;
            }
        }

        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }

        if (stunCooldown > 0)
        {
            stunCooldown -= Time.deltaTime;
        }
        else if (stunFx)
        {
            stunFx.gameObject.SetActive(false);
            stunFx = null;
        }

        if (silenceCooldown > 0)
        {
            silenceCooldown -= Time.deltaTime;
        }
    }

    public virtual void TakeDamage(Unit author, DamageInfo damageInfo)
    {
        // Calculate final damage using the logarithmic formula
        var statsData = runtimeStats.data;

        // Check if the target has evasion
        if (!statsData.ComputeHitOrMiss(author ? author.runtimeStats.data.accuracy : 0))
        {
            DamageNumberHelper.ShowCustomText("<size=4>EVADE</size>",
                (Vector3)damageInfo.impactPoint + Vector3.up * .5f);
            return;
        }

        // Apply the logarithmic formula for damage calculation
        var finalDamage = statsData.ComputeFinalDamage(damageInfo.damage, damageInfo.damageType);

        // Apply damage to health point
        runtimeStats.data.healthPoint.value = Mathf.Max(0, runtimeStats.data.healthPoint.value - finalDamage);

        // Show damage number
        var dmgPrefabName = damageInfo.damageType switch
        {
            DamageType.Physical => "PhysicalDamage",
            DamageType.Critical => "CriticalDamage",
            DamageType.Magical => "MagicalDamage",
            DamageType.Pure => "PureDamage",
            _ => "PhysicalDamage",
        };
        DamageNumberHelper.ShowDamageNumber(dmgPrefabName, finalDamage,
            healthBar.transform.position + Vector3.up * .5f);
    }

    public virtual void TakeStunEffect(float duration, bool stack = false)
    {
        stunCooldown = stack ? stunCooldown + duration : Mathf.Max(stunCooldown, duration);
        if (stunFx == null)
        {
            ObjectPoolManager.Instances.TryGetValue("FXPool", out var pool);
            pool.TryGetReserveOf("HCFX_Stun", out stunFx);
            stunFx.transform.SetParent(transform);
            stunFx.transform.localPosition = healthBar.transform.localPosition + Vector3.down * .5f;
            stunFx.gameObject.SetActive(true);
        }
    }

    public virtual void TakeSilenceEffect(float duration, bool stack = false)
    {
        silenceCooldown = stack ? silenceCooldown + duration : Mathf.Max(silenceCooldown, duration);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}