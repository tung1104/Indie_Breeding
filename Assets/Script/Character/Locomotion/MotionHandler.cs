using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class MotionHandler : MonoBehaviour
{
    public MotionProfile profile;
    public Animator animator;

    [HideInInspector] public float moveDirection;
    [HideInInspector] public float lookDirection;
    [HideInInspector] public int moveSpeedLevel;
    [HideInInspector] public float moveVelocityMagnitude;
    [HideInInspector] public float upVelocity;
    [HideInInspector] public bool isGrounded;
    public Pose animatorDeltaPose = Pose.identity;

    Quaternion smoothLocalRotation;

    AnimatorOverrideController overrideController;
    AnimationClipOverrides clipOverrides;

    private float smoothSteeringAngleBias;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator)
            animator.runtimeAnimatorController = null;
        else
            Debug.LogWarning($"MotionHandler: {name} has no Animator assigned");
    }

    private void Awake()
    {
        if (animator)
        {
            if (profile)
            {
                overrideController = profile.GetNewOverrideController();
                animator.runtimeAnimatorController = overrideController;
                clipOverrides = new AnimationClipOverrides(overrideController.overridesCount);
                overrideController.GetOverrides(clipOverrides);
            }
            else
            {
                Debug.LogWarning($"MotionHandler: {name} has no MotionProfile assigned");
                return;
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullCompletely;
            animator.keepAnimatorStateOnDisable = true;
            animator.fireEvents = false;
            animator.AddComponent<AnimatorTracker>();
        }
        else
            Debug.LogWarning($"MotionHandler: {name} has no Animator assigned");
    }

    private void Update()
    {
        if (animator && profile)
        {
            UpdateBaseMotion(Time.deltaTime);
            UpdateAdvancedMotion(Time.deltaTime);
        }
    }

    private void OnDisable()
    {
        ClearAllPlayRequests();
        animatorDeltaPose = Pose.identity;
    }

    void UpdateBaseMotion(float deltaTime)
    {
        if (playRequests.Count > 0 && playRequests[0].layerIndex == 0)
            return;

        var stateName = "Idle";
        var isMoving = moveSpeedLevel > -1;

        var isSteerable = profile.clips.Walk_Fwd_TurnLeft_90;

        if (isSteerable)
        {
            var steeringDeltaAngle = Mathf.DeltaAngle(transform.eulerAngles.y, moveDirection);
            var steeringAngleBias = Mathf.Clamp(steeringDeltaAngle, -90, 90) / 90f;
            if (!Mathf.Approximately(smoothSteeringAngleBias, steeringAngleBias))
                smoothSteeringAngleBias = Mathf.LerpAngle(smoothSteeringAngleBias, steeringAngleBias, deltaTime * 10);
        }

        if (!isGrounded)
        {
            stateName = upVelocity > 0 ? "Jump" : "Fall";
        }
        else if (isMoving)
        {
            var moveLookDeltaAngle = Mathf.DeltaAngle(lookDirection, moveDirection);
            var isMovingDiagonal = moveLookDeltaAngle != 0;
            switch (moveSpeedLevel)
            {
                case 0:
                    stateName = isMovingDiagonal ? "Walk_4Dir" : (isSteerable ? "Walk_Steer" : "Walk_Forward");
                    break;
                case 1:
                    stateName = isMovingDiagonal ? "Run_4Dir" : (isSteerable ? "Run_Steer" : "Run_Forward");
                    break;
                case 2:
                    stateName = isSteerable ? "Sprint_Steer" : "Sprint_Forward";
                    break;
            }

            if (isSteerable)
                animator.SetFloat(profile.GetParamNameHash("SteeringAngleBias"), smoothSteeringAngleBias);

            var moveSpeedMultiplier = profile.GetMoveSpeedMultiplier(moveSpeedLevel) *
                                      Mathf.Max(moveVelocityMagnitude, profile.minMoveVelocityMagnitude);
            if (profile.correctSpeedWithScale)
                moveSpeedMultiplier /= animator.transform.localScale.x;
            animator.SetFloat(profile.GetParamNameHash("MoveSpeedMultiplier"), moveSpeedMultiplier);

            if (isMovingDiagonal)
            {
                var localRotation = Quaternion.Inverse(transform.rotation) * Quaternion.Euler(0, moveDirection, 0);
                smoothLocalRotation = Quaternion.Slerp(smoothLocalRotation, localRotation, deltaTime * 10);
                var localMoveDirection = smoothLocalRotation * Vector3.forward * (moveSpeedLevel + 1);
                animator.SetFloat(profile.GetParamNameHash("LocalMoveDirectionX"), localMoveDirection.x);
                animator.SetFloat(profile.GetParamNameHash("LocalMoveDirectionZ"), localMoveDirection.z);
            }
        }

        var stateNameHash = profile.GetStateNameHash(stateName);
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash != stateNameHash && !animator.IsInTransition(0))
        {
            var transitionDuration = 0.15f;
            var normalizedTimeOffset = 0f;

            if (isMoving)
            {
                if (profile.moveTransitionDuration > 0)
                    transitionDuration = profile.moveTransitionDuration;
            }
            else
                switch (stateName)
                {
                    case "Idle":
                        normalizedTimeOffset = Random.value;
                        break;
                }

            var timeOffset = normalizedTimeOffset * animator.GetCurrentAnimatorStateInfo(0).length;
            animator.CrossFadeInFixedTime(stateNameHash, transitionDuration, 0, timeOffset);
            //animator.Play(stateNameHash, 0, timeOffset);
        }
    }

    #region Advanced Motion Control

    private struct PlayRequest
    {
        public string state;
        public float transitionDuration;
        public float normalizedTimeOffset;
        public float forceLength;
        public int loopTime;
        public int layerIndex;
        public AnimationClip clip;
    }

    private struct PlayCallback
    {
        public Action begin;
        public Action update;
        public Action end;
        public Action<MotionStateSettingEvent> emit;
    }

    readonly Dictionary<string, PlayCallback> playCallback = new();
    readonly List<PlayRequest> playRequests = new();
    string playingState;
    float playingTime;
    float playingSpeed;
    int playingEmitCount;
    int playingLoopRemain;
    PlayCallback playingCallback;
    public MotionStateSetting playingStateSetting;
    public float playingTimeNormalized;
    readonly Dictionary<string, MotionStateSetting> overrideStateSettings = new();

    public void SubscribeOnEmit(string state, Action<MotionStateSettingEvent> callback)
    {
        if (!playCallback.TryGetValue(state, out var playCb)) playCb = new PlayCallback();
        playCb.emit += callback;
        playCallback[state] = playCb;
    }

    public void SubscribeOnEnd(string state, Action callback)
    {
        if (!playCallback.TryGetValue(state, out var playCb)) playCb = new PlayCallback();
        playCb.end += callback;
        playCallback[state] = playCb;
    }

    public void SubscribeOnUpdate(string state, Action callback)
    {
        if (!playCallback.TryGetValue(state, out var playCb)) playCb = new PlayCallback();
        playCb.update += callback;
        playCallback[state] = playCb;
    }

    public void SetOverrideClip(string state, AnimationClip clip, MotionStateSetting setting)
    {
        clipOverrides[state] = clip;
        overrideController.ApplyOverrides(clipOverrides);
        if (setting.stateName.Equals(state))
            overrideStateSettings.Add(setting.stateName, setting);
    }

    public void ClearAllPlayRequests()
    {
        playRequests.Clear();
        playingState = default;
    }

    public bool Play(string state, float transitionDuration = .15f, float normalizedTimeOffset = 0,
        float forceLength = 0, int loopTime = 0, int layerIndex = 0)
    {
        ClearAllPlayRequests();
        var clip = clipOverrides[state];
        if (!clip)
        {
            Debug.LogWarning($"Game object {name} failed to play {state} because not found!");
            return false;
        }

        playRequests.Add(new PlayRequest
        {
            state = state,
            transitionDuration = transitionDuration,
            normalizedTimeOffset = normalizedTimeOffset,
            forceLength = forceLength,
            loopTime = loopTime,
            layerIndex = layerIndex,
            clip = clip
        });
        return true;
    }

    public bool IsPlaying(string state, bool isGrouped = false)
    {
        if (isGrouped)
            return profile.TryGetGroupedStateNames(state, out var stateNames) &&
                   playRequests.Exists(r => stateNames.Contains(r.state));
        return playRequests.Exists(r => r.state == state);
    }

    public bool IsPlayingAny()
    {
        return playRequests.Count > 0;
    }

    void UpdateAdvancedMotion(float deltaTime)
    {
        // Implement advanced motion control here
        var hasPlayRequest = playRequests.Count > 0;
        var layer1Weight = animator.GetLayerWeight(1);
        float targetLayer1Weight = hasPlayRequest && playRequests[0].layerIndex > 0 ? 1 : 0;
        if (!Mathf.Approximately(layer1Weight, targetLayer1Weight))
            animator.SetLayerWeight(1, Mathf.MoveTowards(layer1Weight, targetLayer1Weight, deltaTime * 15));
        if (!hasPlayRequest) return;

        var playRequest = playRequests[0];
        if (playingState != playRequest.state)
        {
            // Play the new state
            playingState = playRequest.state;
            playingTime = playRequest.normalizedTimeOffset > 0
                ? playRequest.normalizedTimeOffset * playRequest.clip.length
                : 0;
            playingSpeed = playRequest.forceLength > 0 ? playRequest.clip.length / playRequest.forceLength : 1;
            playingEmitCount = 0;
            playingLoopRemain = playRequest.loopTime;
            if (!profile.settingsDictionary.TryGetValue(playingState, out playingStateSetting))
                overrideStateSettings.TryGetValue(playingState, out playingStateSetting);
            playCallback.TryGetValue(playingState, out playingCallback);

            animator.CrossFadeInFixedTime(profile.GetStateNameHash(playingState), playRequest.transitionDuration,
                playRequest.layerIndex, playingTime);
            animator.Update(0);
            //animator.Play(profile.GetStateNameHash(playingState), playRequest.layerIndex, playingTime);
            animator.SetFloat(profile.GetParamNameHash("ActionSpeedMultiplier"), playingSpeed);
            //Debug.Log($"Play {playingState}");
        }

        if (!string.IsNullOrEmpty(playingState))
        {
            playingTime += deltaTime * playingSpeed;
            playingTimeNormalized = playingTime / playRequest.clip.length;
            if (playingTimeNormalized < 1)
            {
                // Update playing state
                if (playingCallback.emit != null && playingStateSetting.events != null &&
                    playingEmitCount < playingStateSetting.events.Length)
                {
                    var ev = playingStateSetting.events[playingEmitCount];
                    if (playingTimeNormalized >= ev.normalizedTime)
                    {
                        playingEmitCount++;
                        playingCallback.emit.Invoke(ev);
                    }
                }
            }
            else
            {
                // Ensure all events are emitted
                if (playingCallback.emit != null && playingStateSetting.events != null)
                    while (playingEmitCount < playingStateSetting.events.Length)
                    {
                        playingCallback.emit.Invoke(playingStateSetting.events[playingEmitCount]);
                        playingEmitCount++;
                    }

                // End playing state
                bool willReplayNextFrame = true;
                if (playingLoopRemain > 0)
                    playingLoopRemain--;
                else
                {
                    switch (playRequest.loopTime)
                    {
                        case 0: // Play once
                            playRequests.RemoveAt(0);
                            playingState = default;
                            //Debug.Log($"End {playingState}");
                            break;
                        case -1: // Loop forever
                            break;
                        case -2: // Play once and stop
                            willReplayNextFrame = false;
                            break;
                    }
                }

                if (willReplayNextFrame)
                {
                    // Mark playing state is none for trigging next frame
                    playingState = default;
                }
            }
        }
    }

    #endregion

#if UNITY_EDITOR
    [ContextMenu("Create New Profile")]
    public void CreateNewProfile()
    {
        profile = ScriptableObject.CreateInstance<MotionProfile>();
        profile.name = gameObject.name;

        // Open diaglog to save the new profile

        string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save Motion Profile", profile.name, "asset",
            "Save Motion Profile");
        if (path.Length > 0)
        {
            UnityEditor.AssetDatabase.CreateAsset(profile, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    internal class AnimatorTracker : MonoBehaviour
    {
        Animator animator;
        MotionHandler motion;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            motion = GetComponentInParent<MotionHandler>();
        }

        private void OnAnimatorMove()
        {
            var deltaPose = new Pose(animator.deltaPosition, animator.deltaRotation);
            motion.animatorDeltaPose = deltaPose;
        }
    }

    internal class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
    {
        public AnimationClipOverrides(int capacity) : base(capacity)
        {
        }

        public AnimationClip this[string name]
        {
            get { return this.Find(x => x.Key.name.Equals(name)).Value; }
            set
            {
                int index = this.FindIndex(x => x.Key.name.Equals(name));
                if (index != -1)
                    this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
            }
        }
    }
}