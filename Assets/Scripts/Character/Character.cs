using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
//using UnityEditor.Animations;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PolygonCollider2D))]
public abstract class Character : MonoBehaviour
{
    // Base values and multipliers
    [Header("Base values")]
    [Range(0.5f, 10)]
    [SerializeField]
    private float BaseSpeed = 1f;
    [Range(1f, 10)]
    [SerializeField]
    private float BaseDownSpeed = 3;
    [Range(0.5f, 10)]
    [SerializeField]
    private float BaseJump = 1f;
    [Range(100, 500)]
    [SerializeField]
    private int BaseHealth = 100;
    [Range(0, 500)]
    private int Health;
    [Range(100, 500)]
    private int MaxHealth;
    [SerializeField]
    [Range(10, 100)]
    private int DeadFallDistance = 10;
    [SerializeField]
    [Range(0, 100)]
    private int BaseFallDamage = 5;
    [SerializeField]
    [Range(1, 10)]
    private int BaseFallLenght = 3;

    // Stat variables
    [Header("Stats")]
    [SerializeField]
    [Range(1, 10)]
    protected int Agility = 1;
    [SerializeField]
    [Range(1, 10)]
    protected int Strength = 1;
    [SerializeField]
    [Range(1, 10)]
    protected int Resistance = 1;
    [SerializeField]
    [Range(1, 10)]
    protected int Vitality = 1;
    [SerializeField]
    [Range(1, 10)]
    protected int Luck = 1;

    // Audio related variables
    [Space]
    [Header("Audio")]
    [SerializeField]
    private string AttackClipName = "";
    [SerializeField]
    private string JumpClipName = "";
    [SerializeField]
    private string DefendClipName = "";
    [SerializeField]
    private string JumpAttackClipName = "";
    [SerializeField]
    private string HitClipName = "";
    [SerializeField]
    private string DeathClipName = "";
    [Space]
    [SerializeField]
    private AudioClip AttackSound;
    [SerializeField]
    private AudioClip DefendSound;
    [SerializeField]
    private AudioClip JumpAttackSound;
    [SerializeField]
    private AudioClip HitSound;
    [SerializeField]
    private AudioClip DeadSound;
    private AudioClip WalkSound,
        RunSound,
        JumpSound,
        FallSound;
    private AudioSource audioSource;

    // Animation related variables
    [Space]
    [Header("Animation")]
    [SerializeField]
    private bool LookingLeft = false;
    public bool MovingLeft { get => LookingLeft; }
    public bool IsDead { get => isDead; }
    public bool HasHealth { get => isAlive; }
    private Animator animator;
    [SerializeField]
    private Sprite DeadSprite;
    //[SerializeField]
    //private List<HitBoxOverride> HitBoxOverrides;
    //private string currentHitbox;
    //private bool lastHitBoxIsLeft;

    //Equipment
    [Space]
    [Header("Initial Equipment")]
    [SerializeField]
    protected Weapon weapon;

    protected Weapon Weapon;
    //public Weapon Weapon { get => weapon; }
    [SerializeField]
    private Armor armor;
    public Armor Armor { get => armor; }
    [SerializeField]
    private Shield shield;
    public Shield Shield { get => shield; }
    private List<EffectName> effects;
    public EffectName[] Effects { get => effects.ToArray(); }
    private List<float> EffectValues;
    [Min(1)]
    private List<int> EffectDurations;

    //Experience related variables
    [Space]
    [Header("Initial level")]
    [Range(1, 10)]
    [SerializeField]
    protected int CurrentLevel = 1;
    public int Level { get => CurrentLevel; }
    protected int CurrentXP;
    public int XP { get => CurrentXP; }

    // Current state variables
    private bool isJumping = false,
        isAttacking = false,
        isDefending = false,
        isHit = false,
        isAlive = true,
        isDead = false,
        isOverPlatform = false,
        isFalling = false,
        isOnGround = true;
    public bool attacking { get => isAttacking; }
    public bool defending { get => isDefending; }

    private Transform currentParent;
    private float fallingTime = 0;
    private float lastHeight = 0;
    protected virtual void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (effects == null)
            effects = new List<EffectName>();
        if (EffectValues == null)
            EffectValues = new List<float>();
        if (EffectDurations == null)
            EffectDurations = new List<int>();
        //if (HitBoxOverrides == null)
        //    HitBoxOverrides = new List<HitBoxOverride>();

        if (BaseFallLenght > DeadFallDistance)
            DeadFallDistance = BaseFallLenght;

        RefreshPolygonCollider();
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        isAlive = true;
        isDead = false;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.sortingLayerName = "Walkers";

        //if (!HitBoxOverrides.Any(_ => _.StateName.Equals("Idle", StringComparison.InvariantCultureIgnoreCase)))
        //    HitBoxOverrides.Add(new HitBoxOverride(
        //    name: "Idle",
        //    points: GetComponent<PolygonCollider2D>().points));

        if (Health == 0 || MaxHealth == 0)
            LevelHealth();

        float scaleArea = (float)System.Math.Round(
            value: 1 / (gameObject.transform.localScale.x * gameObject.transform.localScale.y),
            digits: 1);

        animator.SetFloat("SizeMultiplier", scaleArea);

        SetWeapon(weapon, true);

        FlipX(LookingLeft);

        StartCoroutine(ApplyCurrentEffects());
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (Health <= 0 && isAlive)
            Death();

        if (EffectDurations.Any(x => x <= 0))
        {
            for (int i = 0; i < EffectDurations.Count; i++)
            {
                if (EffectDurations[i] == 0)
                {
                    effects.RemoveAt(i);
                    EffectValues.RemoveAt(i);
                    EffectDurations.RemoveAt(i);
                }
            }
        }

        CircleCollider2D feetCollider = transform.Find("Feet")
            .GetComponent<CircleCollider2D>();
        Vector2 feetheight = new Vector2(
            x: feetCollider.transform.position.x,
            y: feetCollider.transform.position.y - feetCollider.radius);

        float distanceToGround = Mathf.Abs((float)Math.Round(
            value: GroundCheck().y - feetheight.y,
            digits: 3));

        Debug.Log(
            message: name + " distance to ground: \'" + distanceToGround + "\'");

        if (!isOnGround && lastHeight > transform.position.y)
            isFalling = true;

        if (distanceToGround <= 0.01f /*&& !animator.GetBool("Grounded")*/ && isFalling)
        {
            Ground();
        }
        //else if (distanceToGround > 0.01f && isOnGround)
        //{
        //    animator.SetBool("Grounded", false);
        //    isOnGround = false;
        //}

        if (isFalling)
            fallingTime += Time.deltaTime;

        //RefreshPolygonCollider();

        lastHeight = transform.position.y;
    }

    private void LateUpdate()
    {
        RefreshPolygonCollider();
    }

    /// <summary>
    /// Translates the character over the x axis based on the direction vector and a speed
    /// </summary>
    /// <param name="Direction">Direction vector of the movement</param>
    public virtual void Move(Vector2 Direction)
    {
        if (isAttacking || isDefending || isHit || !isAlive)
            return;

        //Makes player go down of a platform
        if (isOverPlatform && Direction.y <= -BaseDownSpeed)
            StartCoroutine(LeavePlatform());

        // Cancels the y axis component before applying lateral movement
        Vector2 direction = new Vector2
        {
            x = Direction.x,
            y = 0
        };

        // Changes the character oritation based on its direction.
        if (direction.x < 0 && !LookingLeft)
        {
            FlipX(true);
        }
        else if (direction.x > 0 && LookingLeft)
        {
            FlipX(false);
        }

        float AbsMovement = Mathf.Abs(direction.x);

        Debug.Log(name + ": movement direction: " + AbsMovement);

        if (!isJumping)
        {
            animator.SetFloat("Speed", AbsMovement);
            if (AbsMovement >= 0 && AbsMovement < 0.5)
            {
                if (audioSource.isPlaying)
                    audioSource.Stop();
                return;
            }
            else if (AbsMovement >= 1 && AbsMovement < 5)
            {
                if (audioSource.clip != WalkSound)
                    audioSource.clip = WalkSound;
            }
            else if (AbsMovement >= 7)
            {
                if (audioSource.clip != RunSound)
                    audioSource.clip = RunSound;
            }
            if (!audioSource.loop)
                audioSource.loop = true;
            if (!audioSource.isPlaying)
                audioSource.Play();
        }

        Debug.Log(name + ": movement speed: " + direction * CalculateSpeed());
        transform.Translate(direction * CalculateSpeed());
    }
    /// <summary>
    /// Starts the attack animation.
    /// </summary>
    public void Attack()
    {
        if (isAttacking || !isAlive || isHit)
            return;

        Debug.Log(
            message: name + "attacking with weapon: \'" + Weapon.name + "\'");

        isAttacking = true;

        //ChangeCollider("Attack", LookingLeft);

        StartCoroutine(PlayAnimationSound("Attack"));

        this.Weapon.Attack(
            time: GetAnimationLength("Attack"),
            origin: transform.position,
            isLeft: LookingLeft);

        animator.SetTrigger("Attack");
    }

    /// <summary>
    /// Starts the defend animation.
    /// </summary>
    public void Defend()
    {
        //if (isDefending || isJumping || !isAlive || isHit)
        //    return;
        if (isDefending || !isAlive || isHit)
            return;

        Debug.Log(
            message: name + " defending.");

        isDefending = true;

        //ChangeCollider("Defend", LookingLeft);

        StartCoroutine(PlayAnimationSound("Defend"));

        animator.SetTrigger("Defend");
    }
    /// <summary>
    /// Starts the jump animation.
    /// </summary>
    public void Jump()
    {
        if (isJumping || isAttacking || isDefending || !isAlive || isHit || isFalling || !isOnGround)
            return;

        isJumping = true;
        isOnGround = false;

        //ChangeCollider("Jump", LookingLeft);

        StartCoroutine(PlayAnimationSound("Jump"));

        animator.SetBool("Grounded", false);
        animator.SetTrigger("Jump");

        float height = CalculateJumpImpulse();

        Debug.Log(
            message: name + " jump, height: " + height.ToString());

        GetComponent<Rigidbody2D>().AddForce(
            force: Vector2.up * height,
            mode: ForceMode2D.Impulse);
    }
    /// <summary>
    /// Starts the hit animation.
    /// </summary>
    private void Hit()
    {
        if (isHit || isDead)
            return;

        Debug.Log(
            message: name + " hit.");

        isHit = true;

        StartCoroutine(PlayAnimationSound("Hit"));

        if (!isAlive)
            animator.SetBool("HasHealth", false);

        animator.SetTrigger("Hit");
    }
    /// <summary>
    /// Starts the dead animation.
    /// </summary>
    protected virtual void Death()
    {
        if (!isAlive || isHit)
            return;

        isAlive = false;

        StartCoroutine(PlayAnimationSound("Death"));

        animator.SetTrigger("Death");

        Debug.Log(
            message: name + " is dead");
    }
    /// <summary>
    /// Reproduces the sound of falling to the ground.
    /// </summary>
    private void Ground()
    {
        animator.SetBool("Grounded", true);
        isFalling = false;
        isJumping = false;
        isOnGround = true;

        Debug.Log(
            message: name + "grounded.");

        if (BaseFallDamage > 0)
        {
            float fallDistance = (float)Math.Round(
                value: Mathf.Abs(Physics2D.gravity.y) * Time.deltaTime * Mathf.Pow(fallingTime, 2) / 2,
                digits: 3);
            Debug.Log(
                message: name + " fall distance: \'" + fallDistance.ToString() + "\' min fall for damage : \'" + BaseFallLenght.ToString() + "\'");

            if (fallDistance >= DeadFallDistance)
            {
                Debug.Log(
                    message: name + " dead fall. Dead fall distance: \'" + DeadFallDistance.ToString() + "\'");
                Death();
                return;
            }

            int minFallLenght = BaseFallLenght;
            if (fallDistance > minFallLenght)
            {
                float damage = (fallDistance - minFallLenght) * BaseFallDamage;

                Debug.Log(
                    message: name + " raw fall damage: " + damage.ToString());

                damage -= damage * (GetArmorlessDefense() * 0.5f);

                Debug.Log(
                    message: name + " applied damage: \'" + damage + "\'");

                Damage(damage);
            }
        }

        fallingTime = 0;
        StartCoroutine(PlayAnimationSound("Ground"));
    }
    /// <summary>
    /// Starts playing the sound asociated to an animation, then waits forsaid animation to end before changing the animation state.
    /// </summary>
    /// <param name="stateName">Name of the state in wich the animation plays.</param>
    /// <returns></returns>
    private IEnumerator PlayAnimationSound(string stateName)
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
        if (audioSource.loop)
            audioSource.loop = false;

        // Replaces the audioSource clip for the corresponding for the animation;
        switch (stateName)
        {
            case "Attack":
                if (audioSource.clip != AttackSound)
                    audioSource.clip = AttackSound;
                break;
            case "Jump":
                if (audioSource.clip != JumpSound)
                    audioSource.clip = JumpSound;
                break;
            case "Ground":
                if (audioSource.clip != FallSound)
                    audioSource.clip = FallSound;
                break;
            case "Defend":
                if (audioSource.clip != DefendSound)
                    audioSource.clip = DefendSound;
                break;
            case "Hit":
                if (audioSource.clip != HitSound)
                    audioSource.clip = HitSound;
                break;
            case "Death":
                if (audioSource.clip != DeadSound)
                    audioSource.clip = DeadSound;
                break;
            default:
                yield break;
        }

        audioSource.time = 0;
        audioSource.Play();

        yield return new WaitForSeconds(
            seconds: GetAnimationLength(
                stateName: stateName)
            );

        audioSource.Stop();

        FinishingAnimation(stateName);
    }
    /// <summary>
    /// Do the final actions after an animation plays.
    /// </summary>
    /// <param name="stateName">Name of the state in wich the animation plays.</param>
    private void FinishingAnimation(string stateName)
    {
        switch (stateName)
        {
            case "Attack":
                isAttacking = false;
                break;
            case "Defend":
                isDefending = false;
                break;
            case "Ground":
                isJumping = false;
                break;
            case "Hit":
                isHit = false;
                if (!isAlive)
                    StartCoroutine(PlayAnimationSound("Death"));
                break;
            case "Death":
                isDead = true;
                animator.enabled = false;
                break;
        }
    }
    /// <summary>
    /// Allows a character to pass through a platform.
    /// </summary>
    /// <returns></returns>
    private IEnumerator LeavePlatform()
    {
        isOverPlatform = false;
        transform.Find("Feet").GetComponent<Collider2D>().enabled = false;
        yield return new WaitForSeconds(0.5f);
        transform.Find("Feet").GetComponent<Collider2D>().enabled = true;
    }
    /// <summary>
    /// Mirrors the character over the x axis.
    /// </summary>
    /// <param name="flip">Look to the left.</param>
    public void FlipX(bool flip)
    {
        GetComponent<SpriteRenderer>().flipX = LookingLeft = flip;

        //normalCollider.enabled = !flip;
        //flipedCollider.enabled = flip;
        //if (isJumping)
        //{
        //    if (currentHitbox != "Jump")
        //    {
        //        currentHitbox = "Jump";
        //        ChangeCollider("Jump", flip);
        //    }
        //}
        //else
        //{
        //    if ((currentHitbox != "Idle" && lastHitBoxIsLeft != flip) || lastHitBoxIsLeft != flip)
        //    {
        //        currentHitbox = "Idle";
        //        lastHitBoxIsLeft = flip;
        //        ChangeCollider("Idle", flip);
        //    }
        //}

        //ChangeCollider("Idle", flip);

        animator.SetBool("LookingLeft", flip);
    }
    /// <summary>
    /// Changes the polygon collider paths to match the physics shape of the current sprite.
    /// </summary>
    private void RefreshPolygonCollider()
    {
        Debug.Log(
            message: name + " refresh polygon collider.");

        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;

        for (int i = 0; i < collider.pathCount; i++)
            collider.SetPath(index: i, points: new List<Vector2>());

        collider.pathCount = sprite.GetPhysicsShapeCount();

        List<Vector2> path = new List<Vector2>();

        for (int i = 0; i < collider.pathCount; i++)
        {
            path.Clear();
            sprite.GetPhysicsShape(i, path);

            if (LookingLeft)
            {
                Debug.Log(
                    message: name + " changing collider to left orientation.");

                path = path.Select(_path => { _path.x *= -1; return _path; })
                    .ToList();
            }

            collider.SetPath(i, path);
        }
    }
    /// <summary>
    /// Character current action includes an attack.
    /// </summary>
    /// <returns></returns>
    public bool IsAttacking()
    {
        return isAttacking;
    }
    /// <summary>
    /// Returns the value of the Agility stat
    /// </summary>
    /// <returns>Value with 2 decimals</returns>
    public float GetAgility()
    {
        return Agility;
    }
    /// <summary>
    /// Returns the value of the Strength stat
    /// </summary>
    /// <returns>Value with 2 decimals</returns>
    public float GetStrength()
    {
        return Strength;
    }
    /// <summary>
    /// Returns the value of the Resistance stat
    /// </summary>
    /// <returns>Value with 2 decimals</returns>
    public float GetResistance()
    {
        return Resistance;
    }
    /// <summary>
    /// Returns the value of the Vitality stat
    /// </summary>
    /// <returns>Value with 2 decimals</returns>
    public float GetVitality()
    {
        return Vitality;
    }
    /// <summary>
    /// Returns a random number between the character's luck value and 10.
    /// </summary>
    /// <returns></returns>
    public int GetLuckyTry()
    {
        if (Luck == 10)
            return 10;

        int result = UnityEngine.Random.Range(this.Luck, 10);

        Debug.Log(
            message: name + " lucky try result: \'" + result.ToString() + "\'");

        return result;
    }
    /// <summary>
    /// Returns the max value of health allowed
    /// </summary>
    /// <returns></returns>
    public int GetTotalHealth()
    {
        return MaxHealth;
    }
    /// <summary>
    /// Sets the value of the maxHealth propierty. Value must be between 100 and 500
    /// </summary>
    /// <param name="value">New value of max health</param>
    public void SetTotalHealth(int value)
    {
        if (value < 100 || value > 500)
            return;
        MaxHealth = value;
    }
    /// <summary>
    /// Returns the value of the health left
    /// </summary>
    /// <returns></returns>
    public int GetCurrentHealth()
    {
        return Health;
    }
    /// <summary>
    /// Sets the value of the health propierty. Must be between 0 and MaxHealth
    /// </summary>
    /// <param name="value">New value</param>
    public void SetCurrentHealth(int value)
    {
        if (value < 0 || value > MaxHealth)
            return;

        Health = value;
    }
    /// <summary>
    /// Returns the sprite assigned to the character
    /// </summary>
    /// <returns></returns>
    public virtual Sprite GetImage()
    {
        return GetComponent<SpriteRenderer>().sprite;
    }
    /// <summary>
    /// Changes the AudioClip played when walking on ground
    /// </summary>
    /// <param name="sound">AudioClip to assign</param>
    public void SetWalkSound(AudioClip sound)
    {
        WalkSound = sound;
    }
    /// <summary>
    /// Changes the AudioClip played when running on ground
    /// </summary>
    /// <param name="sound">AudioClip to assign</param>
    public void SetRunSound(AudioClip sound)
    {
        RunSound = sound;
    }
    /// <summary>
    /// Changes the AudioClip played when a jump starts
    /// </summary>
    /// <param name="sound">AudioClip to assign</param>
    public void SetJumpSound(AudioClip sound)
    {
        JumpSound = sound;
    }
    /// <summary>
    /// Changes the AudioClip played when hitting ground
    /// </summary>
    /// <param name="sound">AudioClip to assign</param>
    public void SetFallSound(AudioClip sound)
    {
        FallSound = sound;
    }
    /// <summary>
    /// Returns the value at a certain position of the EffectValues array.
    /// </summary>
    /// <param name="index">Position of the value.</param>
    /// <returns></returns>
    private float GetEffectValue(int index)
    {
        return EffectValues[index];
    }
    /// <summary>
    /// Returns the value associated to an effect.
    /// </summary>
    /// <param name="effectName">EffectName of the effect.</param>
    /// <returns></returns>
    public float GetEffectValue(EffectName effectName)
    {
        int index = effects.IndexOf(effectName);

        return GetEffectValue(index);
    }
    /// <summary>
    /// Returns the value at a position in the EffectDuration array.
    /// </summary>
    /// <param name="index">Position in the array of the duration.</param>
    /// <returns>Duration in seconds.</returns>
    private int GetEffectDuration(int index)
    {
        return EffectDurations[index];
    }
    /// <summary>
    /// Returns the duration associated to an effect.
    /// </summary>
    /// <param name="effectName">Effect name.</param>
    /// <returns>Duration in seconds.</returns>
    public int GetEffectDuration(EffectName effectName)
    {
        int index = effects.IndexOf(effectName);

        return GetEffectDuration(index);
    }
    /// <summary>
    /// Returns the total damage output for a single attack.
    /// </summary>
    /// <returns></returns>
    public float GetDamage()
    {
        float WeaponDamage = this.Weapon.Damage,
            Strength = this.Strength;
        if (effects.Contains(EffectName.Strength))
            Strength += Strength * GetEffectValue(EffectName.Strength);

        return WeaponDamage + (WeaponDamage * (Strength * 0.1f));
    }
    /// <summary>
    /// Apply the damage caused by an effect
    /// </summary>
    /// <param name="effectName">Name of the effect to apply</param>
    private void EffectDamage(EffectName effectName)
    {
        float result = GetEffectValue(effectName),
            Resistance = this.Resistance;
        if (effects.Contains(EffectName.Resistance))
            Resistance += Resistance * GetEffectValue(EffectName.Resistance);

        result -= result * Resistance * 0.01f;

        if (result <= 0)
            return;

        Damage(result);
    }
    /// <summary>
    /// Returns the damage percentaje nullified by all character defenses.
    /// </summary>
    /// <returns></returns>
    private float GetDefense()
    {

        float Resistance = this.Resistance;
        if (effects.Contains(EffectName.Resistance))
            Resistance += Resistance * GetEffectValue(EffectName.Resistance);

        Resistance *= 0.01f;

        float result = Resistance + armor.Protection;

        if (isDefending)
            result += shield.Protection;

        if (result > 1)
        {
            result = 1;
        }
        else if (result < 0)
        {
            result = 0;
        }

        return result; ;
    }
    /// <summary>
    /// Returns the damage percentaje nullified by the character resistance and resistance effect if applied.
    /// </summary>
    /// <returns></returns>
    private float GetArmorlessDefense()
    {

        float Resistance = this.Resistance;
        if (effects.Contains(EffectName.Resistance))
            Resistance += Resistance * GetEffectValue(EffectName.Resistance);

        Resistance *= 0.01f;

        if (Resistance > 1)
        {
            Resistance = 1;
        }
        else if (Resistance < 0)
        {
            Resistance = 0;
        }

        return Resistance;
    }
    /// <summary>
    /// Changes the current weapon.
    /// </summary>
    /// <param name="weapon">weapon to equip.</param>
    public virtual void SetWeapon(Weapon weapon, bool startWeapon = false)
    {
        if (!startWeapon)
            RemoveWeapon();

        Vector2 weaponInitialPosition = transform.position;

        if (!LookingLeft)
        {
            //weaponInitialPosition.x += 0.25f;
            weaponInitialPosition *= Vector2.left;
        }

        this.Weapon = Instantiate(
            original: weapon.gameObject,
            position: weaponInitialPosition,
            rotation: Quaternion.identity,
            parent: transform).GetComponent<Weapon>();

        this.Weapon.OnAttackHit += OnWeaponAttackHit;
    }

    /// <summary>
    /// Changes the current weapon for the default weapon.
    /// </summary>
    public void UnequipWeapon()
    {
        RemoveWeapon();
        SetWeapon(GameObject.Find("LevelController").GetComponent<LevelController>().GetInitialWeapon());
    }
    /// <summary>
    /// Removes any weapon
    /// </summary>
    private void RemoveWeapon()
    {
        this.Weapon.OnAttackHit -= OnWeaponAttackHit;
        GameObject.Destroy(this.Weapon.gameObject);
    }
    /// <summary>
    /// Returns the length of the currently equiped weapon.
    /// </summary>
    /// <returns></returns>
    public float GetAttackRange()
    {
        return this.Weapon.ARange;
    }
    public Weapon.WeaponKind GetWeaponKind()
    {
        return this.Weapon.Type;
    }
    /// <summary>
    /// Calculates the final moving speed multiplier using BaseSpeed and Agility
    /// </summary>
    /// <returns>Final moving speed. 2 decimals</returns>
    private float CalculateSpeed()
    {
        //float result = (float)System.Math.Round(
        //    value: (BaseSpeed + (Agility * 0.1f)) * Time.deltaTime,
        //    digits: 2);
        float result = (float)System.Math.Round(
            value: (BaseSpeed + (Agility * 0.01f)) * Time.deltaTime,
            digits: 2);
        return result;
    }
    /// <summary>
    /// Calculates the final impulse for jumping
    /// </summary>
    /// <returns>Final jumping impulse. 2 decimals</returns>
    private float CalculateJumpImpulse()
    {
        //float result = (float)System.Math.Round(
        //    value: (BaseJump + (Agility * 0.1f)) * JumpMultiplier,
        //    digits: 2);

        float result = (float)System.Math.Round(
            value: BaseJump + (Agility * 0.1f),
            digits: 2);

        return result;
    }
    /// <summary>
    /// Checks is there is ground under the character.
    /// </summary>
    /// <returns>Position of the ground.</returns>
    public Vector2 GroundCheck()
    {
        int layerMask = 1 << 10;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, layerMask);
        if (hit)
        {
            Debug.Log(
                message: name + " ground check, ground position: \'" + hit.point.ToString() + "\'");
            //return hit.transform.position;
            return hit.point;
        }
        else
        {
            string error = name + " ground not found.";
            Debug.LogError(
                message: error);
            throw new Exception(
                message: error);
        }
    }
    /// <summary>
    /// Calculates the height of a jump based on the current agility
    /// </summary>
    /// <returns></returns>
    public float CalculateMaxJumpHeight()
    {
        float result = 0;

        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        float gravity = rigidbody2D.gravityScale * Physics2D.gravity.magnitude;
        float v = CalculateJumpImpulse() / rigidbody2D.mass;

        result = GroundCheck().y + (v * v) / (2 * gravity);

        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        float headHeight = collider.points
                    .OrderByDescending(_ => _.y)
                    .FirstOrDefault().y;
        if (DeveloperMenuController.viewAIRaytrace)
        {
            Debug.DrawLine(
            start: new Vector2(
                x: transform.position.x,
                y: headHeight),
            end: new Vector2(
                x: transform.position.x,
                y: headHeight + result),
            color: Color.red);
        }
        return result;
    }
    /// <summary>
    /// Sets the current health and the max health based on the vitality stat.
    /// </summary>
    private void LevelHealth()
    {
        int value = BaseHealth + (40 * Vitality);
        SetTotalHealth(value);
        SetCurrentHealth(value);
    }
    /// <summary>
    /// Returns the length of an AnimationClip asociated with an animation state
    /// </summary>
    /// <param name="stateName">Name of the animation state</param>
    /// <returns>Length in seconds</returns>
    private float GetAnimationLength(string stateName)
    {
        float time = 0;
        string motionName;
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;

        switch (stateName)
        {
            case "Attack":
                motionName = AttackClipName;
                break;
            case "Jump":
                motionName = JumpClipName;
                break;
            case "JumpAttack":
                motionName = JumpAttackClipName;
                break;
            case "Defend":
                motionName = DefendClipName;
                break;
            case "Hit":
                motionName = HitClipName;
                break;
            case "Death":
                motionName = DeathClipName;
                break;
            default:
                time = 0.1f;
                return time;
        }

        for (int i = 0; i < ac.animationClips.Length; i++)
            if (ac.animationClips[i].name == motionName)
            {
                time = ac.animationClips[i].length;
                break;
            }

        return time;
    }
    /// <summary>
    /// Applies the damage made by an incoming attack.
    /// </summary>
    /// <param name="value">Damage of the attack.</param>
    /// <returns>Character is dead after attack.</returns>
    public virtual bool ReceiveAttack(float value)
    {
        Debug.Log(
            message: name + " attack received for: " + value.ToString() + " damage.");
        float protection = GetDefense();
        float damage = value;
        damage -= damage * protection;

        Damage(damage);
        return !isAlive;
    }
    /// <summary>
    /// Adds an effect to the effect list.
    /// </summary>
    /// <param name="effect">Name of the effect to add.</param>
    /// <param name="intensity">Value aplicated every second the effect remains active.</param>
    /// <param name="duration">Duration in seconds of the effect.</param>
    public void AddEffect(EffectName effect, int intensity, int duration)
    {
        effects.Add(effect);
        EffectValues.Add(intensity);
        EffectDurations.Add(duration);
    }
    /// <summary>
    /// Reduces the health of the character.
    /// </summary>
    /// <param name="value">Value to reduce the health.</param>
    private void Damage(int value)
    {
        if (value == 0 || isHit)
            return;

        Debug.Log(
            message: name + ": damage received: " + value.ToString());

        if (value > Health)
        {
            value -= Health;
        }

        Health -= value;
        if (Health == 0)
            isAlive = false;
        Hit();
    }
    /// <summary>
    /// Reduces the health of the character.
    /// </summary>
    /// <param name="value">Value to reduce the health.</param>
    private void Damage(float value)
    {
        Damage(Mathf.FloorToInt(value));
    }
    /// <summary>
    /// While character is alive applies the changes made by the different active effects every 1 second.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ApplyCurrentEffects()
    {
        while (isAlive)
        {
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < effects.Count; i++)
            {
                if (EffectDurations[i] < 0)
                    break;

                if (effects[i] == EffectName.Fire || effects[i] == EffectName.Ice || effects[i] == EffectName.Poison)
                    EffectDamage(effects[i]);

                EffectDurations[i]--;
            }
            yield return new WaitForSeconds(1);
        }
    }
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {

        switch (collision.gameObject.tag)
        {
            //case "Ground":
            case "Enemy":
            case "Player":
                Ground();
                isOverPlatform = false;
                break;
            case "Moving Platform":
                //Ground();
                isOverPlatform = true;
                currentParent = transform.parent;
                transform.SetParent(
                    p: collision.collider.transform);
                break;
            case "Platform":
                //Ground();
                isOverPlatform = true;
                break;
            case "Border":
                break;
            case "DeadFall":
                Death();
                break;
        }

    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            //case "Ground":
            //    animator.SetBool("Grounded", false);
            //    transform.SetParent(
            //        p: currentParent);
            //    break;
            case "Moving Platform":
                //animator.SetBool("Grounded", false);
                isOverPlatform = false;
                transform.SetParent(
                    p: currentParent);
                break;
                //case "Platform":
                //    animator.SetBool("Grounded", false);
                //    isOverPlatform = false;
                //    transform.SetParent(
                //        p: currentParent);
                //    break;
        }

    }
    protected abstract void OnWeaponAttackHit(Collider2D collider);
}
/// <summary>
/// Collection of posible effect names.
/// </summary>
public enum EffectName
{
    Normal,
    Fire,
    Poison,
    Ice,
    Strength,
    Resistance,
    Agility,
    Vitality
}
