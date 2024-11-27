using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : StandardSingleton<AbilityManager>
{
    public ObjectPoolManager fxPool;

    List<AbilityInstance> abilityInstancesList;
    Dictionary<Unit, AbilityInstanceStorage> abilityInstancesDict;

    ObjectPoolManager presenterPool;

    private void Start()
    {
        TryGetComponent(out presenterPool);
        ObjectPoolManager.Instances.TryGetValue("FXPool", out fxPool);
        InitIfNeeded();
    }

    private void InitIfNeeded()
    {
        if (abilityInstancesDict != null) return;
        abilityInstancesDict = abilityInstancesDict = new Dictionary<Unit, AbilityInstanceStorage>();
        abilityInstancesList = new List<AbilityInstance>();
    }

    private void OnActiveAbilityEvent(ActiveAbilityInstance instance, MotionStateSettingEvent e)
    {
        var ownerChar = instance.ownerCharacter;
        Transform originTarget = null;
        var destinationTarget = ownerChar.target ? ownerChar.target : null;
        switch (e.eventParam2)
        {
            case "LeftHand":
                originTarget = ownerChar.bipedIk.leftHand;
                break;
            case "Head":
                originTarget = ownerChar.bipedIk.head;
                break;
            case "Mouth":
                originTarget = ownerChar.bipedIk.mouth;
                break;
            case "TargetUnit":
                if (destinationTarget) originTarget = destinationTarget.transform;
                break;
            default:
                originTarget = ownerChar.transform;
                break;
        }

        if (originTarget == null)
        {
            Debug.LogWarning($"Bone {e.eventParam2} not found on {instance.owner}");
            return;
        }

        switch (e.eventName)
        {
            case "SpawnParticle":
                SpawnParticle(e.eventParam1, originTarget);
                break;
            case "ThrowPresenter":
                instance.MarkUsed();
                if (presenterPool.TryGetReserveOf(e.eventParam1, out AbilityPresenter presenter))
                {
                    presenter.transform.SetPositionAndRotation(originTarget.position,
                        instance.ownerCharacter.transform.rotation);
                    presenter.abInstance = presenter.abActiveInstance = instance;
                    presenter.originParent = originTarget;
                    presenter.destinationTarget = destinationTarget;
                    presenter.gameObject.SetActive(true);
                }

                break;
            case "RecallPresenter":
                if (instance.lastThrownPresenter)
                    instance.lastThrownPresenter.Recall();
                break;
        }
    }

    void SpawnParticle(string fxName, Transform targetBone)
    {
        if (fxPool.TryGetReserveOf(fxName, out var fx))
        {
            fx.transform.SetParent(targetBone);
            fx.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            fx.gameObject.SetActive(true);
        }
    }

    public void RegisterAbilities(Unit owner, AbilityProfile[] abilities,
        out AbilityInstanceStorage instanceStorage)
    {
        instanceStorage = new AbilityInstanceStorage();
        if (abilities.Length == 0) return;

        InitIfNeeded();
        for (var i = 0; i < abilities.Length; i++)
        {
            var ability = abilities[i];
            AbilityInstance abiInstance;
            if (ability is PassiveAbilityProfile passive)
            {
                var tmp = new PassiveAbilityInstance { abilityProfile = passive, passive = passive };
                instanceStorage.passiveInstances.Add(tmp);
                abiInstance = tmp;
            }
            else if (ability is ActiveAbilityProfile active)
            {
                var tmp = new ActiveAbilityInstance
                    { abilityProfile = active, active = active, ownerCharacter = (Character)owner };
                instanceStorage.activeInstances.Add(tmp);
                abiInstance = tmp;
            }
            else
            {
                throw new Exception($"Unknown ability type: {ability.GetType()}");
            }

            abiInstance.cooldown = ability.cooldown;
            abiInstance.owner = owner;
            abiInstance.abilityProfile = ability;
            instanceStorage.allInstances.Add(abiInstance);
        }

        abilityInstancesDict.Add(owner, instanceStorage);
        abilityInstancesList.AddRange(instanceStorage.allInstances);
        // Debug.Log(
        //     $"Registered {abilities.Length} abilities for {owner}, total abilities: {abilityInstancesList.Count}");

        for (var i = 0; i < instanceStorage.activeInstances.Count; ++i)
        {
            var abInstance = instanceStorage.activeInstances[i];
            abInstance.stateName = $"Combat_Ability_{i}";
            abInstance.ownerCharacter.motion.SetOverrideClip(abInstance.stateName, abInstance.active.animationClip,
                new MotionStateSetting()
                {
                    stateName = abInstance.stateName,
                    events = abInstance.active.animationEvents,
                    applyRootMotion = abInstance.active.animationApplyRootMotion
                });
            abInstance.ownerCharacter.motion.SubscribeOnEmit(abInstance.stateName,
                e => { OnActiveAbilityEvent(abInstance, e); });
        }
    }

    private void Update()
    {
        for (var i = 0; i < abilityInstancesList.Count; i++)
        {
            var abiInstance = abilityInstancesList[i];
            if (!abiInstance.owner.gameObject.activeSelf) continue;

            if (abiInstance.cooldown > 0)
            {
                abiInstance.cooldown -= Time.deltaTime;
            }
        }
    }
}