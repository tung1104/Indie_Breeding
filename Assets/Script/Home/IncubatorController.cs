using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class IncubatorController : MonoBehaviour
{
    public Incubator incubator;
    public bool isMaxLevel => Data.IncubatorLevel >= GameController.Current.gameData.incubatorIndicators.Count - 1;
    public int eggSlot => GameController.Current.gameData.incubatorIndicators[Data.IncubatorLevel].eggSlot;
    private IncubatorPanel incubatorPanel => HomeUIController.Current.Get<IncubatorPanel>();
    private HatchingPreviewPanel hatchingPreviewPanel => HomeUIController.Current.Get<HatchingPreviewPanel>();

    public void Init()
    {
        incubator.Init();

        string eggJson = Utils.GetData(PlayerPrefsConst.EGG_DATA);

        if (string.IsNullOrEmpty(eggJson))
        {
            var egg = Instantiate(GameController.Current.gameData.eggPrefab);
            AddEgg(egg, 0);

            egg = Instantiate(GameController.Current.gameData.eggPrefab);
            AddEgg(egg, 1);
        }
        else
        {
            var eggDatas = JsonConvert.DeserializeObject<List<EggData>>(eggJson);
            for (int i = 0; i < eggDatas.Count; i++)
            {
                if (eggDatas[i] != null)
                {
                    var egg = Instantiate(GameController.Current.gameData.eggPrefab);
                    egg.data = eggDatas[i];

                    AddEgg(egg, i);
                }
            }
        }
    }

    public void AddEgg(Egg egg, int index = -1)
    {
        index = incubator.AddEgg(egg, index);
        if (index != -1)
        {
            HomeUIController.Current.Get<IncubatorPanel>().InsertEgg(egg, index);
            incubatorPanel.ShowItem(index);
        }
    }

    public void RemoveEgg(int index)
    {
        if (incubator.eggs[index] != null)
        {
            incubatorPanel.HideItem(index);
            incubator.RemoveEgg(index);
            if (!hatchingPreviewPanel.SetNextElement()) hatchingPreviewPanel.SetPreviousElement();
        }
    }
}
