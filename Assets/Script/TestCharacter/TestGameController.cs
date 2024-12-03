using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TestGameController : MonoBehaviour
{
    [FormerlySerializedAs("player")] public Character playerCharacter;

    public PlayActionPanel playActionPanel;

    public Bounds idleWorldBounds;
    Bounds actionWorldBounds;

    IEnumerator Start()
    {
        playActionPanel.SetPlayerCharacter(playerCharacter);

        // ProjectileManager.Instance.SubscribeOnHit((Unit sender, Unit receiver, DamageInfo info) =>
        // {
        //     //Debug.Log($"Projectile of {sender.name} hit {receiver.name} with {info.damage} damage");
        //     receiver.TakeDamage(sender, info);
        // });

        actionWorldBounds = TopdownCameraController.Instance.worldBounds;
        SetPlayActionPanel(true);

        yield return StartCoroutine(PerceptionSystem.Instance.Initialize());
    }

    void SetPlayActionPanel(bool active)
    {
        TopdownCameraController.Instance.SetWorldBounds(
            active ? actionWorldBounds : idleWorldBounds);
        playActionPanel.gameObject.SetActive(active);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            Time.timeScale = 1.15f - Time.timeScale;

        if (Input.GetKeyDown(KeyCode.V))
            SetPlayActionPanel(!playActionPanel.gameObject.activeSelf);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(idleWorldBounds.center, idleWorldBounds.size);
    }
}