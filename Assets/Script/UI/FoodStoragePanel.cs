using UnityEngine;
using UnityEngine.UI;

public class FoodStoragePanel : Panel
{
    public Button overlayButton;
    public Text meatCountNeedText;
    public Text meatCountText;
    public Text message;

    public override void Init()
    {
        overlayButton.onClick.AddListener(Hide);
    }

    public override void BeforeShow()
    {
        var meatCountNeed = HomeController.Current.foodsController.GetTotalFoodForAllDino();
        var meatCount = GameController.Current.playerController.GetPlayerData().food;

        meatCountNeedText.text = meatCountNeed.ToString();
        meatCountText.text = meatCount.ToString();

        if (meatCountNeed > meatCount)
        {
            message.text = "You will run out of meat soon\nGet more by hunting or visiting the store";
            message.color = Color.red;
        }
        else
        {
            message.text = "You have enough meat to feed your children";
            message.color = Color.green;
        }
    }
}
