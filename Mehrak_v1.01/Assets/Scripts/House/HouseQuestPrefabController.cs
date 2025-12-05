using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class HouseQuestPrefabController : MonoBehaviour
{
    [Header("Assign the popup GameObject instance (scene), keep it inactive by default")]
    public GameObject popupInstance; 
    [Header("Quest requirements (leave zeros to randomize on Start)")]
    public int[] requiredCounts;

    [Header("Behavior")]
    public Vector3 popupWorldOffset = new Vector3(0f, 1.2f, 0f);
    public bool reloadSceneOnSuccess = false; 
    public float messageDuration = 1.5f;

    private Transform playerTransform;
    private bool playerIsNear = false;

    // cached children
    private Image[] iconImages;
    private TextMeshProUGUI[] countTexts;
    private TextMeshProUGUI messageText;

    private int toolCount;
    
    private static GameObject sharedPopupInstance = null;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Awake()
    {
        toolCount = Enum.GetNames(typeof(ToolType)).Length;
        EnsureArraySize();
    }

    private void Start()
    {
        EnsureArraySize();

        bool allZero = true;
        foreach (var v in requiredCounts) if (v > 0) { allZero = false; break; }
        if (allZero) RandomizeRequirements();
        
        if (popupInstance == null)
        {
            Debug.LogWarning($"HouseQuestPrefabController on '{name}': popupInstance is not assigned in inspector. Trying to find a scene object named 'HousePopupPrefab'...");
            var found = GameObject.Find("HousePopupPrefab");
            if (found != null)
            {
                popupInstance = found;
                Debug.Log($"HouseQuestPrefabController on '{name}': found scene popupInstance '{popupInstance.name}'.");
            }
            else
            {
                Debug.LogError($"HouseQuestPrefabController on '{name}': popupInstance not found. Assign the existing HousePopupPrefab (scene instance) under Canvas.");
                return;
            }
        }
        else
        {
            #if UNITY_EDITOR
            if (popupInstance.scene.rootCount == 0)
            {
                var searchName = popupInstance.name.Replace("(Clone)", "").Replace(" (Prefab)", "");
                var found = GameObject.Find(searchName);
                if (found != null)
                {
                    Debug.LogWarning($"HouseQuestPrefabController on '{name}': inspector reference '{popupInstance.name}' seems not to be a scene instance. Using scene object '{found.name}' instead.");
                    popupInstance = found;
                }
                else
                {
                    Debug.LogWarning($"HouseQuestPrefabController on '{name}': inspector reference appears to be a prefab asset and no scene object named '{searchName}' was found. Please assign the scene popup instance under Canvas.");
                }
            }
            #endif
        }
        
        if (sharedPopupInstance == null)
        {
            sharedPopupInstance = popupInstance;
        }
        else
        {
            if (popupInstance != sharedPopupInstance)
            {
                Debug.LogWarning($"HouseQuestPrefabController on '{name}': another house already registered the popup. Reusing shared popup instance '{sharedPopupInstance.name}'.");
                if (popupInstance != null) popupInstance.SetActive(false);
                popupInstance = sharedPopupInstance;
            }
        }
        
        CachePopupChildren();
        
        if (popupInstance != null)
            popupInstance.SetActive(false);
    }

    private void EnsureArraySize()
    {
        if (requiredCounts == null || requiredCounts.Length != toolCount)
            requiredCounts = new int[toolCount];
    }

    private void CachePopupChildren()
    {
        if (popupInstance == null)
        {
            Debug.LogError($"HouseQuestPrefabController on '{name}': popupInstance is null in CachePopupChildren.");
            return;
        }
        
        iconImages = new Image[toolCount];
        countTexts = new TextMeshProUGUI[toolCount];
        messageText = null;

        for (int i = 0; i < toolCount; i++)
        {
            iconImages[i] = FindChildComponent<Image>(popupInstance.transform, $"Icon_{i}");
            countTexts[i] = FindChildComponent<TextMeshProUGUI>(popupInstance.transform, $"Count_{i}");
        }
        
        messageText = FindChildComponent<TextMeshProUGUI>(popupInstance.transform, "Message");
        
        var provider = ToolIconProvider.Instance;
        for (int i = 0; i < toolCount; i++)
        {
            if (iconImages[i] != null)
            {
                var rt = iconImages[i].rectTransform;
                rt.localScale = Vector3.one;
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 36f);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 36f);

                if (provider != null)
                {
                    iconImages[i].sprite = provider.GetIcon((ToolType)i);
                    iconImages[i].preserveAspect = true;
                }
            }
        }
        
        string iconsMapped = "";
        string countsMapped = "";
        for (int i = 0; i < toolCount; i++)
        {
            iconsMapped += (iconImages[i] != null ? iconImages[i].name : "null") + (i == toolCount - 1 ? "" : ", ");
            countsMapped += (countTexts[i] != null ? countTexts[i].name : "null") + (i == toolCount - 1 ? "" : ", ");
        }
        Debug.Log($"[HouseQuestPrefabController] Cached icons: {iconsMapped}");
        Debug.Log($"[HouseQuestPrefabController] Cached counts: {countsMapped}");
        if (messageText != null) Debug.Log($"[HouseQuestPrefabController] Cached message text: {messageText.name}");
        else Debug.Log("[HouseQuestPrefabController] Message text not found in popup (optional).");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerIsNear = true;
        playerTransform = other.transform;
        ShowPopup();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerIsNear = false;
        playerTransform = null;
        HidePopup();
    }

    private void Update()
    {
        if (!playerIsNear) return;
        
        if (popupInstance != null && popupInstance.activeSelf)
        {
            UpdatePopupPosition();
            UpdatePopupCounts();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            TryDeliver();
        }
    }

    private void ShowPopup()
    {
        if (popupInstance == null) return;
        if (iconImages == null || countTexts == null) CachePopupChildren();
        UpdatePopupCounts();
        popupInstance.SetActive(true);
        UpdatePopupPosition();
    }

    private void HidePopup()
    {
        if (popupInstance == null) return;
        popupInstance.SetActive(false);
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    private void UpdatePopupCounts()
    {
        var inv = ServiceLocator.Get<IInventoryService>();
        for (int i = 0; i < toolCount; i++)
        {
            int req = requiredCounts[i];
            int have = inv != null ? inv.GetCount((ToolType)i) : 0;
            if (countTexts[i] != null)
            {
                countTexts[i].text = $"{have}/{req}";
            }
            else
            {
                Debug.LogWarning($"HouseQuestPrefabController on '{name}': Count_{i} TextMeshProUGUI not found in popup. Expected child named 'Count_{i}'.");
            }
        }
    }

    private void UpdatePopupPosition()
    {
        var popupRt = popupInstance.GetComponent<RectTransform>();
        if (popupRt == null) return;

        Camera cam = Camera.main;
        Vector3 worldPos = transform.position + popupWorldOffset;
        Vector3 screenPos = cam != null ? cam.WorldToScreenPoint(worldPos) : (Vector3)worldPos;

        var canvas = popupInstance.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        var canvasRect = canvas.GetComponent<RectTransform>();
        var canvasCam = (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace) ? canvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvasCam, out Vector2 localPoint))
        {
            popupRt.anchoredPosition = localPoint;
        }
    }

    private void TryDeliver()
    {
        var inv = ServiceLocator.Get<IInventoryService>();
        if (inv == null)
        {
            ShowTempMessage("Ошибка: инвентарь недоступен");
            return;
        }
        
        for (int i = 0; i < toolCount; i++)
        {
            int need = requiredCounts[i];
            if (need <= 0) continue;
            int have = inv.GetCount((ToolType)i);
            if (have < need)
            {
                ShowTempMessage("Не хватает инструментов");
                return;
            }
        }
        
        for (int i = 0; i < toolCount; i++)
        {
            int need = requiredCounts[i];
            if (need <= 0) continue;
            inv.Add((ToolType)i, -need);
        }

        ShowTempMessage("Победа!");
        
        inv.Reset();
        RandomizeRequirements();
        
        UpdatePopupCounts();
        
        if (reloadSceneOnSuccess)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    private void ShowTempMessage(string text)
    {
        if (messageText != null)
        {
            messageText.text = text;
            messageText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideMessage));
            Invoke(nameof(HideMessage), messageDuration);
        }
        else
        {
            messageText = FindChildComponent<TextMeshProUGUI>(popupInstance.transform, "Message");
            if (messageText == null)
            {
                var go = new GameObject("Message");
                go.transform.SetParent(popupInstance.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(200f, 28f);
                var tmp = go.AddComponent<TextMeshProUGUI>();
    #if UNITY_EDITOR
                if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
    #endif
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 18;
                tmp.color = Color.yellow;
                messageText = tmp;
            }
            messageText.text = text;
            messageText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideMessage));
            Invoke(nameof(HideMessage), messageDuration);
        }
    }

    private void HideMessage()
    {
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    private void RandomizeRequirements(int maxEach = 5)
    {
        var rng = new System.Random();
        for (int i = 0; i < requiredCounts.Length; i++)
            requiredCounts[i] = rng.Next(0, maxEach + 1);
    }
    
    private T FindChildComponent<T>(Transform root, string childName) where T : Component
    {
        if (root == null) return null;
        foreach (Transform child in root)
        {
            if (child.name == childName)
            {
                var comp = child.GetComponent<T>();
                if (comp != null) return comp;
            }
            var res = FindChildComponent<T>(child, childName);
            if (res != null) return res;
        }
        return null;
    }
}