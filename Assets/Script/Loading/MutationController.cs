using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MutationController : MonoBehaviour
{
    public Dictionary<int, DinoInfoData> dinoInfoDatas = new();

    public void Init()
    {
        string json = Utils.GetData(PlayerPrefsConst.DINO_DATA);
        if (!string.IsNullOrEmpty(json))
        {
            var datas = JsonConvert.DeserializeObject<List<DinoInfoData>>(json);
            foreach (var data in datas)
            {
                dinoInfoDatas.Add(data.id, data);
            }
        }
    }

    public void Add(DinoInfoData data)
    {
        dinoInfoDatas.Add(data.id, data);
    }

    public void Remove(DinoInfoData data)
    {
        dinoInfoDatas.Remove(data.id);
    }

    public void Save()
    {
        var value = JsonConvert.SerializeObject(dinoInfoDatas.Values.ToList());
        Utils.SaveData(PlayerPrefsConst.DINO_DATA, value);
    }

    public Dino CreateDino(int id)
    {
        var data = dinoInfoDatas[id];
        var dino = CreateDino(data.id, data.headId, data.bodyId, data.limbsId, data.textureId, data.accessory, data.hp, data.atk, data.speed, data.name, data.status, data.totalTimeMature, data.timerMature, data.foodPercent);
        return dino;
    }

    public Dino CreateDino(int id, Vector2Int hId, Vector2Int bId, Vector2Int lId, Vector2Int ttId, Accessory[] accs, int hp, int atk, int speed, string name, DinoStatus status, int totalTimeMature = 0, int timerMature = 0, int foodPercent = 45)
    {
        var dino = Instantiate(GameController.Current.gameData.dinoPrefab);

        if (id == -1)
        {
            id = int.Parse(Utils.GetData(PlayerPrefsConst.DINO_COUNT)) + 1;
            Utils.SaveData(PlayerPrefsConst.DINO_COUNT, id.ToString());
        }
        if (name == "")
        {
            name = "Dino" + id.ToString("D3");
        }
        dino.name = name;

        var gameData = GameController.Current.gameData;

        DinosPaths dinosPaths = gameData.dinosPaths;
        var hPath = Instantiate(dinosPaths.heads[hId.x].pathInfos[hId.y], dino.transform);
        var bPath = Instantiate(dinosPaths.bodies[bId.x].pathInfos[bId.y], dino.transform);
        var lPath = Instantiate(dinosPaths.limbs[lId.x].pathInfos[lId.y], dino.transform);

        TextureRarity[] tts = gameData.dinoTextures;
        Material mat = new Material(gameData.dinoMat);
        mat.mainTexture = tts[ttId.x].textures[ttId.y];

        hPath.mesh.material = mat;
        bPath.mesh.material = mat;
        lPath.mesh.material = mat;

        Transform accPar = null;
        int typeId = 0;
        Accessories[] dinoAccessories = gameData.dinoAccessories;

        if (accs != null)
        {
            foreach (var ac in accs)
            {
                switch (ac.type)
                {
                    case AccType.hat:
                        accPar = GetChild(hPath.transform, JointName.HEAD_JOINT);
                        typeId = 0;
                        break;

                    case AccType.hand:
                        accPar = GetChild(lPath.transform, JointName.HAND_RIGHT_JOINT);
                        typeId = 1;
                        break;

                    case AccType.wing:
                        accPar = GetChild(bPath.transform, JointName.SPINE_JOINT);
                        typeId = 2;
                        break;

                    case AccType.tail:
                        accPar = GetChild(bPath.transform, JointName.TAIL_JOINT);
                        typeId = 3;
                        break;
                }
                GameObject curAcc = Instantiate(dinoAccessories[typeId].dinoAccRarities[ac.id.x].acc[ac.id.y], accPar);
                curAcc.transform.localPosition = Vector3.zero;
            }
        }

        var lv = Utils.CalDinoLevel(hp, atk, speed);
        var totalTime = totalTimeMature == 0 ? HomeController.Current.foodsController.TotalTimeMature(lv) : totalTimeMature;

        dino.data = new DinoInfoData(id, name, lv, hId, bId, lId, ttId, accs, hp, atk, speed, status, totalTime, timerMature, foodPercent);

        return dino;
    }

    private Transform GetChild(Transform par, string name)
    {
        Transform[] childs = par.GetComponentsInChildren<Transform>();

        foreach (Transform c in childs)
        {
            if (c.name == name)
            {
                return c;
            }
        }
        return null;
    }

    [ContextMenu("Create 10 Dino")]
    public void Create10Dino()
    {
        for (int i = 0; i < 10; i++)
        {
            var dino = CreateDino(-1, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, null, 10, 1, 1, "", DinoStatus.idle);
            Add(dino.data);
            GameController.Current.dinoAvatars.Add(i, null);
        }
        Save();
    }
}
