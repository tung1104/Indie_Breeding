using System;
using UnityEngine;

public class HuntPartySimulator : MonoBehaviour
{
    private HuntParty huntParty;
    private Dino dino;
    private LineRenderer lineRenderer;

    public void Init(HuntParty huntParty)
    {
        this.huntParty = huntParty;

        dino = GameController.Current.mutationController.CreateDino(huntParty.dinosPartyId[0]);
        dino.meat.SetActive(false);
        dino.locomotion.Init();
        dino.locomotion.PlayRunAnim();
        dino.transform.localScale = Vector3.one * 5;
        dino.transform.SetParent(transform);

        lineRenderer = Instantiate(GameController.Current.gameData.lineRendererPrefab);
        lineRenderer.SetPositions(new Vector3[2]);
        lineRenderer.transform.SetParent(transform);
    }

    private void Update()
    {
        var targetPosition = WildController.Current.enemies[huntParty.enemyId].transform.position;
        lineRenderer.SetPosition(1, targetPosition);
        dino.Move(targetPosition, huntParty.huntTime - (float)(DateTime.Now - huntParty.startHuntTimer).TotalSeconds);
    }

    public void Done()
    {

    }
}
