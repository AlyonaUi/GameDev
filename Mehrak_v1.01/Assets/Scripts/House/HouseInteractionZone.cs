using UnityEngine;
using Zenject;

public class HouseInteractionZone : MonoBehaviour
{
    [Tooltip("Ссылка на объект панели дома")]
    public GameObject houseQuestPanelObject;

    [Tooltip("Ссылка на панель победы")]
    public GameObject winLevelPanelObject;

    private HouseQuestPanel housePanel;
    private bool playerNear = false;

    [Inject] private LevelManager levelManager = null;

    private void Awake()
    {
        if (houseQuestPanelObject != null)
        {
            housePanel = houseQuestPanelObject.GetComponent<HouseQuestPanel>();
            houseQuestPanelObject.SetActive(false);
        }
        if (winLevelPanelObject != null)
            winLevelPanelObject.SetActive(false);
    }

    private void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.Q))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (housePanel == null)
        {
            Debug.LogWarning("HouseInteractionZone: housePanel is null");
            return;
        }

        if (housePanel.AreRequirementsMet())
        {
            if (levelManager != null)
                levelManager.ShowWinPanel(winLevelPanelObject);
            else
                Debug.LogWarning("HouseInteractionZone: LevelManager not injected");
        }
        else
        {
            Debug.Log("HouseInteractionZone: requirements NOT met: " + housePanel.GetRequirementsSummary());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            if (houseQuestPanelObject != null) houseQuestPanelObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            if (houseQuestPanelObject != null) houseQuestPanelObject.SetActive(false);
        }
    }
}