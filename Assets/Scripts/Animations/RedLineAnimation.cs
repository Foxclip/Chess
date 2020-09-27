using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedLineAnimation : MonoBehaviour
{
    /// <summary>
    /// Длительность фазы перемещения.
    /// </summary>
    public float drawDuration = 1.0f;
    /// <summary>
    /// Длительность фазы исчезания.
    /// </summary>
    public float fadeoutDuration = 1.0f;
    /// <summary>
    /// Нелинейное изменение скорости: при маленьких значениях анимация замедляется, при больших - ускоряется
    /// </summary>
    public float power = 0.3f;
    /// <summary>
    /// Нелинейность исчезания.
    /// </summary>
    public float fadeoutPower = 1.0f;
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
    /// Стадия исчезания.
    /// </summary>
    private bool fadeoutPhase = false;
    /// <summary>
    /// Вызывется при завершении анимации
    /// </summary>
    private Action finishedCallback;
    /// <summary>
    /// Материал объекта.
    /// </summary>
    private Material material;

    public void FixedUpdate()
    {
        if(active)
        {
            timePassed += Time.fixedDeltaTime;
            // Переход к фазе исчезания
            if(!fadeoutPhase && (timePassed > drawDuration))
            {
                fadeoutPhase = true;
                timePassed = drawDuration;
                finishedCallback();
            }
            // Конец анимации
            if(fadeoutPhase && (timePassed > drawDuration + fadeoutDuration))
            {
                timePassed = drawDuration + fadeoutDuration;
                active = false;
            }
            // Фаза перемещения
            if(!fadeoutPhase)
            {
                float completionPercentage = timePassed / drawDuration;
                float nonlinear = Mathf.Pow(completionPercentage, power);
                Vector3 diff = endPos - beginPos;
                Vector3 diffScaled = diff * nonlinear;
                Vector3 diffScaledHalf = diffScaled / 2.0f;
                Vector3 newPos = beginPos + diffScaledHalf;
                Vector3 newPosAdjusted = new Vector3(newPos.x, newPos.y, 0.5f);
                // Меняем масштаб и позицию объекта
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, diffScaled.magnitude);
                transform.position = newPosAdjusted;
            }
            // Фаза исчезания
            else
            {
                float completionPercentage = (timePassed - drawDuration) / fadeoutDuration;
                float nonlinear = Mathf.Pow(completionPercentage, fadeoutPower);
                float nonlinearReversed = 1 - nonlinear;
                // Меняем прозрачность материала
                material.SetColor("_Color", new Color(1.0f, 0.0f, 0.0f, nonlinearReversed));
            }
        }
    }

    /// <summary>
    /// Запуск анимации.
    /// </summary>
    /// <param name="beginPos">Начало линии.</param>
    /// <param name="endPos">Конец линии.</param>
    public void StartAnimation(Vector3 beginPos, Vector3 endPos, Action finishedCallback)
    {
        this.beginPos = beginPos;
        this.endPos = endPos;
        this.finishedCallback = finishedCallback;
        timePassed = 0.0f;
        active = true;
        material = GetComponent<MeshRenderer>().material;
        // Ставим объект в нужное место
        transform.position = beginPos;
        // Сразу считаем угол
        transform.LookAt(endPos);
    }
}
