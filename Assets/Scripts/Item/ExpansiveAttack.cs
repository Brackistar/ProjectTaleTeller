using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpansiveAttack : WeaponController
{
    float finalDiameter = 0.0f;
    public override IEnumerator Attack(float time, Vector2 origin, float magnitude, bool isLeft, float angle)
    {
        base.Ready(origin);
        float elapsedTime = 0.0f,
            step = 0.0f;
        CircleCollider2D collider;

        try
        {
            collider = (CircleCollider2D)HitBox;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                message: ex.Message);
            yield break;
        }

        finalDiameter = collider.radius + magnitude;

        step = (finalDiameter / time) * 2;

        while (elapsedTime <= time)
        {
            if (collider.radius <= finalDiameter)
            {
                collider.radius += step;
            }
            else
            {
                collider.radius -= step;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return base.Attack();
    }
}
