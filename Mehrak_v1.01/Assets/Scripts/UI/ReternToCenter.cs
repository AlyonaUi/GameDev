using UnityEngine;
using UnityEngine.UI;

public class InstantReturnToCenter : MonoBehaviour
{
    public Transform targetObject; // Мехрак 
    public Vector3 centerPosition = Vector3.zero; 

    private Button _button;

    void Start()
    {
        _button = GetComponent<Button>();
        
        if (_button != null)
        {
            _button.onClick.AddListener(ReturnToCenter);
        }
    }

    private void ReturnToCenter()
    {
        if (targetObject != null)
        {
            // Мгновенное перемещение в центр
            targetObject.position = centerPosition;
        }
        else
        {
            Debug.LogWarning("Target object не назначен!");
        }
    }
}