using System.Collections.Generic;
using UnityEngine;

public class HomeMutationController : MonoBehaviour
{
    public Dictionary<int, Dino> dinoes = new();

    public void Init()
    {
        foreach (var data in GameController.Current.mutationController.dinoInfoDatas.Values)
        {
            var dino = CreateDino(data.id, data.headId, data.bodyId, data.limbsId, data.textureId, data.accessory, data.hp, data.atk, data.speed, data.name, data.status, data.totalTimeMature, data.timerMature, data.foodPercent);
            dino.data = data;
            dino.locomotion.ContinueMove();
        }
    }

    public Dino CreateDino0()
    {
        var dino = CreateDino(-1, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, null, 10, 1, 1, "", DinoStatus.idle);
        return dino;
    }

    public Dino CreateDino1()
    {
        Vector2Int defV = new Vector2Int(0, 1);
        var dino = CreateDino(-1, defV, defV, defV, new Vector2Int(1, 0), null, 10, 1, 1, "", DinoStatus.idle);
        return dino;
    }

    public Dino CreateDino(int id, Vector2Int hId, Vector2Int bId, Vector2Int lId, Vector2Int ttId, Accessory[] accs, int hp, int atk, int speed, string name, DinoStatus status, int totalTimeMature = 0, int timerMature = 0, int foodPercent = 45)
    {
        var dino = GameController.Current.mutationController.CreateDino(id, hId, bId, lId, ttId, accs, hp, atk, speed, name, status, totalTimeMature, timerMature, foodPercent);

        dino.Init(transform, true);

        dinoes.Add(dino.data.id, dino);

        return dino;
    }

    public void RemoveData(DinoInfoData data)
    {
        GameController.Current.mutationController.Remove(data);
    }

    public void AddData(DinoInfoData data)
    {
        GameController.Current.mutationController.Add(data);
    }

    public void SaveData()
    {
        GameController.Current.mutationController.Save();
    }

    public Dino HybridByMomDad(DinoInfoData momInfo, DinoInfoData dadInfo)
    {
        #region PATH_MUTATION
        Vector2Int hId;
        PathRarity[] headP = GameController.Current.gameData.dinosPaths.heads;
        bool isMuH = Utils.CheckMutationHead();
        if (!isMuH)
        {
            hId = Random_0_1() ? momInfo.headId : dadInfo.headId;
        }
        else
        {
            int ra = Utils.RandomByRarity();
            hId = new Vector2Int(ra, Random.Range(0, headP[ra].pathInfos.Length));
        }

        Vector2Int bId;
        PathRarity[] bodyP = GameController.Current.gameData.dinosPaths.heads;
        bool isMuB = Utils.CheckMutationBody();
        if (!isMuB)
        {
            bId = Random_0_1() ? momInfo.bodyId : dadInfo.bodyId;
        }
        else
        {
            int ra = Utils.RandomByRarity();
            bId = new Vector2Int(ra, Random.Range(0, bodyP[ra].pathInfos.Length));
        }

        Vector2Int lId;
        PathRarity[] limbsP = GameController.Current.gameData.dinosPaths.heads;
        bool isMuL = Utils.CheckMutationlimbs();
        if (!isMuL)
        {
            lId = Random_0_1() ? momInfo.headId : dadInfo.headId;
        }
        else
        {
            int ra = Utils.RandomByRarity();
            lId = new Vector2Int(ra, Random.Range(0, limbsP[ra].pathInfos.Length));
        }
        #endregion

        #region TEXTURE_MUTATION
        Vector2Int ttId;
        TextureRarity[] textureRarities = GameController.Current.gameData.dinoTextures;
        bool isMuTT = Utils.CheckMutationTexture();
        if (!isMuTT)
        {
            ttId = Random_0_1() ? momInfo.textureId : dadInfo.textureId;
        }
        else
        {
            int ra = Utils.RandomByRarity();
            ttId = new Vector2Int(ra, Random.Range(0, textureRarities[ra].textures.Length));
        }
        #endregion

        #region ACCESSORIES_MUTATION
        Accessory[] accs;
        bool isMuAccHat = Utils.CheckMutationHat();
        bool isMuAccHand = Utils.CheckMutationHand();
        bool isMuAccWing = Utils.CheckMutationWings();
        bool isMuAccTail = Utils.CheckMutationTails();

        bool isMuAcc = isMuAccHat || isMuAccHand || isMuAccWing || isMuAccTail;
        //Debug.Log("isMuAcc is : " + isMuAcc);
        if (!isMuAcc)
        {
            accs = Random_0_1() ? momInfo.accessory : dadInfo.accessory;
        }
        else
        {
            Accessories[] accessories = GameController.Current.gameData.dinoAccessories;
            List<Accessory> accsMu = new List<Accessory>();

            if (isMuAccHat)
            {
                int ra = Utils.RandomByRarity();
                Accessory acc = new Accessory(AccType.hat, new Vector2Int(ra, Random.Range(0, accessories[0].dinoAccRarities[ra].acc.Length)));
                accsMu.Add(acc);
            }
            if (isMuAccHand)
            {
                int ra = Utils.RandomByRarity();
                Accessory acc = new Accessory(AccType.hand, new Vector2Int(ra, Random.Range(0, accessories[1].dinoAccRarities[ra].acc.Length)));
                accsMu.Add(acc);
            }
            if (isMuAccWing)
            {
                int ra = Utils.RandomByRarity();
                Accessory acc = new Accessory(AccType.wing, new Vector2Int(ra, Random.Range(0, accessories[2].dinoAccRarities[ra].acc.Length)));
                accsMu.Add(acc);
            }
            if (isMuAccTail)
            {
                int ra = Utils.RandomByRarity();
                Accessory acc = new Accessory(AccType.tail, new Vector2Int(ra, Random.Range(0, accessories[3].dinoAccRarities[ra].acc.Length)));
                accsMu.Add(acc);
            }

            accs = accsMu.ToArray();
        }
        #endregion
        int hp = Random_0_1() ? momInfo.hp : dadInfo.hp;
        int atk = Random_0_1() ? momInfo.atk : dadInfo.atk;
        int speed = Random_0_1() ? momInfo.speed : dadInfo.speed;

        //Check have mutation
        //Debug.Log("Mutation is " + (isMuH || isMuB || isMuL || isMuTT || isMuAcc));
        if (isMuH || isMuB || isMuL || isMuTT || isMuAcc)
        {
            int r = Random.Range(0, 3);
            hp += (r == 0) ? Random.Range(1, 4) * 10 : (Utils.CheckMutationHp() ? Random.Range(1, 4) * 10 : 0);
            atk += (r == 1) ? Random.Range(1, 4) : (Utils.CheckMutationAtk() ? Random.Range(1, 4) : 0);
            speed += (r == 2) ? Random.Range(1, 4) : (Utils.CheckMutationSpeed() ? Random.Range(1, 4) : 0);
        }

        var dino = CreateDino(-1, hId, bId, lId, ttId, accs, hp, atk, speed, "", DinoStatus.baby);
        return dino;
    }

    private bool Random_0_1()
    {
        return Random.Range(0, 2) == 1;
    }

    public void DestroyDino(int id)
    {
        RemoveData(dinoes[id].data);
        SaveData();

        Destroy(dinoes[id].gameObject);
        dinoes.Remove(id);
    }
}
