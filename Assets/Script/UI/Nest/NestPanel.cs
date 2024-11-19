using System;
using UnityEngine;
using UnityEngine.UI;

public class NestPanel : Panel
{
    [SerializeField] private NestSelector leftNestSelector;
    [SerializeField] private NestSelector rightNestSelector;
    [SerializeField] private Button overlay;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private GameObject progressBar;
    [SerializeField] private Image progress;
    [SerializeField] private Text remainingTimeTxt;

    public Nest nest;

    public override void Init()
    {
        leftNestSelector.Init(this);
        rightNestSelector.Init(this);

        overlay.onClick.AddListener(Hide);
        upgradeButton.onClick.AddListener(OnClickUpgradeButton);
    }

    public void SetNest(Nest nest)
    {
        this.nest = nest;
    }

    public override void BeforeShow()
    {
        leftNestSelector.Remove();
        rightNestSelector.Remove();

        foreach (var dinoDataInfo in GameController.Current.mutationController.dinoInfoDatas.Values)
        {
            if (dinoDataInfo.id == nest.nestMutationInfo.momId) leftNestSelector.Add(dinoDataInfo);
            else if (dinoDataInfo.id == nest.nestMutationInfo.dadId) rightNestSelector.Add(dinoDataInfo);
        }

        CheckUpgradeButton();
        CheckProgress();
    }

    public void CheckUpgradeButton()
    {
        upgradeButton.gameObject.SetActive(nest.nestMutationInfo.level + 1 < GameController.Current.gameData.nestIndicatorAndCosts.Count);
    }

    public void CheckProgress()
    {
        progressBar.SetActive(leftNestSelector.data != null && rightNestSelector.data != null);
    }

    private void OnClickUpgradeButton()
    {
        uiController.Get<NestUpgraderPanel>().SetNest(nest);
        uiController.Get<NestUpgraderPanel>().OnHide = CheckUpgradeButton;
        uiController.Show<NestUpgraderPanel>(true);
    }

    private void Update()
    {
        var timePassed = (float)(DateTime.Now - nest.nestMutationInfo.startMatingTime).TotalSeconds;
        var timeMating = nest.nestMutationInfo.matingTime;

        progress.fillAmount = timePassed / (timeMating - 1);

        remainingTimeTxt.text = (timeMating - timePassed).Format();
    }
}
