using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NestSelector : MonoBehaviour
{
    public DinoInfoData data;

    [SerializeField] private Button addButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private GameObject info;
    [SerializeField] private Image avatar;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI health;
    [SerializeField] private TextMeshProUGUI attack;
    [SerializeField] private TextMeshProUGUI speed;
    [SerializeField] private bool isLeft;

    public NestPanel nestPanel;

    public void Init(NestPanel nestPanel)
    {
        this.nestPanel = nestPanel;
        addButton.onClick.AddListener(OnClickAddButton);
        removeButton.onClick.AddListener(OnClickRemoveButton);
    }

    private void OnClickAddButton()
    {
        HomeUIController.Current.Get<DinoListPanel>().ChangeMode(DinoListPanel.Mode.Mating);
        HomeUIController.Current.Show<DinoListPanel>(true).OnClickDinoListItem = OnClickDinoListDetail;
    }

    private void OnClickDinoListDetail(DinoInfoData data)
    {
        if (data.status == DinoStatus.baby)
        {
            HomeUIController.Current.Show<NotiPanel>().ChangeTitle("A child needs to be fully grown first");
            return;
        }

        if (data.status == DinoStatus.mating)
        {
            HomeUIController.Current.Show<NotiPanel>().ChangeTitle("This dino is mating");
            return;
        }

        HomeUIController.Current.Hide<DinoListPanel>();

        data.status = DinoStatus.mating;
        var dino = HomeController.Current.homeMutationController.dinoes[data.id];
        dino.isInNest = true;
        HomeController.Current.homeMutationController.SaveData();

        if (isLeft) nestPanel.nest.nestMutationInfo.momId = data.id;
        else nestPanel.nest.nestMutationInfo.dadId = data.id;

        nestPanel.nest.AddDino(dino, isLeft);
        nestPanel.nest.CheckMating();
        HomeController.Current.nestController.Save();

        Add(data);

        HomeUIController.Current.Show<NestPanel>();
    }

    public void Add(DinoInfoData data)
    {
        this.data = data;

        avatar.sprite = GameController.Current.dinoAvatars[data.id];
        name.text = this.data.name;
        level.text = "Lv" + this.data.level.ToString();
        health.text = this.data.hp.ToString();
        attack.text = this.data.atk.ToString();
        speed.text = this.data.speed.ToString();

        ShowInfo(true);
        ShowButton(false);

        nestPanel.CheckProgress();
    }

    private void OnClickRemoveButton()
    {
        data.status = DinoStatus.idle;
        HomeController.Current.homeMutationController.SaveData();

        if (isLeft)
        {
            nestPanel.nest.nestMutationInfo.momId = -1;
            nestPanel.nest.RemoveDino(true);
        }
        else
        {
            nestPanel.nest.nestMutationInfo.dadId = -1;
            nestPanel.nest.RemoveDino(false);
        }

        nestPanel.nest.CheckMating();
        HomeController.Current.nestController.Save();

        Remove();
    }

    public void Remove()
    {
        data = null;

        ShowInfo(false);
        ShowButton(true);

        nestPanel.CheckProgress();
    }

    public void ShowInfo(bool isShow)
    {
        info.SetActive(isShow);
    }

    public void ShowButton(bool isShow)
    {
        addButton.gameObject.SetActive(isShow);
    }
}
