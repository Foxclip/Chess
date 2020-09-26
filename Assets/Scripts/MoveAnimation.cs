using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAnimation : MonoBehaviour
{
    /// <summary>
    /// Длительность анимации
    /// </summary>
    public float duration = 1.0f;
    /// <summary>
    /// Нелинейное изменение скорости: при маленьких значениях анимация замедляется, при больших - ускоряется
    /// </summary>
    public float power = 0.2f;
    /// <summary>
    /// Время в секундах, прошедшее с начала анимации
    /// </summary>
    private float timePassed = 0.0f;
    /// <summary>
    /// Позиция объекта в начале анимации
    /// </summary>
    private Vector3 beginPos;
    /// <summary>
    /// Позиция объекта в конце анимации
    /// </summary>
    private Vector3 endPos;
    /// <summary>
    /// Активна анимация или нет.
    /// </summary>
    private bool active = false;
    /// <summary>
    /// Вызывется при завершении анимации
    /// </summary>
    private Action finishedCallback;

    void Update()
    {
        if(active)
        {
            timePassed += Time.deltaTime;
            if(timePassed > duration)
            {
                timePassed = duration;
                active = false;
                finishedCallback?.Invoke();
            }
            float completionPercentage = timePassed / duration;
            float nonlinear = Mathf.Pow(completionPercentage, power);
            Vector3 diff = endPos - beginPos;
            Vector3 diffScaled = diff * nonlinear;
            Vector3 newPos = beginPos + diffScaled;
            transform.position = newPos;
        }
    }

    /// <summary>
    /// Запуск анимации.
    /// </summary>
    /// <param name="beginScale">Позиция объекта в начале анимации.</param>
    /// <param name="endScale">Позиция объекта в конце анимации.</param>
    public void StartAnimation(Vector3 beginPos, Vector3 endPos, Action finishedCallback = null)
    {
        this.beginPos = beginPos;
        this.endPos = endPos;
        this.finishedCallback = finishedCallback;
        timePassed = 0.0f;
        active = true;
    }
}
