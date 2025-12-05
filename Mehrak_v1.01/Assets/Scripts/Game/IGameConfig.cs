using UnityEngine;

public interface IGameConfig
{
    Vector2 SpawnAreaCenter { get; }
    Vector2 SpawnAreaSize { get; }
    float MinToolDistance { get; }
    float MinDistanceFromPlayer { get; }
    float SpawnInterval { get; }
}