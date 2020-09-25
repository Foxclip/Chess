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
    /// Время в секундах, оставшееся до конца анимации
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
    /// Масштаб префаба.
    /// </summary>
    private Vector3 prefabScale;
    /// <summary>
    /// Авктивна анимация или нет.
    /// </summary>
    private bool active = false;

    void Update()
    {
        if(active) 
        {
            timePassed += Time.deltaTime;
            if(timePassed > duration)
            {
                timePassed = duration;
                active = false;
            }
            float completionPercentage = timePassed / duration;
            float objectScale = (float)Utils.MapRange(completionPercentage, 0.0f, 1.0f, beginScale, endScale);
            transform.localScale = prefabScale * objectScale;
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
        prefabScale = transform.localScale;
        transform.localScale = prefabScale * beginScale;
    }
}
