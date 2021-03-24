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
        if (angle <= 0)
            throw new ArgumentOutOfRangeException(
                paramName: "angle",
                message: "attack angle can't be 0.");

        base.Ready(origin);

        float elapsedTime = 0.0f;

        Vector3 axis = isLeft ? Vector3.back : Vector3.forward;

        float distanceToGround = Physics2D.Raycast(
            origin: origin,
            direction: Vector2.down).distance;

        float weaponGroundLine = Mathf.Sqrt(
            Mathf.Pow(magnitude, 2) - Mathf.Pow(distanceToGround, 2));

        Vector2 groundPoint = new Vector2(
            x: origin.x + weaponGroundLine,
            y: origin.y - distanceToGround);

        if (isLeft)
            groundPoint.x *= -1;

        float angleToGround = Vector2.Angle(
            from: origin,
            to: groundPoint);
        float angleLeft = angle - angleToGround;

        HitBox.transform.RotateAround(
            point: origin,
            axis: -axis,
            angle: angleLeft);

        while (elapsedTime <= time)
        {
            HitBox.transform.RotateAround(
                point: origin,
                axis: axis,
                angle: (angle / time) * elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield return base.Attack();

    }
}
