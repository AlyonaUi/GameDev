using UnityEngine;

public interface IToolFactory
{
    void Prewarm();
    ToolController Spawn(ToolType type, Vector2 position);
}