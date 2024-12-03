using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MovementHandler), typeof(PerceptionHandler), typeof(MotionHandler))]
public class Character : Unit
{
    public MovementHandler movement;
    public PerceptionHandler perception;
    public MotionHandler motion;
    public string projectileId;

    public Unit target, lockedTarget;

    public BipedIKHandler bipedIk;
    public Weapon leftHandWeapon, rightHandWeapon;

    private bool isAttacking, isCasting, isDamaging;

    [ContextMenu("Remove All Child Colliders")]
    void RemoveAllChildColliders()
    {
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            if (col.gameObject != gameObject)
                DestroyImmediate(col);
    }

    private void Reset()
    {
        movement = GetComponent<MovementHandler>();
        perception = GetComponent<PerceptionHandler>();
        motion = GetComponent<MotionHandler>();
    }

    private void OnValidate()
    {
        if (!perception || !movement || (perception.capsuleRadius == movement.capsuleRadius &&
                                         perception.capsuleHeight == movement.capsuleHeight)) return;
        perception.capsuleRadius = movement.capsuleRadius;
        perception.capsuleHeight = movement.capsuleHeight;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.DrawRay(Vector3.zero, Vector3.forward * (movement.capsuleRadius + defaultStatsData.attackRange));
    }

    protected override void Awake()
    {
        base.Awake();

        movement.input.moveSpeedLevel = -1;
        movement.moveSpeeds = motion.profile.GetMoveSpeedArray();
        movement.orientationSpeed = motion.profile.orientationSpeed;
        movement.acceleration = motion.profile.acceleration;
        movement.deceleration = motion.profile.deceleration;

        if (!motion.animator.TryGetComponent(out bipedIk))
            bipedIk = motion.animator.gameObject.AddComponent<BipedIKHandler>();

        motion.SubscribeOnEmit("Combat_Attack_0", OnAttackEmitEvent);
        motion.SubscribeOnEnd("Combat_Attack_0", ClearLockedTarget);
        motion.SubscribeOnEnd("Combat_Ability_0", ClearLockedTarget);
        motion.SubscribeOnEnd("Combat_Ability_1", ClearLockedTarget);
        motion.SubscribeOnEnd("Combat_Ability_2", ClearLockedTarget);

        perception.SubscribeTargetChanged((Unit t) => target = t);

        movement.BeforeCharacterUpdateCb += BeforeMovementUpdate;
        movement.AfterCharacterUpdateCb += AfterMovementUpdate;

        if (bipedIk.leftHand && bipedIk.rightHand)
        {
            leftHandWeapon = bipedIk.leftHand.GetComponentInChildren<Weapon>();
            rightHandWeapon = bipedIk.rightHand.GetComponentInChildren<Weapon>();

            StackWeaponStats(leftHandWeapon);
            StackWeaponStats(rightHandWeapon);
        }
    }

    void ClearLockedTarget()
    {
        lockedTarget = null;
    }

    void StackWeaponStats(Weapon weapon)
    {
        if (!weapon) return;
        runtimeStats.data.attackSpeed += weapon.attackSpeed;
        runtimeStats.data.attackRange += weapon.attackRange;
        Debug.Log($"Stacked weapon stats from {weapon.name} to {name}");
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        perception.team = team;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        perception.enabled = true;
        target = null;
        attackTrigger = false;
    }

    protected override void Update()
    {
        base.Update();

        if (!gameObject.activeSelf) return;

        if (!runtimeStats.isAlive ||
            ((isCasting || isAttacking || isDamaging) && motion.PlayingStateSetting.applyRootMotion))
        {
            movement.ForceSetDeltaPose(motion.animatorDeltaPose);
            movement.input.moveDirectionYaw = movement.input.lookDirectionYaw = transform.eulerAngles.y;
        }

        if (!runtimeStats.isAlive)
        {
            if (deactivateTimer > 0) return;
            if (!motion.IsPlaying("Die"))
            {
                if (!motion.Play("Die", loopTime: -2))
                {
                    deactivateTimer = .1f;
                    return;
                }

                perception.enabled = false;
                movement.SetCapsuleTrigger(true);
            }
            else if (motion.PlayingTimeNormalized > 1.5f)
            {
                deactivateTimer = 1f;
            }

            return;
        }

        if (movement.isKinematic)
            UpdateAIBehaviour(Time.deltaTime);
        else
            perception.ForceSetVelocity(movement.state.velocity);

        // Update motion play requests for combat
        isAttacking = motion.IsPlaying("Combat_Attack", true);
        isDamaging = motion.IsPlaying("Combat_Hit", true);
        isCasting = motion.IsPlaying("Combat_Ability", true);

        if (isAttacking || isDamaging || isCasting || stunCooldown > 0)
        {
            attackTrigger = false;
            castTrigger = -1;
            return;
        }

        for (var i = 0; i < abilityInstances.activeInstances.Count; i++)
        {
            var abInstance = abilityInstances.activeInstances[i];
            if (!abInstance.IsReady || silenceCooldown > 0 || movement.input.moveSpeedLevel > -1) continue;
            motion.Play(abInstance.stateName, forceLength: abInstance.active.animationForceLength);
            isCasting = true;
            lockedTarget = target;
            return;
        }

        if (!attackTrigger) return;
        attackTrigger = false;
        if (attackCooldown > 0) return;
        motion.Play("Combat_Attack_0", forceLength: 1.0f / runtimeStats.data.attackSpeed, layerIndex: 0);
        isAttacking = true;
        if (CheckTargetIsInRange())
            lockedTarget = target;
    }

    public override void TakeDamage(Unit author, DamageInfo damageInfo)
    {
        base.TakeDamage(author, damageInfo);

        // motion.Play("Combat_Hit_0", normalizedTimeOffset: Random.value * .25f, forceLength: 0.25f);
        // motion.Play("Combat_Hit_0");
        // isDamaging = true;
        if (damageInfo.impactForce > 0)
            movement.AddImpulseForce(damageInfo.impactDirection * damageInfo.impactForce);

        if (!target && author is Character character)
            perception.SetPriorityTarget(character.perception);
    }

    public override void TakeStunEffect(float duration, bool stack = false)
    {
        base.TakeStunEffect(duration, stack);

        if (bipedIk.head) stunFx.transform.position = bipedIk.head.position + Vector3.up * 0.5f;
    }

    void OnAttackEmitEvent(MotionStateSettingEvent e)
    {
        if (!lockedTarget) return;
        attackCooldown =
            motion.PlayingTimeNormalized * (1.0f / runtimeStats.data.attackSpeed); // Set cooldown based on attack speed
        var isCritical = runtimeStats.data.criticalChance > 0 && Random.value < runtimeStats.data.criticalChance;
        // if (isCritical)
        //     Debug.LogError(
        //         $"{name} emit critical hit! {runtimeStats.data.criticalChance} : {runtimeStats.data.criticalChance > 0}");
        var dmgInfo = new DamageInfo
        {
            damage = runtimeStats.data.physicalDamage * (isCritical ? runtimeStats.data.criticalDamageMultiplier : 1),
            damageType = isCritical ? DamageType.Critical : DamageType.Physical,
            impactDirection = transform.forward,
            impactForce = 0,
            impactPoint = transform.position + transform.forward * runtimeStats.data.attackRange
        };
        switch (e.eventName)
        {
            case "LHand_Shoot":
                if (leftHandWeapon)
                    FireProjectile(leftHandWeapon.projectileId, leftHandWeapon.GetCastPosition());
                else if (bipedIk.leftHand)
                    FireProjectile(projectileId, bipedIk.leftHand.position);
                break;
            case "RHand_Shoot":
                if (rightHandWeapon)
                    FireProjectile(rightHandWeapon.projectileId, rightHandWeapon.GetCastPosition());
                else if (bipedIk.rightHand)
                    FireProjectile(projectileId, bipedIk.rightHand.position);
                break;
            case "Hide_RHand_Weapon":
                rightHandWeapon.gameObject.SetActive(false);
                break;
            case "Show_RHand_Weapon":
                rightHandWeapon.gameObject.SetActive(true);
                break;
            case "DealDamage":
                if (lockedTarget)
                    lockedTarget.TakeDamage(this, dmgInfo);
                break;
        }

        lockedTarget = null;
    }

    void FireProjectile(string useProjectileId, Vector3 position)
    {
        var direction = transform.forward;
        if (CheckTargetIsInRange())
            direction = target.transform.TransformPoint(target.bounds.center) - position;
        ProjectileManager.Instance.SpawnProjectile(useProjectileId, position, direction, this);
    }

    public bool CheckTargetIsInRange(float range = 0, float fov = 0)
    {
        if (!target) return false;

        if (range == 0) range = runtimeStats.data.attackRange + movement.capsuleRadius;
        if (fov == 0) fov = 15;

        var dirToTarget = target.transform.position - transform.position;
        if (fov > 0 && Vector3.Angle(transform.forward, dirToTarget) > fov)
            return false;
        if (range > 0 && (target.transform.position - transform.position).sqrMagnitude > range * range)
            return false;

        return true;
    }

    int GetMoveSpeedLevelOf(float flatVelocityMagnitude)
    {
        if (flatVelocityMagnitude < 0.1f) return -1;
        if (flatVelocityMagnitude < movement.GetMoveSpeed(0) + 0.1f) return 0;
        return flatVelocityMagnitude < movement.GetMoveSpeed(1) + 0.1f ? 1 : 2;
    }

    void AfterMovementUpdate(float deltaTime)
    {
        // Update motion state
        motion.moveDirection = movement.input.moveDirectionYaw;
        motion.lookDirection = movement.input.lookDirectionYaw;
        var flatVelocity = Vector3.ProjectOnPlane(movement.state.baseVelocity, transform.up);
        var flatVelocityMagnitude = flatVelocity.magnitude;
        motion.moveVelocityMagnitude = flatVelocityMagnitude;
        motion.moveSpeedLevel = GetMoveSpeedLevelOf(flatVelocityMagnitude);
        motion.upVelocity = 0;
        if (!(motion.isGrounded = movement.state.isGrounded))
            motion.upVelocity = Vector3.Dot(movement.state.velocity, transform.up);
    }

    void BeforeMovementUpdate(float deltaTime)
    {
        var lookDirYaw = movement.input.moveDirectionYaw;

        if (movement.input.moveSpeedLevel > -1)
        {
            if (!runtimeStats.isAlive || isDamaging || isCasting || stunCooldown > 0)
                movement.input.moveSpeedLevel = -1;
            if (isAttacking) motion.ClearAllPlayRequests();
        }

        if (target)
        {
            if (movement.input.moveSpeedLevel < 0) //CheckTargetIsInAttackRange(checkFOV: false))
            {
                lookDirYaw = Quaternion.LookRotation(target.transform.position - transform.position).eulerAngles.y;
                movement.input.moveDirectionYaw = lookDirYaw;
                if (CheckTargetIsInRange())
                    attackTrigger = true;
            }
        }

        if (!isAttacking && !isDamaging && !isCasting && stunCooldown <= 0)
            movement.input.lookDirectionYaw = lookDirYaw;
    }

    float wanderingCountdown;
    float anchorLength = 5;
    public float anchorMaxLength = 10;
    public Transform anchor;
    public Character leader;

    void UpdateAIBehaviour(float deltaTime)
    {
        // Update perception path destination
        var desiredMoveSpeedLevel = GetMoveSpeedLevelOf(perception.DecidedMoveDelta.magnitude);

        var isOverAnchor = anchor && (transform.position - anchor.position).sqrMagnitude >
            anchorLength * anchorLength;

        if (isOverAnchor && !leader && target) isOverAnchor = false;

        if (isOverAnchor)
        {
            if (perception.RelativeDistanceToTarget(anchor.position) > anchorLength)
            {
                perception.pathDestination = anchor.position;
                perception.maxMoveSpeed = movement.GetMoveSpeed(1);
            }

            if (anchorLength > 2)
                anchorLength = Mathf.Max(anchorLength - deltaTime, 2);
        }
        else if (target)
        {
            if (CheckTargetIsInRange(0, -1) || isAttacking)
            {
                if (perception.pathDestination != default)
                    perception.ReleaseCurrentPath();
                perception.maxMoveSpeed = 0;
                desiredMoveSpeedLevel = -1; // Ensure no movement
            }
            else
            {
                perception.pathDestination = target.transform.position +
                                             (target.transform.position - transform.position).normalized;
                perception.maxMoveSpeed = movement.GetMoveSpeed(1);
            }

            if (anchor && anchorLength < anchorMaxLength)
                anchorLength = anchorMaxLength;
        }
        else
        {
            // Move wandering
            if (wanderingCountdown > 0)
            {
                wanderingCountdown -= deltaTime;
            }
            else
            {
                wanderingCountdown = Random.Range(4.0f, 8.0f);
                perception.maxMoveSpeed = movement.GetMoveSpeed(Random.Range(0, 2));
                perception.pathDestination = anchor
                    ? perception.GetRandomPoint(anchor.position + (anchor.position - transform.position).normalized,
                        anchorLength)
                    : perception.GetRandomPoint();
            }

            if (anchor && anchorLength < anchorMaxLength)
                anchorLength += deltaTime;
        }

        // Update movement input based on perception
        var input = movement.input;
        input.moveSpeedLevel = desiredMoveSpeedLevel;
        if (input.moveSpeedLevel > -1)
            input.moveDirectionYaw = Quaternion.LookRotation(perception.DecidedMoveDelta).eulerAngles.y;
        movement.input = input;
    }
}