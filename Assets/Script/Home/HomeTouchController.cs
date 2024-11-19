using UnityEngine;

public class HomeTouchController : TouchController
{
    protected override void OnTouch(Collider collider)
    {
        if (collider.CompareTag("Nest"))
        {
            var nest = collider.GetComponent<Nest>();
            HomeUIController.Current.Get<NestPanel>().SetNest(nest);
            HomeUIController.Current.Show<NestPanel>();
        }

        if (collider.CompareTag("Collector"))
        {
            HomeUIController.Current.Show<IncubatorPanel>();
        }

        if (collider.CompareTag("EggInNest"))
        {
            var egg = collider.GetComponent<Egg>();
            egg.nest.AddEggToIncubator();
        }

        if (collider.CompareTag("NestEmpty"))
        {
            var nestEmpty = collider.GetComponent<NestEmpty>();
            nestEmpty.OnClick();
        }

        if (collider.CompareTag("Dino"))
        {
            var dino = collider.GetComponent<Dino>();
            if (dino.data.status != DinoStatus.mating)
            {
                HomeUIController.Current.Get<DinoDetailPanel>().data = dino.data;
                HomeUIController.Current.Show<DinoDetailPanel>();
            }
        }

        if (collider.CompareTag("FoodStorage"))
        {
            HomeUIController.Current.Show<FoodStoragePanel>();
        }
    }
}
