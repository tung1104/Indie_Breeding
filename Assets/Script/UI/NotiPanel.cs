using UnityEngine.UI;

public class NotiPanel : Panel
{
    public Text titleText;
    public Button overlayButton;

    public override void Init()
    {
        overlayButton.onClick.AddListener(Hide);
    }

    public void ChangeTitle(string title)
    {
        titleText.text = title;
    }
}
