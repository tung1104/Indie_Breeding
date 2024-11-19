using UnityEngine;

public class Egg : MonoBehaviour
{
    public Nest nest;
    public Animator animator;
    public EggData data = null;
    public int indexInPanel;

    public void PlayAnimation()
    {
        animator.enabled = true;
    }

    public void StopAnimation()
    {
        animator.enabled = false;
    }
}
