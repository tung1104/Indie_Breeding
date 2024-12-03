using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public enum RuntimeStatsBuffType
{
    HealthPoint,
    MaxHealthPoint,
    ManaPoint,
    MaxManaPoint,
    PhysicalDamage,
    PhysicalResistance,
    CriticalChance,
    CriticalDamageMultiplier,
    CriticalResistance,
    MagicalDamage,
    MagicalResistance,
    Accuracy,
    Evasion,
    AttackSpeed,
    AttackRange,
}

[Serializable]
public class RuntimeStatsData
{
    public RuntimeStatsCapacity healthPoint;
    public RuntimeStatsCapacity manaPoint;

    public float physicalDamage;
    public float physicalResistance;
    public float criticalChance;
    public float criticalDamageMultiplier;
    public float criticalResistance;
    public float magicalDamage;
    public float magicalResistance;

    public float accuracy;
    public float evasion;

    public float attackSpeed;
    public float attackRange;

    public bool ComputeHitOrMiss(float checkAccuracy)
    {
        if (evasion <= 0) return true;
        var chanceToHit = 1 - Mathf.Clamp01(evasion - checkAccuracy);
        return Random.value < chanceToHit;
    }

    public float ComputeFinalDamage(float baseDamage, DamageType damageType)
    {
        var finalDamage = baseDamage;

        switch (damageType)
        {
            case DamageType.Physical:
            case DamageType.Critical:
                // Physical Damage Reduction Formula
                var armorReduction = physicalResistance / (physicalResistance + 5);
                if (damageType == DamageType.Critical)
                    armorReduction = (physicalResistance + criticalResistance) /
                                     (physicalResistance + criticalResistance + 10);
                finalDamage = baseDamage * (1 - armorReduction);
                break;
            case DamageType.Magical:
                // Magical Damage Reduction Formula
                var effectiveReduction = magicalResistance / (1 + magicalResistance);
                finalDamage = baseDamage * (1 - effectiveReduction);
                break;
            case DamageType.Pure:
                // Pure Damage ignores resistances
                finalDamage = baseDamage;
                break;
        }

        return Mathf.Max(finalDamage, 0); // Ensure damage is non-negative
    }

    public RuntimeStatsData Clone(byte level = 0)
    {
        var clone = (RuntimeStatsData)MemberwiseClone();
        clone.healthPoint = new RuntimeStatsCapacity(healthPoint.maxValue);
        clone.manaPoint = new RuntimeStatsCapacity(manaPoint.maxValue);

        // Level scaling factor based on logarithmic growth
        var levelFactor = Mathf.Log(level + 2); // +2 avoids log(1) at level 0

        // Adjust stats based on Dota 2 scaling philosophy
        clone.healthPoint.maxValue += Mathf.RoundToInt(200 * levelFactor); // Health scales significantly with level
        clone.manaPoint.maxValue += Mathf.RoundToInt(100 * levelFactor); // Mana scales slightly less
        clone.physicalDamage += 10 * levelFactor; // Incremental physical damage growth
        clone.physicalResistance += 2 * levelFactor; // Small armor increase per level
        clone.criticalChance += 0.005f * levelFactor; // Minimal increase in crit chance
        clone.criticalDamageMultiplier += 0.01f * levelFactor; // Critical damage multiplier grows slowly
        clone.criticalResistance += 0.005f * levelFactor; // Small boost to critical resistance
        clone.magicalDamage += 8 * levelFactor; // Magic damage grows moderately
        clone.magicalResistance += 0.01f * levelFactor; // Gradual increase in magic resistance
        clone.accuracy += 0.002f * levelFactor; // Small accuracy boost
        clone.evasion += 0.002f * levelFactor; // Small evasion boost
        clone.attackSpeed += 0.01f * levelFactor; // Incremental attack speed growth

        return clone;
    }
}

[Serializable]
public struct RuntimeStatsBuff
{
    public RuntimeStatsBuffType type;
    public float value;
    [Range(0, 1)] public float percent;
    public float duration;
    public string effectId;
    public bool stack;
    public string buffId;

    [Tooltip("True: Áp dụng từ từ, False: Áp dụng ngay lập tức và phục hồi khi hết thời gian")]
    public bool applyOverTime;

    [HideInInspector] public float applyValueTotal;
    [HideInInspector] public float timeElapsed;
    [HideInInspector] public ParticleFX effectInstance;
    [HideInInspector] public Object caster;

    public void Init()
    {
        buffId = Guid.NewGuid().ToString();
    }
}

[Serializable]
public class RuntimeStatsCapacity
{
    public float maxValue;
    public float value;

    public float Normalized => value / maxValue;

    public RuntimeStatsCapacity(float maxValue, float value = 0)
    {
        this.maxValue = maxValue;
        this.value = value > 0 ? value : maxValue;
    }
}

public class RuntimeStatsHandler : MonoBehaviour
{
    public RuntimeStatsData data;

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
        RemoveAllBuffs();
    }

    void RemoveAllBuffs()
    {
        foreach (var buff in buffs)
            if (buff.effectInstance)
                buff.effectInstance.ForceStop();
        buffs.Clear();
    }

    public void ManualUpdate(float deltaTime)
    {
        var isAliveNow = data.healthPoint.value > 0;
        if (!isAlive.Equals(isAliveNow))
        {
            isAlive = isAliveNow;
            if (!isAlive)
                RemoveAllBuffs();
        }
    }

    public void AddBuff(RuntimeStatsBuff buff)
    {
        if (!isAlive) return;
        if (string.IsNullOrEmpty(buff.buffId))
        {
            Debug.LogError($"Buff {buff.effectId} has no buffId, stack = 'false' will not work");
        }
        else if (!buff.stack)
        {
            for (var i = 0; i < buffs.Count; i++)
            {
                var tmp = buffs[i];
                if (!tmp.buffId.Equals(buff.buffId)) continue;
                tmp.timeElapsed = 0;
                buffs[i] = tmp;
                return;
            }
        }

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
            RuntimeStatsBuffType.HealthPoint => data.healthPoint.value,
            RuntimeStatsBuffType.MaxHealthPoint => data.healthPoint.maxValue,
            RuntimeStatsBuffType.ManaPoint => data.manaPoint.value,
            RuntimeStatsBuffType.MaxManaPoint => data.manaPoint.maxValue,
            RuntimeStatsBuffType.PhysicalDamage => data.physicalDamage,
            RuntimeStatsBuffType.PhysicalResistance => data.physicalResistance,
            RuntimeStatsBuffType.CriticalChance => data.criticalChance,
            RuntimeStatsBuffType.CriticalDamageMultiplier => data.criticalDamageMultiplier,
            RuntimeStatsBuffType.CriticalResistance => data.criticalResistance,
            RuntimeStatsBuffType.MagicalDamage => data.magicalDamage,
            RuntimeStatsBuffType.MagicalResistance => data.magicalResistance,
            RuntimeStatsBuffType.Accuracy => data.accuracy,
            RuntimeStatsBuffType.Evasion => data.evasion,
            RuntimeStatsBuffType.AttackSpeed => data.attackSpeed,
            RuntimeStatsBuffType.AttackRange => data.attackRange,
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
                data.healthPoint.value += valueToApply;
                break;
            case RuntimeStatsBuffType.MaxHealthPoint:
                data.healthPoint.maxValue += valueToApply;
                break;
            case RuntimeStatsBuffType.ManaPoint:
                data.manaPoint.value += valueToApply;
                break;
            case RuntimeStatsBuffType.MaxManaPoint:
                data.manaPoint.maxValue += valueToApply;
                break;
            case RuntimeStatsBuffType.PhysicalDamage:
                data.physicalDamage += valueToApply;
                break;
            case RuntimeStatsBuffType.PhysicalResistance:
                data.physicalResistance += valueToApply;
                break;
            case RuntimeStatsBuffType.CriticalChance:
                data.criticalChance += valueToApply;
                break;
            case RuntimeStatsBuffType.CriticalDamageMultiplier:
                data.criticalDamageMultiplier += valueToApply;
                break;
            case RuntimeStatsBuffType.CriticalResistance:
                data.criticalResistance += valueToApply;
                break;
            case RuntimeStatsBuffType.MagicalDamage:
                data.magicalDamage += valueToApply;
                break;
            case RuntimeStatsBuffType.MagicalResistance:
                data.magicalResistance += valueToApply;
                break;
            case RuntimeStatsBuffType.Accuracy:
                data.accuracy += valueToApply;
                break;
            case RuntimeStatsBuffType.Evasion:
                data.evasion += valueToApply;
                break;
            case RuntimeStatsBuffType.AttackSpeed:
                data.attackSpeed += valueToApply;
                break;
            case RuntimeStatsBuffType.AttackRange:
                data.attackRange += valueToApply;
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
                data.healthPoint.value -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.MaxHealthPoint:
                data.healthPoint.maxValue -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.ManaPoint:
                data.manaPoint.value -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.MaxManaPoint:
                data.manaPoint.maxValue -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.PhysicalDamage:
                data.physicalDamage -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.PhysicalResistance:
                data.physicalResistance -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.CriticalChance:
                data.criticalChance -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.CriticalDamageMultiplier:
                data.criticalDamageMultiplier -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.CriticalResistance:
                data.criticalResistance -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.MagicalDamage:
                data.magicalDamage -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.MagicalResistance:
                data.magicalResistance -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.Accuracy:
                data.accuracy -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.Evasion:
                data.evasion -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.AttackSpeed:
                data.attackSpeed -= buff.applyValueTotal;
                break;
            case RuntimeStatsBuffType.AttackRange:
                data.attackRange -= buff.applyValueTotal;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}