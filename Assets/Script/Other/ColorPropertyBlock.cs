using UnityEngine;

public class ColorPropertyBlock : MonoBehaviour
{
    public Color MaterialColor;

    private MaterialPropertyBlock propertyBlock;

    private void OnValidate()
    {
        ChangeColor();
    }

    private void ChangeColor()
    {
        propertyBlock ??= new MaterialPropertyBlock();
        Renderer renderer = GetComponentInChildren<Renderer>();
        propertyBlock.SetColor("_Color", MaterialColor);
        renderer.SetPropertyBlock(propertyBlock);
    }

    public void ChangeColor(Color color)
    {
        MaterialColor = color;
        ChangeColor();
    }
}