using System;
using UnityEngine;

public struct InventoryChangedEvent
{
    public int[] counts;
}

public class InventoryService : IInventoryService
{
    private readonly int[] counts;
    private readonly IEventBus bus;
    
    public const int maxCapacity = 100;

    public InventoryService(IEventBus bus)
    {
        if (bus == null)
        {
            Debug.LogError("InventoryService: IEventBus is null in constructor!");
            throw new ArgumentNullException(nameof(bus));
        }

        this.bus = bus;
        counts = new int[Enum.GetNames(typeof(ToolType)).Length];

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
        
        try
        {
            var tc = evt.tool as ToolController;
            if (tc == null)
            {
                Debug.LogWarning("InventoryService: collected tool is not ToolController");
                return;
            }

            if (CanAddMore(tc.ToolType))
            {
                Add(tc.ToolType, 1);
                Debug.Log($"InventoryService: collected {tc.ToolType}, newCount = {GetCount(tc.ToolType)}");
            }
            else
            {
                Debug.Log($"InventoryService: cannot collect {tc.ToolType}, inventory full (max {maxCapacity})");
            }
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

        if (counts[idx] < 99)
        {
            counts[idx] = Mathf.Max(0, counts[idx] + amount);
            
            Debug.Log($"InventoryService.Add: type={type} newCount={counts[idx]}");
            
            var copy = (int[])counts.Clone();
            bus.Publish(new InventoryChangedEvent { counts = copy });

            Debug.Log("InventoryService.Add: InventoryChangedEvent published");
            
        }
    }
    
    public bool CanAddMore(ToolType type, int amount = 1)
    {
        var idx = (int)type;
        if (idx < 0 || idx >= counts.Length) return false;
        
        return counts[idx] + amount <= maxCapacity;
    }
    
    public bool IsFull(ToolType type)
    {
        return GetCount(type) >= maxCapacity;
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