using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HammerController : ToolController
{
    private Coroutine lifeRoutine;

    public override void OnSpawn()
    {
        base.OnSpawn();
        if (lifeRoutine != null) StopCoroutine(lifeRoutine);
        lifeRoutine = StartCoroutine(LifeCycle());
    }

    private IEnumerator LifeCycle()
    {
        float elapsed = 0f;
        bool blinking = false;

        while (elapsed < data.lifetime)
        {
            if (!blinking && elapsed >= data.blinkAt)
            {
                blinking = true;
                if (view != null)
                    view.StartBlinking(0.25f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        
        ReturnToPool();
    }

    public override void Collect()
    {
        if (col != null) col.enabled = false;

        if (view != null)
        {
            view.PlayCollectEffect(() =>
            {
                events?.Publish(new ToolCollectedEvent { tool = this });
                ReturnToPool();
            });
        }
        else
        {
            events?.Publish(new ToolCollectedEvent { tool = this });
            ReturnToPool();
        }
    }

    public override void ReturnToPool()
    {
        if (lifeRoutine != null) StopCoroutine(lifeRoutine);
        base.ReturnToPool();
    }
}