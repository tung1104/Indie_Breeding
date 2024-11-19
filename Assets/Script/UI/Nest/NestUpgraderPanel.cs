using UnityEngine.UI;

public class NestUpgraderPanel : Panel
{
    public Button overlay;
    public Button upgradeButton;
    public Text levelTxt;
    public Text boostTxt;
    public Text strawTxt;
    public Text hideTxt;
    public Text clawTxt;
    public Text crystal;

    public Nest nest;

    public override void Init()
    {
        overlay.onClick.AddListener(Hide);
        upgradeButton.onClick.AddListener(OnClickUpgradeButton);
    }

    public void SetNest(Nest nest)
    {
        this.nest = nest;
    }

    public override void BeforeShow()
    {
        var level = nest.nestMutationInfo.level;
        levelTxt.text = "Lv." + (level + 1) + " >> " + "Lv." + (level + 2);
        var boostCurrent = HomeController.Current.upgradeController.GetNestBoostByIndex(nest.index);
        var boostNext = GameController.Current.gameData.nestIndicatorAndCosts[level + 1].nestBoost;
        boostTxt.text = "Mating boost + " + boostCurrent + "% >> " + boostNext + "%";

        var playerData = GameController.Current.playerController.GetPlayerData();
        var nestIndicatorAndCosts = GameController.Current.gameData.nestIndicatorAndCosts;
        strawTxt.text = playerData.nestRs.straw + "/" + nestIndicatorAndCosts[level].cost.straw;
        hideTxt.text = playerData.nestRs.hide + "/" + nestIndicatorAndCosts[level].cost.hide;
        clawTxt.text = playerData.nestRs.claw + "/" + nestIndicatorAndCosts[level].cost.claw;
        if (nestIndicatorAndCosts[level].cost.crystal != 0)
            crystal.text = playerData.nestRs.crystal + "/" + nestIndicatorAndCosts[level].cost.crystal;
    }

    private void OnClickUpgradeButton()
    {
        if (HomeController.Current.upgradeController.CheckAndUpdateNest(nest.index))
        {
            nest.nestMutationInfo.level++;
            HomeController.Current.upgradeController.SetNestByIndex(nest.index, nest.nestMutationInfo);
            Hide();
        }
        else
        {
            uiController.Get<NotiPanel>().ChangeTitle("Not enough items!");
            uiController.Show<NotiPanel>(true);
        }
    }
}
