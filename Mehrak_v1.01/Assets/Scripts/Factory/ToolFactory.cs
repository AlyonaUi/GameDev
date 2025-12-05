using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ToolPrefabEntry
{
    public ToolType toolType;
    public GameObject prefab;
    public ToolDataSO data;
}

public class ToolFactory : MonoBehaviour, IToolFactory
{
    [Tooltip("Назначьте prefabs и ToolDataSO здесь")]
    public List<ToolPrefabEntry> entries = new List<ToolPrefabEntry>();

    private IToolPool pool;
    private IEventBus events;

    private void Awake()
    {
        // Не пытаемся доставать сервисы в Awake(), чтобы не зависеть от порядка инициализации
    }
    
    public void Prewarm()
    {
        // Получаем зависимости здесь, когда Bootstrap/ServiceLocator уже зарегистрировал сервисы
        pool = ServiceLocator.Get<IToolPool>();
        events = ServiceLocator.Get<IEventBus>();

        if (pool == null)
        {
            Debug.LogError("IToolPool not found in ServiceLocator. Make sure Bootstrap registers IToolPool before calling Prewarm.");
            return;
        }

        foreach (var e in entries) // Смотрим всё ли добавлено об инструментах (prefab, data)
        {
            if (e.prefab == null || e.data == null)
            {
                Debug.LogWarning($"ToolFactory Prewarm: entry for {e.toolType} has null prefab or data. Skipping.");
                continue;
            }

            pool.RegisterType(e.toolType); // 

            for (int i = 0; i < Mathf.Max(0, e.data.maxCount); i++) // Создаём пока не максимальное кол-во
            {
                var go = Instantiate(e.prefab, transform); // Создаём новый GameObject по префабу и ставим его дочерним объектом ToolFactory
                var controller = go.GetComponent<ToolController>();
                if (controller == null)
                {
                    Debug.LogError("Prefab missing ToolController: " + e.prefab.name);
                    Destroy(go);
                    continue;
                }

                controller.Initialize(e.toolType, e.data);
                go.SetActive(false);
                pool.AddToPool(e.toolType, controller);
            }
        }
    }

    public ToolController Spawn(ToolType type, Vector2 position)
    {
        if (pool == null)
        {
            pool = ServiceLocator.Get<IToolPool>();
            events = ServiceLocator.Get<IEventBus>();
            if (pool == null)
            {
                Debug.LogError("IToolPool not found in ServiceLocator. Cannot spawn tools.");
                return null;
            }
        }

        var instance = pool.Get(type); // Пытаемся взять готовый объект из пула, иначе пытаемся его создать
        if (instance == null)
        {
            foreach (var e in entries)
            {
                if (e.toolType == type)
                {
                    if (e.prefab == null || e.data == null)
                    {
                        Debug.LogError($"ToolFactory Spawn: missing prefab or data for {type}");
                        return null;
                    }

                    var go = Instantiate(e.prefab, transform);
                    var controller = go.GetComponent<ToolController>();
                    if (controller == null)
                    {
                        Debug.LogError("Prefab missing ToolController: " + e.prefab.name);
                        Destroy(go);
                        return null;
                    }

                    controller.Initialize(e.toolType, e.data);
                    instance = controller;
                    break;
                }
            }
        }

        if (instance != null)
        {
            instance.transform.position = position;
            instance.OnSpawn();
        }
        return instance;
    }
}