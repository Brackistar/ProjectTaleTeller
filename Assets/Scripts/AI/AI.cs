using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AI : MonoBehaviour
{
    protected Enemy Self;
    protected bool isWalking;
    protected bool isTargetSpoted;
    [SerializeField]
    [Range(0.1f, 5)]
    protected float DetectionRange = 1;
    [SerializeField]
    [Range(0.1f, 5)]
    protected float Visibility = 2;
    [SerializeField]
    [Range(1, 6)]
    protected float Aggresion = 3;
    [SerializeField]
    [Range(1, 6)]
    protected float Defense = 3;
    protected float maxJumpHeight;
    protected bool changeWalkDirection = false;

    public AIState currentState { get; protected set; }

    protected virtual void Awake()
    {
        if (Self == null)
            Self = GetComponent<Enemy>();

        isWalking = false;
        isTargetSpoted = false;

        currentState = AIState.Idle;
    }
    protected virtual void Start()
    {
        StartCoroutine(StartBehaviour());
        maxJumpHeight = Self.CalculateMaxJumpHeight();
    }

    protected virtual void Update()
    {
        RaycastHit2D ray1,
                ray2,
                ray3;

        PolygonCollider2D bodyCollider = GetComponent<PolygonCollider2D>();
        CircleCollider2D feetCollider = transform.Find("Feet")
                .GetComponent<CircleCollider2D>();
        int layerMask;
        float side,
            back,
            top = transform.TransformPoint(
                position: bodyCollider.points
                    .OrderByDescending(_ => _.y)
                    .FirstOrDefault()).y,
            bottom = transform.TransformPoint(
                position: bodyCollider.points
                    .OrderBy(_ => _.y)
                    .FirstOrDefault()).y - feetCollider.radius + 0.1f;

        Vector2 direction;

        if (Self.MovingLeft)
        {
            side = transform.TransformPoint(
                position: bodyCollider.points
                    .OrderBy(_ => _.x)
                    .FirstOrDefault()).x;
            back = transform.TransformPoint(
                position: bodyCollider.points
                    .OrderByDescending(_ => _.x)
                    .FirstOrDefault()).x;
            direction = Vector2.left;
        }
        else
        {
            side = transform.TransformPoint(
                position: bodyCollider.points
                    .OrderByDescending(_ => _.x)
                    .FirstOrDefault()).x;
            back = transform.TransformPoint(
                position: bodyCollider.points
                    .OrderBy(_ => _.x)
                    .FirstOrDefault()).x;
            direction = Vector2.right;
        }

        // Detect if an enemy is in detection range
        layerMask = 1 << 8;
        ray1 = Physics2D.Raycast(
            origin: new Vector2(
                x: back,
                y: bottom),
            direction: -direction,
            distance: DetectionRange,
            layerMask: layerMask);
        ray2 = Physics2D.Raycast(
            origin: new Vector2(
                x: back,
                y: (top - bottom) * 0.5f),
            direction: -direction,
            distance: DetectionRange,
            layerMask: layerMask);
        ray3 = Physics2D.Raycast(
            origin: new Vector2(
                x: back,
                y: top),
            direction: -direction,
            distance: DetectionRange,
            layerMask: layerMask);

        if ((ray1 || ray2 || ray3) && !isTargetSpoted)
        {
            currentState = AIState.EnemySpoted;
        }
        else if ((!ray1 && !ray2 && !ray3) && isTargetSpoted)
        {
            currentState = AIState.EnemyOutOfSight;
        }

        // Debug section
        Debug.DrawLine(
            start: new Vector2(back, bottom),
            end: new Vector2(back, bottom) + (-direction * DetectionRange),
            color: Color.yellow);
        Debug.DrawLine(
            start: new Vector2(back, (top - bottom) * 0.5f),
            end: new Vector2(back, (top - bottom) * 0.5f) + (-direction * DetectionRange),
            color: Color.yellow);
        Debug.DrawLine(
            start: new Vector2(back, top),
            end: new Vector2(back, top) + (-direction * DetectionRange),
            color: Color.yellow);
        // Debug end

        // Detect if an enemy is visible
        ray1 = Physics2D.Raycast(
            origin: new Vector2(
                x: side,
                y: top),
            direction: LevelController.AngleToVector2(360 - (Self.FieldOfVision * 0.5f)) * -direction,
            distance: Visibility,
            layerMask: layerMask);
        ray2 = Physics2D.Raycast(
            origin: new Vector2(
                x: side,
                y: top),
            direction: direction,
            distance: Visibility,
            layerMask: layerMask);
        ray3 = Physics2D.Raycast(
            origin: new Vector2(
                x: side,
                y: top),
            direction: LevelController.AngleToVector2(Self.FieldOfVision * 0.5f) * direction,
            distance: Visibility,
            layerMask: layerMask);

        if ((ray1 || ray2 || ray3) && !isTargetSpoted)
        {
            currentState = AIState.EnemySpoted;
        }
        else if ((!ray1 && !ray2 && !ray3) && isTargetSpoted)
        {
            currentState = AIState.EnemyOutOfSight;
        }


        // Debug section
        Debug.DrawLine(
            start: new Vector2(side, top),
            end: new Vector2(side, top) + (LevelController.AngleToVector2(360 - (Self.FieldOfVision * 0.5f)) * direction * DetectionRange),
            color: Color.yellow);
        Debug.DrawLine(
            start: new Vector2(side, top),
            end: new Vector2(side, top) + (direction * DetectionRange),
            color: Color.yellow);
        Debug.DrawLine(
            start: new Vector2(side, top),
            end: new Vector2(side, top) + (LevelController.AngleToVector2(Self.FieldOfVision * 0.5f) * direction * DetectionRange),
            color: Color.yellow);
        // Debug end

        // Detect if before is a jumpable object and jumps over it
        if (isWalking)
        {
            maxJumpHeight = Self.CalculateMaxJumpHeight();
            layerMask = 1 << 10;
            int distance = 2;

            ray1 = Physics2D.Raycast(
                origin: new Vector2(
                    x: side,
                    y: bottom),
                direction: direction,
                distance: distance,
                layerMask: layerMask);
            ray2 = Physics2D.Raycast(
                origin: new Vector2(
                    x: side,
                    y: bottom + 0.4f),
                direction: direction,
                distance: distance,
                layerMask: layerMask);
            ray3 = Physics2D.Raycast(
                origin: new Vector2(
                    x: side,
                    y: maxJumpHeight - 0.1f),
                direction: direction,
                distance: distance,
                layerMask: layerMask);

            // Debug section
            Debug.DrawLine(
                start: new Vector2(side, bottom),
                end: new Vector2(side, bottom) + (direction * distance),
                color: Color.green);
            Debug.DrawLine(
                start: new Vector2(side, bottom + 0.4f),
                end: new Vector2(side, bottom + 0.4f) + (direction * distance),
                color: Color.green);
            Debug.DrawLine(
                start: new Vector2(side, maxJumpHeight - 0.1f),
                end: new Vector2(side, maxJumpHeight - 0.1f) + (direction * distance),
                color: Color.green);
            // Debug end

            if (ray1 && ray2)
            {
                if (ray2.distance <= ray1.distance && !ray3)
                {
                    Self.Jump();
                }
                else
                {
                    changeWalkDirection = true;
                }
            }
        }
    }

    private IEnumerator StartBehaviour()
    {
        while (enabled)
        {
            Debug.Log(
                message: name + " current status \'" + currentState.ToString() + "\'");
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

        Self.AlertStatus(
            state: currentState);

        float targetXPosition = Self.Target.transform.position.x,
            selfXPosition = Self.transform.position.x;

        if (Self.MovingLeft && targetXPosition > selfXPosition)
        {
            Self.FlipX(false);
        }
        else if (!Self.MovingLeft && targetXPosition < selfXPosition)
        {
            Self.FlipX(true);
        }
    }
    protected virtual void EnemyOutOfSight()
    {
        isTargetSpoted = false;
        currentState = AIState.Idle;
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
