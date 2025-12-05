using UnityEngine;

[CreateAssetMenu(fileName = "ToolData", menuName = "Mehrak/ToolData", order = 0)]
public class ToolDataSO : ScriptableObject
{
    public ToolType toolType;
    public Sprite sprite;
    public float speed = 1f;           
    public float rotationSpeed = 90f;  
    public int maxCount = 5;           
    public float lifetime = 10f;       
    public float blinkAt = 7f;         
    public float collectDistance = 0.6f; 
    
    [Header("Movement tuning (used by Axe)")]
    [Tooltip("How often the axe picks a new random target direction (seconds)")]
    public float directionChangeInterval = 2f;

    [Tooltip("How fast the axe smooths/turns towards the target direction (larger = snappier)")]
    public float turnSmoothness = 6f;

    [Tooltip("Distance ahead to check for obstacles (CircleCast). If obstacle found, axe will steer away.")]
    public float avoidObstacleDistance = 0.6f;

    [Tooltip("Radius used for obstacle detection (CircleCast radius)")]
    public float avoidObstacleRadius = 0.25f;

    [Tooltip("Which layers are considered obstacles for avoidance (default: everything)")]
    public LayerMask obstacleMask = ~0;

    [Header("Bounds behaviour")]
    [Tooltip("If true, axe will try to stay inside spawn area's bounds defined by GameConfigSO")]
    public bool stayInBounds = true;

    [Tooltip("If true, axe will reflect direction at bounds; otherwise it just picks a new random direction")]
    public bool reflectOnBounds = true;
}