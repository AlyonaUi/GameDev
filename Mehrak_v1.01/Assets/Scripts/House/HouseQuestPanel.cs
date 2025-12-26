using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

[Serializable]
public class QuestPairUI
{
    public ToolType tool;
    public GameObject root;
    public Image icon;
    public TMP_Text countText;
    [HideInInspector] public int requiredCount;
}

public class HouseQuestPanel : MonoBehaviour
{
    [Header("UI pairs")]
    public QuestPairUI[] pairs;

    [Header("Required range")]
    public int minRequired = 2;
    public int maxRequired = 10;

    private IInventoryService inventory;
    private IEventBus eventBus;

    [Inject]
    public void Construct(IInventoryService inv, IEventBus bus)
    {
        inventory = inv;
        eventBus = bus;
    }

    private void Awake()
    {
        GenerateRequirements();
    }

    private void OnEnable()
    {
        if (eventBus != null)
            eventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);

        UpdateAll();
    }

    private void OnDisable()
    {
        if (eventBus != null)
            eventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
    }

    private void OnInventoryChanged(InventoryChangedEvent evt)
    {
        UpdateAll(evt.counts);
    }

    public void GenerateRequirements()
    {
        if (pairs == null) return;
        for (int i = 0; i < pairs.Length; i++)
        {
            if (pairs[i] == null) continue;
            pairs[i].requiredCount = UnityEngine.Random.Range(minRequired, maxRequired + 1);
            if (pairs[i].root != null)
                pairs[i].root.SetActive(true);
        }
        UpdateAll();
    }

    public bool AreRequirementsMet()
    {
        if (pairs == null) return false;
        var counts = inventory != null ? inventory.GetAllCounts() : Enumerable.Repeat(0, Enum.GetNames(typeof(ToolType)).Length).ToArray();
        foreach (var p in pairs)
        {
            if (p == null) continue;
            int current = counts[(int)p.tool];
            if (current < p.requiredCount) return false;
        }
        return true;
    }

    public string GetRequirementsSummary()
    {
        if (pairs == null) return "";
        return string.Join(" | ", pairs.Select(p => $"{p.tool}:{p.requiredCount}").ToArray());
    }

    private void UpdateAll()
    {
        int[] counts = null;
        if (inventory != null)
        {
            try { counts = inventory.GetAllCounts(); }
            catch { counts = null; }
        }
        if (counts == null) counts = Enumerable.Repeat(0, Enum.GetNames(typeof(ToolType)).Length).ToArray();
        UpdateAll(counts);
    }

    private void UpdateAll(int[] counts)
    {
        if (pairs == null) return;
        for (int i = 0; i < pairs.Length; i++)
        {
            var p = pairs[i];
            if (p == null || p.countText == null) continue;
            int current = 0;
            int idx = (int)p.tool;
            if (counts != null && idx >= 0 && idx < counts.Length) current = counts[idx];
            p.countText.text = $"{current}/{p.requiredCount}";
            if (p.icon != null)
                p.icon.color = (current >= p.requiredCount) ? Color.green : Color.white;
        }
    }
}