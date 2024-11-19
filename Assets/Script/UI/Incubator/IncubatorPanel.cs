using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IncubatorPanel : Panel
{
    public Button upgradeButton;
    public Text totalEggNumTxt;
    public List<IncubatorItem> incubatorItems;
    public Button closeBtn;
    public Transform content;

    public override void Init()
    {
        upgradeButton.onClick.AddListener(OnClickUpgradeButton);
        closeBtn.onClick.AddListener(Hide);
        for (int i = 0; i < 24; i++)
        {
            incubatorItems.Add(null);
        }
    }

    public override void BeforeShow()
    {
        HomeController.Current.eggViewerController.Init();
        CheckUpgrade();
    }

    public void CheckUpgrade()
    {
        upgradeButton.gameObject.SetActive(!HomeController.Current.incubatorController.isMaxLevel);
        totalEggNumTxt.text = HomeController.Current.incubatorController.incubator.numberEggCurrent + "/" + HomeController.Current.incubatorController.eggSlot;
    }

    private void OnClickUpgradeButton()
    {
        uiController.Show<IncubatorUpgraderPanel>(true);
    }

    public void InsertEgg(Egg egg, int index)
    {
        egg.indexInPanel = 1;
        foreach (var item in incubatorItems)
        {
            if (item != null && item.gameObject.activeSelf)
            {
                egg.indexInPanel++;
            }
        }
        var incubatorItem = Instantiate(GameController.Current.gameData.incubatorItemPrefab, content);
        incubatorItem.button.onClick.AddListener(() => OnClickEggButton(index));
        incubatorItem.egg = egg;
        incubatorItem.worldImage.AddWorldObject(HomeController.Current.eggViewerController.eggViewers[index].transform);
        incubatorItems[index] = incubatorItem;
    }

    private void OnClickEggButton(int index)
    {
        uiController.Get<HatchingPreviewPanel>().SetElementAt(index);
        uiController.Show<HatchingPreviewPanel>();
    }

    public void ShowItem(int index)
    {
        incubatorItems[index].gameObject.SetActive(true);
    }

    public void HideItem(int index)
    {
        incubatorItems[index].gameObject.SetActive(false);
        foreach (var item in incubatorItems)
        {
            if (item != null && item.egg.indexInPanel > incubatorItems[index].egg.indexInPanel)
            {
                item.egg.indexInPanel--;
            }
        }
    }
}
