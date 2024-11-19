using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public static class Data
{
    public static int IncubatorLevel
    {
        get => PlayerPrefs.GetInt(PlayerPrefsConst.INCUBATOR_LEVEL, 0);
        set => PlayerPrefs.SetInt(PlayerPrefsConst.INCUBATOR_LEVEL, value);
    }

    public static int MapLevel
    {
        get => PlayerPrefs.GetInt(PlayerPrefsConst.MAP_LEVEL, 0);
        set => PlayerPrefs.SetInt(PlayerPrefsConst.MAP_LEVEL, value);
    }

    private static List<HuntParty> huntParties;
    public static List<HuntParty> HuntParties
    {
        get
        {
            if (huntParties == null)
            {
                var data = PlayerPrefs.GetString(PlayerPrefsConst.HUNT_PARTIES, "");
                huntParties = string.IsNullOrEmpty(data) ? new() : JsonConvert.DeserializeObject<List<HuntParty>>(data);
            }
            return huntParties;
        }
    }
    public static void SaveHuntParties() => PlayerPrefs.SetString(PlayerPrefsConst.HUNT_PARTIES, JsonConvert.SerializeObject(huntParties));

    private static List<EnemyInfo> enemiesInfo;
    public static List<EnemyInfo> EnemiesInfo
    {
        get
        {
            if (enemiesInfo == null)
            {
                var data = PlayerPrefs.GetString(PlayerPrefsConst.ENEMIES_INFO, "");
                enemiesInfo = string.IsNullOrEmpty(data) ? new() : JsonConvert.DeserializeObject<List<EnemyInfo>>(data);
            }
            return enemiesInfo;
        }
    }
    public static void SaveEnemiesInfo() => PlayerPrefs.SetString(PlayerPrefsConst.ENEMIES_INFO, JsonConvert.SerializeObject(enemiesInfo));
}
