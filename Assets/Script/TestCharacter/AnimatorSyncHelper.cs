using System;
using UnityEngine;

public class AnimatorSyncHelper : MonoBehaviour
{
    [SerializeField] private Animator rootAnimator; // Animator chính
    [SerializeField] private Animator[] partAnimators; // Các Animator phụ (đầu, chân, ...)

    private int[] currentStateHashes; // Lưu trữ trạng thái hiện tại của rootAnimator
    private float[] currentNormalizedTimes; // Lưu trữ thời gian normalized của rootAnimator

    private RuntimeAnimatorController currentController; // Controller hiện tại

    private void Start()
    {
        if (rootAnimator == null) return;

        // Lưu controller hiện tại để theo dõi thay đổi
        if (rootAnimator != null)
            currentController = rootAnimator.runtimeAnimatorController;

        // Đồng bộ ban đầu
        SyncControllers();
        SyncParameters();

        int layerCount = rootAnimator.layerCount;
        currentStateHashes = new int[layerCount];
        currentNormalizedTimes = new float[layerCount];
    }

    private void Update()
    {
        SyncState();
        SyncParameters();
    }

    private void OnDisable()
    {
        rootAnimator.Play("Idle");
        rootAnimator.Update(0);
        foreach (var partAnimator in partAnimators)
        {
            partAnimator.Play("Idle", 0, 0);
            partAnimator.Update(0);
        }
    }

    /// <summary>
    /// Đồng bộ controller giữa root và partAnimators
    /// </summary>
    private void SyncControllers()
    {
        foreach (var partAnimator in partAnimators)
        {
            if (partAnimator != null)
                partAnimator.runtimeAnimatorController = currentController;
        }
    }

    /// <summary>
    /// Đồng bộ trạng thái (state) giữa rootAnimator và partAnimators
    /// </summary>
    private void SyncState()
    {
        for (int layer = 0; layer < rootAnimator.layerCount; layer++)
        {
            var stateInfo = rootAnimator.GetCurrentAnimatorStateInfo(layer);
            int stateHash = stateInfo.shortNameHash;

            // Kiểm tra nếu trạng thái mới hoặc thời gian normalized khác biệt
            if (currentStateHashes[layer] != stateHash ||
                Mathf.Abs(currentNormalizedTimes[layer] - stateInfo.normalizedTime) > 0.01f)
            {
                currentStateHashes[layer] = stateHash;
                currentNormalizedTimes[layer] = stateInfo.normalizedTime;

                foreach (var partAnimator in partAnimators)
                {
                    if (partAnimator != null)
                    {
                        partAnimator.Play(stateHash, layer, stateInfo.normalizedTime);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Đồng bộ các tham số giữa rootAnimator và partAnimators
    /// </summary>
    private void SyncParameters()
    {
        foreach (var partAnimator in partAnimators)
        {
            if (partAnimator == null) continue;

            foreach (var param in rootAnimator.parameters)
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Float:
                        partAnimator.SetFloat(param.name, rootAnimator.GetFloat(param.name));
                        break;
                    case AnimatorControllerParameterType.Int:
                        partAnimator.SetInteger(param.name, rootAnimator.GetInteger(param.name));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        partAnimator.SetBool(param.name, rootAnimator.GetBool(param.name));
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        if (rootAnimator.GetBool(param.name))
                            partAnimator.SetTrigger(param.name);
                        break;
                }
            }
        }
    }
}