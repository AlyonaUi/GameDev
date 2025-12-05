using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ToolView : MonoBehaviour, IToolView
{
    [Header("Components")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public ParticleSystem collectVfxPrefab;

    private Coroutine blinkCoroutine;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public GameObject GameObject => gameObject;

    public void Initialize(ToolType type, ToolDataSO data)
    {
        if (data != null && spriteRenderer != null)
            spriteRenderer.sprite = data.sprite;
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null) spriteRenderer.sprite = sprite;
    }

    public void Show()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (animator != null) animator.enabled = true;
    }

    public void Hide()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (animator != null) animator.enabled = false;
    }

    public void StartBlinking(float blinkInterval = 0.25f)
    {
        StopBlinking();
        blinkCoroutine = StartCoroutine(BlinkRoutine(blinkInterval));
    }

    public void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    private IEnumerator BlinkRoutine(float interval)
    {
        while (true)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(interval);
        }
    }

    public void PlayCollectEffect(Action onComplete = null)
    {
        if (animator != null)
        {
            animator.SetTrigger("Collect");
        }

        if (collectVfxPrefab != null)
        {
            var vfx = Instantiate(collectVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx.gameObject, vfx.main.duration + 0.5f);
        }
        
        onComplete?.Invoke();
    }
}