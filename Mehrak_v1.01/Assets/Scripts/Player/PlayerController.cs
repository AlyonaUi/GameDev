using UnityEngine;

/// Top-down player controller.
/// Читает ввод в Update и перемещает Rigidbody2D в FixedUpdate.

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IPlayer
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;

    // Сохранённый ввод, используется в FixedUpdate
    private Vector2 moveInput = Vector2.zero;

    // IPlayer реализация
    public Vector2 Position => rb != null ? rb.position : (Vector2)transform.position;
    public Transform Transform => transform;
    public float Speed => speed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        rb.bodyType = RigidbodyType2D.Kinematic;    // Принудительно устанавливаем основные настройки
        rb.freezeRotation = true;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        gameObject.tag = "Player";

        // Регистрируем игрока как IPlayer в ServiceLocator (если нужно)
        ServiceLocator.Register<IPlayer>(this);
    }

    private void Update()
    {
        // Читаем ввод в Update
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        moveInput = input;
    }

    private void FixedUpdate() // Перемещаем Rigidbody в FixedUpdate с фиксированным интервалом (обычно 0.02f)
    {
        if (rb != null)
        {
            if (moveInput.sqrMagnitude > 0.0001f)   // Вычисляем квадрат длины вектора быстрее, игнорируем микро-ввод
            {
                var nextPos = rb.position + moveInput * speed * Time.fixedDeltaTime;
                rb.MovePosition(nextPos);
            }
        }
    }

    private void OnDestroy()
    {
        // опционально: если надо
    }
}