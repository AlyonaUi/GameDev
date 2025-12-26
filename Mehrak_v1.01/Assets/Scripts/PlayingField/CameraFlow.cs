using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Camera))]
public class CameraFlow :  MonoBehaviour
{
    [Header("Назначение игрока и фона")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer background;
    
    [Header("Настройки")]
    [SerializeField, Range(0.1f, 1f)] private float smoothness = 0.3f;
    [SerializeField, Tooltip("Камера ограничена фоном")] private bool useBounds = true;
    [SerializeField] private string backgroundSortingLayer = "Background";
    
    private Camera cam;
    private Vector3 currentVelocity;
    
    void Awake()
    {
        cam = GetComponent<Camera>();
        ValidateReferences();
    }
    
    void ValidateReferences()
    {
        if (player == null)
        {
            // Автоопределение игрока
            TryFindPlayer();
        }
        
        if (background == null && useBounds)
        {
            // Автоопределение фона
            TryFindBackground();
            
            Debug.LogWarning("Background reference is not set. Camera will have no bounds.");
        }
    }
    
    void TryFindPlayer()
    {
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            player = playerController.transform;
            return;
        }
        
        Debug.LogError("Player не найден! Назначьте игрока в инспекторе");
    }

    void TryFindBackground()
    {
        // Получаем ВСЕ SpriteRenderer на сцене
        SpriteRenderer[] allSprites = FindObjectsOfType<SpriteRenderer>();
        
        if (allSprites.Length == 0)
        {
            Debug.LogError("Нет SpriteRenderers в сцене!");
        }
        
        // Фильтруем по Sorting Layer
        var backgroundSprites = allSprites
            .Where(s => s.sortingLayerName == backgroundSortingLayer)
            .ToArray();
        
        if (backgroundSprites.Length == 0)
        {
            Debug.LogError($"Не найдено спрайтов на Sorting Layer '{backgroundSortingLayer}'!");
        }
        background = backgroundSprites[0];
        
        Debug.LogWarning("Background не найден! У камеры не будет границ");
    }
    
    void LateUpdate()
    {
        if (player == null) return;
        
        Vector3 targetPosition = player.position;
        targetPosition.z = transform.position.z;
        
        if (useBounds && background != null)
        {
            targetPosition = ApplyBounds(targetPosition);
        }
        
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothness
        );
    }

    Vector3 ApplyBounds(Vector3 position)
    {
        Bounds bounds = background.bounds;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        
        // Рассчитываем допустимые границы для центра камеры
        float leftBound = bounds.min.x + camWidth;
        float rightBound = bounds.max.x - camWidth;
        float bottomBound = bounds.min.y + camHeight;
        float topBound = bounds.max.y - camHeight;
        
        // Применяем ограничения
        position.x = Mathf.Clamp(position.x, leftBound, rightBound);
        position.y = Mathf.Clamp(position.y, bottomBound, topBound);
        
        return position;
    }

    // Метод для других скриптов, чтобы обновить цель
    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
    }
    
    // Метод для других скриптов, чтобы обновить фон
    public void SetBackground(SpriteRenderer newBackground)
    {
        background = newBackground;
    }
}