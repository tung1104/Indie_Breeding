using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public Pose offsetPose;
    public HumanBodyBones humanBodyBone;

    void Reset()
    {
        offsetPose = new Pose(transform.localPosition, transform.localRotation);

        // Determine the human body bone based on the parent object
        var animator = GetComponentInParent<Animator>();
        if (animator)
        {
            if (animator.isHuman)
            {
                var bones = System.Enum.GetValues(typeof(HumanBodyBones));
                foreach (HumanBodyBones bone in bones)
                {
                    if (bone < HumanBodyBones.LastBone && animator.GetBoneTransform(bone) == transform.parent)
                    {
                        humanBodyBone = bone;
                        break;
                    }
                }
            }
        }
    }
}
