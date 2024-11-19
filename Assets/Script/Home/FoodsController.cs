using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodsController : MonoBehaviour
{
    public FoodStorage foodStorage;
    public Dictionary<int, Dino> dinoes => HomeController.Current.homeMutationController.dinoes;

    public void Init()
    {
        InvokeRepeating(nameof(UpdateDinoStatus), 0, 1);
    }

    public void UpdateDinoStatus()
    {
        var food = GameController.Current.playerController.GetPlayerData().food;

        foodStorage.ShowMeat(food > 0);

        foreach (var dino in dinoes.Values)
        {
            if (dino.data.status == DinoStatus.baby)
            {
                if (dino.data.foodPercent > 0)
                {
                    dino.data.foodPercent -= 1;
                    dino.data.timerMature += 1;
                    dino.transform.localScale = Vector3.one + ((float)dino.data.timerMature / dino.data.totalTimeMature) * 0.3f * Vector3.one;
                    if (dino.data.timerMature >= dino.data.totalTimeMature)
                    {
                        dino.data.status = DinoStatus.idle;
                    }
                }

                if (dino.data.foodPercent < 20)
                {
                    dino.meat.SetActive(true);

                    if (food > 0 && !dino.locomotion.canEat)
                    {
                        dino.locomotion.SetCanEat(true);
                        dino.locomotion.OnEat = () => StartCoroutine(OnEat(dino));
                    }
                }
            }
        }

        HomeController.Current.homeMutationController.SaveData();
    }

    public IEnumerator OnEat(Dino dino)
    {
        if (IsEatSuccess())
        {
            dino.locomotion.PlayEatAnim();

            yield return new WaitForSeconds(1);

            dino.meat.SetActive(false);
            dino.data.foodPercent = 45;
        }

        dino.locomotion.SetCanEat(false);
    }

    public int MeatPerTime()
    {
        return GameController.Current.gameData.MCONST / GameController.Current.gameData.GTIME * 25;
    }
    public int TotalTimeMature(int dinoLv)
    {
        return dinoLv * GameController.Current.gameData.GTIME * 20 / 25;
    }

    public bool IsEatSuccess()
    {
        PlayerController playerController = GameController.Current.playerController;
        int playerFood = playerController.GetPlayerData().food;
        if (playerFood >= MeatPerTime())
        {
            playerFood -= MeatPerTime();
            playerController.SetPlayerData_Food(playerFood);
            foodStorage.ShowMeat(playerFood > 0);
            HomeUIController.Current.homeUI.UpdateView();
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetTotalFoodForAllDino()
    {
        int totalFood = 0;
        foreach (DinoInfoData dt in GameController.Current.mutationController.dinoInfoDatas.Values.ToList())
        {
            if (dt.status == DinoStatus.baby)
            {
                totalFood += MeatPerTime();
            }
        }

        return totalFood;
    }
}
