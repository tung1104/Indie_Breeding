using Kamgam.UGUIWorldImage;
using System;
using UnityEngine;
using UnityEngine.UI;

public class IncubatorItem : MonoBehaviour
{
    public Egg egg;
    public Button button;
    public Text timeRemainingTxt;
    public WorldImage worldImage;

    private void Update()
    {
        if (egg != null && egg.data.mom != null && egg.data.dad != null)
        {
            var timePassed = (DateTime.Now - egg.data.startTime).TotalSeconds;
            var totalLevel = (egg.data.mom.level + egg.data.dad.level);
            var mattingCoefficient = GameController.Current.gameData.mattingCoefficient;
            var hatchingBoost = HomeController.Current.upgradeController.GetCurrentIncubatorHatchingBoost() / 100f;
            var timeHatching = totalLevel * mattingCoefficient * (1 - hatchingBoost);
            if (timeHatching > timePassed)
            {
                timeRemainingTxt.text = (timeHatching - timePassed).Format();
                timeRemainingTxt.gameObject.SetActive(true);
            }
            else
            {
                timeRemainingTxt.gameObject.SetActive(false);
            }
        }
        else
        {
            timeRemainingTxt.gameObject.SetActive(false);
        }
    }
}
