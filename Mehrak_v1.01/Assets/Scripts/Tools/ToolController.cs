using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class ToolController : MonoBehaviour, ITool
{
    protected ToolType type;
    protected ToolDataSO data;
    protected IToolPool pool;
    protected IEventBus events;

    protected Collider2D col;
    protected IToolView view;

    public Action<ToolController> OnCollected;

    public GameObject GameObject => this.gameObject;

    public ToolType ToolType => type;
    
    protected bool canBeCollected = true;

    public virtual void Initialize(ToolType t, ToolDataSO toolData)
    {
        type = t;
        data = toolData;
        col = GetComponent<Collider2D>();
        pool = ServiceLocator.Get<IToolPool>();
        events = ServiceLocator.Get<IEventBus>();
        gameObject.tag = "Tool";
        
        if (events != null)
        {
            events.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
        }
        
        view = GetComponent<IToolView>();
        if (view == null) view = GetComponentInChildren<IToolView>();

        if (view != null)
            view.Initialize(type, data);
        else
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && data != null) sr.sprite = data.sprite;
        }
        
        CheckIfCanBeCollected();
    }
    
    // Обработчик изменений инвентаря
    private void OnInventoryChanged(InventoryChangedEvent evt)
    {
        CheckIfCanBeCollected();
    }
    
    protected virtual void CheckIfCanBeCollected()
    {
        if (events == null) return;
        
        try
        {
            var inventory = ServiceLocator.Get<IInventoryService>();
            if (inventory != null)
            {
                canBeCollected = !inventory.IsFull(type);
                
                if (!canBeCollected)
                {
                    Debug.Log($"Tool {type} cannot be collected - inventory full");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"ToolController.CheckIfCanBeCollected error: {ex.Message}");
        }
    }

    public virtual void OnSpawn()
    {
        if (view != null) view.Show();
        if (col != null) col.enabled = true;
        gameObject.SetActive(true);
        CheckIfCanBeCollected();
    }

    public virtual void Collect()
    {
        
        // Проверяем, можно ли собирать
        if (!canBeCollected)
        {
            Debug.Log($"Cannot collect {type} - inventory full");
            // Можно воспроизвести звук отказа или показать анимацию
            return;
        }
        
        if (col != null) col.enabled = false;

        if (view != null)
        {
            view.PlayCollectEffect(() =>
            {
                OnCollected?.Invoke(this);
                events?.Publish(new ToolCollectedEvent { tool = this });
                ReturnToPool();
            });
        }
        else
        {
            OnCollected?.Invoke(this);
            events?.Publish(new ToolCollectedEvent { tool = this });
            ReturnToPool();
        }
    }

    public virtual void ReturnToPool()
    {
        StopAllCoroutines();
        if (view != null)
        {
            view.StopBlinking();
            view.Hide();
        }
        pool?.Return(type, this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canBeCollected)
        { 
            Collect();
        }
    }
    
    // Отписываемся от событий при уничтожении
    private void OnDestroy()
    {
        if (events != null)
        {
            events.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
        }
    }
}

public struct ToolCollectedEvent
{
    public ToolController tool;
}