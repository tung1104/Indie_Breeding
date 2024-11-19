using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

[CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/GameData", order = 1)]
public class GameData : ScriptableObject
{
    public string CSV_URL;

    [Tooltip("PathRarity 0 to 5: Normal to Mystic \nEach PathRarity have list of path")]
    public DinosPaths dinosPaths;

    [Tooltip("PathRarity 0 to 5: Normal to Mystic \nEach PathRarity have list of texture")]
    public TextureRarity[] dinoTextures;

    public Texture[] eggTextures;

    [Tooltip("Dino Accessories 0 to 3 : hat, hand, wing, tails \nDinoAccRarity 0 to 5: Normal to Mystic \nEach DinoAccRarity have list of accessories")]
    public Accessories[] dinoAccessories;

    public List<int> rarityMutation;
    public int headMutationPercent;
    public int bodyMutationPercent;
    public int limbsMutationPercent;
    public int textureMutationPercent;
    public int hatMutationPercent;
    public int wingsMutationPercent;
    public int handMutationPercent;
    public int tailMutationPercent;
    public int hpMutationPercent;
    public int atkMutationPercent;
    public int speedMutationPercent;
    public int mattingCoefficient;
    public int nestCostStep;
    public int MCONST;
    public int GTIME;
    public int wild_timemax;
    public List<IncubatorInfo> incubatorUpgradeCosts;
    public List<IncubatorIndicator> incubatorIndicators;
    public List<NestIndicatorAndCost> nestIndicatorAndCosts;
    public List<MapConfig> mapConfig;
    public Material dinoMat;
    public List<int> meatHunterCoef;
    public List<Enemy> enemiesPrefab;
    public Dino dinoPrefab;
    public LineRenderer lineRendererPrefab;
    public Nest nestPrefab;
    public NestEmpty nestEmptyPrefab;
    public Egg eggPrefab;
    public RenderTexture hatchingRenderTexture;
    public RenderTexture captureRenderTexture;
    public IncubatorItem incubatorItemPrefab;

    public IEnumerator Init()
    {
        yield return ReadGoogleSheetCsv(OnlineConfig.MUTATION_RARITY_GID, GetRarityMutationConfig);
        yield return ReadGoogleSheetCsv(OnlineConfig.RENERAL_GID, GetReneralConfig);
        yield return ReadGoogleSheetCsv(OnlineConfig.INCUBATOR_UPGRADE_GID, GetIncubatorUpgradeConfig);
    }

    IEnumerator ReadGoogleSheetCsv(string gid, UnityAction<string> action)
    {
        string csvUrl = CSV_URL + gid;

        UnityWebRequest www = UnityWebRequest.Get(csvUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string csvData = www.downloadHandler.text;
            action(csvData);
        }
    }

    private void GetRarityMutationConfig(string data)
    {
        string[] lines = data.Split('\n');
        List<int> rarities = new List<int>();
        foreach (var l in lines)
        {
            if (int.TryParse(l, out int rs))
            {
                rarities.Add(rs);
            }
        }
        rarityMutation = rarities;
    }

    private void GetReneralConfig(string data)
    {
        string[] lines = data.Split('\n');
        string[] spl = lines[1].Split(',');
        if (int.TryParse(spl[0], out int rs0))
        {
            headMutationPercent = rs0;
        }
        if (int.TryParse(spl[1], out int rs1))
        {
            bodyMutationPercent = rs1;
        }
        if (int.TryParse(spl[2], out int rs2))
        {
            limbsMutationPercent = rs2;
        }
        if (int.TryParse(spl[3], out int rs3))
        {
            textureMutationPercent = rs3;
        }
        if (int.TryParse(spl[4], out int rs4))
        {
            hatMutationPercent = rs4;
        }
        if (int.TryParse(spl[5], out int rs5))
        {
            wingsMutationPercent = rs5;
        }
        if (int.TryParse(spl[6], out int rs6))
        {
            handMutationPercent = rs6;
        }
        if (int.TryParse(spl[7], out int rs7))
        {
            tailMutationPercent = rs7;
        }
        if (int.TryParse(spl[8], out int rs8))
        {
            hpMutationPercent = rs8;
        }
        if (int.TryParse(spl[9], out int rs9))
        {
            atkMutationPercent = rs9;
        }
        if (int.TryParse(spl[10], out int rs10))
        {
            speedMutationPercent = rs10;
        }
        if (int.TryParse(spl[11], out int rs11))
        {
            mattingCoefficient = rs11;
        }
        if (int.TryParse(spl[12], out int rs12))
        {
            nestCostStep = rs12;
        }
        if (int.TryParse(spl[13], out int rs13))
        {
            MCONST = rs13;
        }
        if (int.TryParse(spl[14], out int rs14))
        {
            GTIME = rs14;
        }
    }

    public void GetIncubatorUpgradeConfig(string data)
    {
        string[] lines = data.Split('\n');
        List<IncubatorInfo> costs = new List<IncubatorInfo>();
        for (int i = 1; i < lines.Length; i++)
        {
            string[] spl = lines[i].Split(',');
            IncubatorInfo cost = new IncubatorInfo(int.Parse(spl[1]), int.Parse(spl[2]), int.Parse(spl[3]), int.Parse(spl[4]), int.Parse(spl[5]));
            costs.Add(cost);
        }
        incubatorUpgradeCosts = costs;
    }
}
