using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
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
    [Range(0.5f, 10)]
    [SerializeField]
    private float BaseJump = 1f;
    [Range(100, 500)]
    [SerializeField]
    private int JumpMultiplier = 300;
    [Range(100, 500)]
    [SerializeField]
    private int BaseHealth = 100;
    [Range(0, 500)]
    private int Health;
    [Range(100, 500)]
    private int MaxHealth;
    // Stat variables
    [Header("Stats")]
    [SerializeField]
    [Range(1, 10)]
    private int Agility = 1;
    [SerializeField]
    [Range(1, 10)]
    private int Strength = 1;
    [SerializeField]
    [Range(1, 10)]
    private int Resistance = 1;
    [SerializeField]
    [Range(1, 10)]
    private int Vitality = 1;
    [SerializeField]
    [Range(1, 10)]
    private int Luck = 1;
    [Space]
    // Audio related variables
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
    // Current state variables
    private bool isJumping = false,
        isAttacking = false,
        isDefending = false,
        isHit = false,
        isAlive = true,
        isDead = false;
    [Space]
    // Animation related variables
    [Header("Animation")]
    [SerializeField]
    private bool LookingLeft = false;
    public bool MovingLeft { get => LookingLeft; }
    public bool IsDead { get => isDead; }
    public bool HasHealth { get => isAlive; }
    private Animator animator;
    [SerializeField]
    private Sprite DeadSprite;
    [SerializeField]
    private List<HitBoxOverride> HitBoxOverrides;
    private string currentHitbox;
    private bool lastHitBoxIsLeft;

    [Space]
    [Header("Initial Equipment")]
    //Equipment
    [SerializeField]
    private Weapon weapon;
    public Weapon Weapon { get => weapon; }
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
    [Space]
    [Header("Initial level")]
    //Experience related variables
    [Range(1, 10)]
    [SerializeField]
    protected int CurrentLevel = 1;
    public int Level { get => CurrentLevel; }
    [SerializeField]
    [Min(0)]
    protected int CurrentXP,
        BaseXP;
    public int XP { get => CurrentXP; }
    private void Awake()
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

        //if (normalCollider == null || flipedCollider == null)
        //{
        //    Collider2D[] colliders = GetComponents<PolygonCollider2D>();

        //    normalCollider = GetComponents<PolygonCollider2D>().FirstOrDefault(
        //        _ => _.enabled);

        //    flipedCollider = GetComponents<PolygonCollider2D>().FirstOrDefault(
        //        _ => !_.enabled);
        //}

        if (HitBoxOverrides == null)
            HitBoxOverrides = new List<HitBoxOverride>();
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        isAlive = true;
        isDead = false;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.sortingLayerName = "Walkers";

        if (!HitBoxOverrides.Any(_ => _.StateName.Equals("Idle", StringComparison.InvariantCultureIgnoreCase)))
            HitBoxOverrides.Add(new HitBoxOverride(
            name: "Idle",
            points: GetComponent<PolygonCollider2D>().points));

        if (Health == 0 || MaxHealth == 0)
            LevelHealth();

        float scaleArea = (float)System.Math.Round(
            value: 1 / (gameObject.transform.localScale.x * gameObject.transform.localScale.y),
            digits: 1);

        animator.SetFloat("SizeMultiplier", scaleArea);

        SetWeapon(weapon, true);

        FlipX(LookingLeft);

        //ChangeCollider("Idle", LookingLeft);

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

    }

    /// <summary>
    /// Translates the character over the x axis based on the direction vector and a speed
    /// </summary>
    /// <param name="Direction">Direction vector of the movement</param>
    public virtual void Move(Vector2 Direction)
    {
        if (isAttacking || isDefending || isHit || !isAlive)
            return;

        // Cancels the y axis cmponent before applying lateral movement
        Vector2 direction = new Vector2
        {
            x = Direction.x,
            y = 0
        };

        // Changes the character oritation based on its direction.
        if (direction.x < 0)
        {
            FlipX(true);
        }
        else if (direction.x > 0)
        {
            FlipX(false);
        }

        float AbsMovement = Mathf.Abs(direction.x);

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

        transform.Translate(direction * CalculateSpeed());
    }
    public void FlipX(bool flip)
    {
        GetComponent<SpriteRenderer>().flipX = LookingLeft = flip;

        //normalCollider.enabled = !flip;
        //flipedCollider.enabled = flip;
        if (isJumping)
        {
            if (currentHitbox != "Jump")
            {
                currentHitbox = "Jump";
                ChangeCollider("Jump", flip);
            }
        }
        else
        {
            if ((currentHitbox != "Idle" && lastHitBoxIsLeft != flip) || lastHitBoxIsLeft != flip)
            {
                currentHitbox = "Idle";
                lastHitBoxIsLeft = flip;
                ChangeCollider("Idle", flip);
            }
        }

        animator.SetBool("LookingLeft", flip);
    }
    private void ChangeCollider(string HitboxOverrideName, bool flip = false, float time = 0.1f)
    {
        if (HitBoxOverrides.Any(_ => _.StateName.Equals(HitboxOverrideName, StringComparison.InvariantCultureIgnoreCase)))
        {
            HitBoxOverride _ = HitBoxOverrides.FirstOrDefault(_override => _override.StateName.Equals(HitboxOverrideName, StringComparison.InvariantCultureIgnoreCase));

            if (flip)
            {
                StartCoroutine(_.OverrideInverseX(
                    collider: GetComponent<PolygonCollider2D>(),
                    time: time));
            }
            else
            {
                StartCoroutine(_.OverrideInverse(
                    collider: GetComponent<PolygonCollider2D>(),
                    time: time));
            }
        }

    }
    /// <summary>
    /// Starts the attack animation.
    /// </summary>
    public void Attack()
    {
        if (isAttacking || !isAlive || isHit)
            return;

        isAttacking = true;
        bool flip = animator.GetBool("LookingLeft");
        ChangeCollider("Attack", flip);

        StartCoroutine(PlayAnimationSound("Attack"));

        weapon.Attack(
            time: GetAnimationLength("Attack"),
            origin: transform.position,
            isLeft: flip);

        animator.SetTrigger("Attack");
    }

    /// <summary>
    /// Starts the defend animation.
    /// </summary>
    public void Defend()
    {
        if (isDefending || isJumping || !isAlive || isHit)
            return;

        isDefending = true;

        bool flip = animator.GetBool("LookingLeft");
        ChangeCollider("Defend", flip);

        StartCoroutine(PlayAnimationSound("Defend"));

        animator.SetTrigger("Defend");
    }
    /// <summary>
    /// Starts the jump animation.
    /// </summary>
    public void Jump()
    {
        if (isJumping || isAttacking || isDefending || !isAlive || isHit)
            return;

        isJumping = true;

        bool flip = animator.GetBool("LookingLeft");
        ChangeCollider("Jump", flip);

        StartCoroutine(PlayAnimationSound("Jump"));

        animator.SetBool("Grounded", false);
        animator.SetTrigger("Jump");

        GetComponent<Rigidbody2D>().AddForce(Vector2.up * CalculateJumpHeight());
    }
    /// <summary>
    /// Starts the hit animation.
    /// </summary>
    private void Hit()
    {
        if (isHit || isDead)
            return;

        isHit = true;

        StartCoroutine(PlayAnimationSound("Hit"));

        if (!isAlive)
            animator.SetBool("HasHealth", false);

        animator.SetTrigger("Hit");
    }
    protected virtual void Death()
    {
        if (!isAlive)
            return;

        isAlive = false;

        StartCoroutine(PlayAnimationSound("Death"));

        animator.SetTrigger("Death");
    }
    /// <summary>
    /// Reproduces the sound of falling to the ground.
    /// </summary>
    private void Ground()
    {
        animator.SetBool("Grounded", true);

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
    /// Do the final actions ater an animation plays.
    /// </summary>
    /// <param name="stateName">Name of the state in wich the animation plays.</param>
    private void FinishingAnimation(string stateName)
    {
        switch (stateName)
        {
            case "Attack":
                isAttacking = false;
                //attackLeftCollider.enabled = attackRightCollider.enabled = false;
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
    public Sprite GetImage()
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
        float WeaponDamage = Weapon.Damage,
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

        Resistance = Resistance * 0.01f;

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
    /// Changes the current weapon.
    /// </summary>
    /// <param name="weapon">weapon to equip.</param>
    public void SetWeapon(Weapon weapon, bool startWeapon = false)
    {
        if (!startWeapon)
            RemoveWeapon();

        Vector2 weaponInitialPosition = transform.position;

        if (!LookingLeft)
        {
            weaponInitialPosition.x += 0.25f;
        }

        this.weapon = Instantiate(
            original: weapon.gameObject,
            position: weaponInitialPosition,
            rotation: Quaternion.identity,
            parent: transform).GetComponent<Weapon>();

        this.weapon.OnAttackHit += OnWeaponAttackHit;
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
        this.weapon.OnAttackHit -= OnWeaponAttackHit;
        GameObject.Destroy(this.weapon.gameObject);
    }
    /// <summary>
    /// Calculates the final moving speed multiplier using BaseSpeed, Agility and deltaTime
    /// </summary>
    /// <returns>Final moving speed. 2 decimals</returns>
    private float CalculateSpeed()
    {
        float result = (float)System.Math.Round(
            value: (BaseSpeed + (Agility * 0.01f)) * Time.deltaTime,
            digits: 2);
        return result;
    }
    /// <summary>
    /// Calculates the final impulse for jumping
    /// </summary>
    /// <returns>Final jumping impulse. 2 decimals</returns>
    private float CalculateJumpHeight()
    {
        float result = (float)System.Math.Round(
            value: (BaseJump + (Agility * 0.1f)) * JumpMultiplier,
            digits: 2);
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
            message: gameObject.name + ": damage received: " + value.ToString());

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
            case "Ground":
                Ground();
                break;
            case "MovingGround":
                Ground();
                transform.SetParent(
                    p: collision.collider.transform);
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
            case "MovingGround":
                transform.SetParent(
                    p: null);
                break;
        }

    }

    protected abstract void OnWeaponAttackHit(Collider2D collider);
}
[System.Serializable]
public class HitBoxOverride
{
    public string StateName;
    public Vector2[] Points;

    public HitBoxOverride(string name, IEnumerable<Vector2> points)
    {
        StateName = name;
        Points = points.OrderBy(_ => _.x)
            .ToArray();
    }
}
public static class HitBoxOverrideHelper
{
    //[Tooltip("State in which the override is used")]
    //[SerializeField]
    //private string StateName;
    //public string Name { get => StateName; }
    //[Space]
    //[Tooltip("Position of every point, must correspond to the original polygon i point count")]
    //[SerializeField]
    //private Vector2[] Points;

    /// <summary>
    /// Point override for a PolygonCollider2D
    /// </summary>
    /// <param name="name">Name of the state in which the override is used</param>
    /// <param name="points">Coordinates of the points of the polygon</param>
    //public HitBoxOverrideHelper(string name, IEnumerable<Vector2> points)
    //{
    //    StateName = name;
    //    Points = points
    //        .OrderBy(_ => _.x)
    //        .ToArray();
    //}

    //private void Awake()
    //{
    //    Points = Points
    //        .OrderBy(_ => _.x)
    //        .ToArray();
    //}
    /// <summary>
    /// Returns the points that conform the override.
    /// </summary>
    /// <returns></returns>
    //public Vector2[] GetOverridePoints()
    //{
    //    return Points;
    //}
    ///// <summary>
    ///// Returns the points that conform the override inverted on the x axis.
    ///// </summary>
    ///// <returns></returns>
    //public Vector2[] GetInversePointsX()
    //{
    //    var result = Points.ToList();
    //    result.ForEach(point => point.x *= -1);
    //    return result.ToArray();
    //}
    ///// <summary>
    ///// Returns the points that conform the override inverted on the y axis.
    ///// </summary>
    ///// <returns></returns>
    //public Vector2[] GetInversePointsY()
    //{
    //    var result = Points.ToList();
    //    result.ForEach(point => point.y *= -1);
    //    return result.ToArray();
    //}
    ///// <summary>
    ///// Returns the points that conform the override inverted on both the x and y axis.
    ///// </summary>
    ///// <returns></returns>
    //public Vector2[] GetInversePoints()
    //{
    //    var result = Points.ToList();
    //    result.ForEach(_ => { _.x *= -1; _.y *= -1; });
    //    return result.ToArray();
    //}
    /// <summary>
    /// Changes the points of a collider with the points of a HitBoxOverride over time.
    /// </summary>
    /// <param name="collider">PolygonCollider2D to override</param>
    /// <param name="hitBoxOverride">HitBoxOveriide to use</param>
    /// <param name="time">Time for the change to complete</param>
    /// <returns></returns>
    public static IEnumerator Override(this HitBoxOverride hitBoxOverride, PolygonCollider2D collider, float time = 0.1f)
    {
        if (collider.GetTotalPointCount() != hitBoxOverride.Points.Length)
            throw new ArgumentOutOfRangeException(paramName: "collider");

        float elapsedTime = 0,
            step;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            for (int i = 0; i < collider.GetTotalPointCount(); i++)
            {
                Vector2 current = collider.points[i],
                    target = hitBoxOverride.Points[i];

                step = Vector2.Distance(current, target) / (time - elapsedTime) * Time.deltaTime;

                collider.points[i] = Vector2.MoveTowards(
                    current: current,
                    target: target,
                    maxDistanceDelta: step);
            }
            yield return null;
        }
    }
    public static IEnumerator OverrideInverseX(this HitBoxOverride hitBoxOverride, PolygonCollider2D collider, float time = 0.1f)
    {
        List<Vector2> _points = hitBoxOverride.Points
            .ToList();
        _points.ForEach(point => point.x *= -1);

        HitBoxOverride _ = new HitBoxOverride(
            name: hitBoxOverride.StateName,
            points: _points.ToArray());

        return _.Override(collider, time);
    }
    public static IEnumerator OverrideInverseY(this HitBoxOverride hitBoxOverride, PolygonCollider2D collider, float time = 0.1f)
    {
        List<Vector2> _points = hitBoxOverride.Points
            .ToList();
        _points.ForEach(point => point.y *= -1);

        HitBoxOverride _ = new HitBoxOverride(
            name: hitBoxOverride.StateName,
            points: _points.ToArray());

        return _.Override(collider, time);
    }
    public static IEnumerator OverrideInverse(this HitBoxOverride hitBoxOverride, PolygonCollider2D collider, float time = 0.1f)
    {
        List<Vector2> _points = hitBoxOverride.Points
            .ToList();
        _points.ForEach(point => { point.x *= -1; point.y *= -1; });

        HitBoxOverride _ = new HitBoxOverride(
            name: hitBoxOverride.StateName,
            points: _points.ToArray());

        return _.Override(collider, time);
    }
    /// <summary>
    /// Changes the shape of a collider to the shape of the override.
    /// </summary>
    /// <param name="collider">Collider to change</param>
    /// <param name="time">Time to complete the change</param>
    //public void Override(PolygonCollider2D collider, float time = 0.1f)
    //{
    //    StartCoroutine(Override(
    //        collider: collider,
    //        hitBoxOverride: this,
    //        time: time));
    //}
    ///// <summary>
    ///// Changes the shape of a collider to the shape of the override, inversing the x axis of the override.
    ///// </summary>
    ///// <param name="collider">Collider to change</param>
    ///// <param name="time">Time to complete the change</param>
    //public void OverrideInverseX(PolygonCollider2D collider, float time = 0.1f)
    //{
    //    StartCoroutine(Override(
    //        collider: collider,
    //        pointsOverride: this.GetInversePointsX(),
    //        time: time));
    //}
    ///// <summary>
    ///// Changes the shape of a collider to the shape of the override, inversing the y axis of the override.
    ///// </summary>
    ///// <param name="collider">Collider to change</param>
    ///// <param name="time">Time to complete the change</param>
    //public void OverrideInverseY(PolygonCollider2D collider, float time = 0.1f)
    //{
    //    StartCoroutine(Override(
    //        collider: collider,
    //        pointsOverride: this.GetInversePointsY(),
    //        time: time));
    //}
    ///// <summary>
    ///// Changes the shape of a collider to the shape of the override, inversing both the x and y axis of the override.
    ///// </summary>
    ///// <param name="collider">Collider to change</param>
    ///// <param name="time">Time to complete the change</param>
    //public void OverrideInverse(PolygonCollider2D collider, float time = 0.1f)
    //{
    //    StartCoroutine(Override(
    //        collider: collider,
    //        pointsOverride: this.GetInversePoints(),
    //        time: time));
    //}
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
