public class HomeUIController : UIController
{
    public static HomeUIController Current;

    public HomeUI homeUI;

    private void Awake()
    {
        Current = this;

        Init();
        HideAll();

    }

    private void Start()
    {
        homeUI.Init();
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
