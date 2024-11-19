using UnityEngine.UI;

public class IncubatorUpgraderPanel : Panel
{
    public Button overlay;
    public Button upgradeButton;
    public Text levelTxt;
    public Text basePopulationTxt;
    public Text maxNestTxt;
    public Text huntingPartySizeTxt;
    public Text hatchingBoostTxt;
    public Text woodTxt;
    public Text toothTxt;
    public Text boneTxt;
    public Text stoneTxt;
    public Text specialItemTxt;

    public override void Init()
    {
        overlay.onClick.AddListener(Hide);
        upgradeButton.onClick.AddListener(OnClickUpgradeButton);
    }

    public override void BeforeShow()
    {
        var incubatorInfo = GameController.Current.playerController.GetPlayerData().incubatorRs;
        var incubatorIndicators = GameController.Current.gameData.incubatorIndicators;
        var incubatorUpgradeCosts = GameController.Current.gameData.incubatorUpgradeCosts;
        levelTxt.text = "Lv." + Data.IncubatorLevel + " >> " + "Lv." + (Data.IncubatorLevel + 1);
        basePopulationTxt.text = "Base population: " + incubatorIndicators[Data.IncubatorLevel].basePopulation + " >> " + incubatorIndicators[Data.IncubatorLevel + 1].basePopulation;
        maxNestTxt.text = "Max nest: " + incubatorIndicators[Data.IncubatorLevel].maxNest + " >> " + incubatorIndicators[Data.IncubatorLevel + 1].maxNest;
        huntingPartySizeTxt.text = "Hunting party size: " + incubatorIndicators[Data.IncubatorLevel].huntingPartySize + " >> " + incubatorIndicators[Data.IncubatorLevel].huntingPartySize;
        hatchingBoostTxt.text = "Hatching Boost + " + incubatorIndicators[Data.IncubatorLevel].hachingBootPercent + "% >> " + incubatorIndicators[Data.IncubatorLevel].hachingBootPercent + "%";
        woodTxt.text = incubatorInfo.wood + "/" + incubatorUpgradeCosts[Data.IncubatorLevel].wood;
        toothTxt.text = incubatorInfo.tooth + "/" + incubatorUpgradeCosts[Data.IncubatorLevel].tooth;
        boneTxt.text = incubatorInfo.bone + "/" + incubatorUpgradeCosts[Data.IncubatorLevel].bone;
        stoneTxt.text = incubatorInfo.stone + "/" + incubatorUpgradeCosts[Data.IncubatorLevel].stone;
        specialItemTxt.text = incubatorInfo.specialItem + "/" + incubatorUpgradeCosts[Data.IncubatorLevel].specialItem;
    }

    private void OnClickUpgradeButton()
    {
        if (HomeController.Current.upgradeController.CheckAndUpdateIncubator())
        {
            HomeController.Current.nestController.Upgrade();
            uiController.Get<IncubatorPanel>().CheckUpgrade();
            HomeUIController.Current.homeUI.UpdateView();
            Hide();
        }
        else
        {
            uiController.Get<NotiPanel>().ChangeTitle("Not enough items!");
            uiController.Show<NotiPanel>(true);
        }
    }
}
