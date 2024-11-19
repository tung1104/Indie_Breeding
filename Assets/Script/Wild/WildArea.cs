using UnityEngine;

[RequireComponent(typeof(ColorPropertyBlock))]
public class WildArea : MonoBehaviour
{
    public float radius;
    public ColorPropertyBlock colorPropertyBlock;

    private void OnValidate()
    {
        if (colorPropertyBlock == null)
        {
            colorPropertyBlock = gameObject.AddComponent<ColorPropertyBlock>();
        }
    }
}
