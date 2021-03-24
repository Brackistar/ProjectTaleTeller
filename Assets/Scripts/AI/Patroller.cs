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

    protected override void Start()
    {
        Vector2 currentPosition = transform.position;

        startPosition = currentPosition;

        stopPosition = new Vector2(
            x: currentPosition.x + Distance,
            y: currentPosition.y);

        CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = DetectionRange;

        base.Start();
    }

    protected override IEnumerator IdleAction()
    {
        int speed = 0;
        while (enabled && currentState == AIState.Idle)
        {
            if (!isWalking)
                isWalking = true;

            if (transform.position.x >= stopPosition.x)
            {
                speed -= (int)patrolSpeed;
            }
            else if (transform.position.x <= startPosition.x)
            {
                speed += (int)patrolSpeed;
            }
            Enemy.Move(new Vector2(
                    x: speed,
                    y: 0));
            yield return null;
        }
        isWalking = false;
    }

    protected override IEnumerator EnemySpoted()
    {
        if ((transform.position - Enemy.Target.transform.position).normalized.x < 0)
            Enemy.FlipX(true);
        currentState = AIState.Combat;
        return null;
    }

    protected override IEnumerator Await()
    {
        yield return new WaitForSeconds(AttentionSpan);
        currentState = AIState.Idle;
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
