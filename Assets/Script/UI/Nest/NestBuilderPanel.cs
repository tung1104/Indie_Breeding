using UnityEngine.UI;

public class NestBuilderPanel : Panel
{
    public Text text;
    public Button overlay;
    public Button buildButton;
    public NestEmpty nestEmpty;

    public override void Init()
    {
        overlay.onClick.AddListener(Hide);
        buildButton.onClick.AddListener(OnClickBuildButton);
    }

    public override void BeforeShow()
    {
        var playerData = GameController.Current.playerController.GetPlayerData();

        text.text = playerData.nestRs.straw + "/" + HomeController.Current.upgradeController.GetNextCostBuildNest();
    }

    public void SetNestEmpty(NestEmpty nestEmpty)
    {
        this.nestEmpty = nestEmpty;
    }

    public void OnClickBuildButton()
    {
        Hide();
        nestEmpty.gameObject.SetActive(false);
        HomeController.Current.nestController.Build();
        HomeController.Current.nestController.Save();
    }
}
