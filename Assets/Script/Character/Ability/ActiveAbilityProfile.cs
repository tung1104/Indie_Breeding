using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveAbility", menuName = "ScriptableObjects/ActiveAbility")]
public class ActiveAbilityProfile : AbilityProfile
{
    // public enum TargetType
    // {
    //     None,
    //     Unit,
    //     Point,
    //     Area,
    // }

    public AnimationClip animationClip;
    public float animationForceLength;

    public MotionStateSettingEvent[] animationEvents;
    public bool animationApplyRootMotion;

    // public TargetType targetType;
    public float castRange;
    public float castFOV;
}

[Serializable]
public class ActiveAbilityInstance : AbilityInstance
{
    public Character ownerCharacter;
    public ActiveAbilityProfile active;
    public string stateName;

    public override bool IsReady
    {
        get
        {
            if (cooldown > 0) return false;
            return active.castRange < 0 || ownerCharacter.CheckTargetIsInRange(active.castRange, active.castFOV);
        }
    }

    public override void MarkUsed() =>
        cooldown = abilityProfile.cooldown + (active.duration > 0 ? active.duration : active.animationClip.length);
}