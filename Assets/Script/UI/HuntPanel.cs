using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HuntPanel : Panel
{
    public Button overlay;
    public Text titleText;
    public Image enemyAvatar;
    public Text enemyHpText;
    public Text enemyAtkText;
    public Image dinoAvatar;
    public Text dinoHpText;
    public Text dinoAtkText;
    public Text groupText;
    public Button slotDinoButtonPrefab;
    public Text timeHuntText;
    public Button huntButton;
    public Button closeButton;
    public Sprite emptySprite;

    public EnemyInfo enemyInfo;

    private List<Button> slotDinoButtons = new();
    private int huntPartySize;
    private HuntParty huntParty;
    private List<DinoInfoData> dinoInfosSorted;

    public override void Init()
    {
        overlay.onClick.AddListener(Hide);
        huntButton.onClick.AddListener(OnClickHuntButton);
        closeButton.onClick.AddListener(OnClickCloseButton);

        huntPartySize = GameController.Current.gameData.incubatorIndicators[Data.IncubatorLevel].huntingPartySize;

        for (int i = 0; i < huntPartySize; i++)
        {
            var slotButton = Instantiate(slotDinoButtonPrefab, slotDinoButtonPrefab.transform.parent);
            slotButton.gameObject.SetActive(true);
            var id = i;
            slotButton.onClick.AddListener(() => OnClickSlotButton(id));
            slotDinoButtons.Add(slotButton);
        }
        slotDinoButtonPrefab.gameObject.SetActive(false);
    }

    public void OnClickSlotButton(int id)
    {
        WildUIController.Current.Get<DinoListPanel>().ChangeMode(DinoListPanel.Mode.Hunt);
        WildUIController.Current.Show<DinoListPanel>(true);
    }

    public override void BeforeShow()
    {
        SortDinoInfoDatasByHp();

        CreateHuntParty();

        UpdateView();
    }

    private void SortDinoInfoDatasByHp()
    {
        dinoInfosSorted = new List<DinoInfoData>();
        foreach (var dinoInfo in GameController.Current.mutationController.dinoInfoDatas.Values.ToList())
        {
            if (dinoInfo.status == DinoStatus.idle)
            {
                dinoInfosSorted.Add(dinoInfo);
            }
        }
        dinoInfosSorted.Sort((a, b) => a.hp.CompareTo(b.hp));
    }

    private void CreateHuntParty()
    {
        huntParty = new HuntParty
        {
            dinosPartyId = new(),
            enemyId = enemyInfo.id,
        };

        for (int i = 0; i < dinoInfosSorted.Count; i++)
        {
            if (i < huntPartySize)
            {
                var dinoInfo = dinoInfosSorted[i];

                huntParty.dinosPartyId.Add(dinoInfo.id);
                huntParty.isWin = IsHuntWin(huntParty);

                if (huntParty.isWin) break;
            }
        }

        huntParty.huntTime = GetHuntTime(huntParty.dinosPartyId, enemyInfo.id);
    }

    public float GetHuntTime(List<int> dinosPartyId, int enemyId)
    {
        Vector3 enemyPos = WildController.Current.enemies[enemyId].transform.position * 100;
        float wild_timebase = (enemyPos.magnitude / 3000) * GameController.Current.gameData.wild_timemax;

        float average_spd = 0;
        foreach (var index in dinosPartyId)
        {
            average_spd += GameController.Current.mutationController.dinoInfoDatas[index].speed;
        }
        average_spd /= dinosPartyId.Count;

        float wild_runtime = wild_timebase / (1 + average_spd / 100);

        return wild_runtime;
    }

    public bool IsHuntWin(HuntParty party)
    {
        List<int> idDinos = party.dinosPartyId;
        float average_dino_hp = 0;
        float average_dino_atk = 0;
        foreach (var idD in idDinos)
        {
            average_dino_hp += GameController.Current.mutationController.dinoInfoDatas[idD].hp;
            average_dino_atk += GameController.Current.mutationController.dinoInfoDatas[idD].atk;
        }
        average_dino_hp /= idDinos.Count;
        average_dino_atk /= idDinos.Count;

        var enemyInfo = WildController.Current.enemies[party.enemyId].info;
        float enemy_turn = Mathf.Ceil(average_dino_hp / enemyInfo.atk) * idDinos.Count;
        float dino_turn = Mathf.Ceil(enemyInfo.hp / (idDinos.Count * average_dino_atk));

        return dino_turn <= enemy_turn;
    }

    private void UpdateView()
    {
        var totalHp = 0;
        var totalAtk = 0;
        foreach (var idDino in huntParty.dinosPartyId)
        {
            totalHp += GameController.Current.mutationController.dinoInfoDatas[idDino].hp;
            totalAtk += GameController.Current.mutationController.dinoInfoDatas[idDino].atk;
        }
        dinoHpText.text = totalHp.ToString();
        dinoAtkText.text = totalAtk.ToString();

        enemyHpText.text = enemyInfo.hp.ToString();
        enemyAtkText.text = enemyInfo.atk.ToString();

        foreach (var slotButton in slotDinoButtons) slotButton.image.sprite = emptySprite;

        for (int i = 0; i < huntParty.dinosPartyId.Count; i++)
        {
            var idDino = huntParty.dinosPartyId[i];
            slotDinoButtons[i].image.sprite = GameController.Current.dinoAvatars[idDino];
        }

        timeHuntText.text = huntParty.huntTime.Format();
    }

    private void OnClickHuntButton()
    {
        huntParty.startHuntTimer = DateTime.Now;
        Data.HuntParties.Add(huntParty);
        Data.SaveHuntParties();
        WildController.Current.CreateHuntParty(huntParty);
        Hide();
    }

    private void OnClickCloseButton()
    {
        Hide();
    }
}
