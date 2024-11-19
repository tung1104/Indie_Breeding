using UnityEngine;

public class NestPlace : MonoBehaviour
{
    public Nest nest;
    public NestEmpty nestEmpty;

    public void Init()
    {
        nest.gameObject.SetActive(false);
        nestEmpty.gameObject.SetActive(false);
    }

    public void Prepare()
    {
        nest.gameObject.SetActive(false);
        nestEmpty.gameObject.SetActive(true);
    }

    public void Build(NestMutationInfo info, int index)
    {
        nest.Init(info, index);
        nest.gameObject.SetActive(true);
        nestEmpty.gameObject.SetActive(false);
    }
}
