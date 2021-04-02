using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SwingAttack : WeaponController
{
    public override IEnumerator Attack(float time, Vector2 origin, float magnitude, bool isLeft, float angle)
    {
        Vector2 MaxYPoint = transform.parent
            .gameObject
            .GetComponent<PolygonCollider2D>()
            .points
            .OrderByDescending(_ => _.y)
            .FirstOrDefault();
        float characterHeigth = transform.parent.TransformPoint(MaxYPoint).y,
            characterHandPosition;
        Vector3 axis;

        if (isLeft)
        {
            Vector2 LeftPosition = transform.parent
            .gameObject
            .GetComponent<PolygonCollider2D>()
            .points
            .OrderBy(_ => _.x)
            .FirstOrDefault();
            characterHandPosition = transform.parent.TransformPoint(LeftPosition).x;

            axis = Vector3.forward;
        }
        else
        {
            Vector2 LeftPosition = transform.parent
            .gameObject
            .GetComponent<PolygonCollider2D>()
            .points
            .OrderByDescending(_ => _.x)
            .FirstOrDefault();
            characterHandPosition = transform.parent.TransformPoint(LeftPosition).x;

            axis = Vector3.back;
        }
        Vector2 initialPosition = new Vector2(
            x: characterHandPosition,
            y: characterHeigth);

        base.Ready(initialPosition);

        float elapsedTime = 0.0f,
            step = (angle / time) * Time.deltaTime;

        while (elapsedTime < time)
        {
            HitBox.transform.RotateAround(
                point: origin,
                axis: axis,
                angle: step);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        base.Ready(origin);

        yield return base.Attack();
    }
}
