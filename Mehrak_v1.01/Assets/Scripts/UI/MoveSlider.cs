using UnityEngine;
using UnityEngine.UI;

public class SpriteTransparencyController : MonoBehaviour
{
    public Slider transparencySlider;
    public GameObject targetObject;

    [Range(0f, 1f)] public float minAlpha = 0.3f;
    [Range(0f, 1f)] public float maxAlpha = 1f;

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    void Start()
    {
        // Находим целевой объект
        if (targetObject == null)
        {
            targetObject = GameObject.FindGameObjectWithTag("Player");
        }

        // Получаем SpriteRenderer
        if (targetObject != null)
        {
            _spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
        }

        // Настраиваем слайдер
        if (transparencySlider != null)
        {
            transparencySlider.minValue = minAlpha;
            transparencySlider.maxValue = maxAlpha;
            transparencySlider.value = maxAlpha;
            transparencySlider.onValueChanged.AddListener(OnTransparencyChanged);
        }
    }

    void OnTransparencyChanged(float alphaValue)
    {
        if (_spriteRenderer != null)
        {
            Color newColor = _spriteRenderer.color;
            newColor.a = alphaValue;
            _spriteRenderer.color = newColor;
        }
    }
}
