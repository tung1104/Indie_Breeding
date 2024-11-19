public class HomeController : SimpleSingleton<HomeController>
{
    public HomeMutationController homeMutationController;
    public UpgradeController upgradeController;
    public EggViewerController eggViewerController;
    public NestController nestController;
    public IncubatorController incubatorController;
    public FoodsController foodsController;
    public AstarPathController astarPathController;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        astarPathController.Init();
        eggViewerController.Init();
        homeMutationController.Init();
        upgradeController.Init();
        nestController.Init();
        incubatorController.Init();
        foodsController.Init();
    }
}
