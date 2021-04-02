using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void AttackEnd();
public abstract class WeaponController : MonoBehaviour
{
    protected Collider2D HitBox;
    public AttackEnd OnAttackEnd;
    public virtual IEnumerator Attack(float time, Vector2 origin, float magnitude, bool isLeft = false, float angle = 0)
    {
        return Attack();
    }
    protected virtual IEnumerator Attack()
    {
        OnAttackEnd?.Invoke();
        return null;
    }
    public void SetHitbox(Collider2D collider)
    {
        HitBox = collider;
    }
    public virtual void Ready(Vector2 position)
    {
        transform.position = position;
        transform.rotation = Quaternion.identity;
    }
    protected Vector2 AngleToVector2(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector2(
            x: Mathf.Cos(radian),
            y: Mathf.Sin(radian));
    }
    protected Vector2 AngleToVector2(float angle, float magnitude)
    {
        return AngleToVector2(angle) * magnitude;
    }
}
