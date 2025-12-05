using UnityEngine;

public interface IPlayer
{
    Vector2 Position { get; }
    Transform Transform { get; }
    float Speed { get; }
}