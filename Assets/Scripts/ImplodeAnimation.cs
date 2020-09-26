using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImplodeAnimation : ScaleAnimationBase
{
    /// <summary>
    /// Длительность анимации
    /// </summary>
    public float duration = 1.0f;
    /// <summary>
    /// Нелинейное изменение размера: при маленьких значениях анимация замедляется, при больших - ускоряется
    /// </summary>
    public float power = 0.2f;

    public void FixedUpdate()
    {
        if(active)
        {
            timePassed += Time.fixedDeltaTime;
            if(timePassed > duration)
            {
                timePassed = duration;
                active = false;
                Destroy(gameObject);
            }
            float completionPercentage = timePassed / duration;
            float nonlinear = Mathf.Pow(completionPercentage, power);
            float objectScale = (float)Utils.MapRange(nonlinear, 0.0f, 1.0f, beginScale, endScale);
            spriteTransform.localScale = savedSpriteScale * objectScale;
        }
    }
}
