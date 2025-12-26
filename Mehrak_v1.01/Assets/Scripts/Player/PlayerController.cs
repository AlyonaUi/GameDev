using System.Collections;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour, IPlayer
{
    [SerializeField] private float speed = 5f;

    [InjectOptional] private LevelBounds injectedLevelBounds = null;
    [SerializeField] private LevelBounds levelBoundsFallback = null;

    private Rigidbody2D rb;
    private Vector2 moveInput = Vector2.zero;
    private LevelBounds levelBounds;
    private Vector2 playerHalfSize = new Vector2(0.5f, 0.5f);
    
    public Vector2 Position => rb != null ? rb.position : (Vector2)transform.position;
    public Transform Transform => transform;
    public float Speed => speed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        ServiceLocator.Register<IPlayer>(this);

        levelBounds = injectedLevelBounds != null ? injectedLevelBounds : levelBoundsFallback;
        if (levelBounds == null)
            levelBounds = FindObjectOfType<LevelBounds>();
    }

    private void Start()
    {
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        yield return null;
        
        var cols = GetComponents<Collider2D>();
        Collider2D usedCol = null;
        if (cols != null && cols.Length > 0)
        {
            foreach (var c in cols)
            {
                if (!c.isTrigger)
                {
                    usedCol = c;
                    break;
                }
            }
            if (usedCol == null) usedCol = cols[0];
        }

        if (usedCol != null)
        {
            var ext = usedCol.bounds.extents;
            playerHalfSize = new Vector2(Mathf.Abs(ext.x), Mathf.Abs(ext.y));
        }
        
        if (levelBounds != null)
        {
            Vector2 clamped = levelBounds.ClampPosition(rb.position, playerHalfSize);
            if (clamped != rb.position)
            {
                rb.linearVelocity = Vector2.zero;
                rb.position = clamped;
                transform.position = clamped;
            }
        }
    }

    private void Update()
    {
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f) input.Normalize();
        moveInput = input;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (moveInput.sqrMagnitude > 0.0001f)
        {
            var nextPos = rb.position + moveInput * speed * Time.fixedDeltaTime;
            if (levelBounds != null)
                nextPos = levelBounds.ClampPosition(nextPos, playerHalfSize);
            rb.MovePosition(nextPos);
        }
        else
        {
            if (levelBounds != null)
            {
                var clamped = levelBounds.ClampPosition(rb.position, playerHalfSize);
                if (clamped != rb.position)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.position = clamped;
                    transform.position = clamped;
                }
            }
        }
    }
}