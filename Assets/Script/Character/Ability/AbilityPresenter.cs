using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class AbilityPresenter : MonoBehaviour
{
    public float duration = 1;
    public float fadeOutDuration = 0f;
    public float damage = 10;
    public float impactForce = 1;
    public float stunDuration = 0f;
    public RuntimeStatsBuff[] buffs;

    public AbilityInstance abInstance;
    public ActiveAbilityInstance abActiveInstance;
    public PassiveAbilityInstance abPassiveInstance;

    [FormerlySerializedAs("followParentPosition")]
    public bool followOriginPosition;

    public bool rotateTowardsDestination;

    public float presentUpdateRate = 10;

    public Transform originParent;
    public Unit destinationTarget;

    private float timer;
    private float accumulationTime;

    private ParticleSystem[] particles;

    private bool isPresenting;

    protected abstract void Appearance();
    protected abstract void Presenting(float deltaTime);
    protected abstract void Disappearance();

    protected virtual void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var particle in particles)
        {
            var main = particle.main;
            main.playOnAwake = false;
        }
    }

    protected virtual void OnEnable()
    {
        if (abInstance == null) return;
        Appearance();
        foreach (var particle in particles)
        {
            particle.Play();
        }

        abInstance.lastThrownPresenter = this;

        if (duration > 0) timer = duration;
    }

    protected virtual void OnDisable()
    {
        if (abInstance == null) return;
        Disappearance();
        abInstance.lastThrownPresenter = null;
        abInstance = null;
        abPassiveInstance = null;
        abActiveInstance = null;
        timer = 0;
        isPresenting = false;
    }

    public void Recall()
    {
        if (fadeOutDuration > 0)
        {
            timer = fadeOutDuration;
            foreach (var particle in particles)
            {
                particle.Stop();
            }
        }
        else
        {
            gameObject.SetActive(false);
            //Debug.Log("Recalled");
        }
    }

    protected virtual void Update()
    {
        if (abInstance == null) return;

        // Add to the accumulator
        accumulationTime += Time.deltaTime;
        var fixedDeltaTime = 1.0f / presentUpdateRate;

        // While enough time has passed for an update.
        while (accumulationTime >= fixedDeltaTime)
        {
            accumulationTime -= fixedDeltaTime;

            // Update the presenter
            if (timer > 0)
            {
                timer -= fixedDeltaTime;
                if (timer <= 0)
                {
                    gameObject.SetActive(false);
                    return;
                    //Debug.Log("Recalled");
                }
            }

            isPresenting = (duration > 0 && timer > fadeOutDuration) || timer <= 0;
            if (isPresenting)
                Presenting(fixedDeltaTime);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (abInstance == null) return;
    }

    protected virtual void LateUpdate()
    {
        if (abInstance == null) return;
        if (!isPresenting) return;

        if (originParent && followOriginPosition)
        {
            transform.position = originParent.position;
        }

        if (destinationTarget && rotateTowardsDestination)
        {
            transform.LookAt(destinationTarget.transform.position + destinationTarget.bounds.center);
        }
    }
}