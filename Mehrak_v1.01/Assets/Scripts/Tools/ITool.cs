using UnityEngine;

public interface ITool
{
    void Initialize(ToolType type, ToolDataSO data);
    void OnSpawn();
    void Collect();
    void ReturnToPool();
    GameObject GameObject { get; }
}