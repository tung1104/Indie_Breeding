using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class GraphicsSettingsHelper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void SelfInitialize()
    {
        // Disable SRP batching one more time to make sure it's disabled
        if (UseSRPBatcher)
            Debug.LogWarning("SRP Batcher is enabled. Disabling it to prevent issues with GPU Instancing!");
        else
            GraphicsSettings.useScriptableRenderPipelineBatching = false;

        // Force frame rate to 90
        Application.targetFrameRate = 60;
        // Force physics update rate to 30
        Time.fixedDeltaTime = 1f / 30f;
    }

    #region SRP Batcher

    const string MENUITEM_SRPBATCHER = "Tools/Graphics Settings/SRP Batcher";

    static bool UseSRPBatcher
    {
        get { return (QualitySettings.renderPipeline as UniversalRenderPipelineAsset).useSRPBatcher; }
        set
        {
            for (int i = 0; i < QualitySettings.count; i++)
            {
                var urpAsset = QualitySettings.GetRenderPipelineAssetAt(i) as UniversalRenderPipelineAsset;
                urpAsset.useSRPBatcher = value;

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(urpAsset);
#endif
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem(MENUITEM_SRPBATCHER)]
    static void ToggleSRPBatcher()
    {
        UseSRPBatcher = !UseSRPBatcher;
    }

    [UnityEditor.MenuItem(MENUITEM_SRPBATCHER, true)]
    static bool ToggleSRPBatcherValidate()
    {
        UnityEditor.Menu.SetChecked(MENUITEM_SRPBATCHER, UseSRPBatcher);
        return true;
    }
#endif

    #endregion

    #region Depth Texture

    const string MENUITEM_DEPTHTEXTURE = "Tools/Graphics Settings/Depth Texture";

    static bool DepthTexture
    {
        get { return (QualitySettings.renderPipeline as UniversalRenderPipelineAsset).supportsCameraDepthTexture; }
        set
        {
            for (int i = 0; i < QualitySettings.count; i++)
            {
                var urpAsset = QualitySettings.GetRenderPipelineAssetAt(i) as UniversalRenderPipelineAsset;
                urpAsset.supportsCameraDepthTexture = value;

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(urpAsset);
#endif
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem(MENUITEM_DEPTHTEXTURE)]
    static void ToggleDepthTexture()
    {
        DepthTexture = !DepthTexture;
    }

    [UnityEditor.MenuItem(MENUITEM_DEPTHTEXTURE, true)]
    static bool ToggleDepthTextureValidate()
    {
        UnityEditor.Menu.SetChecked(MENUITEM_DEPTHTEXTURE, DepthTexture);
        return true;
    }
#endif

    #endregion
}