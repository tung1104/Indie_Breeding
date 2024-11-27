using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepEffectTracker : MonoBehaviour
{
    public ObjectPoolManager fxPool;
    public Transform leftFoot;
    public Transform rightFoot;
    public float offsetDistance = 0.01f;

    float previousLeftFootY, previousRightFootY;
    bool leftFootStep, rightFootStep;
    float minLeftFootY = 1, minRightFootY = 1;

    private void Start()
    {
        ObjectPoolManager.Instances.TryGetValue("FXPool", out fxPool);
        if (TryGetComponent(out Animator animator))
        {
            leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }

        previousLeftFootY = leftFoot.position.y;
        previousRightFootY = rightFoot.position.y;
    }

    private void Update()
    {
        CheckFootstep(leftFoot, ref previousLeftFootY, ref minLeftFootY, ref leftFootStep);
        //CheckFootstep(rightFoot, ref previousRightFootY, ref rightFootStep);
    }

    private void CheckFootstep(Transform foot, ref float previousFootY, ref float minFootY, ref bool footStep)
    {
        // Chuyển vị trí chân về tọa độ cục bộ để tính độ chênh lệch
        float currentFootY = transform.InverseTransformPoint(foot.position).y;
        float footDifference = currentFootY - previousFootY;

        if (Mathf.Abs(footDifference) > offsetDistance)
        {


            // // Kiểm tra xem chân vừa chạm đất
            // if (!footStep && footDifference < offsetDistance)
            // {
            //     // Spawn hiệu ứng bước chân
            //     fxPool.TrySpawnInstance("FootstepEffect", foot.position, Quaternion.identity, out _);
            //     footStep = true;
            // }
            // // Kiểm tra xem chân vừa nhấc lên khỏi mặt đất
            // else if (footStep && footDifference > offsetDistance)
            // {
            //     footStep = false;
            // }

            previousFootY = currentFootY;
        }
    }
}
