using System;
using UnityEngine;

public class HuntPartyController : MonoBehaviour
{
    public static HuntPartyController Current;

    public Action<int> OnFinishHunt;

    private void Awake()
    {
        Current = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InvokeRepeating(nameof(UpdateHuntPartyProcess), 0, 1);
    }

    private void UpdateHuntPartyProcess()
    {
        for (int i = Data.HuntParties.Count - 1; i >= 0; i--)
        {
            var huntParty = Data.HuntParties[i];
            if ((DateTime.Now - huntParty.startHuntTimer).TotalSeconds >= huntParty.huntTime)
            {
                OnFinishHunt?.Invoke(huntParty.enemyId);
                Data.HuntParties.RemoveAt(i);
            }
        }
    }
}
