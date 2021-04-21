using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void AttackHit(Collider2D collider);
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Weapon : Item
{
    [SerializeField]
    private WeaponKind Kind;
    public WeaponKind Type { get => Kind; }
    /// <summary>
    /// Value applied by the Item's effect.
    /// </summary>
    [SerializeField]
    [Range(0, 10)]
    private int EffectValue;
    /// <summary>
    /// Name of the effect applied
    /// </summary>
    [SerializeField]
    private EffectName Effect;
    /// <summary>
    /// Duration in seconds of the applied effect.
    /// </summary>
    [SerializeField]
    [Range(0, 60)]
    private int EffectDuration;
    /// <summary>
    /// Collider representing the physical boundaries of the weapon
    /// </summary>
    protected Collider2D HitBox;
    [SerializeField]
    [Range(0, 5)]
    protected float Range;
    public float ARange { get => Range; }
    [SerializeField]
    [Range(0, 180)]
    protected float AttackAngle;
    /// <summary>
    /// 
    /// </summary>
    private WeaponController Controller;
    /// <summary>
    /// Base damage made by the weapon at each hit.
    /// </summary>
    public float Damage => base.GetValue();
    /// <summary>
    /// Event raised when the hitbox of the weapon collides with a rigidbody.
    /// </summary>
    public event AttackHit OnAttackHit;

    private void Awake()
    {
        //if (this.HitBox == null)
        //    this.HitBox = GetComponent<Collider2D>();
        //HitBox.enabled = false;
        //HitBox.isTrigger = true;

        //if (Controller == null)
        //    Controller = GetComponent<WeaponController>();

        //GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
    }
    private void Start()
    {
        if (this.HitBox == null)
            this.HitBox = GetComponent<Collider2D>();
        this.HitBox.enabled = false;
        this.HitBox.isTrigger = true;

        if (Controller == null)
            Controller = GetComponent<WeaponController>();

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

        Controller.SetHitbox(
            collider: HitBox);
        Controller.OnAttackEnd += attackEnd;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetEffectValue()
    {
        return EffectValue;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public EffectName GetEffectName()
    {
        return Effect;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetEffectDuration()
    {
        return EffectDuration;
    }
    /// <summary>
    /// Starts the attack and enables the weapon hitbox
    /// </summary>
    /// <param name="time">Duration of the attack</param>
    /// <param name="origin">Start point for an atack</param>
    /// <param name="isLeft">Is the character looking to the left of the screen</param>
    public virtual void Attack(float time, Vector2 origin, bool isLeft)
    {
        HitBox.enabled = true;
        StartCoroutine(
            Controller.Attack(
                time: time,
                origin: origin,
                magnitude: Range,
                isLeft: isLeft,
                angle: AttackAngle)
            );
    }
    /// <summary>
    /// Starts the attack and enables the weapon hitbox
    /// </summary>
    /// <param name="time"></param>
    /// <param name="origin"></param>
    /// <param name="isLeft"></param>
    /// <param name="angle"></param>
    public virtual void Attack(float time, Vector2 origin, bool isLeft, float angle)
    {
        HitBox.enabled = true;
        StartCoroutine(
            Controller.Attack(
                time: time,
                origin: origin,
                magnitude: Range,
                isLeft: isLeft,
                angle: angle)
            );
    }
    /// <summary>
    /// Disables the weapon hitbox
    /// </summary>
    protected void attackEnd()
    {
        HitBox.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject != gameObject.transform.parent.gameObject)
        {
            attackEnd();
            OnAttackHit?.Invoke(collider);
        }

    }
    /// <summary>
    /// Defines the weapon
    /// </summary>
    public enum WeaponKind
    {
        Fist,
        Sword,
        Axe,
        Spear,
        Slime
    }
}
