using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DinoListItem : MonoBehaviour
{
    public Button Button => button;
    public Action<DinoInfoData> OnClick;
    public DinoInfoData data;
    public bool IsSelected;

    [SerializeField] private Image background;
    [SerializeField] private Image avatar;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI health;
    [SerializeField] private TextMeshProUGUI attack;
    [SerializeField] private TextMeshProUGUI speed;
    [SerializeField] private TextMeshProUGUI state;
    [SerializeField] private Button button;
    [SerializeField] private Button overlay;

    private void Start()
    {
        button.onClick.AddListener(OnClickButton);
        overlay.onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        OnClick?.Invoke(data);
    }

    public void Setup(DinoInfoData dinoInfoData)
    {
        data = dinoInfoData;

        avatar.sprite = GameController.Current.dinoAvatars[data.id];
        name.text = data.name;
        level.text = "Lv" + data.level.ToString();
        health.text = data.hp.ToString();
        attack.text = data.atk.ToString();
        speed.text = data.speed.ToString();
        state.text = data.status.ToString().ToUpper();
    }

    public void ShowOverlay(bool isEnable)
    {
        overlay.gameObject.SetActive(isEnable);
    }

    public void Toggle()
    {
        IsSelected = !IsSelected;
        Select(IsSelected);
    }

    public void Select(bool isSelected)
    {
        IsSelected = isSelected;
        if (IsSelected)
        {
            background.color = new Color(1, 0, 0, 0.5f);
        }
        else
        {
            background.color = new Color(1, 1, 1, 1);
        }
    }
}
