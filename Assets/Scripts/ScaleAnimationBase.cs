using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleAnimationBase : MonoBehaviour
{
    /// <summary>
    /// Время в секундах, прошедшее с начала анимации
    /// </summary>
    protected float timePassed = 0.0f;
    /// <summary>
    /// Масштаб объекта в начале анимации
    /// </summary>
    protected float beginScale;
    /// <summary>
    /// Масштаб объекта в конце анимации
    /// </summary>
    protected float endScale;
    /// <summary>
    /// Сохраненный масштаб спрайта, до применения всех анимаций
    /// </summary>
    protected Vector3 savedSpriteScale;
    /// <summary>
    /// Активна анимация или нет.
    /// </summary>
    protected bool active = false;
    /// <summary>
    /// Спрайт, размер котогоро будет изменяться
    /// </summary>
    protected Transform spriteTransform;

    /// <summary>
    /// Запуск анимации.
    /// </summary>
    /// <param name="beginScale">Масштаб объекта в начале анимации.</param>
    /// <param name="endScale">Масштаб объекта в конце анимации.</param>
    /// <param name="deleteAfterEnd">Удалить GameObject после завершения анимации.</param>
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
