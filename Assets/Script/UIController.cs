using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public bool HasUIEnabled => uiEnableCount > 0;
    public Panel[] panels;

    private Dictionary<Type, Panel> dic = new Dictionary<Type, Panel>();
    private Panel current;
    private int uiEnableCount;

    public void Init()
    {
        GetComponent<CanvasScaler>().matchWidthOrHeight = Camera.main.aspect > .75f ? 1 : 0;

        foreach (var panel in panels)
        {
            dic.Add(panel.GetType(), panel);
            panel.Init();
            panel.uiController = this;
        }

        current = panels[0];
    }

    public T Show<T>(bool keepCurrent = false) where T : Panel
    {
        if (keepCurrent)
        {
            var panel = dic[typeof(T)];
            panel.BeforeShow();
            panel.Show();
            return panel as T;
        }
        else
        {
            current.Hide();
            current = dic[typeof(T)];
            current.BeforeShow();
            current.Show();
            return current as T;
        }
    }

    public T Hide<T>() where T : Panel
    {
        var panel = dic[typeof(T)];
        panel.BeforeHide();
        panel.Hide();
        return panel as T;
    }

    public void HideAll()
    {
        foreach (var panel in dic.Values) panel.Hide();
        uiEnableCount = 0;
    }

    public T Get<T>() where T : Panel
    {
        return dic[typeof(T)] as T;
    }

    public virtual void IncreaseEnableCount()
    {
        uiEnableCount++;
    }

    public virtual void DecreaseEnableCount()
    {
        uiEnableCount--;
    }
}
