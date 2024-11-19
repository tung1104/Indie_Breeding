using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WildController : MonoBehaviour
{
    public static WildController Current;

    public List<Enemy> enemies = new();
    private Dictionary<int, HuntPartySimulator> huntPartySimulators = new();

    private void Awake()
    {
        Current = this;
    }

    private void Start()
    {
        if (Data.EnemiesInfo.Count == 0)
        {
            var id = 0;
            foreach (var config in GameController.Current.gameData.mapConfig[Data.MapLevel].enemyConfigs)
            {
                for (int i = 0; i < config.quant; i++)
                {
                    var enemy = CreateEnemyInfo(id, config, config.radiusRange);
                    Data.EnemiesInfo.Add(enemy);
                    id++;
                }
            }
            Data.SaveEnemiesInfo();
        }

        foreach (var enemyInfo in Data.EnemiesInfo)
        {
            var enemy = Instantiate(GameController.Current.gameData.enemiesPrefab[enemyInfo.idType], new Vector3(enemyInfo.positionX, 0, enemyInfo.positionZ), Quaternion.identity, transform);
            enemy.SetInfo(enemyInfo);
            enemies.Add(enemy);
        }

        foreach (var huntParty in Data.HuntParties)
        {
            CreateHuntParty(huntParty);
        }

        HuntPartyController.Current.OnFinishHunt += OnFinishHunt;
    }

    public EnemyInfo CreateEnemyInfo(int id, EnemyConfig config, Vector2 radiusRange)
    {
        EnemyInfo info = new();
        info.id = id;
        info.idType = config.dinoTypes[Random.Range(0, config.dinoTypes.Count)];
        int lv = Random.Range(config.levelMin, config.levelMax + 1);
        info.level = lv;
        int hp = Mathf.FloorToInt(lv * config.hp_coef / config.wild_coef) * 10;
        info.hp = hp;
        info.atk = lv - (hp / 10);
        var positionRandom = Random.insideUnitCircle.normalized * Random.Range(radiusRange.x, radiusRange.y);
        info.positionX = positionRandom.x;
        info.positionZ = positionRandom.y;
        info.radiusMin = radiusRange.x;
        info.radiusMax = radiusRange.y;

        return info;
    }

    public void CreateHuntParty(HuntParty huntParty)
    {
        var huntPartySimulator = new GameObject("HuntPartySimulator").AddComponent<HuntPartySimulator>();
        huntPartySimulator.Init(huntParty);
        huntPartySimulator.transform.SetParent(transform);
        huntPartySimulators.Add(huntParty.enemyId, huntPartySimulator);
    }

    private void OnFinishHunt(int id)
    {
        huntPartySimulators[id].Done();
        huntPartySimulators.Remove(id);
    }

    private void OnDestroy()
    {
        HuntPartyController.Current.OnFinishHunt -= OnFinishHunt;
    }
}
