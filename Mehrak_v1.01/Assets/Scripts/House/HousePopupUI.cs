using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HousePopupUI : MonoBehaviour
{
    public static HousePopupUI Instance { get; private set; }

    [Header("Optional prefab: must be a UI element (RectTransform)")]
    public GameObject windowPrefab;

    [Header("Appearance")]
    public Vector2 panelSize = new Vector2(420f, 96f);
    public Vector2 iconSize = new Vector2(36f, 36f);
    public float followSpeed = 0f;
    
    private Canvas canvas;
    private RectTransform canvasRect;
    private Camera canvasCamera;
    private GameObject activeWindow;
    private RectTransform activeRect;
    
    private Image[] iconImages;
    private TextMeshProUGUI[] countTexts;
    
    private TextMeshProUGUI messageText;
    
    private Transform targetHouse;
    private Vector3 worldOffset;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        ResolveCanvas();
    }

    private void ResolveCanvas()
    {
        canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
            canvasCamera = (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace) ? canvas.worldCamera : null;
            Debug.Log("[HousePopupUI] Resolved Canvas: " + canvas.name);
        }
        else
        {
            canvasRect = null;
            canvasCamera = null;
            Debug.LogWarning("[HousePopupUI] No Canvas found during ResolveCanvas.");
        }
    }

    private void Update()
    {
        if (activeWindow != null && targetHouse != null)
        {
            UpdatePosition();
        }
    }

    public void ShowForHouse(HouseQuest house, Vector3 offset)
    {
        if (house == null) return;

        targetHouse = house.transform;
        worldOffset = offset;

        if (activeWindow == null) CreateWindow();

        UpdateContents(house);
        if (activeWindow != null) activeWindow.SetActive(true);
        UpdatePosition();
    }

    public void Hide()
    {
        if (activeWindow != null) activeWindow.SetActive(false);
        targetHouse = null;
    }

    public void ShowMessage(string message, float duration = 1.5f)
    {
        try
        {
            EnsureWindowExists();

            if (messageText == null)
            {
                CreateTemporaryMessage(); // При необходимости создаст холст
            }

            if (messageText == null)
            {
                Debug.LogWarning("[HousePopupUI] ShowMessage: messageText still null, cannot show message: " + message);
                return;
            }

            messageText.text = message;
            messageText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideMessage));
            Invoke(nameof(HideMessage), duration);
        }
        catch (Exception ex)
        {
            Debug.LogError("[HousePopupUI] ShowMessage exception: " + ex);
        }
    }

    private void HideMessage()
    {
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    private void CreateWindow()
    {
        EnsureWindowExists();
    }

    private void EnsureWindowExists()
    {
        if (activeWindow != null) return;
        
        if (canvas == null) ResolveCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("[HousePopupUI] No Canvas found — creating fallback Canvas 'HousePopupUI_AutoCanvas'. Prefer having a Canvas in scene and placing HousePopupUI under it.");
            var go = new GameObject("HousePopupUI_AutoCanvas");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            canvasRect = canvas.GetComponent<RectTransform>();
            canvasCamera = null;
        }
        
        if (windowPrefab != null && windowPrefab.GetComponent<RectTransform>() != null)
        {
            activeWindow = Instantiate(windowPrefab, canvas.transform);
            activeRect = activeWindow.GetComponent<RectTransform>();
            GrabChildrenFromHierarchy(activeWindow.transform);
            activeWindow.SetActive(false);
            Debug.Log("[HousePopupUI] Instantiated windowPrefab as activeWindow.");
            return;
        }
        
        activeWindow = new GameObject("HouseQuestWindow");
        activeWindow.transform.SetParent(canvas.transform, false);
        activeRect = activeWindow.AddComponent<RectTransform>();
        activeRect.sizeDelta = panelSize;

        var bg = activeWindow.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.7f);
        
        activeRect.anchorMin = new Vector2(0.5f, 1f);
        activeRect.anchorMax = new Vector2(0.5f, 1f);
        activeRect.pivot = new Vector2(0.5f, 1f);
        activeRect.anchoredPosition = new Vector2(0f, -10f);

        var holder = new GameObject("Holder");
        holder.transform.SetParent(activeWindow.transform, false);
        var holderRt = holder.AddComponent<RectTransform>();
        holderRt.anchorMin = Vector2.zero;
        holderRt.anchorMax = Vector2.one;
        holderRt.offsetMin = new Vector2(6, 6);
        holderRt.offsetMax = new Vector2(-6, -6);

        var hLayout = holder.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 12;
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childForceExpandHeight = false;
        hLayout.childForceExpandWidth = false;

        int toolCount = Enum.GetNames(typeof(ToolType)).Length;
        iconImages = new Image[toolCount];
        countTexts = new TextMeshProUGUI[toolCount];

        for (int i = 0; i < toolCount; i++)
        {
            var pair = new GameObject($"Pair_{i}");
            pair.transform.SetParent(holder.transform, false);
            var pairRt = pair.AddComponent<RectTransform>();
            pairRt.sizeDelta = new Vector2((panelSize.x - 24f) / toolCount, panelSize.y - 16f);

            var vLayout = pair.AddComponent<VerticalLayoutGroup>();
            vLayout.spacing = 6;
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childForceExpandHeight = false;
            vLayout.childForceExpandWidth = false;

            var iconGO = new GameObject($"Icon_{i}");
            iconGO.transform.SetParent(pair.transform, false);
            var iconRt = iconGO.AddComponent<RectTransform>();
            iconRt.sizeDelta = iconSize;
            var img = iconGO.AddComponent<Image>();
            var provider = ToolIconProvider.Instance;
            if (provider != null) img.sprite = provider.GetIcon((ToolType)i);
            img.preserveAspect = true;
            iconImages[i] = img;

            var txtGO = new GameObject($"Count_{i}");
            txtGO.transform.SetParent(pair.transform, false);
            var txtRt = txtGO.AddComponent<RectTransform>();
            txtRt.sizeDelta = new Vector2(80f, 22f);
            
            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
#if UNITY_EDITOR
            if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
#endif
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 16;
            tmp.text = "0/0";
            countTexts[i] = tmp;
        }
        
        var msgGO = new GameObject("Message");
        msgGO.transform.SetParent(activeWindow.transform, false);
        var msgRt = msgGO.AddComponent<RectTransform>();
        msgRt.anchorMin = new Vector2(0.5f, 0.5f);
        msgRt.anchorMax = new Vector2(0.5f, 0.5f);
        msgRt.anchoredPosition = Vector2.zero;
        msgRt.sizeDelta = new Vector2(panelSize.x - 32f, 28f);
        
        var msgTmp = msgGO.AddComponent<TextMeshProUGUI>();
#if UNITY_EDITOR
        if (TMP_Settings.defaultFontAsset != null) msgTmp.font = TMP_Settings.defaultFontAsset;
#endif
        msgTmp.alignment = TextAlignmentOptions.Center;
        msgTmp.fontSize = 20;
        msgTmp.color = Color.yellow;
        msgTmp.gameObject.SetActive(false);
        messageText = msgTmp;

        activeWindow.SetActive(false);
        Debug.Log("[HousePopupUI] Created fallback activeWindow under Canvas.");
    }

    private void CreateTemporaryMessage()
    {
        if (canvas == null) ResolveCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("[HousePopupUI] CreateTemporaryMessage: no Canvas found, creating fallback Canvas.");
            var go = new GameObject("HousePopupUI_AutoCanvas");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            canvasRect = canvas.GetComponent<RectTransform>();
            canvasCamera = null;
        }
        
        Transform parent = (activeWindow != null) ? activeWindow.transform : canvas.transform;
        if (parent == null)
        {
            Debug.LogError("[HousePopupUI] CreateTemporaryMessage: both activeWindow and canvas.transform are null. Aborting message creation.");
            return;
        }

        var msgGO = new GameObject("HousePopupTempMessage");
        msgGO.transform.SetParent(parent, false);
        var msgRt = msgGO.AddComponent<RectTransform>();
        msgRt.anchorMin = new Vector2(0.5f, 0.5f);
        msgRt.anchorMax = new Vector2(0.5f, 0.5f);
        msgRt.anchoredPosition = Vector2.zero;
        msgRt.sizeDelta = new Vector2(panelSize.x - 32f, 28f);

        var tmp = msgGO.AddComponent<TextMeshProUGUI>();
#if UNITY_EDITOR
        if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
#endif
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 20;
        tmp.color = Color.yellow;
        tmp.gameObject.SetActive(false);
        messageText = tmp;

        Debug.Log("[HousePopupUI] Created temporary message text (HousePopupTempMessage). Parent: " + parent.name);
    }

    private void GrabChildrenFromHierarchy(Transform root)
    {
        var imgs = root.GetComponentsInChildren<Image>(true);
        var tmps = root.GetComponentsInChildren<TextMeshProUGUI>(true);

        int toolCount = Enum.GetNames(typeof(ToolType)).Length;
        iconImages = new Image[toolCount];
        countTexts = new TextMeshProUGUI[toolCount];

        for (int i = 0; i < toolCount; i++)
        {
            var icon = root.Find($"Icon_{i}");
            if (icon != null) iconImages[i] = icon.GetComponent<Image>();
            var count = root.Find($"Count_{i}");
            if (count != null) countTexts[i] = count.GetComponent<TextMeshProUGUI>();
        }

        if (iconImages[0] == null || countTexts[0] == null)
        {
            for (int i = 0; i < toolCount; i++)
            {
                if (i < imgs.Length) iconImages[i] = imgs[i];
                if (i < tmps.Length) countTexts[i] = tmps[i];
            }
        }

        var msg = root.Find("Message");
        if (msg != null) messageText = msg.GetComponent<TextMeshProUGUI>();
    }

    private void UpdateContents(HouseQuest house)
    {
        if (countTexts == null || iconImages == null) return;

        for (int i = 0; i < countTexts.Length; i++)
        {
            var t = (ToolType)i;
            int req = house.GetRequired(t);
            var inv = ServiceLocator.Get<IInventoryService>();
            int have = inv != null ? inv.GetCount(t) : 0;
            if (countTexts[i] != null) countTexts[i].text = $"{have}/{req}";

            var provider = ToolIconProvider.Instance;
            if (provider != null && iconImages[i] != null)
            {
                iconImages[i].sprite = provider.GetIcon(t);
                iconImages[i].preserveAspect = true;
            }
        }
    }

    private void UpdatePosition()
    {
        if (activeRect == null || canvasRect == null || targetHouse == null) return;

        Vector3 worldPos = targetHouse.position + worldOffset;
        Vector3 screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(worldPos) : (Vector3)worldPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvasCamera, out Vector2 localPoint);

        if (followSpeed <= 0f)
            activeRect.anchoredPosition = localPoint;
        else
            activeRect.anchoredPosition = Vector2.Lerp(activeRect.anchoredPosition, localPoint, Time.deltaTime * followSpeed);
    }
}