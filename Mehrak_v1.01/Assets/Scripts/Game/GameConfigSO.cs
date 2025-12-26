using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Mehrak/GameConfig", order = 1)]
public class GameConfigSO : ScriptableObject, IGameConfig
{
    [Header("Spawn area (center and size)")]
    public Vector2 spawnAreaCenter = Vector2.zero;
    public Vector2 spawnAreaSize = new Vector2(15f, 10f);

    [Tooltip("Min distance between spawned tools")]
    public float minToolDistance = 1.0f;

    [Tooltip("Min distance from player (Mehrak)")]
    public float minDistanceFromPlayer = 2.0f;

    public float spawnInterval = 1.0f;
    
    public Vector2 SpawnAreaCenter => spawnAreaCenter;
    public Vector2 SpawnAreaSize => spawnAreaSize;
    public float MinToolDistance => minToolDistance;
    public float MinDistanceFromPlayer => minDistanceFromPlayer;
    public float SpawnInterval => spawnInterval;
}