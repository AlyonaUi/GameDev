using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class WinLevelPanelController : MonoBehaviour
{
    public Button retryButton;
    public TMP_Text youWinText;
    public TMP_Text retryText;
    public Image imageWinMehrak;

    [Inject] private LevelManager levelManager = null;

    private void Awake()
    {
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
        gameObject.SetActive(false);
    }

    private void OnRetryClicked()
    {
        levelManager?.RestartGame();
    }
}