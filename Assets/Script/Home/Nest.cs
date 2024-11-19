using System;
using UnityEngine;

public class Nest : MonoBehaviour
{
    public Dino dinoLeft;
    public Transform placeLeft;
    public Dino dinoRight;
    public Transform placeRight;
    public Egg egg;

    public NestMutationInfo nestMutationInfo;
    public int index;

    public void Init(NestMutationInfo nestMutationInfo, int index)
    {
        this.nestMutationInfo = nestMutationInfo;
        this.index = index;

        foreach (var dino in HomeController.Current.homeMutationController.dinoes.Values)
        {
            if (dino.data.id == nestMutationInfo.momId) AddDino(dino, true);
            else if (dino.data.id == nestMutationInfo.dadId) AddDino(dino, false);
        }

        egg.gameObject.SetActive(false);
        egg.StopAnimation();
    }

    public void CheckMating()
    {
        if (dinoLeft != null && dinoRight != null)
        {
            nestMutationInfo.startMatingTime = DateTime.Now;
            var boost = HomeController.Current.upgradeController.GetNestBoostByIndex(index) / 100;
            var timer = HomeController.Current.upgradeController.CalculateMatingCountdown(dinoLeft.data.level, dinoRight.data.level);
            nestMutationInfo.matingTime = timer * (1 - boost);
            HomeController.Current.nestController.Save();
        }
    }

    public void AddDino(Dino dino, bool isLeft)
    {
        if (isLeft)
        {
            dinoLeft = dino;
            dino.locomotion.Mate();
            dino.transform.SetPositionAndRotation(placeLeft.position, placeLeft.rotation);
        }
        else
        {
            dinoRight = dino;
            dino.locomotion.Mate();
            dino.transform.SetPositionAndRotation(placeRight.position, placeRight.rotation);
        }

        dino.EnableCollider(false);
        dino.shadow.SetActive(false);
    }

    public void RemoveDino(bool isLeft)
    {
        Dino dino;
        if (isLeft) dino = dinoLeft;
        else dino = dinoRight;

        dino.Init(HomeController.Current.homeMutationController.transform, true);
        dino.locomotion.ContinueMove();
        dino.EnableCollider(true);
        dino.shadow.SetActive(true);
        dino.isInNest = false;

        if (isLeft) dinoLeft = null;
        else dinoRight = null;

        nestMutationInfo.matingTime = -1;
        HomeController.Current.nestController.Save();
    }

    public void AddEggToIncubator()
    {
        if (HomeController.Current.incubatorController.incubator.numberEggCurrent < HomeController.Current.incubatorController.eggSlot)
        {
            var eggClone = Instantiate(egg);
            eggClone.PlayAnimation();
            eggClone.transform.localScale = Vector3.one;
            var colliders = eggClone.GetComponents<Collider>();
            foreach (var collider in colliders) DestroyImmediate(collider);
            eggClone.data.mom = dinoLeft.data;
            eggClone.data.dad = dinoRight.data;
            eggClone.data.startTime = DateTime.Now;
            HomeController.Current.incubatorController.AddEgg(eggClone);

            nestMutationInfo.startMatingTime = DateTime.Now;
            HomeController.Current.nestController.Save();
        }
        else
        {
            HomeUIController.Current.Get<NotiPanel>().ChangeTitle("Incubator is full!");
            HomeUIController.Current.Show<NotiPanel>(true);
        }
    }

    private void Update()
    {
        if (nestMutationInfo.matingTime == -1) egg.gameObject.SetActive(false);
        else egg.gameObject.SetActive((DateTime.Now - nestMutationInfo.startMatingTime).TotalSeconds >= nestMutationInfo.matingTime - 1);
    }
}
