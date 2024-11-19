public class WildUIController : UIController
{
    public static WildUIController Current;

    private void Awake()
    {
        Current = this;
        Init();
        HideAll();
    }

    public override void IncreaseEnableCount()
    {
        base.IncreaseEnableCount();

        CameraController.Current.canTouch = !HasUIEnabled;
    }

    public override void DecreaseEnableCount()
    {
        base.DecreaseEnableCount();

        CameraController.Current.canTouch = !HasUIEnabled;
    }
}
