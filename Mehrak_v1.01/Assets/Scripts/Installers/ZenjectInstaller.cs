using UnityEngine;
using Zenject;

public class ZenjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IEventBus>().To<EventBus>().AsSingle().NonLazy();
        Container.Bind<IInventoryService>().To<InventoryService>().AsSingle().NonLazy();
        Container.Bind<LevelManager>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<PlayerController>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<HouseQuestPanel>().FromComponentInHierarchy().AsSingle().NonLazy();
        
        Container.QueueForInject(this);
    }

    private void Start()
    {
        // Регистрируем в ServiceLocator, чтобы старые классы работали
        var bus = Container.Resolve<IEventBus>();
        var inv = Container.Resolve<IInventoryService>();
        ServiceLocator.Register<IEventBus>(bus);
        ServiceLocator.Register<IInventoryService>(inv);
        Debug.Log($"ZenjectInstaller: IEventBus={(bus!=null)}, IInventoryService={(inv!=null)} registered in ServiceLocator");
    }
}