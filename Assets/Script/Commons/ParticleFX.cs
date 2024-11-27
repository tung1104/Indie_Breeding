using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ParticleFX : MonoBehaviour
{
    [SerializeField] ParticleSystem[] particleSystems;
    [SerializeField] float maxLifetime;
    [SerializeField] bool loop;

    [FormerlySerializedAs("isCounting")] public bool isStopped;

    float timeElapsed;

    Transform followTarget;
    private Pose followOffset;
    bool followTargetRotation;

    public void SetFollowTarget(Transform target, bool followRotation = false)
    {
        followTarget = target;
        followOffset = new Pose(target.InverseTransformPoint(transform.position),
            Quaternion.Inverse(target.rotation) * transform.rotation);
    }

    private void Reset()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        loop = false;
        maxLifetime = 0;
        foreach (var particle in particleSystems)
        {
            if (particle.main.loop)
                loop = true;

            // Access the main module to get lifetime properties
            var main = particle.main;

            // If startLifetime is a curve or random between two constants, get the maximum possible lifetime
            float psMaxLifetime = main.startLifetime.constantMax;
            maxLifetime = Mathf.Max(maxLifetime, psMaxLifetime);
        }
    }

    private void OnEnable()
    {
        isStopped = !loop;
    }

    private void OnDisable()
    {
        timeElapsed = 0;
        followTarget = null;
    }

    public void ForceStop()
    {
        foreach (var particle in particleSystems)
        {
            particle.Stop();
        }

        followTarget = null;
        isStopped = true;
    }

    private void Update()
    {
        if (isStopped)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= maxLifetime)
                gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (!followTarget) return;
        if (followTargetRotation)
            transform.SetPositionAndRotation(followTarget.TransformPoint(followOffset.position),
                followTarget.rotation * followOffset.rotation);
        else
            transform.position = followTarget.TransformPoint(followOffset.position);
    }
}