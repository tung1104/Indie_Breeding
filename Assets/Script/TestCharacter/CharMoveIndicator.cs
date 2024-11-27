using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMoveIndicator : MonoBehaviour
{
    public float distance;
    public MovementHandler movement;

    MeshRenderer meshRenderer;

    Quaternion smoothRotation;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        if (!movement)
        {
            movement = GetComponentInParent<MovementHandler>();
        }
    }

    private void LateUpdate()
    {
        if (movement)
        {
            var rotation = Quaternion.Euler(0, movement.input.moveDirectionYaw, 0);
            smoothRotation = Quaternion.Slerp(smoothRotation, rotation, Time.deltaTime * 20);
            bool isMoving = movement.input.moveSpeedLevel > -1;
            if (meshRenderer.enabled != isMoving)
            {
                if (isMoving)
                    smoothRotation = rotation;
                meshRenderer.enabled = isMoving;
            }
            float dist = distance + movement.capsuleRadius;
            transform.SetPositionAndRotation(movement.transform.position + smoothRotation *
                Vector3.forward * dist + Vector3.up * 0.1f, smoothRotation * Quaternion.Euler(90, 0, 0));
        }
    }
}
