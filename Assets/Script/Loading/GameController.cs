using System.Collections.Generic;
using UnityEngine;

public class GameController : SimpleSingleton<GameController>
{
    public GameData gameData;
    public PlayerController playerController;
    public MutationController mutationController;
    public bool isConfigOnLine;
    public Dictionary<int, Sprite> dinoAvatars = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        if (isConfigOnLine) StartCoroutine(gameData.Init());

        playerController.Init();
        mutationController.Init();

        var dinoCountString = Utils.GetData(PlayerPrefsConst.DINO_COUNT);
        if (dinoCountString == "") Utils.SaveData(PlayerPrefsConst.DINO_COUNT, "0");

        var dinoCount = int.Parse(Utils.GetData(PlayerPrefsConst.DINO_COUNT));
        for (int i = 1; i <= dinoCount; i++)
        {
            var texture = Utils.LoadTexture("Dino" + i);
            dinoAvatars.Add(i, texture.ToSprite());
        }
    }
}
