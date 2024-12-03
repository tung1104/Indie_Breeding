using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public class MovementHandler : MonoBehaviour, ICharacterController
{
    [System.Serializable]
    public struct InputData
    {
        public float moveDirectionYaw;
        public int moveSpeedLevel;
        public float lookDirectionYaw;
        public bool jump;
    }

    [System.Serializable]
    public struct StateData
    {
        public Vector3 baseVelocity;
        public Vector3 velocity;
        public bool isGrounded;
        public Collider groundedCollider;
        public Vector3 groundedNormal;
    }

    public float capsuleRadius = 0.5f;
    public float capsuleHeight = 2.0f;
    public float[] moveSpeeds = { 1.0f, 2.0f, 3.0f };
    public float acceleration = 5.0f;
    public float deceleration = 5.0f;
    public float orientationSpeed = 5.0f;
    public float maxJumpHeight = 1.0f;
    public float maxSlopeAngle = 45;
    public float mass = 1.0f;

    [Tooltip("If true, the object will not be affected by physics, instead, it will fake physics.")]
    public bool isKinematic = true;

    KinematicCharacterMotor motor;

    public InputData input;
    public StateData state;

    Pose inputDeltaPose;
    bool handleDeltaPoseThisFrame;

    private List<Vector3> impulseForces;

    public Action<float> AfterCharacterUpdateCb { get; set; }
    public Action<float> BeforeCharacterUpdateCb { get; set; }

    private void Awake()
    {
        // Initialize components
        motor = gameObject.AddComponent<KinematicCharacterMotor>();
        motor.CharacterController = this;
        motor.SetCapsuleDimensions(capsuleRadius, capsuleHeight, capsuleHeight / 2f);
        motor.MaxStableSlopeAngle = maxSlopeAngle;
        motor.SimulatedCharacterMass = mass;
        motor.StepHandling = StepHandlingMethod.Standard;
        motor.MaxStepHeight = 0.2f;
        motor.LedgeAndDenivelationHandling = false;
        motor.InteractiveRigidbodyHandling = true;
        motor.CheckMovementInitialOverlaps = false;
        motor.CollidableLayers =
            MovementSystem.Instance.obstacleLayerMask; // | (1 << MovementSystem.Instance.characterLayer);
        motor.StableGroundLayers = MovementSystem.Instance.obstacleLayerMask;
        impulseForces = new List<Vector3>();
    }

    private void OnEnable()
    {
        input.lookDirectionYaw = input.moveDirectionYaw = transform.eulerAngles.y;
        motor.ApplyState(new KinematicCharacterMotorState()
            { Position = transform.position, Rotation = transform.rotation });
        motor.Capsule.enabled = motor.enabled = true;
    }

    private void OnDisable()
    {
        motor.Capsule.enabled = motor.enabled = false;
        motor.ForceUnground(0);
        motor.Capsule.isTrigger = false;
        input = default;
        isKinematic = true;
        handleDeltaPoseThisFrame = false;
        inputDeltaPose = Pose.identity;
        state = default;
    }

    private void Update()
    {
        if (!motor.enabled)
        {
            var deltaTime = Time.deltaTime;
            BeforeCharacterUpdateCb?.Invoke(deltaTime);
            UpdateGroundingState();
            var rotation = transform.rotation;
            UpdateRotation(ref rotation, deltaTime);
            transform.rotation = rotation;
            UpdateVelocity(ref state.baseVelocity, deltaTime);
            transform.position += state.baseVelocity * deltaTime;
            state.velocity = state.baseVelocity;
            AfterCharacterUpdateCb?.Invoke(deltaTime);
        }

        // var enableMotor = !isKinematic;
        // if (motor.enabled != enableMotor)
        //     motor.enabled = enableMotor;
    }

    private void UpdateGroundingState()
    {
        state.isGrounded = false;
        if (state.isGrounded) return;
        const float raycastGroundCheckDistance = .2f;
        if (!Physics.Raycast(transform.position + transform.up * raycastGroundCheckDistance / 2f,
                -transform.up, out var hitInfo, raycastGroundCheckDistance)) return;
        var angle = Vector3.Angle(hitInfo.normal, Vector3.up);
        if (!(angle <= maxSlopeAngle)) return;
        state.isGrounded = true;
        state.groundedCollider = hitInfo.collider;
        state.groundedNormal = hitInfo.normal;
    }

    public void SetCapsuleTrigger(bool isTrigger)
    {
        motor.Capsule.isTrigger = isTrigger;
    }

    public void ForceSetDeltaPose(Pose pose)
    {
        inputDeltaPose = pose;
        handleDeltaPoseThisFrame = true;
    }

    public float GetMoveSpeed(int level)
    {
        return moveSpeeds[Mathf.Min(level, moveSpeeds.Length - 1)];
    }

    public void AddImpulseForce(Vector3 force)
    {
        impulseForces.Add(force);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, state.baseVelocity);
        Gizmos.DrawSphere(transform.position + state.baseVelocity, .01f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + transform.up * .02f, state.velocity);
        Gizmos.DrawSphere(transform.position + state.velocity, .01f);


        Gizmos.color = Color.cyan;
        UnityEditor.Handles.Disc(transform.rotation, transform.position, Vector3.up, capsuleRadius, false, 0);
        UnityEditor.Handles.Disc(transform.rotation, transform.position + transform.up * capsuleHeight,
            Vector3.up, capsuleRadius, false, 0);

        Gizmos.color = Color.yellow;
        if (state.isGrounded)
        {
            Gizmos.DrawRay(transform.position, state.groundedNormal);
            Gizmos.DrawLine(transform.position, state.groundedCollider.transform.position);
        }
    }
#endif

    #region ICharacterController implementation

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (handleDeltaPoseThisFrame)
        {
            currentRotation *= inputDeltaPose.rotation;
        }
        else
        {
            var targetRotation = Quaternion.Euler(0, input.lookDirectionYaw, 0);
            currentRotation = Quaternion.Slerp(currentRotation, targetRotation,
                1 - Mathf.Exp(-orientationSpeed * deltaTime));
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (handleDeltaPoseThisFrame)
        {
            var deltaPosition = inputDeltaPose.position / Time.deltaTime;
            currentVelocity = new Vector3(deltaPosition.x, currentVelocity.y, deltaPosition.z);
        }
        else
        {
            var moveSpeed = input.moveSpeedLevel < 0
                ? 0
                : moveSpeeds[Mathf.Min(input.moveSpeedLevel, moveSpeeds.Length - 1)];
            var isMovingDiagonal = Mathf.DeltaAngle(input.lookDirectionYaw, input.moveDirectionYaw) != 0;
            var inputMove = Quaternion.Euler(0, input.moveDirectionYaw, 0) * Vector3.forward * moveSpeed;
            var applyAcceleration = acceleration;
            if (inputMove.sqrMagnitude <= currentVelocity.sqrMagnitude) applyAcceleration = deceleration;
            if (!state.isGrounded) applyAcceleration /= 5f;
            var moveVelocity = Vector3.Lerp(currentVelocity, inputMove, 1 - Mathf.Exp(-applyAcceleration * deltaTime));
            currentVelocity = new Vector3(moveVelocity.x, currentVelocity.y, moveVelocity.z);
        }

        if (state.isGrounded)
        {
            // Reorient velocity on slope
            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, state.groundedNormal);

            // Apply jump
            if (input.jump && !handleDeltaPoseThisFrame)
            {
                currentVelocity += state.groundedNormal * Mathf.Sqrt(2 * Physics.gravity.magnitude * maxJumpHeight);
                motor.ForceUnground(0.1f);
            }
        }
        else
        {
            // Apply gravity
            currentVelocity += Physics.gravity * deltaTime;
        }

        // Apply impulse forces
        for (var i = 0; i < impulseForces.Count; i++)
        {
            var force = impulseForces[i];
            currentVelocity += force / mass;
        }

        impulseForces.Clear();
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        BeforeCharacterUpdateCb?.Invoke(deltaTime);
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        state.isGrounded = motor.GroundingStatus.IsStableOnGround;
        state.groundedCollider = motor.GroundingStatus.GroundCollider;
        state.groundedNormal = motor.GroundingStatus.GroundNormal;
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        state.baseVelocity = motor.BaseVelocity;
        state.velocity = motor.Velocity;
        AfterCharacterUpdateCb?.Invoke(deltaTime);

        if (handleDeltaPoseThisFrame)
        {
            handleDeltaPoseThisFrame = false;
            inputDeltaPose = Pose.identity;
        }
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    #endregion
}