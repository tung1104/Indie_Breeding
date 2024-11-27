using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct DamageInfo
{
    public float damage;
    public float impactForce;
    [HideInInspector] public float3 impactDirection;
    [HideInInspector] public float3 impactPoint;
}

public abstract class Unit : MonoBehaviour
{
    [Header("Entity Stats")] public float maxHealth = 100;
    public AbilityProfile[] abilityProfiles;
    public Bounds bounds;
    public RuntimeStatsHandler runtimeStats;
    public bool attackTrigger;
    public int castTrigger = -1;

    SampleHealthBar healthBar;
    MeshRenderer[] meshRenderers;
    SkinnedMeshRenderer[] skinnedMeshRenderers;

    protected AbilityInstanceStorage abilityInstances;

    protected float attackCooldown, stunCooldown, silenceCooldown;

    protected ParticleFX stunFx, silenceFx;

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
    }

    protected virtual void Start()
    {
        // Register abilities
        AbilityManager.Instance.RegisterAbilities(this, abilityProfiles, out abilityInstances);
    }

    protected virtual void OnEnable()
    {
        runtimeStats.healthPoint = new RuntimeStatsHandler.Capacity(maxHealth);
    }

    protected virtual void OnDisable()
    {
        
    }

    protected virtual void Update()
    {
        if (!Mathf.Approximately(healthBar.value, runtimeStats.healthPoint.Percent))
        {
            healthBar.value = runtimeStats.healthPoint.Percent;
            healthBar.showTrigger = true;
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

        if (transform.position.y < -1.5f)
        {
            Debug.LogWarning($"Object {name} is die coz underwater");
            runtimeStats.healthPoint.value = 0;
        }
    }

    public void DealDamage(Unit target)
    {
        DealDamage(target, new DamageInfo()
        {
            damage = 10,
        });
    }

    public void DealDamage(Unit target, DamageInfo damageInfo)
    {
        target.TakeDamage(this, damageInfo);
    }

    public virtual void TakeDamage(Unit author, DamageInfo damageInfo)
    {
        runtimeStats.healthPoint.value = Mathf.Max(0, runtimeStats.healthPoint.value - damageInfo.damage);
        DamageNumberHelper.ShowDamageNumber("Damage", damageInfo.damage,
            healthBar.transform.position + Vector3.down * .5f);
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