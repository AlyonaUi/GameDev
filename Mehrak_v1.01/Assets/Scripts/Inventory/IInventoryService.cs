using System;
/// Абстракция для инвентаря.
public interface IInventoryService
{
    void Add(ToolType type, int amount = 1);
    int GetCount(ToolType type);
    int[] GetAllCounts();
    void Reset();
}