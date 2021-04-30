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
    /// <summary>
    /// Most at left point of patroll.
    /// </summary>
    protected Vector2 startPosition;
    /// <summary>
    /// Most at right point of patroll.
    /// </summary>
    protected Vector2 stopPosition;
    protected int speed;

    protected override void Start()
    {
        //Vector2 currentPosition = transform.position;

        //startPosition = new Vector2(
        //    x: currentPosition.x - Distance * 0.5f,
        //    y: currentPosition.y); ;

        //stopPosition = new Vector2(
        //    x: currentPosition.x + Distance * 0.5f,
        //    y: currentPosition.y);

        SetPatrolLimits();

        //CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
        //circleCollider.isTrigger = true;
        //circleCollider.radius = DetectionRange;

        speed = (int)patrolSpeed;

        base.Start();
    }

    protected void SetPatrolLimits()
    {
        Vector2 currentPosition = transform.position;

        SetPatrolLimits(
            start: new Vector2(
                x: currentPosition.x - Distance * 0.5f,
                y: currentPosition.y),
            stop: new Vector2(
                x: currentPosition.x + Distance * 0.5f,
                y: currentPosition.y)
            );
    }

    public void SetPatrolLimits(Vector2 start, Vector2 stop)
    {
        startPosition = start;
        stopPosition = stop;
    }

    protected override void IdleAction()
    {
        base.IdleAction();
        isWalking = true;

        if (changeWalkDirection)
        {
            Debug.Log(
                message: name + " patrol direction changed by force.");

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
            // Detects if there is a fall in front
            CircleCollider2D feetCollider = transform.Find("Feet")
            .GetComponent<CircleCollider2D>();

            Vector2 FeetSidePoint,
                FarSidePoint;

            RaycastHit2D ray1,
                ray2;

            int layerMask = 1 << 10;



            if (speed < 0)
            {
                FeetSidePoint = new Vector2(
                    x: feetCollider.transform.position.x - (feetCollider.radius * 2),
                    y: feetCollider.transform.position.y);

                FarSidePoint = FeetSidePoint + (0.25f * Vector2.left);
            }
            else
            {
                FeetSidePoint = new Vector2(
                    x: feetCollider.transform.position.x,
                    y: feetCollider.transform.position.y);

                FarSidePoint = FeetSidePoint + (0.25f * Vector2.right);
            }

            ray1 = Physics2D.Raycast(
                origin: FeetSidePoint,
                direction: Vector2.down,
                distance: feetCollider.radius + 0.1f,
                layerMask: layerMask);

            ray2 = Physics2D.Raycast(
                origin: FarSidePoint,
                direction: Vector2.down,
                distance: feetCollider.radius + 0.3f,
                layerMask: layerMask);

            // Debug Section
            if (DeveloperMenuController.viewAIRaytrace)
            {
                Debug.DrawRay(
                start: FeetSidePoint,
                dir: Vector2.down * (feetCollider.radius + 0.1f),
                color: Color.yellow);
                Debug.DrawRay(
                    start: FarSidePoint,
                    dir: Vector2.down * (feetCollider.radius + 0.3f),
                    color: Color.yellow);
            }
            // End debug

            if (ray1 && !ray2)
            {
                Debug.Log(
                    message: name + " fall detected, changing patrol direction.");

                changeWalkDirection = true;
                //if (patrolSpeed < 0)
                //{
                //    startPosition = transform.position;
                //    Debug.Log(
                //        message: name + " new start position set: \'" + startPosition.ToString() + "\'");
                //}
                //else
                //{
                //    stopPosition = transform.position;
                //    Debug.Log(
                //        message: name + " new stop position set: \'" + stopPosition.ToString() + "\'");
                //}
            }

            // Change direction if character is offlimits
            if ((transform.position.x > stopPosition.x && speed > 0) || (transform.position.x < startPosition.x && speed < 0))
                changeWalkDirection = true;

            // Change speed if patrol limits are near
            if (Mathf.Abs(transform.position.x - stopPosition.x) <= 0.05f)
            {
                speed = -(int)patrolSpeed;
            }

            if (Mathf.Abs(transform.position.x - startPosition.x) <= 0.05f)
            {
                speed = (int)patrolSpeed;
            }

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

    protected override void EnemyOutOfSight()
    {
        base.EnemyOutOfSight();

        currentState = AIState.EnemyOutOfSight;
        StartCoroutine(WaitBeforeIdle());
    }
    /// <summary>
    /// Awaits for the duration in seconds of Attetion span before changing the current state to Idle.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitBeforeIdle()
    {
        Debug.Log(
            message: name + " waiting for: " + AttentionSpan.ToString() + "s before go to Idle.");

        yield return new WaitForSeconds(AttentionSpan);

        currentState = AIState.Idle;
    }

    protected override void CombatAction()
    {
        base.CombatAction();

        if (GetAggresionChance() > 5)
        {
            Self.Attack();
        }
        else if (GetDefenseChance() > 5 && Vector2.Distance(Self.transform.position, Self.Target.transform.position) <= (Self.Target.GetAttackRange() + 0.5f))
        {
            Self.Defend();
        }
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
