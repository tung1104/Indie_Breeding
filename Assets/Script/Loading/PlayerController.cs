using Newtonsoft.Json;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public void Init()
    {

    }

    public PlayerData GetPlayerData()
    {
        PlayerData playerData;
        string pData = Utils.GetData(PlayerPrefsConst.PLAYER_DATA);
        if (!string.IsNullOrEmpty(pData))
        {
            playerData = JsonConvert.DeserializeObject<PlayerData>(pData);
            return playerData;
        }

        return new PlayerData("", 0, new IncubatorInfo(1000, 1000, 1000, 1000, 1000), new NestInfo(1000, 1000, 1000, 1000), 1000);
    }

    public void SetPlayerData(PlayerData pData)
    {
        string dt = JsonConvert.SerializeObject(pData);
        Utils.SaveData(PlayerPrefsConst.PLAYER_DATA, dt);
    }

    public void SetPlayerData_IncubatorResources(IncubatorInfo incubatorRs)
    {
        PlayerData pData = GetPlayerData();
        PlayerData saveData = new PlayerData(pData.name, pData.level, incubatorRs, pData.nestRs, pData.food);
        SetPlayerData(saveData);
    }

    public void SetPlayerData_NestResources(NestInfo nestRs)
    {
        PlayerData pData = GetPlayerData();
        PlayerData saveData = new PlayerData(pData.name, pData.level, pData.incubatorRs, nestRs, pData.food);
        SetPlayerData(saveData);
    }

    public void SetPlayerData_Food(int food)
    {
        PlayerData pData = GetPlayerData();
        PlayerData saveData = new PlayerData(pData.name, pData.level, pData.incubatorRs, pData.nestRs, food);
        SetPlayerData(saveData);
    }

    public void SetPlayerData_Level(int level)
    {
        PlayerData pData = GetPlayerData();
        PlayerData saveData = new PlayerData(pData.name, level, pData.incubatorRs, pData.nestRs, pData.food);
        SetPlayerData(saveData);
    }

    public void SetPlayerData_Name(string name)
    {
        PlayerData pData = GetPlayerData();
        PlayerData saveData = new PlayerData(name, pData.level, pData.incubatorRs, pData.nestRs, pData.food);
        SetPlayerData(saveData);
    }
}
