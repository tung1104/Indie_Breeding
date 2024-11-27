using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AP_SphereArea : AbilityPresenter
{
    public float radius = 1;
    public Vector3 offsetPosition;

    protected override void Appearance()
    {
        var hits = Physics.OverlapSphere(transform.TransformPoint(offsetPosition), radius,
            1 << abInstance.owner.gameObject.layer);
        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out Unit unit) || unit == abInstance.owner) continue;
            abInstance.owner.DealDamage(unit, new DamageInfo()
            {
                damage = damage,
                impactForce = impactForce,
                impactDirection = (unit.transform.position - transform.position).normalized,
                impactPoint = unit.transform.position
            });
            unit.TakeStunEffect(stunDuration);
        }
    }

    protected override void Presenting(float deltaTime)
    {
    }

    protected override void Disappearance()
    {
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(offsetPosition), radius);
    }
}