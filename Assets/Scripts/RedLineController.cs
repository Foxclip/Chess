using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedLineController : MonoBehaviour
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

    public void FixedUpdate()
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
            Vector3 diff = endPos - beginPos;
            Vector3 diffScaled = diff * nonlinear;
            Vector3 diffScaledHalf = diffScaled / 2.0f;
            Vector3 newPos = beginPos + diffScaledHalf;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, diffScaled.magnitude);
            transform.position = newPos;
        }
    }

    /// <summary>
    /// Запуск анимации.
    /// </summary>
    /// <param name="beginPos">Начало линии.</param>
    /// <param name="endPos">Конец линии.</param>
    public void StartAnimation(Vector3 beginPos, Vector3 endPos)
    {
        this.beginPos = beginPos;
        this.endPos = endPos;
        timePassed = 0.0f;
        active = true;
        // Ставим объект в нужное место
        transform.position = beginPos;
        // Сразу считаем угол
        transform.LookAt(endPos);
        Debug.Log(transform.rotation.eulerAngles);
    }
}
