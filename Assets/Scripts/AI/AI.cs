using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AI : MonoBehaviour
{
    protected Enemy Enemy;
    protected bool isWalking;
    protected bool isTargetSpoted;
    [SerializeField]
    [Range(0.1f, 5)]
    protected float DetectionRange;
    
    public AIState currentState { get; protected set; }

    protected virtual void Awake()
    {
        if (Enemy == null)
            Enemy = GetComponent<Enemy>();

        isWalking = false;
        isTargetSpoted = false;

        currentState = AIState.Idle;
    }
    protected virtual void Start()
    {
        StartCoroutine(StartBehaviour());
    }

    protected virtual void Update()
    {
        Vector2 direction,
            topRay,
            middleRay,
            bottomRay;

        float topPosition,
            middlePosition,
            bottomPosition,
            sidePosition;

        RaycastHit2D topHit,
            middleHit,
            bottomHit;



        PolygonCollider2D collider = GetComponents<PolygonCollider2D>()
            .FirstOrDefault(_ => _.enabled);

        topPosition = collider.points.Max(_ => _.y);
        bottomPosition = collider.points.Min(_ => _.y);
        middlePosition = topPosition - ((topPosition - bottomPosition) / 2);

        if (Enemy.MovingLeft)
        {
            sidePosition = collider.points.Min(_ => _.x);
            direction = Vector2.left;
        }
        else
        {
            sidePosition = collider.points.Max(_ => _.x);
            direction = Vector2.right;
        }

        topRay = new Vector2(x: sidePosition, y: topPosition);
        middleRay = new Vector2(x: sidePosition, y: middlePosition);
        bottomRay = new Vector2(x: sidePosition, y: bottomPosition);

        // Actions at 1m sight
        topHit = Physics2D.Raycast(
            origin: topRay,
            direction: direction,
            distance: 1f);
        middleHit = Physics2D.Raycast(
            origin: middleRay,
            direction: direction,
            distance: 1f);
        bottomHit = Physics2D.Raycast(
            origin: bottomRay,
            direction: direction,
            distance: 1f);

        if (topHit || middleHit || bottomHit)
        {
            try
            {
                bool isGround = topHit.transform.CompareTag("Ground") ||
                    middleHit.transform.CompareTag("Ground") ||
                    bottomHit.transform.CompareTag("Ground");
                if (isGround && isWalking)
                {
                    Enemy.Jump();
                }
            }
            catch (NullReferenceException)
            {

            }
        }
    }

    private IEnumerator StartBehaviour()
    {
        while (enabled)
        {
            switch (currentState)
            {
                case AIState.Combat:
                    CombatAction();
                    break;
                case AIState.LowHealth:
                    LowHealth();
                    break;
                case AIState.HighHealth:
                    HighHealth();
                    break;
                case AIState.AllyLowHealth:
                    AllyLowHealth();
                    break;
                case AIState.EnemyLevelHigher:
                    EnemyLevelHigher();
                    break;
                case AIState.EnemySpoted:
                    EnemySpoted();
                    break;
                case AIState.EnemyOutOfSight:
                    EnemyOutOfSight();
                    break;
                default:
                    IdleAction();
                    break;
            }
            yield return null;
        }
    }
    protected virtual void IdleAction()
    {
    }
    protected virtual void CombatAction()
    {
    }
    protected virtual void LowHealth()
    {
    }
    protected virtual void HighHealth()
    {
    }
    protected virtual void AllyLowHealth()
    {
    }
    protected virtual void EnemyLevelHigher()
    {
    }
    protected virtual void EnemySpoted()
    {
        isTargetSpoted = true;
    }
    protected virtual void EnemyOutOfSight()
    {
        isTargetSpoted = false;
    }
}
public enum AIState
{
    Idle,
    Combat,
    LowHealth,
    HighHealth,
    AllyLowHealth,
    EnemyLevelHigher,
    EnemySpoted,
    EnemyOutOfSight
}
