using UnityEngine;

public class FoodStorage : MonoBehaviour
{
    public GameObject meat;

    public void ShowMeat(bool isShow)
    {
        meat.SetActive(isShow);
    }

    public Vector3 GetRandomPointAround()
    {
        var angle = Quaternion.Euler(new Vector3(0, Random.Range(0, 100f) * 3.6f, 0));
        return transform.position + angle * Vector3.right * Random.Range(0.15f, 0.2f);
    }
}
