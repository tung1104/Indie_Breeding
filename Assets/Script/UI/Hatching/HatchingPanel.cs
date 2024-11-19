using Kamgam.UGUIWorldImage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HatchingPanel : Panel
{
    public WorldImage worldImage;
    public Button receiveBtn;
    public Button expulsionBtn;
    public Text levelTxt;
    public Text nameTxt;
    public Text healthTxt;
    public Text attackTxt;
    public Text speedTxt;
    public GameObject content;
    public GameObject button;

    private Dino dinoJustCreated;

    private HomeMutationController mutationController => HomeController.Current.homeMutationController;
    private List<Egg> eggs => HomeController.Current.incubatorController.incubator.eggs;

    public override void Init()
    {
        worldImage.RenderTexture = GameController.Current.gameData.hatchingRenderTexture;

        receiveBtn.onClick.AddListener(OnClickReceiveButton);
        expulsionBtn.onClick.AddListener(OnClickExpulsionButton);
    }

    public void CreateEgg(int idEgg)
    {
        worldImage.RemoveWorldObject(worldImage.GetWorldObjectAt(0));
        worldImage.AddWorldObject(HomeController.Current.eggViewerController.eggViewers[idEgg].transform);

        if (mutationController.dinoes.Count == 0)
        {
            dinoJustCreated = mutationController.CreateDino0();
            expulsionBtn.interactable = false;

            mutationController.SaveData();
        }
        else if (mutationController.dinoes.Count == 1)
        {
            dinoJustCreated = mutationController.CreateDino1();
            expulsionBtn.interactable = false;

            mutationController.SaveData();
        }
        else
        {
            dinoJustCreated = mutationController.HybridByMomDad(eggs[idEgg].data.mom, eggs[idEgg].data.dad);
            expulsionBtn.interactable = true;
        }

        dinoJustCreated.Init(null, false, "3DToUI", true);
        dinoJustCreated.locomotion.Hatch();

        HideContent();
        HomeController.Current.eggViewerController.eggViewers[idEgg].OpenEgg(dinoJustCreated);
        HomeController.Current.eggViewerController.eggViewers[idEgg].OpenDone = OpenDone;
    }

    private void HideContent()
    {
        content.SetActive(false);
        button.SetActive(false);
    }

    private void OpenDone()
    {
        ShowContent();

        StartCoroutine(Capture());
    }

    private IEnumerator Capture()
    {
        worldImage.RenderTexture = GameController.Current.gameData.captureRenderTexture;

        yield return new WaitForEndOfFrame();

        var dinoAvatars = GameController.Current.dinoAvatars;
        var sprite = GameController.Current.gameData.captureRenderTexture.Save("Dino" + dinoJustCreated.data.id).ToSprite();
        dinoAvatars.Add(dinoJustCreated.data.id, sprite);

        worldImage.RenderTexture = GameController.Current.gameData.hatchingRenderTexture;
        worldImage.ForceRenderTextureUpdate();
    }

    private void ShowContent()
    {
        levelTxt.text = "Lv" + dinoJustCreated.data.level;
        nameTxt.text = dinoJustCreated.data.name;
        healthTxt.text = dinoJustCreated.data.hp.ToString();
        attackTxt.text = dinoJustCreated.data.atk.ToString();
        speedTxt.text = dinoJustCreated.data.speed.ToString();

        content.SetActive(true);
        button.SetActive(true);
    }

    private void OnClickReceiveButton()
    {
        mutationController.AddData(dinoJustCreated.data);
        mutationController.SaveData();

        dinoJustCreated.Init(HomeController.Current.homeMutationController.transform, true);
        dinoJustCreated.locomotion.ContinueMove();

        if (HomeController.Current.incubatorController.incubator.numberEggCurrent > 0) uiController.Show<HatchingPreviewPanel>();
        else uiController.Show<IncubatorPanel>();

        HomeUIController.Current.homeUI.UpdateView();
    }

    private void OnClickExpulsionButton()
    {
        HomeController.Current.homeMutationController.DestroyDino(dinoJustCreated.data.id);

        if (HomeController.Current.incubatorController.incubator.numberEggCurrent > 0) uiController.Show<HatchingPreviewPanel>();
        else uiController.Show<IncubatorPanel>();
    }
}
