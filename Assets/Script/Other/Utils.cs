using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Utils
{
    public static void SaveData(string key, string value)
    {
        if (value == PlayerPrefs.GetString(key))
        {
            return;
        }

        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    public static string GetData(string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            return "";
        }

        return PlayerPrefs.GetString(key);
    }

    public static int RandomByRarity(List<int> rarityList)
    {
        int randomValue = UnityEngine.Random.Range(0, 100);
        int cumulative = 0;

        int chosenIndex = -1;
        for (int i = 0; i < rarityList.Count; i++)
        {
            cumulative += rarityList[i];
            if (randomValue < cumulative)
            {
                chosenIndex = i;
                break;
            }
        }

        return chosenIndex;
    }

    public static bool CheckMutation(int percent)
    {
        int r = UnityEngine.Random.Range(0, 100);
        return (r < percent);
    }

    //=======================================
    public static int RandomByRarity()
    {
        return RandomByRarity(GameController.Current.gameData.rarityMutation);
    }

    public static bool CheckMutationHead()
    {
        return CheckMutation(GameController.Current.gameData.headMutationPercent);
    }
    public static bool CheckMutationBody()
    {
        return CheckMutation(GameController.Current.gameData.bodyMutationPercent);
    }
    public static bool CheckMutationlimbs()
    {
        return CheckMutation(GameController.Current.gameData.limbsMutationPercent);
    }
    public static bool CheckMutationTexture()
    {
        return CheckMutation(GameController.Current.gameData.textureMutationPercent);
    }
    public static bool CheckMutationHat()
    {
        return CheckMutation(GameController.Current.gameData.hatMutationPercent);
    }
    public static bool CheckMutationWings()
    {
        return CheckMutation(GameController.Current.gameData.wingsMutationPercent);
    }
    public static bool CheckMutationHand()
    {
        return CheckMutation(GameController.Current.gameData.handMutationPercent);
    }
    public static bool CheckMutationTails()
    {
        return CheckMutation(GameController.Current.gameData.tailMutationPercent);
    }
    public static bool CheckMutationHp()
    {
        return CheckMutation(GameController.Current.gameData.hpMutationPercent);
    }
    public static bool CheckMutationAtk()
    {
        return CheckMutation(GameController.Current.gameData.atkMutationPercent);
    }
    public static bool CheckMutationSpeed()
    {
        return CheckMutation(GameController.Current.gameData.speedMutationPercent);
    }

    public static int CalDinoLevel(int hp, int atk, int speed)
    {
        int lv = hp / 10 + atk + speed;
        return lv;
    }

    public static int CalFoodNeedToGrowUp(int level)
    {
        return level * GameController.Current.gameData.MCONST;
    }

    public static Texture2D Save(this Texture texture, string name)
    {
        RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 32);
        RenderTexture.active = renderTexture;
        Graphics.Blit(texture, renderTexture);

        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        File.WriteAllBytes(Application.persistentDataPath + "/" + name + ".png", texture2D.EncodeToPNG());

        return texture2D;
    }

    public static Texture2D LoadTexture(string name)
    {
        var path = Application.persistentDataPath + "/" + name + ".png";
        if (!File.Exists(path)) return null;

        byte[] bytes = File.ReadAllBytes(path);

        Texture2D texture = new Texture2D(512, 512, TextureFormat.RGB24, false);
        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        return texture;
    }

    public static Sprite ToSprite(this Texture2D texture)
    {
        if (texture == null) return null;
        return Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(0.5f, 0.5f));
    }

    public static string Format(this float time, string provider = "{0:00}:{1:00}:{2:00}")
    {
        var timeSpan = TimeSpan.FromSeconds(Mathf.Clamp(time, 0, time));
        return string.Format(provider, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

    public static string Format(this double time, string provider = "{0:00}:{1:00}:{2:00}")
    {
        var timeSpan = TimeSpan.FromSeconds(Mathf.CeilToInt(Mathf.Clamp((float)time, 0, (float)time)));
        return string.Format(provider, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }
}
