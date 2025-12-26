using UnityEngine;
using Zenject;

public class LevelManager : MonoBehaviour
{
    [Inject] private IInventoryService inventory = null;
    [Inject] private PlayerController playerController = null;
    [Inject] private HouseQuestPanel housePanel = null; 

    [Header("Ссылки")]
    public GameObject winLevelPanelObject; 
    public Transform playerStartTransform;
    public HouseQuestPanel houseQuestPanelObject; 

    private void Start()
    {
        if (winLevelPanelObject != null) winLevelPanelObject.SetActive(false);
    }

    public void ShowWinPanel(GameObject winPanel)
    {
        if (winPanel == null) return;
        
        if (playerController != null)
            playerController.enabled = false;
        
        Time.timeScale = 0f;
        
        winPanel.SetActive(true);

        Debug.Log("LevelManager: Win panel shown, game paused.");
    }

    public void RestartGame()
    {
        Debug.Log("LevelManager: RestartGame called.");
        
        if (winLevelPanelObject != null) winLevelPanelObject.SetActive(false);
        
        Time.timeScale = 1f;
        
        inventory?.Reset();
        
        HouseQuestPanel panel = houseQuestPanelObject ?? housePanel;
        panel?.GenerateRequirements();
        
        if (panel != null) panel.gameObject.SetActive(false);
        
        if (playerController != null && playerStartTransform != null)
        {
            var playerGO = playerController.gameObject;
            var rb = playerGO.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = playerStartTransform.position;
                rb.linearVelocity = Vector2.zero;
            }
            playerGO.transform.position = playerStartTransform.position;
        }
        
        if (playerController != null)
            playerController.enabled = true;

        Debug.Log("LevelManager: Restart complete.");
    }
}