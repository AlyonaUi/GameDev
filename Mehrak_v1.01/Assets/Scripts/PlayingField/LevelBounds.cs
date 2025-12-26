using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LevelBounds : MonoBehaviour
{
    [Tooltip("Collider2D, который описывает границы уровня. Если пусто — будет использован Collider2D на этом GameObject.")]
    public Collider2D boundsCollider;

    private Vector2 min;
    private Vector2 max;

    private void Awake()
    {
        if (boundsCollider == null)
            boundsCollider = GetComponent<Collider2D>();

        if (boundsCollider == null)
        {
            Debug.LogError("LevelBounds: нет Collider2D для вычисления границ. Добавьте BoxCollider2D или TilemapCollider2D на объект.");
            return;
        }
        
        var b = boundsCollider.bounds;
        min = b.min;
        max = b.max;
    }
    
    public Vector2 ClampPosition(Vector2 desiredPos, Vector2 halfSize)
    {
        float x = Mathf.Clamp(desiredPos.x, min.x + halfSize.x, max.x - halfSize.x);
        float y = Mathf.Clamp(desiredPos.y, min.y + halfSize.y, max.y - halfSize.y);
        return new Vector2(x, y);
    }
    
    private void OnDrawGizmosSelected()
    {
        Collider2D col = boundsCollider ? boundsCollider : GetComponent<Collider2D>();
        if (col == null) return;
        var b = col.bounds;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(b.center, b.size);
    }
}