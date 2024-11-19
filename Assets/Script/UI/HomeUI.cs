using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeUI : MonoBehaviour
{
    public Text dinoCountText;
    public Text foodText;
    public Text boneText;
    public Button dinoListButton;
    public Button wildButton;
    public Button foodButton;
    public Button settingsButton;
    public Button bonesButton;
    private int basePopulation => HomeController.Current.upgradeController.GetIncubatorIndicatorByLevel(Data.IncubatorLevel).basePopulation;

    public void Init()
    {
        dinoListButton.onClick.AddListener(OnClickDinoListButton);
        wildButton.onClick.AddListener(OnClickWildButton);
        foodButton.onClick.AddListener(OnClickFoodButton);
        settingsButton.onClick.AddListener(OnClickSettingsButton);
        bonesButton.onClick.AddListener(OnClickBonesButton);
        UpdateView();
    }

    private void OnClickBonesButton()
    {
        var playerData = new PlayerData("", 0, new IncubatorInfo(0, 0, 0, 0, 0), new NestInfo(0, 0, 0, 0), 0);
        GameController.Current.playerController.SetPlayerData(playerData);
        UpdateView();
    }

    private void OnClickSettingsButton()
    {
    }

    private void OnClickFoodButton()
    {
        var playerData = new PlayerData("", 0, new IncubatorInfo(10000, 10000, 10000, 10000, 10000), new NestInfo(10000, 10000, 10000, 10000), 10000);
        GameController.Current.playerController.SetPlayerData(playerData);
        UpdateView();
    }

    private void OnClickWildButton()
    {
        SceneManager.LoadScene("02-Wild");
    }

    private void OnClickDinoListButton()
    {
        HomeUIController.Current.Get<DinoListPanel>().ChangeMode(DinoListPanel.Mode.Normal);
        HomeUIController.Current.Show<DinoListPanel>(true).OnClickDinoListItem = OnClickDinoListItem;
    }

    private void OnClickDinoListItem(DinoInfoData data)
    {
        HomeUIController.Current.Get<DinoDetailPanel>().data = data;
        HomeUIController.Current.Show<DinoDetailPanel>(true);
    }

    public void UpdateView()
    {
        var playerData = GameController.Current.playerController.GetPlayerData();
        foodText.text = playerData.food.ToString();
        boneText.text = playerData.incubatorRs.bone.ToString();
        dinoCountText.text = HomeController.Current.homeMutationController.dinoes.Count + "/" + basePopulation;
    }
}
