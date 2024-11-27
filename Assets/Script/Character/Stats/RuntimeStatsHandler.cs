using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RuntimeStatsBuffType
{
    HealthPoint,
    MaxHealthPoint,
    ManaPoint,
    MaxManaPoint,
    PhysicalDamage,
    PhysicalResistance,
    CriticalChance,
    CriticalDamage,
    CriticalResistance,
    MagicalDamage,
    MagicalResistance,
    Accuracy,
    Evasion,
    AttackSpeed,
    MovementSpeed,
}

[Serializable]
public struct RuntimeStatsBuff
{
    public RuntimeStatsBuffType type;
    public float value;
    [Range(0, 1)] public float percent;
    public float duration;
    public string effectId;

    [Tooltip("True: Áp dụng từ từ, False: Áp dụng ngay lập tức và phục hồi khi hết thời gian")]
    public bool applyOverTime;

    [HideInInspector] public float applyValueTotal;
    [HideInInspector] public float timeElapsed;
    [HideInInspector] public ParticleFX effectInstance;
}

public class RuntimeStatsHandler : MonoBehaviour
{
    [Serializable]
    public class Capacity
    {
        public float maxValue;
        public float value;

        public float Percent => value / maxValue;

        public Capacity(float maxValue)
        {
            this.maxValue = value = maxValue;
        }

        public Capacity(float maxValue, float value)
        {
            this.maxValue = maxValue;
            this.value = value;
        }
    }

    public Capacity healthPoint;
    public Capacity manaPoint;

    public float physicalDamage;
    public float physicalResistance;
    public float criticalChance;
    public float criticalDamage;
    public float criticalResistance;
    public float magicalDamage;
    public float magicalResistance;

    public float accuracy;
    public float evasion;

    public float attackSpeed;
    public float movementSpeed;

    public float experience;

    public bool isAlive;

    public List<RuntimeStatsBuff> buffs = new();

    Unit unit;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void OnEnable()
    {
        isAlive = true;
    }

    private void OnDisable()
    {
        foreach (var buff in buffs)
            if (buff.effectInstance)
                buff.effectInstance.ForceStop();
        buffs.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            AddBuff(new RuntimeStatsBuff()
            {
                type = RuntimeStatsBuffType.HealthPoint,
                value = -10,
                duration = 5,
                applyOverTime = true
            });

        var isAliveNow = healthPoint.value > 0;
        if (!isAlive.Equals(isAliveNow))
        {
            isAlive = isAliveNow;

            if (!isAlive)
                OnDisable();
        }
    }

    public void AddBuff(RuntimeStatsBuff buff)
    {
        ObjectPoolManager.Instances.TryGetValue("FXPool", out var fxPool);
        if (fxPool.TrySpawnInstance(buff.effectId, unit.transform.position + unit.bounds.center,
                transform.rotation, out buff.effectInstance))
            buff.effectInstance.SetFollowTarget(transform);
        buff.applyValueTotal = buff.value + GetCurrentStatsValue(buff.type) * buff.percent;
        buffs.Add(buff);
        if (buff.duration > 0 && !buff.applyOverTime)
        {
            // Buff có thời gian nhưng không áp dụng từ từ, chỉ áp dụng ngay lập tức
            ApplyBuffEffect(buff);
        }
    }

    private float GetCurrentStatsValue(RuntimeStatsBuffType type)
    {
        return type switch
        {
            RuntimeStatsBuffType.HealthPoint => healthPoint.value,
            RuntimeStatsBuffType.MaxHealthPoint => healthPoint.maxValue,
            RuntimeStatsBuffType.ManaPoint => manaPoint.value,
            RuntimeStatsBuffType.MaxManaPoint => manaPoint.maxValue,
            RuntimeStatsBuffType.PhysicalDamage => physicalDamage,
            RuntimeStatsBuffType.PhysicalResistance => physicalResistance,
            RuntimeStatsBuffType.CriticalChance => criticalChance,
            RuntimeStatsBuffType.CriticalDamage => criticalDamage,
            RuntimeStatsBuffType.CriticalResistance => criticalResistance,
            RuntimeStatsBuffType.MagicalDamage => magicalDamage,
            RuntimeStatsBuffType.MagicalResistance => magicalResistance,
            RuntimeStatsBuffType.Accuracy => accuracy,
            RuntimeStatsBuffType.Evasion => evasion,
            RuntimeStatsBuffType.AttackSpeed => attackSpeed,
            RuntimeStatsBuffType.MovementSpeed => movementSpeed,
            _ => 0
        };
    }

    private void FixedUpdate()
    {
        for (var i = buffs.Count - 1; i >= 0; i--)
        {
            var buff = buffs[i];

            // Áp dụng buff từ từ (tăng dần theo thời gian)
            if (buff.applyOverTime)
            {
                if (buff.timeElapsed < buff.duration)
                {
                    buff.timeElapsed += Time.fixedDeltaTime;
                    var appliedValue = buff.applyValueTotal / buff.duration * Time.fixedDeltaTime;
                    ApplyBuffEffect(buff, appliedValue);
                    buffs[i] = buff; // Cập nhật thời gian hiệu lực
                }
                else
                {
                    if (buff.effectInstance) buff.effectInstance.ForceStop();
                    buffs.RemoveAt(i); // Gỡ bỏ buff khi hết thời gian
                    continue;
                }
            }
            else
            {
                // Buff ngay lập tức chỉ cần gỡ bỏ khi hết thời gian
                if (buff.timeElapsed < buff.duration)
                {
                    buff.timeElapsed += Time.fixedDeltaTime;
                }
                else
                {
                    if (buff.effectInstance) buff.effectInstance.ForceStop();
                    RevertBuffEffect(buff);
                    buffs.RemoveAt(i); // Gỡ bỏ buff khi hết thời gian
                    continue;
                }
            }

            buffs[i] = buff;
        }
    }

    private void ApplyBuffEffect(RuntimeStatsBuff buff, float appliedValue = 0)
    {
        // Áp dụng buff tùy theo loại
        var valueToApply = appliedValue != 0 ? appliedValue : buff.applyValueTotal;

        switch (buff.type)
        {
            case RuntimeStatsBuffType.HealthPoint:
                healthPoint.value += valueToApply;
                break;
            case RuntimeStatsBuffType.MaxHealthPoint:
                healthPoint.maxValue += valueToApply;
                break;
            case RuntimeStatsBuffType.ManaPoint:
                manaPoint.value += valueToApply;
                break;
            case RuntimeStatsBuffType.MaxManaPoint:
                manaPoint.maxValue += valueToApply;
                break;
            case RuntimeStatsBuffType.PhysicalDamage:
                physicalDamage += valueToApply;
                break;
            case RuntimeStatsBuffType.PhysicalResistance:
                physicalResistance += valueToApply;
                break;
            case RuntimeStatsBuffType.CriticalChance:
                criticalChance += valueToApply;
                break;
            case RuntimeStatsBuffType.CriticalDamage:
                criticalDamage += valueToApply;
                break;
            case RuntimeStatsBuffType.CriticalResistance:
                criticalResistance += valueToApply;
                break;
            case RuntimeStatsBuffType.MagicalDamage:
                magicalDamage += valueToApply;
                break;
            case RuntimeStatsBuffType.MagicalResistance:
                magicalResistance += valueToApply;
                break;
            case RuntimeStatsBuffType.Accuracy:
                accuracy += valueToApply;
                break;
            case RuntimeStatsBuffType.Evasion:
                evasion += valueToApply;
                break;
            case RuntimeStatsBuffType.AttackSpeed:
                attackSpeed += valueToApply;
                break;
            case RuntimeStatsBuffType.MovementSpeed:
                movementSpeed += valueToApply;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void RevertBuffEffect(RuntimeStatsBuff buff)
    {
        // Gỡ bỏ buff tùy theo loại
        switch (buff.type)
        {
            case RuntimeStatsBuffType.HealthPoint:
                healthPoint.value -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.MaxHealthPoint:
                healthPoint.maxValue -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.ManaPoint:
                manaPoint.value -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.MaxManaPoint:
                manaPoint.maxValue -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.PhysicalDamage:
                physicalDamage -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.PhysicalResistance:
                physicalResistance -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.CriticalChance:
                criticalChance -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.CriticalDamage:
                criticalDamage -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.CriticalResistance:
                criticalResistance -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.MagicalDamage:
                magicalDamage -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.MagicalResistance:
                magicalResistance -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.Accuracy:
                accuracy -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.Evasion:
                evasion -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.AttackSpeed:
                attackSpeed -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.MovementSpeed:
                movementSpeed -= buff.applyValueTotal;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}