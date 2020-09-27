using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeAnimation : MonoBehaviour
{
    /// <summary>
    /// Маскимальное расстояние разброса
    /// </summary>
    public float maxDistance = 1.0f;
    /// <summary>
    /// Длительность анимации
    /// </summary>
    public float duration = 1.0f;
    /// <summary>
    /// Нелинейность затуханя разброса.
    /// </summary>
    public float power = 1.0f;
    /// <summary>
    /// Время в секундах, прошедшее с начала анимации
    /// </summary>
    private float timePassed = 0.0f;
    /// <summary>
    /// Активна анимация или нет.
    /// </summary>
    private bool active = false;
    /// <summary>
    /// Начальная позиция объекта
    /// </summary>
    private Vector3 initialPosition;
    /// <summary>
    /// Спрайт, который будет анимирован
    /// </summary>
    private Transform spriteTransform;

    public void FixedUpdate()
    {
        if(active)
        {
            timePassed += Time.fixedDeltaTime;
            if(timePassed > duration)
            {
                timePassed = duration;
                transform.position = initialPosition;
                active = false;
            }
            float completionPercentage = timePassed / duration;
            float nonlinear = Mathf.Pow(completionPercentage, power);
            Vector3 diff = UnityEngine.Random.insideUnitCircle * maxDistance * (1 - nonlinear);
            spriteTransform.position = initialPosition + diff;
        }
    }

    /// <summary>
    /// Запуск анимации.
    /// </summary>
    public void StartAnimation()
    {
        timePassed = 0.0f;
        active = true;
        spriteTransform = transform.GetChild(0);
        // Сохраняем пначальную позицию
        initialPosition = transform.position;
    }
}
