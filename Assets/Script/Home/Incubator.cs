using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Incubator : MonoBehaviour
{
    public List<Transform> eggSlots;

    [NonSerialized] public List<Egg> eggs = new List<Egg>();
    [NonSerialized] public int numberEggCurrent;

    public void Init()
    {
        eggSlots.ForEach(egg => eggs.Add(null));
    }

    public int AddEgg(Egg egg, int index = -1)
    {
        if (numberEggCurrent < HomeController.Current.incubatorController.eggSlot)
        {
            var indexSlotEmpty = index >= 0 ? index : eggs.FindIndex(egg => egg == null);
            egg.transform.position = eggSlots[indexSlotEmpty].position;
            eggs[indexSlotEmpty] = egg;

            numberEggCurrent++;

            SaveData();

            return indexSlotEmpty;
        }

        return -1;
    }

    public int RemoveEgg(int index)
    {
        DestroyImmediate(eggs[index].gameObject);

        numberEggCurrent--;

        SaveData();

        return index;
    }

    public void SaveData()
    {
        var eggDatas = new List<EggData>();
        foreach (var egg in eggs)
        {
            if (egg == null) eggDatas.Add(null);
            else eggDatas.Add(egg.data);
        }
        Utils.SaveData(PlayerPrefsConst.EGG_DATA, JsonConvert.SerializeObject(eggDatas));
    }
}
