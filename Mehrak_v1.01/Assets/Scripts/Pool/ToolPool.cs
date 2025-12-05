using System.Collections.Generic;
using UnityEngine;

public class ToolPool : IToolPool
{
    private readonly Dictionary<ToolType, Queue<ToolController>> pool = new Dictionary<ToolType, Queue<ToolController>>();
    private readonly Dictionary<ToolType, List<ToolController>> activeLists = new Dictionary<ToolType, List<ToolController>>();

    public void RegisterType(ToolType type)
    {
        if (!pool.ContainsKey(type)) pool[type] = new Queue<ToolController>();
        if (!activeLists.ContainsKey(type)) activeLists[type] = new List<ToolController>();
    }

    public void AddToPool(ToolType type, ToolController instance)
    {
        instance.gameObject.SetActive(false);
        if (!pool.ContainsKey(type)) RegisterType(type);
        pool[type].Enqueue(instance);
    }

    public ToolController Get(ToolType type)
    {
        if (!pool.ContainsKey(type)) RegisterType(type);
        ToolController instance = null;
        if (pool[type].Count > 0)
            instance = pool[type].Dequeue();

        if (instance != null)
        {
            activeLists[type].Add(instance);
            instance.gameObject.SetActive(true);
        }
        return instance;
    }

    public void Return(ToolType type, ToolController instance)
    {
        if (!pool.ContainsKey(type)) RegisterType(type);
        instance.gameObject.SetActive(false);
        if (activeLists[type].Contains(instance)) activeLists[type].Remove(instance);
        pool[type].Enqueue(instance);
    }

    public int ActiveCount(ToolType type)
    {
        if (!activeLists.ContainsKey(type)) return 0;
        return activeLists[type].Count;
    }

    public IEnumerable<ToolController> GetActiveTools()
    {
        foreach (var kv in activeLists)
        {
            foreach (var tool in kv.Value)
                yield return tool;
        }
    }
}