using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleAnimation : MonoBehaviour
{
    /// <summary>
    /// Длительность анимации
    /// </summary>
    public float duration = 1.0f;
    /// <summary>
    /// Нелинейное изменение размера: при маленьких значениях анимация замедляется, при больших - ускоряется
    /// </summary>
    public float power = 0.2f;
    /// <summary>
    /// Время в секундах, прошедшее с начала анимации
    /// </summary>
    private float timePassed = 0.0f;
    /// <summary>
    /// Масштаб объекта в начале анимации
    /// </summary>
    private float beginScale;
    /// <summary>
    /// Масштаб объекта в конце анимации
    /// </summary>
    private float endScale;
    /// <summary>
    /// Сохраненный масштаб спрайта, до применения всех анимаций
    /// </summary>
    private Vector3 savedSpriteScale;
    /// <summary>
    /// Активна анимация или нет.
    /// </summary>
    private bool active = false;
    /// <summary>
    /// Спрайт, размер котогоро будет изменяться
    /// </summary>
    private Transform spriteTransform;

    void FixedUpdate()
    {
        if(active) 
        {
            timePassed += Time.fixedDeltaTime;
            if(timePassed > duration)
            {
                timePassed = duration;
                active = false;
            }
            float completionPercentage = timePassed / duration;
            float nonlinear = Mathf.Pow(completionPercentage, power);
            float objectScale = (float)Utils.MapRange(nonlinear, 0.0f, 1.0f, beginScale, endScale);
            spriteTransform.localScale = savedSpriteScale * objectScale;
        }
    }

    /// <summary>
    /// Запуск анимации.
    /// </summary>
    /// <param name="beginScale">Масштаб объекта в начале анимации.</param>
    /// <param name="endScale">Масштаб объекта в конце анимации.</param>
    public void StartAnimation(float beginScale, float endScale)
    {
        this.beginScale = beginScale;
        this.endScale = endScale;
        timePassed = 0.0f;
        active = true;
        // Иначе в первом кадре анимации объект будет в исходном размере
        spriteTransform = transform.GetChild(0);
        savedSpriteScale = spriteTransform.localScale;
        spriteTransform.localScale = savedSpriteScale * beginScale;
    }
}
