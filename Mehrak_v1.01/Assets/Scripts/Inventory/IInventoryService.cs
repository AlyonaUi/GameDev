using System;
/// Абстракция для инвентаря.
public interface IInventoryService
{
    void Add(ToolType type, int amount = 1);
    bool IsFull(ToolType type);
    int GetCount(ToolType type);
    int[] GetAllCounts();
    void Reset();
}