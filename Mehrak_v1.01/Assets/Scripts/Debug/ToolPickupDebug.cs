using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ToolPickupDebug : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"ToolPickupDebug: Player entered trigger on {gameObject.name}");
        }
    }
}
