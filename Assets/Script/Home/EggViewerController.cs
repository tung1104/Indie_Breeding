using System.Collections.Generic;
using UnityEngine;

public class EggViewerController : MonoBehaviour
{
    public EggViewer eggViewerPrefab;
    public List<EggViewer> eggViewers;

    public void Init()
    {
        if (eggViewers.Count == 0)
        {
            for (int i = 0; i < 24; i++)
            {
                var eggViewer = Instantiate(eggViewerPrefab, transform);
                eggViewer.transform.localPosition = new Vector3(i * 5, 0, 0);
                eggViewers.Add(eggViewer);
            }
        }

        foreach (var eggViewer in eggViewers)
        {
            eggViewer.Init();
        }
    }
}
