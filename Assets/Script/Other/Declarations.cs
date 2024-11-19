using System;
using System.Collections.Generic;
using UnityEngine;

public enum DinoStatus
{
    mating,
    hunting,
    egg_robbing,
    robbing,
    recovering,
    baby,
    idle
}

public enum EggStt
{
    wait,
    ready_open
}

public enum AccType
{
    hat,
    hand,
    wing,
    tail
}

public enum PathType
{
    head,
    body,
    limbs
}

[Serializable]
public class PathRarity
{
    public PathInfo[] pathInfos;
}

[Serializable]
public class DinosPaths
{
    public PathRarity[] heads;
    public PathRarity[] bodies;
    public PathRarity[] limbs;
}

[Serializable]
public class Accessories
{
    public DinoAccRarity[] dinoAccRarities;
}

[Serializable]
public class DinoAccRarity
{
    public GameObject[] acc;
}

[Serializable]
public class DinoAccessories
{
    public Accessories[] accessories;
}
public class Accessory
{
    public AccType type;
    public Vector2Int id;

    public Accessory(AccType type, Vector2Int id)
    {
        this.type = type;
        this.id = id;
    }
}


[Serializable]
public class TextureRarity
{
    public Texture[] textures;
}

[Serializable]
public class EggData
{
    public DinoInfoData mom;
    public DinoInfoData dad;
    public DateTime startTime;
}

public class DinoInfoData
{
    public int id;
    public string name;
    public Vector2Int headId;
    public Vector2Int bodyId;
    public Vector2Int limbsId;
    public Vector2Int textureId;
    public Accessory[] accessory;
    public int level;
    public int hp;
    public int atk;
    public int speed;
    public DinoStatus status = DinoStatus.idle;
    public int totalTimeMature;
    public int timerMature;
    public int foodPercent;

    public DinoInfoData(int id, string name, int level, Vector2Int headId, Vector2Int bodyId, Vector2Int limbsId, Vector2Int textureId, Accessory[] accessory, int hp, int atk, int speed, DinoStatus status, int totalTimeMature, int timerMature, int foodPercent)
    {
        this.id = id;
        this.name = name;
        this.level = level;
        this.headId = headId;
        this.bodyId = bodyId;
        this.limbsId = limbsId;
        this.textureId = textureId;
        this.accessory = accessory;
        this.hp = hp;
        this.atk = atk;
        this.speed = speed;
        this.status = status;
        this.totalTimeMature = totalTimeMature;
        this.timerMature = timerMature;
        this.foodPercent = foodPercent;
    }
}

[Serializable]
public class IncubatorInfo
{
    public int wood;
    public int tooth;
    public int bone;
    public int stone;
    public int specialItem;

    public IncubatorInfo(int wood, int tooth, int bone, int stone, int specialItem)
    {
        this.wood = wood;
        this.tooth = tooth;
        this.bone = bone;
        this.stone = stone;
        this.specialItem = specialItem;
    }
}

[Serializable]
public class IncubatorIndicator
{
    public int basePopulation;
    public int maxNest;
    public float hachingBootPercent;
    public int huntingPartySize;
    public int eggSlot;
    public int maxPop;

    public IncubatorIndicator(int basePopulation, int maxNest, float hachingBootPercent, int huntingPartySize, int eggSlot, int maxPop)
    {
        this.basePopulation = basePopulation;
        this.maxNest = maxNest;
        this.hachingBootPercent = hachingBootPercent;
        this.huntingPartySize = huntingPartySize;
        this.eggSlot = eggSlot;
        this.maxPop = maxPop;
    }
}

[Serializable]
public class NestInfo
{
    public int straw;
    public int hide;
    public int claw;
    public int crystal;

    public NestInfo(int straw, int hide, int claw, int crystal)
    {
        this.straw = straw;
        this.hide = hide;
        this.claw = claw;
        this.crystal = crystal;
    }
}

[Serializable]
public class NestIndicatorAndCost
{
    public float nestBoost;
    public NestInfo cost;
}

[Serializable]
public class NestMutationInfo
{
    public int level;
    public int momId;
    public int dadId;
    public DateTime startMatingTime;
    public float matingTime;

    public NestMutationInfo(int level, int momId, int dadId, DateTime startMatingTime, float matingTime)
    {
        this.level = level;
        this.momId = momId;
        this.dadId = dadId;
        this.startMatingTime = startMatingTime;
        this.matingTime = matingTime;
    }
}

public class PlayerData
{
    public string name;
    public int level;
    public IncubatorInfo incubatorRs;
    public NestInfo nestRs;
    public int food;

    public PlayerData(string name, int level, IncubatorInfo incubatorRs, NestInfo nestRs, int food)
    {
        this.name = name;
        this.level = level;
        this.incubatorRs = incubatorRs;
        this.nestRs = nestRs;
        this.food = food;
    }
}

[Serializable]
public class HuntParty
{
    public List<int> dinosPartyId = new();
    public DateTime startHuntTimer;
    public float huntTime;
    public int enemyId;
    public bool isWin;
}

[Serializable]
public class MapConfig
{
    public List<EnemyConfig> enemyConfigs;
}

[Serializable]
public class EnemyConfig
{
    public Vector2 radiusRange;
    public int levelMin;
    public int levelMax;
    public int quant;
    public int hp_coef;
    public int atk_coef;
    public int wild_coef;
    public List<int> dinoTypes = new List<int>();
}

public class EnemyInfo
{
    public int id;
    public int idType;
    public int level;
    public int hp;
    public int atk;
    public float positionX;
    public float positionZ;
    public float radiusMin;
    public float radiusMax;
}