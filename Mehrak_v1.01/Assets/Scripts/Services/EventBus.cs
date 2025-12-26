using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, Delegate> subscribers = new Dictionary<Type, Delegate>();

    public void Subscribe<T>(Action<T> handler)
    {
        var t = typeof(T);
        if (subscribers.TryGetValue(t, out var existing))
            subscribers[t] = Delegate.Combine(existing, handler);
        else
            subscribers[t] = handler;
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        var t = typeof(T);
        if (subscribers.TryGetValue(t, out var existing))
        {
            var updated = Delegate.Remove(existing, handler);
            if (updated == null) subscribers.Remove(t);
            else subscribers[t] = updated;
        }
    }

    public void Publish<T>(T evt)
    {
        var t = typeof(T);
        Debug.Log($"EventBus.Publish<{t.Name}> called");

        if (subscribers.TryGetValue(t, out var d))
        {
            var handler = d as Action<T>;
            if (handler != null)
            {
                handler.Invoke(evt);
                Debug.Log($"EventBus: invoked handlers for {t.Name}");
            }
        }
        else
        {
            Debug.Log($"EventBus: no handlers for {t.Name}");
        }
    }
}