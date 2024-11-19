using System;
using UnityEngine.UI;

public class ConfirmPanel : Panel
{
    public Action OnConfirm;
    public Action OnCancel;

    public Text titleText;
    public Text confirmText;
    public Text cancelText;
    public Button confirmButton;
    public Button cancelButton;

    public override void Init()
    {
        confirmButton.onClick.AddListener(OnClickConfirmButton);
        cancelButton.onClick.AddListener(OnClickCancelButton);
    }

    public void Setup(string title = "Are you sure?", string confirm = "Expel", string cancel = "Return")
    {
        titleText.text = title;
        confirmText.text = confirm;
        cancelText.text = cancel;
    }

    private void OnClickConfirmButton()
    {
        OnConfirm?.Invoke();
        Hide();
    }

    private void OnClickCancelButton()
    {
        OnCancel?.Invoke();
        Hide();
    }
}
