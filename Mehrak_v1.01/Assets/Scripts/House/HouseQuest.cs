using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class HouseQuest : MonoBehaviour
{
    [Header("Requirements (set in inspector or left zero to randomize at Start)")]
    public int[] requiredCounts; 

    [Header("UI Settings")]
    public Vector3 popupOffset = new Vector3(0f, 1.2f, 0f); 
    
    [NonSerialized] public bool playerIsNear = false;
    [NonSerialized] public Transform playerTransform;

    private void Reset()
    {
        EnsureArraySize();
    }

    private void Start()
    {
        EnsureArraySize();
        
        bool allZero = true;
        foreach (var v in requiredCounts) if (v > 0) { allZero = false; break; }
        if (allZero) RandomizeRequirements();
    }

    private void EnsureArraySize()
    {
        int n = Enum.GetNames(typeof(ToolType)).Length;
        if (requiredCounts == null || requiredCounts.Length != n)
            requiredCounts = new int[n];
    }

    public void RandomizeRequirements(int maxEach = 5)
    {
        EnsureArraySize();
        var rng = new System.Random();
        for (int i = 0; i < requiredCounts.Length; i++)
            requiredCounts[i] = rng.Next(0, maxEach + 1);
    }

    public int GetRequired(ToolType t) => (requiredCounts != null && (int)t < requiredCounts.Length) ? requiredCounts[(int)t] : 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerIsNear = true;
        playerTransform = other.transform;
        
        var ui = HousePopupUI.Instance;
        if (ui != null) ui.ShowForHouse(this, popupOffset);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerIsNear = false;
        playerTransform = null;

        var ui = HousePopupUI.Instance;
        if (ui != null) ui.Hide();
    }

    private void Update()
    {
        if (!playerIsNear) return;
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryDeliver();
        }
    }

    private void TryDeliver()
    {
        var inv = ServiceLocator.Get<IInventoryService>();
        if (inv == null)
        {
            Debug.LogWarning("HouseQuest.TryDeliver: no IInventoryService registered.");
            HousePopupUI.Instance?.ShowMessage("Ошибка: инвентарь недоступен", 2f);
            return;
        }
        
        for (int i = 0; i < requiredCounts.Length; i++)
        {
            var t = (ToolType)i;
            int need = requiredCounts[i];
            if (need <= 0) continue;
            int have = inv.GetCount(t);
            if (have < need)
            {
                HousePopupUI.Instance?.ShowMessage("Не хватает инструментов", 2f);
                return;
            }
        }
        
        for (int i = 0; i < requiredCounts.Length; i++)
        {
            var t = (ToolType)i;
            int need = requiredCounts[i];
            if (need <= 0) continue;
            inv.Add(t, -need); // remove from inventory
        }

        HousePopupUI.Instance?.ShowMessage("Победа!", 2f);
        
        StartCoroutine(ReloadAfterDelay(1.2f));
    }

    private System.Collections.IEnumerator ReloadAfterDelay(float delay)
    {
        var inv = ServiceLocator.Get<IInventoryService>();
        inv?.Reset();

        yield return new WaitForSeconds(delay);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}