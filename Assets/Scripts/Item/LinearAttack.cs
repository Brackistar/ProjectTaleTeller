using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearAttack : WeaponController
{
    public override IEnumerator Attack(float time, Vector2 origin, float magnitude, bool isLeft, float angle)
    {
        base.Ready(origin);
        float elapsedTime = 0.0f,
            step;

        //step = (magnitude / time) * 2;
        //step = (magnitude * 2) / time;

        while (elapsedTime < time)
        {
            Vector2 target = origin + LevelController.AngleToVector2(angle, magnitude);

            if (isLeft)
                target.x *= -1;

            step = (Vector2.Distance(transform.position, target) / (time - elapsedTime) * Time.deltaTime) * 2;

            if ((Vector2)transform.position != target)
            {
                transform.position = Vector2.MoveTowards(
                    current: transform.position,
                    target: target,
                    maxDistanceDelta: step);
            }
            else
            {
                transform.position = Vector2.MoveTowards(
                    current: transform.position,
                    target: target,
                    maxDistanceDelta: -step);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return base.Attack();
    }
}
