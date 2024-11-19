using System;
using UnityEngine;

public class Panel : MonoBehaviour
{
    public Action OnShow;
    public Action OnBeforeShow;
    public Action OnHide;
    public Action OnBeforeHide;

    public UIController uiController;

    public virtual void Init()
    {

    }

    public virtual void BeforeShow()
    {
        OnBeforeShow?.Invoke();
    }

    public virtual void Show()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            OnShow?.Invoke();
            uiController.IncreaseEnableCount();
        }
    }

    public virtual void BeforeHide()
    {
        OnBeforeHide?.Invoke();
    }

    public virtual void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            OnHide?.Invoke();
            uiController.DecreaseEnableCount();
        }
    }
}
