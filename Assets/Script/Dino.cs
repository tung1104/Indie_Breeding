using UnityEngine;

public class Dino : MonoBehaviour
{
    public DinoInfoData data;
    public DinoLocomotion locomotion;
    public bool isInNest;
    public GameObject meat;
    public GameObject shadow;

    public void Init(Transform parent, bool isShowShadow, string layerName = "Default", bool preventScale = false)
    {
        meat.SetActive(data.status == DinoStatus.baby && data.foodPercent < 20);
        shadow.SetActive(isShowShadow);
        locomotion.Init();

        transform.SetParent(parent);
        transform.SetPositionAndRotation(HomeController.Current.astarPathController.RandomNode(), Quaternion.identity);

        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.gameObject.layer = LayerMask.NameToLayer(layerName);
        }

        if (preventScale)
        {
            transform.localScale = Vector3.one;
        }
        else if (data.status != DinoStatus.baby)
        {
            transform.localScale = Vector3.one * 1.3f;
        }
    }

    public void Move(Vector3 targetPoint, float time)
    {
        var speed = (targetPoint - transform.position).magnitude / time;
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPoint - transform.position), Time.deltaTime * 10);
    }

    public void EnableCollider(bool enabled)
    {
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = enabled;
        }
    }
}
