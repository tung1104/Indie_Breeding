using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class EggViewer : MonoBehaviour
{
    public Action OpenDone;
    public Animator animator;
    public Transform dinoParent;
    public GameObject defaultEgg;
    public GameObject crackedEgg;

    private Coroutine openEggCoroutine;

    public void Init()
    {
        animator.Play("Egg_Ready", 0, 0);

        defaultEgg.SetActive(true);
        crackedEgg.SetActive(false);
    }

    public void OpenEgg(Dino dino)
    {
        Init();

        dino.transform.SetParent(dinoParent);
        dino.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        dinoParent.localScale = Vector3.zero;

        if (openEggCoroutine != null) StopCoroutine(openEggCoroutine);
        openEggCoroutine = StartCoroutine(OpenEggCoroutine());
    }

    public IEnumerator OpenEggCoroutine()
    {
        animator.Play("Egg_Ready", 0, 0);

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        defaultEgg.SetActive(false);
        crackedEgg.SetActive(true);

        dinoParent.gameObject.SetActive(true);
        dinoParent.DOScale(1, 1f);

        animator.Play("Egg_Hatch", 0, 0);

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        crackedEgg.SetActive(false);

        yield return new WaitForEndOfFrame();

        OpenDone?.Invoke();
    }
}

