using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Canvas))]
public class InventoryUI : MonoBehaviour
{
    [Header("UI Elements (assign in Inspector)")]
    public GameObject inventoryPanel;
    public Image hammerIconImage;
    public TMP_Text hammerCountText;
    public Image axeIconImage;
    public TMP_Text axeCountText;
    public Image sawIconImage;
    public TMP_Text sawCountText;
    
    private IEventBus bus;
    private IInventoryService inv;
    private bool subscribed = false;
    private Coroutine waitForBusCoroutine;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        TryEnsureSubscription();
    }

    private void OnDisable()
    {
        Unsubscribe();
        if (waitForBusCoroutine != null)
        {
            StopCoroutine(waitForBusCoroutine);
            waitForBusCoroutine = null;
        }
    }

    private void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        
        if (inv == null) inv = ServiceLocator.Get<IInventoryService>();
        if (inv != null)
        {
            ApplyCounts(inv.GetAllCounts());
        }
        
        ApplyIconsFromProvider();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryPanel != null)
                inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }

    private void TryEnsureSubscription()
    {
        if (subscribed) return;

        bus = ServiceLocator.Get<IEventBus>();
        inv = ServiceLocator.Get<IInventoryService>();

        if (bus != null)
        {
            bus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            subscribed = true;
            Debug.Log("InventoryUI: subscribed to InventoryChangedEvent");
            if (inv != null)
                ApplyCounts(inv.GetAllCounts());
            else
                ApplyCounts(new int[] { 0, 0, 0 });
        }
        else
        {
            if (waitForBusCoroutine == null)
                waitForBusCoroutine = StartCoroutine(WaitForBusAndSubscribe());
        }
    }

    private IEnumerator WaitForBusAndSubscribe()
    {
        float timeout = 3f;
        float elapsed = 0f;
        while (elapsed < timeout)
        {
            bus = ServiceLocator.Get<IEventBus>();
            inv = ServiceLocator.Get<IInventoryService>();
            if (bus != null)
            {
                bus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
                subscribed = true;
                Debug.Log("InventoryUI: subscribed to InventoryChangedEvent (after wait)");
                if (inv != null)
                    ApplyCounts(inv.GetAllCounts());
                else
                    ApplyCounts(new int[] { 0, 0, 0 });
                waitForBusCoroutine = null;
                yield break;
            }
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        Debug.LogWarning("InventoryUI: EventBus not found within timeout; UI will not receive updates until EventBus exists.");
        waitForBusCoroutine = null;
    }

    private void Unsubscribe()
    {
        if (subscribed && bus != null)
        {
            bus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            Debug.Log("InventoryUI: unsubscribed from InventoryChangedEvent");
        }
        subscribed = false;
        bus = null;
    }

    private void OnInventoryChanged(InventoryChangedEvent evt)
    {
        Debug.Log("InventoryUI: received InventoryChangedEvent");
        ApplyCounts(evt.counts);
    }

    private void ApplyCounts(int[] counts)
    {
        if (counts == null || counts.Length < 3) return;

        if (sawCountText != null) sawCountText.text = counts[(int)ToolType.Saw].ToString();
        if (axeCountText != null) axeCountText.text = counts[(int)ToolType.Axe].ToString();
        if (hammerCountText != null) hammerCountText.text = counts[(int)ToolType.Hammer].ToString();
    }

    private void ApplyIconsFromProvider()
    {
        var provider = ToolIconProvider.Instance;
        if (provider == null)
        {
            Debug.LogWarning("InventoryUI: ToolIconProvider.Instance is null - icons won't be set automatically.");
            return;
        }

        if (sawIconImage != null && sawIconImage.sprite == null)
            sawIconImage.sprite = provider.GetIcon(ToolType.Saw);
        if (axeIconImage != null && axeIconImage.sprite == null)
            axeIconImage.sprite = provider.GetIcon(ToolType.Axe);
        if (hammerIconImage != null && hammerIconImage.sprite == null)
            hammerIconImage.sprite = provider.GetIcon(ToolType.Hammer);
    }
}