using System;
using System.Collections.Generic;
using UnityEngine;

public class NestController : MonoBehaviour
{
    public List<NestPlace> nestPlaces;

    private List<NestMutationInfo> nestMutationInfos;
    private int maxNest => HomeController.Current.upgradeController.GetIncubatorIndicatorByLevel(Data.IncubatorLevel).maxNest;
    private NestEmpty nestEmpty;

    public void Init()
    {
        foreach (var dino in HomeController.Current.homeMutationController.dinoes.Values) dino.isInNest = dino.data.status == DinoStatus.mating;

        nestMutationInfos = HomeController.Current.upgradeController.GetNestData();
        if (nestMutationInfos.Count == 0)
        {
            nestMutationInfos.Add(new NestMutationInfo(0, -1, -1, DateTime.MaxValue, -1));
            Save();
        }

        foreach (var nestPlace in nestPlaces)
        {
            nestPlace.Init();
        }

        for (int i = 0; i <= nestMutationInfos.Count; i++)
        {
            if (i < nestMutationInfos.Count) Create(i);
            else CreateEmpty(i);
        }
    }

    public void Create(int index)
    {
        nestPlaces[index].Build(nestMutationInfos[index], index);
    }

    public void CreateEmpty(int index)
    {
        if (nestMutationInfos.Count < maxNest)
        {
            nestPlaces[index].Prepare();
        }
    }

    public void Build()
    {
        nestMutationInfos.Add(new NestMutationInfo(0, -1, -1, DateTime.MaxValue, -1));
        Create(nestMutationInfos.Count - 1);
        CreateEmpty(nestMutationInfos.Count);
    }

    public void Upgrade()
    {
        if (!nestEmpty || !nestEmpty.gameObject.activeSelf)
        {
            CreateEmpty(nestMutationInfos.Count);
        }
    }

    public void Save()
    {
        HomeController.Current.upgradeController.SetNestData(nestMutationInfos);
    }
}
