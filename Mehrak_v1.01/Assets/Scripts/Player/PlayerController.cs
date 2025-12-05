using UnityEngine;

/// Top-down player controller.
/// Читает ввод в Update и перемещает Rigidbody2D в FixedUpdate.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IPlayer
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private PlayerStateMachine fsm;

    // Сохранённый ввод, используется в FixedUpdate
    private Vector2 moveInput = Vector2.zero;
    
    public Vector2 Position => rb != null ? rb.position : (Vector2)transform.position;
    public Transform Transform => transform;
    public float Speed => speed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        fsm = new PlayerStateMachine();
        fsm.OnStateChanged += (s) => { /* место для анимаций/логов */ };
        gameObject.tag = "Player";

        // Регистрируем игрока как IPlayer в ServiceLocator
        ServiceLocator.Register<IPlayer>(this);
    }

    private void Update()
    {
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        moveInput = input;
        fsm.UpdateState(input);
    }

    private void FixedUpdate()
    {
        // Перемещаем Rigidbody в FixedUpdate с фиксированным шагом
        if (rb != null)
        {
            if (moveInput.sqrMagnitude > 0.0001f)
            {
                var nextPos = rb.position + moveInput * speed * Time.fixedDeltaTime;
                rb.MovePosition(nextPos);
            }
            // если moveInput == 0, мы просто не двигаем тело — дрейф отсутствует
        }
    }

    private void OnDestroy()
    {
        
    }
}