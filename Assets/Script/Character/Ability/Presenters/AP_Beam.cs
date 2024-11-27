using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AP_Beam : AbilityPresenter
{
    public float maxDistance = 10;
    [SerializeField] ParticleSystem[] beamParticles;
    [SerializeField] ParticleSystem[] impactParticles;

    public float distance = 10;

    protected override void Appearance()
    {
        Presenting(0);
    }

    protected override void Presenting(float deltaTime)
    {
        //distance = bActiveInstance.active.castRange;

        Debug.DrawRay(transform.position, transform.forward * maxDistance, Color.red);
        if (Physics.Raycast(transform.position, transform.forward, out var hit, maxDistance))
        {
            distance = hit.distance;
            //Debug.Log($"Hit {hit.collider.name}");
            if (hit.collider.TryGetComponent(out Unit unit))
            {
                abInstance.owner.DealDamage(unit, new DamageInfo()
                {
                    damage = damage,
                    impactForce = impactForce,
                    impactDirection = transform.forward,
                    impactPoint = hit.point
                });
            }
        }

        var impactPosLoc = Vector3.down * distance;
        for (var i = 0; i < impactParticles.Length; i++)
        {
            var impact = impactParticles[i];
            if (impact.transform.localPosition != impactPosLoc)
                impact.transform.localPosition = impactPosLoc;
        }

        for (var i = 0; i < beamParticles.Length; i++)
        {
            var beam = beamParticles[i];
            var main = beam.main;
            if (main.startSizeY.constant != distance)
                main.startSizeY = distance;
        }
    }

    protected override void Disappearance()
    {
    }
}