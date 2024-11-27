using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Equipment
{
    public Vector3 castOffsetPosition;
    public string projectileId;

    public float attackSpeed = 0;
    public float attackRange = 0;

    public Vector3 GetCastPosition()
    {
        return transform.position + transform.TransformVector(castOffsetPosition);
    }

    private void OnDrawGizmosSelected()
    {
        if (castOffsetPosition != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + transform.TransformVector(castOffsetPosition), 0.02f);
        }
    }
}
