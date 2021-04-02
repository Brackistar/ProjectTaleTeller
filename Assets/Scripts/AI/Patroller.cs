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

        CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = DetectionRange;

        speed = (int)patrolSpeed;

        base.Start();
    }

    protected override void IdleAction()
    {

        if (Mathf.Abs(transform.position.x - stopPosition.x)<= 0.25f)
            speed = -(int)patrolSpeed;

        if (Mathf.Abs(transform.position.x - startPosition.x) <= 0.25f)
            speed = (int)patrolSpeed;

        Enemy.Move(new Vector2(
                x: speed,
                y: 0));

    }

    protected override void EnemySpoted()
    {
        if ((transform.position - Enemy.Target.transform.position).normalized.x < 0)
            Enemy.FlipX(true);
        currentState = AIState.Combat;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != gameObject)
        {
            if (collision.gameObject.GetComponent<Character>() == Enemy.Target)
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
