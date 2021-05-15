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
    [Tooltip("Distance from the back of the Character in which another character is detected.")]
    [Range(0.1f, 5)]
    protected float DetectionRange = 1;
    [SerializeField]
    [Tooltip("Distance in front of the character for the visual field.")]
    [Range(0.1f, 5)]
    protected float Visibility = 2;
    [SerializeField]
    [Range(1, 5)]
    protected float Aggresion = 3;
    [SerializeField]
    [Range(1, 5)]
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
        if (DeveloperMenuController.viewAIRaytrace)
        {
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
        }
        // Debug end

        // Detect if an enemy is visible
        ray1 = Physics2D.Raycast(
            origin: new Vector2(
                x: side,
                y: top),
            direction: LevelController.AngleToVector2(360 - (Self.FieldOfVision * 0.5f)) * direction,
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
        //Debug.DrawLine(
        //    start: new Vector2(side, top),
        //    end: new Vector2(side, top) + (LevelController.AngleToVector2(360 - (Self.FieldOfVision * 0.5f)) * direction * DetectionRange),
        //    color: Color.yellow);
        //Debug.DrawLine(
        //    start: new Vector2(side, top),
        //    end: new Vector2(side, top) + (direction * DetectionRange),
        //    color: Color.yellow);
        //Debug.DrawLine(
        //    start: new Vector2(side, top),
        //    end: new Vector2(side, top) + (LevelController.AngleToVector2(Self.FieldOfVision * 0.5f) * direction * DetectionRange),
        //    color: Color.yellow);
        if (DeveloperMenuController.viewAIRaytrace)
        {
            Debug.DrawRay(
            start: new Vector2(side, top),
            dir: (LevelController.AngleToVector2(Self.FieldOfVision * 0.5f) * direction) * Visibility,
            color: Color.yellow);
            Debug.DrawRay(
                start: new Vector2(side, top),
                dir: direction * Visibility,
                color: Color.yellow);
            Debug.DrawRay(
                start: new Vector2(side, top),
                dir: (LevelController.AngleToVector2(360 - (Self.FieldOfVision * 0.5f)) * direction) * Visibility,
                color: Color.yellow);
        }
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
            if (DeveloperMenuController.viewAIRaytrace)
            {
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
            }
            // Debug end

            if (ray1 && ray2)
            {
                //if (ray2.distance <= ray1.distance && !ray3)
                //{
                //    Self.Jump();
                //    Debug.Log(
                //        message: name + " jumplable obstacle found. Jumping.");
                //}
                //else
                //{
                //    changeWalkDirection = true;
                //    Debug.Log(
                //        message:name+" non-jumplable obstacle found.");
                //}
                if (ray2.distance <= ray1.distance)
                {
                    if (!ray3)
                    {
                        Self.Jump();
                        Debug.Log(
                            message: name + " jumplable obstacle found. Jumping.");
                    }
                    else
                    {
                        changeWalkDirection = true;
                        Debug.Log(
                            message: name + " non-jumplable obstacle found.");
                    }
                }
            }
        }
    }
    /// <summary>
    /// Calls the function asociated with every AI state
    /// </summary>
    /// <returns></returns>
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
        Debug.Log(
            message: name + "Idle action started.");
    }
    protected virtual void CombatAction()
    {
        Debug.Log(
            message: name + "Combat action started.");
    }
    protected virtual void LowHealth()
    {
        Debug.Log(
            message: name + "Low health action started.");
    }
    protected virtual void HighHealth()
    {
        Debug.Log(
            message: name + "High health action started.");
    }
    protected virtual void AllyLowHealth()
    {
        Debug.Log(
            message: name + "Ally health low action started.");
    }
    protected virtual void EnemyLevelHigher()
    {
        Debug.Log(
            message: name + "Enemy level higher action started.");
    }
    protected virtual void EnemySpoted()
    {
        Debug.Log(
            message: name + "Enemy spoted action started.");

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
        Debug.Log(
            message: name + "Enemy out of sight action started.");

        isTargetSpoted = false;
        currentState = AIState.Idle;
    }
    /// <summary>
    /// Chance of an aggresive responce between Aggresion and 10.
    /// </summary>
    /// <returns></returns>
    protected float GetAggresionChance()
    {
        return UnityEngine.Random.Range(Aggresion, 10);
    }
    /// <summary>
    /// Chance of a defensive responce between Defense and 10.
    /// </summary>
    /// <returns></returns>
    protected float GetDefenseChance()
    {
        return UnityEngine.Random.Range(Defense, 10);
    }

    /// <summary>
    /// Awaits for some time before changing the current state to the target state.
    /// </summary>
    /// <param name="target">AIState of the AI after the wait.</param>
    /// <returns></returns>
    protected IEnumerator WaitBeforeStateChange(AIState target, float time)
    {
        Debug.Log(
            message: name + " waiting for: " + time.ToString() + "s before go to state: \'" + target.ToString() + "\'");

        yield return new WaitForSeconds(time);

        currentState = target;
    }
}
/// <summary>
/// Possible state of a character's AI
/// </summary>
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
public enum MoveSpeed
{
    walk = 3,
    run = 5
}
