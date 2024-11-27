using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedIKHandler : MonoBehaviour
{
    public Transform leftHand;
    public Transform rightHand;
    public Transform head;
    public Transform mouth;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator.isHuman)
        {
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            head = animator.GetBoneTransform(HumanBodyBones.Head);
        }
    }
}