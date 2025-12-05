using System;
using UnityEngine;

public interface IToolView
{
    void Initialize(ToolType type, ToolDataSO data);

    // Установить спрайт напрямую
    void SetSprite(Sprite sprite);

    // Показать/скрыть визуальную часть (рендерер, анимации)
    void Show();
    void Hide();

    // Мигание (используется молотком)
    void StartBlinking(float blinkInterval = 0.25f);
    void StopBlinking();

    // Эффект сбора: можно проиграть анимацию/частицы и вызывать колбек
    void PlayCollectEffect(Action onComplete = null);

    // Доступ к GameObject
    GameObject GameObject { get; }
}