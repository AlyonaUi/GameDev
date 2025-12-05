using System;
using UnityEngine;

/// InventoryChangedEvent: публикуется при каждом изменении (копия массива counts)
public struct InventoryChangedEvent
{
    public int[] counts;
}

/// Простой сервис инвентаря — хранит числа по типам инструментов,
/// подписывается на ToolCollectedEvent и публикует InventoryChangedEvent.
public class InventoryService : IInventoryService
{
    private readonly int[] counts;
    private readonly IEventBus bus;

    public InventoryService(IEventBus bus)
    {
        if (bus == null)
        {
            Debug.LogError("InventoryService: IEventBus is null in constructor!");
            throw new ArgumentNullException(nameof(bus));
        }

        this.bus = bus;
        counts = new int[Enum.GetNames(typeof(ToolType)).Length];

        // Подписываемся на событие сбора инструментов
        bus.Subscribe<ToolCollectedEvent>(OnToolCollected);
        Debug.Log("InventoryService: subscribed to ToolCollectedEvent");
    }

    private void OnToolCollected(ToolCollectedEvent evt)
    {
        if (evt.tool == null)
        {
            Debug.LogWarning("InventoryService: ToolCollectedEvent.tool is null");
            return;
        }

        // Пытаемся получить тип инструмента из ToolController
        try
        {
            var tc = evt.tool as ToolController;
            if (tc == null)
            {
                Debug.LogWarning("InventoryService: collected tool is not ToolController");
                return;
            }

            Add(tc.ToolType, 1);
            Debug.Log($"InventoryService: collected {tc.ToolType}, newCount = {GetCount(tc.ToolType)}");
        }
        catch (Exception ex)
        {
            Debug.LogError("InventoryService.OnToolCollected exception: " + ex);
        }
    }

    public void Add(ToolType type, int amount = 1)
    {
        var idx = (int)type;
        if (idx < 0 || idx >= counts.Length) return;
        counts[idx] = Mathf.Max(0, counts[idx] + amount);

        // публикуем копию массива
        var copy = (int[])counts.Clone();
        bus.Publish(new InventoryChangedEvent { counts = copy });
    }

    public int GetCount(ToolType type)
    {
        var idx = (int)type;
        if (idx < 0 || idx >= counts.Length) return 0;
        return counts[idx];
    }

    public int[] GetAllCounts()
    {
        return (int[])counts.Clone();
    }

    public void Reset()
    {
        for (int i = 0; i < counts.Length; i++) counts[i] = 0;
        bus.Publish(new InventoryChangedEvent { counts = (int[])counts.Clone() });
    }
}