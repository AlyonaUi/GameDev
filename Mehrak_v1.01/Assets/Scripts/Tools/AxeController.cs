using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AxeController : ToolController
{
    private Rigidbody2D rb;
    private Vector2 currentDirection;
    private Vector2 targetDirection;
    private Coroutine dirRoutine;

    public System.Action<Vector2> OnDirectionChanged;

    private IGameConfig config;

    public override void Initialize(ToolType t, ToolDataSO toolData)
    {
        base.Initialize(t, toolData);
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        rb = GetComponent<Rigidbody2D>();

        currentDirection = Random.insideUnitCircle.normalized;
        targetDirection = currentDirection;
        
        config = ServiceLocator.Get<IGameConfig>();

        if (dirRoutine != null) StopCoroutine(dirRoutine);
        dirRoutine = StartCoroutine(DirectionRoutine());
    }

    private IEnumerator DirectionRoutine()
    {
        PickNewDirection();
        float interval = Mathf.Max(0.05f, data.directionChangeInterval);
        while (true)
        {
            yield return new WaitForSeconds(interval);
            PickNewDirection();
        }
    }

    private void PickNewDirection()
    {
        Vector2 candidate;
        int attempts = 6;
        for (int i = 0; i < attempts; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            candidate = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

            if (data.stayInBounds && config != null)
            {
                var projected = (Vector2)transform.position + candidate * Mathf.Max(1f, data.avoidObstacleDistance + data.speed);
                var bounds = GetSpawnBounds(config);
                if (bounds.Contains(projected))
                {
                    targetDirection = candidate;
                    OnDirectionChanged?.Invoke(targetDirection);
                    return;
                }
                else
                {
                    if (data.reflectOnBounds)
                    {
                        var reflected = ReflectDirectionAgainstBounds(candidate, projected, bounds);
                        targetDirection = reflected.normalized;
                        OnDirectionChanged?.Invoke(targetDirection);
                        return;
                    }
                }
            }
            else
            {
                targetDirection = candidate;
                OnDirectionChanged?.Invoke(targetDirection);
                return;
            }
        }

        targetDirection = -currentDirection;
        OnDirectionChanged?.Invoke(targetDirection);
    }

    private Rect GetSpawnBounds(IGameConfig cfg)
    {
        var half = cfg.SpawnAreaSize * 0.5f;
        var min = cfg.SpawnAreaCenter - half;
        return new Rect(min, cfg.SpawnAreaSize);
    }

    private Vector2 ReflectDirectionAgainstBounds(Vector2 dir, Vector2 projectedPos, Rect bounds)
    {
        Vector2 normal = Vector2.zero;
        if (projectedPos.x < bounds.xMin) normal += Vector2.right;
        else if (projectedPos.x > bounds.xMax) normal += Vector2.left;

        if (projectedPos.y < bounds.yMin) normal += Vector2.up;
        else if (projectedPos.y > bounds.yMax) normal += Vector2.down;

        if (normal == Vector2.zero) return -dir;
        return Vector2.Reflect(dir, normal.normalized);
    }

    private void FixedUpdate()
    {
        if (rb == null || data == null) return;

        currentDirection = Vector2.Lerp(currentDirection, targetDirection, data.turnSmoothness * Time.fixedDeltaTime).normalized;

        var origin = rb.position;
        RaycastHit2D hit = Physics2D.CircleCast(origin, data.avoidObstacleRadius, currentDirection, data.avoidObstacleDistance, data.obstacleMask);
        if (hit.collider != null && hit.collider.gameObject != this.gameObject)
        {
            var reflected = Vector2.Reflect(currentDirection, hit.normal).normalized;
            targetDirection = reflected;
            OnDirectionChanged?.Invoke(targetDirection);
        }

        if (data.stayInBounds)
        {
            var cfg = config ?? ServiceLocator.Get<IGameConfig>();
            if (cfg != null)
            {
                var bounds = GetSpawnBounds(cfg);
                var nextPos = rb.position + currentDirection * data.speed * Time.fixedDeltaTime;
                if (!bounds.Contains(nextPos))
                {
                    if (data.reflectOnBounds)
                    {
                        var reflected = ReflectDirectionAgainstBounds(currentDirection, nextPos, bounds).normalized;
                        currentDirection = reflected;
                        targetDirection = reflected;
                        OnDirectionChanged?.Invoke(targetDirection);
                        nextPos = rb.position + currentDirection * data.speed * Time.fixedDeltaTime;
                    }
                    else
                    {
                        PickNewDirection();
                    }
                }
                rb.MovePosition(nextPos);
                return;
            }
        }

        rb.MovePosition(rb.position + currentDirection * data.speed * Time.fixedDeltaTime);
    }

    public override void ReturnToPool()
    {
        if (dirRoutine != null) StopCoroutine(dirRoutine);
        base.ReturnToPool();
    }
}