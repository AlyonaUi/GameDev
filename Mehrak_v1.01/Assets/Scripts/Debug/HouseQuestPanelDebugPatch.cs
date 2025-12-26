using UnityEngine;
using Zenject;

public class HouseQuestPanelDebugPatch : MonoBehaviour
{
    IInventoryService inv;
    IEventBus bus;

    [Inject]
    public void Construct(IInventoryService inventory, IEventBus eventBus)
    {
        inv = inventory;
        bus = eventBus;
        Debug.Log($"HouseQuestPanelDebugPatch.Construct: inv={(inv!=null)}, bus={(bus!=null)}");
    }

    private void OnEnable()
    {
        if (bus != null)
        {
            bus.Subscribe<InventoryChangedEvent>(OnInvChanged);
            Debug.Log("HouseQuestPanelDebugPatch: subscribed to InventoryChangedEvent");
        }
        else
            Debug.LogWarning("HouseQuestPanelDebugPatch: eventBus is null on enable");
    }

    private void OnDisable()
    {
        if (bus != null)
            bus.Unsubscribe<InventoryChangedEvent>(OnInvChanged);
    }

    private void OnInvChanged(InventoryChangedEvent evt)
    {
        Debug.Log("HouseQuestPanelDebugPatch: InventoryChangedEvent received, counts: {string.Join(\",\", evt.counts)}");
    }
}