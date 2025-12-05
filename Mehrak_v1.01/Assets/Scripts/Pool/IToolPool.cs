using System.Collections.Generic;

public interface IToolPool
{
    void RegisterType(ToolType type);
    void AddToPool(ToolType type, ToolController instance);
    ToolController Get(ToolType type);
    void Return(ToolType type, ToolController instance);
    int ActiveCount(ToolType type);
    IEnumerable<ToolController> GetActiveTools();
}