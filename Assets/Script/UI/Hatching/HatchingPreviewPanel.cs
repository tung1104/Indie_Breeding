using Kamgam.UGUIWorldImage;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HatchingPreviewPanel : Panel
{
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button previousBtn;
    [SerializeField] private Text countTxt;
    [SerializeField] private Button openBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private WorldImage worldImage;
    [SerializeField] private Text timeRemainingTxt;

    private int indexCurrent;
    private List<Egg> eggs => HomeController.Current.incubatorController.incubator.eggs;
    private int basePopulation => HomeController.Current.upgradeController.GetIncubatorIndicatorByLevel(Data.IncubatorLevel).basePopulation;

    public override void Init()
    {
        openBtn.onClick.AddListener(OnClickOpenButton);
        closeBtn.onClick.AddListener(OnClickCloseButton);
        nextBtn.onClick.AddListener(OnClickNextButton);
        previousBtn.onClick.AddListener(OnClickPreviousButton);
    }

    private void OnClickOpenButton()
    {
        if (HomeController.Current.homeMutationController.dinoes.Count >= basePopulation)
        {
            uiController.Get<NotiPanel>().ChangeTitle("Reached maximum population limit");
            uiController.Show<NotiPanel>(true);
        }
        else
        {
            uiController.Show<HatchingPanel>().CreateEgg(indexCurrent);
            HomeController.Current.incubatorController.RemoveEgg(indexCurrent);
            HomeController.Current.eggViewerController.Init();
        }
    }

    public void OnClickCloseButton()
    {
        uiController.Show<IncubatorPanel>();
    }

    private void OnClickNextButton()
    {
        SetNextElement();
    }

    public bool SetNextElement()
    {
        for (int i = indexCurrent + 1; i < eggs.Count; i++)
        {
            if (eggs[i] != null)
            {
                SetElementAt(i);
                return true;
            }
        }

        return false;
    }

    private void OnClickPreviousButton()
    {
        SetPreviousElement();
    }

    public bool SetPreviousElement()
    {
        for (int i = indexCurrent - 1; i >= 0; i--)
        {
            if (eggs[i] != null)
            {
                SetElementAt(i);
                return true;
            }
        }

        return false;
    }

    public void SetElementAt(int index = 0)
    {
        indexCurrent = index;

        worldImage.RemoveWorldObject(worldImage.GetWorldObjectAt(0));
        worldImage.AddWorldObject(HomeController.Current.eggViewerController.eggViewers[index].transform);

        countTxt.text = eggs[index].indexInPanel + "/" + HomeController.Current.incubatorController.incubator.numberEggCurrent;
    }

    private void Update()
    {
        var egg = eggs[indexCurrent];

        if (egg != null && egg.data.mom != null && egg.data.dad != null)
        {
            var timePassed = (DateTime.Now - egg.data.startTime).TotalSeconds;
            var totalLevel = egg.data.mom.level + egg.data.dad.level;
            var mattingCoefficient = GameController.Current.gameData.mattingCoefficient;
            var hatchingBoost = HomeController.Current.upgradeController.GetCurrentIncubatorHatchingBoost() / 100f;
            var timeHatching = totalLevel * mattingCoefficient * (1 - hatchingBoost);
            if (timeHatching > timePassed)
            {
                timeRemainingTxt.text = (timeHatching - timePassed).Format();
                timeRemainingTxt.gameObject.SetActive(true);
                openBtn.interactable = false;
            }
            else
            {
                timeRemainingTxt.gameObject.SetActive(false);
                openBtn.interactable = true;
            }
        }
        else
        {
            timeRemainingTxt.gameObject.SetActive(false);
        }
    }
}
