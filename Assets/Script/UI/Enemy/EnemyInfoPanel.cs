using UnityEngine;
using UnityEngine.UI;

public class EnemyInfoPanel : Panel
{
    public EnemyInfo enemyInfo;
    public Button overlay;
    public Text nameText;
    public Text levelText;
    public Text meatText;
    public Button huntButton;
    public RectTransform board;
    public Vector3 positionScreen;

    public override void Init()
    {
        overlay.onClick.AddListener(Hide);
        huntButton.onClick.AddListener(OnClickHuntButton);
    }

    public override void BeforeShow()
    {
        levelText.text = "Lv." + enemyInfo.level;
        meatText.text = "" + GameController.Current.gameData.meatHunterCoef[0] * enemyInfo.level;
        if (positionScreen.x + 150 > Screen.width)
        {
            positionScreen.x = Screen.width - 150;
        }
        if (positionScreen.x - 150 < 0)
        {
            positionScreen.x = 150;
        }
        if (positionScreen.y + 360 > Screen.height)
        {
            positionScreen.y = Screen.height - 360;
        }
        if (positionScreen.y < 0)
        {
            positionScreen.y = 0;
        }
        board.anchoredPosition = positionScreen;
    }

    private void OnClickHuntButton()
    {
        WildUIController.Current.Get<HuntPanel>().enemyInfo = enemyInfo;
        WildUIController.Current.Show<HuntPanel>();
    }
}
