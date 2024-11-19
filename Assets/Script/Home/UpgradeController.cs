using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeController : MonoBehaviour
{
    public void Init()
    {

    }

    public IncubatorIndicator GetIncubatorIndicatorByLevel(int lv)
    {
        List<IncubatorIndicator> incubatorIndicators = GameController.Current.gameData.incubatorIndicators;
        if (lv < incubatorIndicators.Count)
        {
            return incubatorIndicators[lv];
        }
        else
        {
            return null;
        }
    }

    public bool CheckIncubatorMaxLevel()
    {
        return Data.IncubatorLevel >= GameController.Current.gameData.incubatorUpgradeCosts.Count;
    }

    public float GetCurrentIncubatorHatchingBoost()
    {
        List<IncubatorIndicator> incubatorIndicators = GameController.Current.gameData.incubatorIndicators;
        if (Data.IncubatorLevel < incubatorIndicators.Count)
        {
            return incubatorIndicators[Data.IncubatorLevel].hachingBootPercent;
        }
        else
        {
            return 0f;
        }
    }

    public bool CheckAndUpdateIncubator()
    {
        if (Data.IncubatorLevel >= GameController.Current.gameData.incubatorUpgradeCosts.Count)
        {
            return false;
        }

        GameController gameController = GameController.Current;
        var playerIncuRs = gameController.playerController.GetPlayerData().incubatorRs;
        if (playerIncuRs != null)
        {
            IncubatorInfo currCost = gameController.gameData.incubatorUpgradeCosts[Data.IncubatorLevel];
            if (playerIncuRs.wood < currCost.wood || playerIncuRs.tooth < currCost.tooth ||
                playerIncuRs.bone < currCost.bone || playerIncuRs.stone < currCost.stone ||
                playerIncuRs.specialItem < currCost.specialItem)
            {
                return false;
            }
            else
            {
                Data.IncubatorLevel++;
                IncubatorInfo newPlayerRs = new IncubatorInfo(playerIncuRs.wood - currCost.wood,
                    playerIncuRs.tooth - currCost.tooth,
                    playerIncuRs.bone - currCost.bone,
                    playerIncuRs.stone - currCost.stone,
                    playerIncuRs.specialItem - currCost.specialItem);
                gameController.playerController.SetPlayerData_IncubatorResources(newPlayerRs);
                return true;
            }
        }

        return false;
    }

    //--------NEST---------
    public List<NestMutationInfo> GetNestData()
    {
        List<NestMutationInfo> nmi = new List<NestMutationInfo>();
        string nData = Utils.GetData(PlayerPrefsConst.NEST_DATA);
        if (!string.IsNullOrEmpty(nData))
        {
            nmi = JsonConvert.DeserializeObject<List<NestMutationInfo>>(nData);
        }

        return nmi;
    }

    public void SetNestData(List<NestMutationInfo> nmi)
    {
        Utils.SaveData(PlayerPrefsConst.NEST_DATA, JsonConvert.SerializeObject(nmi));
    }

    public void SetNestByIndex(int ind, NestMutationInfo info)
    {
        List<NestMutationInfo> nmi = GetNestData();
        if (ind < nmi.Count)
        {
            nmi[ind] = info;
        }
        SetNestData(nmi);
    }

    public float GetNestBoostByIndex(int idNest)
    {
        List<NestMutationInfo> nmi = GetNestData();
        List<NestIndicatorAndCost> nestIndicatorAndCost = GameController.Current.gameData.nestIndicatorAndCosts;
        if (idNest < nmi.Count)
        {
            if (nmi[idNest].level < nestIndicatorAndCost.Count)
            {
                return nestIndicatorAndCost[nmi[idNest].level].nestBoost;
            }
        }

        return 0;
    }

    public bool CheckNestMaxLevel(int idNest)
    {
        List<NestMutationInfo> nmi = GetNestData();
        List<NestIndicatorAndCost> nestIndicatorAndCost = GameController.Current.gameData.nestIndicatorAndCosts;

        if (idNest < nmi.Count)
        {
            return !(nmi[idNest].level < nestIndicatorAndCost.Count);
        }
        else
        {
            return false;
        }
    }

    public int GetNextCostBuildNest()
    {
        List<NestMutationInfo> nmi = GetNestData();
        return nmi.Count * GameController.Current.gameData.nestCostStep;
    }

    public float CalculateMatingCountdown(int momLv, int dadLv)
    {
        return (dadLv + 1 + momLv + 1) * GameController.Current.gameData.mattingCoefficient;
    }

    public bool CheckAndUpdateNest(int idNest)
    {
        int lv;
        if (CheckNestMaxLevel(idNest))
        {
            return false;
        }
        else
        {
            lv = GetNestData()[idNest].level;
        }

        GameController gameController = GameController.Current;
        var playerNestRs = gameController.playerController.GetPlayerData().nestRs;
        if (playerNestRs != null)
        {
            NestInfo currCost = gameController.gameData.nestIndicatorAndCosts[lv].cost;
            if (playerNestRs.straw < currCost.straw || playerNestRs.hide < currCost.hide ||
                playerNestRs.claw < currCost.claw || playerNestRs.crystal < currCost.crystal)
            {
                return false;
            }
            else
            {
                var curInfo = GetNestData()[idNest];
                if (curInfo != null)
                {
                    curInfo.level = lv + 1;
                    SetNestByIndex(idNest, curInfo);
                }

                NestInfo newPlayerRs = new NestInfo(playerNestRs.straw - currCost.straw,
                    playerNestRs.hide - currCost.hide,
                    playerNestRs.crystal - currCost.claw,
                    playerNestRs.crystal - currCost.crystal);
                gameController.playerController.SetPlayerData_NestResources(newPlayerRs);
                return true;
            }
        }

        return false;
    }
}
