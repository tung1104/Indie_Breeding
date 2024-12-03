using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "MotionProfile", menuName = "ScriptableObjects/Motion Profile")]
public class MotionProfile : ScriptableObject
{
    [Serializable]
    public struct MoveLevel
    {
        public float moveSpeed;
        public float moveSpeedMultiplier;
    }

    public MoveLevel[] moveLevels = new MoveLevel[3] {
        new() { moveSpeed = 1, moveSpeedMultiplier = 1 },
        new() { moveSpeed = 2, moveSpeedMultiplier = 1 },
        new() { moveSpeed = 3, moveSpeedMultiplier = 1 }
    };

    public bool correctSpeedWithScale = true;
    public float orientationSpeed = 5;
    public float acceleration = 5;
    public float deceleration = 5;

    public MotionStateClips clips;
    public MotionStateSetting[] settings;

    public float minMoveVelocityMagnitude;
    public float moveTransitionDuration;

    AnimatorStateData animatorStateData;

    IndexedDictionary<string, MotionStateSetting> settingsDictionary;

    public bool TryGetStateSetting(string stateName, out MotionStateSetting setting)
    {
        return settingsDictionary.TryGetValue(stateName, out setting);
    }
    
    public bool TryGetGroupedStateNames(string stateName, out List<string> groupedStateNames)
    {
        return animatorStateData.groupedStateNamesDict.TryGetValue(stateName, out groupedStateNames);
    }
    
    public float GetMoveSpeedMultiplier(int level)
    {
        return level < moveLevels.Length ? moveLevels[level].moveSpeedMultiplier : 1f;
    }

    public float[] GetMoveSpeedArray()
    {
        float[] moveSpeeds = new float[moveLevels.Length];
        for (int i = 0; i < moveLevels.Length; i++)
        { moveSpeeds[i] = moveLevels[i].moveSpeed; }
        return moveSpeeds;
    }

    private void OnDisable()
    {
        animatorStateData = null;
    }

    void InitIfNeeded()
    {
        if (animatorStateData != null) return;
        animatorStateData = Resources.Load<AnimatorStateData>("AnimatorStateData");
        //Debug.Log($"Init Motion Profile {name}");

        settingsDictionary = new IndexedDictionary<string, MotionStateSetting>();
        foreach (var setting in settings)
            settingsDictionary.Add(setting.stateName, setting);
    }

    public int GetStateNameHash(string stateName)
    {
        int index = animatorStateData.stateNames.IndexOf(stateName);
        return animatorStateData.stateHashes[index];
    }

    public int GetParamNameHash(string paramName)
    {
        int index = animatorStateData.parameterNames.IndexOf(paramName);
        return animatorStateData.parameterHashes[index];
    }

    public AnimatorOverrideController GetNewOverrideController()
    {
        InitIfNeeded();
        var animatorController = new AnimatorOverrideController
        { runtimeAnimatorController = animatorStateData.animatorController };
        foreach (var clip in clips.GetType().GetFields())
        { animatorController[clip.Name] = clips.GetType().GetField(clip.Name).GetValue(clips) as AnimationClip; }
        return animatorController;
    }
}

[Serializable]
public struct MotionStateSetting
{
    public string stateName;
    public MotionStateSettingEvent[] events;
    public bool applyRootMotion;
}

[Serializable]
public struct MotionStateSettingEvent
{
    public string eventName;
    [Range(0, 1)]
    public float normalizedTime;
    public string eventParam1;
    public string eventParam2;
}


[Serializable]
public struct MotionStateClips
{
    [Header("[GENERAL]")]
    public AnimationClip Idle;
    public AnimationClip Jump;
    public AnimationClip Fall;
    public AnimationClip Land;
    public AnimationClip Die;

    [Header("[LOCOMOTION]")]
    [Header("Walk Straight")]
    public AnimationClip Walk_Forward;
    public AnimationClip Walk_Backward;
    public AnimationClip Walk_StrafeLeft;
    public AnimationClip Walk_StrafeRight;

    // [Header("Walk Diagonal")]
    // public AnimationClip Walk_CrossLeftBack;
    // public AnimationClip Walk_CrossRightBack;
    // public AnimationClip Walk_CrossLeftFront;
    // public AnimationClip Walk_CrossRightFront;

    //[Header("Walk Turn")]
    public AnimationClip Walk_Fwd_TurnLeft_90;
    public AnimationClip Walk_Fwd_TurnRight_90;
    // public AnimationClip Walk_Bwd_TurnLeft_90;
    // public AnimationClip Walk_Bwd_TurnRight_90;
    
    [Header("Run Straight")]
    public AnimationClip Run_Forward;
    public AnimationClip Run_Backward;
    public AnimationClip Run_StrafeLeft;
    public AnimationClip Run_StrafeRight;

    // [Header("Run Diagonal")]
    // public AnimationClip Run_CrossLeftBack;
    // public AnimationClip Run_CrossRightBack;
    // public AnimationClip Run_CrossLeftFront;
    // public AnimationClip Run_CrossRightFront;

    //[Header("Run Turn")]
    public AnimationClip Run_TurnLeft_90;
    public AnimationClip Run_TurnRight_90;

    [Header("Sprint")]
    public AnimationClip Sprint_Forward;
    public AnimationClip Sprint_TurnLeft_90;
    public AnimationClip Sprint_TurnRight_90;

    [Header("Turn Around")]
    public AnimationClip TurnAroundLeft_90;
    public AnimationClip TurnAroundRight_90;

    [Header("[INTERACTION]")]
    public AnimationClip Combat_Idle;

    public AnimationClip Combat_Attack_0;
    public AnimationClip Combat_Attack_1;
    public AnimationClip Combat_Attack_2;

    public AnimationClip Combat_Hit_0;
    public AnimationClip Combat_Hit_1;
    public AnimationClip Combat_Hit_2;
    public AnimationClip Combat_Block_0;
    public AnimationClip Combat_Block_1;
    public AnimationClip Combat_Block_2;
}