using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class AbilityProfile : ScriptableObject
{
    public float cooldown;
    public float duration;
}

[Serializable]
public abstract class AbilityInstance
{
    public AbilityProfile abilityProfile;
    public Unit owner;
    public float cooldown;
    public AbilityPresenter lastThrownPresenter;
    public virtual bool IsReady => cooldown <= 0;
    public virtual void MarkUsed() => cooldown = abilityProfile.cooldown;
}

[Serializable]
public class AbilityInstanceStorage
{
    public List<AbilityInstance> allInstances = new();
    public List<PassiveAbilityInstance> passiveInstances = new();
    public List<ActiveAbilityInstance> activeInstances = new();
}