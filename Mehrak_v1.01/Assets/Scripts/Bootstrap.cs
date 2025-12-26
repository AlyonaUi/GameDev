using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private IPlayer _player;
    public ToolFactory toolFactory;
    public GameConfigSO gameConfig;
    private IEventBus bus;
    private IToolPool pool;
    private InventoryService inventoryService;
    
    private void Awake()
    {
        ServiceLocator.Clear();
        
        // Регистрируем IEventBus
        bus = new EventBus();
        ServiceLocator.Register<IEventBus>(bus);
        var inventoryService = new InventoryService(bus);
        ServiceLocator.Register<IInventoryService>(inventoryService);
        Debug.Log("Bootstrap: IInventoryService registered");

        // Регистрируем IToolPool
        pool = new ToolPool();
        ServiceLocator.Register<IToolPool>(pool);

        // Регистрируем IToolFactory если предусмотрен компонент ToolFactory (разрешить доступ по интерфейсу)
        if (toolFactory != null)
        {
            ServiceLocator.Register<IToolFactory>(toolFactory as IToolFactory);
            ServiceLocator.Register<ToolFactory>(toolFactory);
        }

        // Регистрируем GameConfig
        if (gameConfig != null)
        {
            ServiceLocator.Register<IGameConfig>(gameConfig as IGameConfig);
            ServiceLocator.Register<GameConfigSO>(gameConfig);
        }

        // Создаём spawner
        var spawnerGO = new GameObject("ToolSpawner");
        var spawner = spawnerGO.AddComponent<ToolSpawner>();
        spawner.factory = toolFactory;
        spawner.config = gameConfig;

        // Создаём и регистрируем InventoryService
        inventoryService = new InventoryService(bus);
        ServiceLocator.Register<IInventoryService>(inventoryService);
        
        bus.Subscribe<ToolCollectedEvent>(OnToolCollected);
        
        ServiceLocator.Register<IPlayer>(_player);
    }
    
    private void Start()
    {
        var factoryInterface = ServiceLocator.Get<IToolFactory>();
        if (factoryInterface != null)
        {
            factoryInterface.Prewarm();
        }
        else if (toolFactory != null)
        {
            toolFactory.Prewarm();
        }
    }

    private void OnToolCollected(ToolCollectedEvent evt)
    {
        Debug.Log($"Collected: {evt.tool.name} ({evt.tool.GetType().Name})");
    }
}
