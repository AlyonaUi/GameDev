using UnityEngine;
using UnityEngine.UI;

public class SimpleGlobalLightToggle : MonoBehaviour
{
    public UnityEngine.Rendering.Universal.Light2D globalLight;

    public float normalBrightness = 1f;
    public float dimmedBrightness = 0.3f;

    void Start()
    {
        Toggle toggle = GetComponent<Toggle>();
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(SwitchLight);

            // Устанавливаем начальное состояние
            SwitchLight(toggle.isOn);
        }
    }

    void SwitchLight(bool isNormal)
    {
        if (globalLight == null)
        {
            Debug.LogWarning("Global Light не назначен!");
            return;
        }

        if (isNormal)
        {
            // Обычный свет
            globalLight.intensity = normalBrightness;
            globalLight.color = Color.white;
        }
        else
        {
            // Затемнённый свет
            globalLight.intensity = dimmedBrightness;
            globalLight.color = new Color(0.4f, 0.4f, 0.5f);
        }
    }
}