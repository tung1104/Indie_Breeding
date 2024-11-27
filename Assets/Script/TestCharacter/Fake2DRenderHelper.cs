using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage), typeof(AspectRatioFitter))]
public class Fake2DRenderHelper : MonoBehaviour
{
    public enum Resolutions
    {
        HD_720p,
        FHD_1080p,
        QHD_1440p,
        UHD_2160p
    }

    public enum ColorFormats
    {
        LDR,
        HDR
    }

    readonly Vector2Int[] resolutionArray = { new(1280, 720), new(1920, 1080), new(2560, 1440), new(3840, 2160) };
    readonly RenderTextureFormat[] colorFormatArray = { RenderTextureFormat.ARGB32, RenderTextureFormat.ARGBHalf };

    [SerializeField] RawImage rawImage;
    [SerializeField] AspectRatioFitter aspectRatioFitter;
    [SerializeField] new Camera camera;

    [SerializeField] Resolutions resolution;
    [SerializeField] ColorFormats colorFormat;

    RenderTexture renderTexture;

    private void Reset()
    {
        rawImage = GetComponent<RawImage>();
        aspectRatioFitter = GetComponent<AspectRatioFitter>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        RenderTexture renTex;
        if (!rawImage || !(renTex = rawImage.texture as RenderTexture)) return;
        var res = resolutionArray[(int)resolution];
        if (Screen.orientation == ScreenOrientation.Portrait)
            res = new Vector2Int(res.y, res.x);
        var clr = colorFormatArray[(int)colorFormat];
        if (renTex.width != res.x || renTex.height != res.y || renTex.format != clr)
        {
            renTex.DiscardContents();
            renTex.Release();
            renTex.width = res.x;
            renTex.height = res.y;
            renTex.format = clr;
            Debug.Log($"RenderTexture updated: {res.x}x{res.y} {clr}");
            renTex.Create();
            UnityEditor.EditorUtility.SetDirty(renTex);
        }
    }
#endif

    private void Awake()
    {
#if !UNITY_EDITOR
        var canvas = GetComponentInParent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera.gameObject.SetActive(false);
#endif
        var res = resolutionArray[(int)resolution];
        if (Screen.orientation == ScreenOrientation.Portrait)
            res = new Vector2Int(res.y, res.x);

        var resAspectRatio = (float)res.x / res.y;
        var adjustAspectRatio = aspectRatioFitter.aspectRatio;
        var screenAspectRatio = (float)Screen.width / Screen.height;
        res.y = Mathf.RoundToInt(res.x / screenAspectRatio * (adjustAspectRatio / resAspectRatio));
        aspectRatioFitter.enabled = false;
        rawImage.rectTransform.offsetMin = Vector2.zero; // Reset left and bottom offsets
        rawImage.rectTransform.offsetMax = Vector2.zero; // Reset right and top offsets
        
        var clr = colorFormatArray[(int)colorFormat];
        renderTexture = new RenderTexture(res.x, res.y, 0, clr);
        renderTexture.Create();
        renderTexture.filterMode = FilterMode.Point;
        camera.targetTexture = renderTexture;
        rawImage.texture = renderTexture;
    }

    private void OnDestroy()
    {
        renderTexture.Release();
    }
}