using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SampleHealthBar : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;
    [Tooltip("If this value is under 0, then the health bar always remains visible")]
    [SerializeField] float fadeOutDuration = 1f;
    [SerializeField] float fadeOutDelay = .5f;
    public Color fillColor = Color.gray;
    [Range(0, 1)] public float value;
    public bool showTrigger;

    MaterialPropertyBlock propertyBlock;
    static readonly int ValueID = Shader.PropertyToID("_Value");
    static readonly int AlphaID = Shader.PropertyToID("_Alpha");
    static readonly int ColorID = Shader.PropertyToID("_FillColor");

    private float currentAlpha, fadeOutCd;

    Color cachedFillColor;
    float cachedValue;
    float cachedAlpha;

    private void Awake()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        propertyBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        RefreshFadeOutCd();
    }

    private void Update()
    {
        UpdateVisibility();
        UpdateMaterialProperties();
    }

    void RefreshFadeOutCd()
    {
        currentAlpha = 1f;
        fadeOutCd = fadeOutDelay;
    }

    private void UpdateVisibility()
    {
        if (fadeOutDuration >= 0)
        {
            if (showTrigger)
            {
                showTrigger = false;
                RefreshFadeOutCd();
            }
            else if (currentAlpha > 0)
            {
                if (fadeOutCd > 0)
                    fadeOutCd -= Time.deltaTime;
                else
                    currentAlpha = Mathf.MoveTowards(currentAlpha, 0, Time.deltaTime / fadeOutDuration);
            }
        }
        else
        {
            currentAlpha = 1f;
        }

        bool visible = currentAlpha > 0;
        if (meshRenderer.enabled != visible)
            meshRenderer.enabled = visible;
    }

    private void UpdateMaterialProperties()
    {
        //meshRenderer.GetPropertyBlock(propertyBlock);
        bool isChanged = false;

        if (fillColor != cachedFillColor)
        {
            propertyBlock.SetColor(ColorID, fillColor);
            cachedFillColor = fillColor;
            isChanged = true;
        }

        if (value != cachedValue)
        {
            propertyBlock.SetFloat(ValueID, value);
            cachedValue = value;
            isChanged = true;
        }

        if (currentAlpha != cachedAlpha)
        {
            propertyBlock.SetFloat(AlphaID, currentAlpha);
            cachedAlpha = currentAlpha;
            isChanged = true;
        }

        if (isChanged)
            meshRenderer.SetPropertyBlock(propertyBlock);
    }

}
