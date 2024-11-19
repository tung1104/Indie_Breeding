using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DinoListPanel : Panel
{
    public enum Mode { Normal, Mating, Hunt }

    public Action<DinoInfoData> OnClickDinoListItem;
    public Action<List<DinoInfoData>> OnSelectDone;

    [SerializeField] private Transform content;
    [SerializeField] private DinoListItem dinoListItemPrefab;
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private Button sortDownButton;
    [SerializeField] private Button overlay;
    [SerializeField] private Toggle readyToggle;
    [SerializeField] private GameObject title;
    [SerializeField] private Toggle expelDinoToggle;
    [SerializeField] private Button expelButton;
    [SerializeField] private Image contentBackground;
    [SerializeField] private Button selectButton;

    private List<DinoListItem> dinoListItems = new();
    private bool sortDown;
    private int keyCurrent;
    private bool isFilterDinoReady = true;
    private Mode mode;

    public override void Init()
    {
        dropdown.onValueChanged.AddListener(ChangeDropdown);
        sortDownButton.onClick.AddListener(ChangeSortDown);
        overlay.onClick.AddListener(Hide);
        readyToggle.onValueChanged.AddListener(OnReadyToggleValueChange);
        expelDinoToggle.onValueChanged.AddListener(OnExpelDinoValueChange);
        expelButton.onClick.AddListener(OnClickExpelButton);
        selectButton.onClick.AddListener(OnClickSelectButton);
    }

    private void OnClickSelectButton()
    {
        var dinoInfoDatas = new List<DinoInfoData>();
        foreach (var dinoListItem in dinoListItems)
        {
            if (dinoListItem.IsSelected)
            {
                dinoInfoDatas.Add(dinoListItem.data);
            }
        }
        OnSelectDone?.Invoke(dinoInfoDatas);
    }

    private void OnClickExpelButton()
    {
        uiController.Show<ConfirmPanel>(true).OnConfirm = OnExpel;
    }

    private void OnExpel()
    {
        foreach (var item in dinoListItems)
        {
            if (item.IsSelected)
            {
                HomeController.Current.homeMutationController.DestroyDino(item.data.id);
            }
        }

        HomeUIController.Current.homeUI.UpdateView();

        Setup();
    }

    public void ChangeMode(Mode mode)
    {
        this.mode = mode;

        switch (mode)
        {
            case Mode.Normal:
                title.SetActive(false);
                expelDinoToggle.gameObject.SetActive(true);
                expelButton.gameObject.SetActive(true);
                selectButton.gameObject.SetActive(false);
                break;
            case Mode.Mating:
                title.SetActive(true);
                expelDinoToggle.gameObject.SetActive(false);
                expelButton.gameObject.SetActive(false);
                selectButton.gameObject.SetActive(false);
                break;
            case Mode.Hunt:
                title.SetActive(true);
                expelDinoToggle.gameObject.SetActive(false);
                expelButton.gameObject.SetActive(false);
                selectButton.gameObject.SetActive(true);
                break;
        }
    }

    private void OnReadyToggleValueChange(bool isOn)
    {
        isFilterDinoReady = isOn;
        Setup();
    }

    private void OnExpelDinoValueChange(bool isOn)
    {
        foreach (var item in dinoListItems)
        {
            item.Select(false);
        }

        expelButton.gameObject.SetActive(false);

        if (isOn)
        {
            contentBackground.color = new Color(1, 0, 0, .5f);
        }
        else
        {
            contentBackground.color = new Color(0.9f, 0.9f, 0.9f, 1);
        }
    }

    public override void BeforeShow()
    {
        Setup();
    }

    public void Setup()
    {
        expelDinoToggle.isOn = false;
        expelButton.gameObject.SetActive(false);

        var i = 0;
        foreach (var dinoInfoData in GameController.Current.mutationController.dinoInfoDatas.Values)
        {
            DinoListItem dinoListItem = null;
            if (i < dinoListItems.Count)
            {
                dinoListItem = dinoListItems[i];
            }
            else
            {
                dinoListItem = Instantiate(dinoListItemPrefab, content);
                dinoListItems.Add(dinoListItem);
            }
            SetupItem(dinoListItem, dinoInfoData);
            i++;
        }

        while (i < dinoListItems.Count)
        {
            var dinoListItem = dinoListItems[i];
            dinoListItem.gameObject.SetActive(false);
            i++;
        }

        Sort();

        for (i = 0; i < GameController.Current.mutationController.dinoInfoDatas.Count; i++)
        {
            dinoListItems[i].transform.SetAsLastSibling();
        }
    }

    private void SetupItem(DinoListItem dinoListItem, DinoInfoData dinoInfoData)
    {
        dinoListItem.Setup(dinoInfoData);
        dinoListItem.ShowOverlay(dinoListItem.data.status != DinoStatus.idle);
        dinoListItem.gameObject.SetActive(!isFilterDinoReady || dinoListItem.data.status == DinoStatus.idle);
        dinoListItem.OnClick = (data) =>
        {
            if (expelDinoToggle.isOn)
            {
                if (data.status == DinoStatus.mating)
                {
                    uiController.Show<NotiPanel>(true).ChangeTitle("This dino is busy");
                }
                else
                {
                    dinoListItem.Toggle();
                    CheckExpelButton();
                }
            }
            else
            {
                OnClickDinoListItem?.Invoke(data);
            }
        };
    }

    private void CheckExpelButton()
    {
        foreach (var item in dinoListItems)
        {
            if (item.IsSelected)
            {
                expelButton.gameObject.SetActive(true);
                return;
            }
        }

        expelButton.gameObject.SetActive(false);
    }

    private void ChangeDropdown(int key)
    {
        keyCurrent = key;
        Setup();
    }

    private void ChangeSortDown()
    {
        sortDown = !sortDown;
        sortDownButton.transform.Rotate(0, 0, 180);
        Setup();
    }

    private void Sort()
    {
        var list = new List<DinoListItem>();
        for (int i = 0; i < GameController.Current.mutationController.dinoInfoDatas.Count; i++) list.Add(dinoListItems[i]);

        switch (keyCurrent)
        {
            case 0:
                list = SortByName(list);
                list = SortByLevel(list);
                break;
            case 1:
                list = SortByName(list);
                break;
            case 2:
                list = SortByName(list);
                list = SortByHealth(list);
                break;
            case 3:
                list = SortByName(list);
                list = SortByAttack(list);
                break;
            case 4:
                list = SortByName(list);
                list = SortBySpeed(list);
                break;
        }

        for (int i = 0; i < GameController.Current.mutationController.dinoInfoDatas.Count; i++) dinoListItems[i] = list[i];
    }

    private List<DinoListItem> SortByLevel(List<DinoListItem> list)
    {
        if (sortDown) list.Sort((a, b) => a.data.level.CompareTo(b.data.level));
        else list.Sort((a, b) => b.data.level.CompareTo(a.data.level));

        return list;
    }

    private List<DinoListItem> SortByName(List<DinoListItem> list)
    {
        if (sortDown) list.Sort((a, b) => b.data.name.CompareTo(a.data.name));
        else list.Sort((a, b) => a.data.name.CompareTo(b.data.name));

        return list;
    }

    private List<DinoListItem> SortByHealth(List<DinoListItem> list)
    {
        if (sortDown) list.Sort((a, b) => a.data.hp.CompareTo(b.data.hp));
        else list.Sort((a, b) => b.data.hp.CompareTo(a.data.hp));

        return list;
    }

    private List<DinoListItem> SortByAttack(List<DinoListItem> list)
    {
        if (sortDown) list.Sort((a, b) => a.data.atk.CompareTo(b.data.atk));
        else list.Sort((a, b) => b.data.atk.CompareTo(a.data.atk));

        return list;
    }

    private List<DinoListItem> SortBySpeed(List<DinoListItem> list)
    {
        if (sortDown) list.Sort((a, b) => a.data.speed.CompareTo(b.data.speed));
        else list.Sort((a, b) => b.data.speed.CompareTo(a.data.speed));

        return list;
    }
}
