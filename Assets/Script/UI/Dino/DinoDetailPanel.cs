using UnityEngine.UI;

public class DinoDetailPanel : Panel
{
    public DinoInfoData data;
    public Text levelText;
    public Text nameText;
    public Image avatar;
    public Text hpText;
    public Text atkText;
    public Text speedText;
    public Text foodPercentText;
    public Image progressMature;
    public Text timeRemainingText;
    public Button expelButton;
    public Button overlayButton;

    public override void Init()
    {
        expelButton.onClick.AddListener(OnClickExpelButton);
        overlayButton.onClick.AddListener(Hide);
    }

    public override void BeforeShow()
    {
        levelText.text = "Lv" + data.level.ToString();
        nameText.text = data.name.ToString();
        avatar.sprite = GameController.Current.dinoAvatars[data.id];
        hpText.text = data.hp.ToString();
        atkText.text = data.atk.ToString();
        speedText.text = data.speed.ToString();
    }

    private void OnClickExpelButton()
    {
        if (data.status == DinoStatus.mating)
        {
            uiController.Show<NotiPanel>().ChangeTitle("This dino is mating");
        }
        else
        {
            uiController.Show<ConfirmPanel>(true).OnConfirm = OnExpel;
        }
    }

    private void OnExpel()
    {
        HomeController.Current.homeMutationController.DestroyDino(data.id);
        uiController.Get<DinoListPanel>().Setup();
        Hide();
        HomeUIController.Current.homeUI.UpdateView();
    }

    private void Update()
    {
        if (data.status == DinoStatus.baby && data.timerMature < data.totalTimeMature)
        {
            foodPercentText.gameObject.SetActive(true);
            progressMature.transform.parent.gameObject.SetActive(true);
            foodPercentText.text = data.foodPercent + "%";
            progressMature.fillAmount = (float)data.timerMature / data.totalTimeMature;
            timeRemainingText.text = ((float)data.totalTimeMature - data.timerMature).Format();
        }
        else
        {
            foodPercentText.gameObject.SetActive(false);
            progressMature.transform.parent.gameObject.SetActive(false);
        }
    }
}
