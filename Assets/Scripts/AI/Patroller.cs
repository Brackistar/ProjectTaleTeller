using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patroller : AI
{
    [SerializeField]
    [Range(0.5f, 5)]
    protected float Distance;
    [SerializeField]
    [Range(0.1f, 1)]
    protected float AttentionSpan;
    [SerializeField]
    protected PatrolSpeed patrolSpeed;
    protected Vector2 startPosition,
        stopPosition;
    protected int speed;

    protected override void Start()
    {
        Vector2 currentPosition = transform.position;

        startPosition = new Vector2(
            x: currentPosition.x - Distance * 0.5f,
            y: currentPosition.y); ;

        stopPosition = new Vector2(
            x: currentPosition.x + Distance * 0.5f,
            y: currentPosition.y);

        //CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
        //circleCollider.isTrigger = true;
        //circleCollider.radius = DetectionRange;

        speed = (int)patrolSpeed;

        base.Start();
    }

    protected override void IdleAction()
    {
        base.IdleAction();
        isWalking = true;
        //if (Mathf.Abs(transform.position.x - stopPosition.x) <= 0.25f)
        //    speed = -(int)patrolSpeed;

        //if (Mathf.Abs(transform.position.x - startPosition.x) <= 0.25f)
        //    speed = (int)patrolSpeed;

        if (changeWalkDirection)
        {
            changeWalkDirection = false;
            if (speed == (int)patrolSpeed)
            {
                speed = -(int)patrolSpeed;
            }
            else
            {
                speed = (int)patrolSpeed;
            }
        }
        else
        {
            if (Mathf.Abs(transform.position.x - stopPosition.x) <= 0.25f)
                speed = -(int)patrolSpeed;

            if (Mathf.Abs(transform.position.x - startPosition.x) <= 0.25f)
                speed = (int)patrolSpeed;
        }

        Debug.Log(
            message: name + " patrol with speed: " + speed.ToString());

        Self.Move(new Vector2(
                x: speed,
                y: 0));
    }

    protected override void EnemySpoted()
    {
        base.EnemySpoted();
        isWalking = false;
        Self.Move(Vector2.zero);
        //if ((transform.position - Self.Target.transform.position).normalized.x < 0)
        //    Self.FlipX(true);
        currentState = AIState.Combat;
    }

    protected override void CombatAction()
    {
        base.CombatAction();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != gameObject)
        {
            if (collision.gameObject.GetComponent<Character>() == Self.Target)
            {
                currentState = AIState.EnemySpoted;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnTriggerEnter2D(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }

    public enum PatrolSpeed
    {
        walk = 1,
        run = 8
    }
}
