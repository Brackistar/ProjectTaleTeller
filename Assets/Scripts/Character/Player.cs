using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void LevelUpEvent(Player source);
public class Player : Character
{
    [Header("Player Info")]
    [SerializeField]
    private string PlayerName;
    public string Name { get => PlayerName; }
    [TextArea]
    [SerializeField]
    private string History;
    [Header("Inventory")]
    [SerializeField]
    private Weapon.WeaponKind AllowedWeaponKind;
    public int NextLevelXP { get; private set; }
    public bool isLevelUp { get; private set; }
    public event LevelUpEvent OnLevelUp;
    public event EventHandler OnLevelUpDone;

    protected override void Awake()
    {
        if (NextLevelXP <= 0)
            NextLevelXP = GetNextLevelXP();
        base.Awake();
    }

    protected override void Start()
    {
        if (gameObject.layer != 8)
            gameObject.layer = 8;

        base.Start();

        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            CalculateMaxJumpHeight();
        }
    }
    /// <summary>
    /// Returns the value of the History propierty
    /// </summary>
    /// <returns></returns>
    public string GetHistory()
    {
        return History;
    }
    /// <summary>
    /// Increases the XP value.
    /// </summary>
    /// <param name="XP">Value to add to the current XP.</param>
    public void AddXP(int XP)
    {
        Debug.Log(
            message: name + " gain xp: " + XP);

        if (CurrentXP == NextLevelXP)
            return;
        CurrentXP += XP;
        if (CurrentXP >= NextLevelXP)
        {
            int leftXP = CurrentXP - NextLevelXP;
            CurrentXP = 0 + leftXP;
            isLevelUp = true;

            OnLevelUp?.Invoke(this);
        }
    }
    /// <summary>
    /// Increases the player level by 1.
    /// </summary>
    public void LevelUp()
    {
        if (!isLevelUp)
            return;
        CurrentLevel++;
        isLevelUp = false;
        NextLevelXP = GetNextLevelXP();

        SetHealth();

        OnLevelUpDone?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// Change the value of the character stats.
    /// </summary>
    /// <param name="agility">New agility value</param>
    /// <param name="strength">New strength value</param>
    /// <param name="resistance">New resistance value</param>
    /// <param name="vitality">New vitality value</param>
    public void ChangeStatValues(int agility, int strength, int resistance, int vitality)
    {
        if (Agility != agility)
            Agility = agility;
        if (Strength != strength)
            Strength = strength;
        if (Resistance != resistance)
            Resistance = resistance;
        if (Vitality != vitality)
            Vitality = vitality;
    }
    /// <summary>
    /// Calculate the XP needed to level up
    /// </summary>
    /// <returns></returns>
    private int GetNextLevelXP()
    {
        return 100 * (int)System.Math.Pow(10, CurrentLevel);
    }
    public override void SetWeapon(Weapon weapon, bool startWeapon = false)
    {
        if (weapon.Type == this.AllowedWeaponKind)
        {
            base.SetWeapon(weapon, startWeapon);
        }
        else
        {
            base.SetWeapon(
                weapon: GameObject.Find("LevelController")
                    .GetComponent<LevelController>()
                    .GetInitialWeapon(),
                startWeapon: startWeapon);
        }
    }
    /// <summary>
    /// Sets the initial status of a new Character.
    /// </summary>
    /// <param name="Name">Name displayed on the status screen.</param>
    /// <param name="Story">Story displayed on the status screen.</param>
    public void SetIdentity(string Name, string Story)
    {
        PlayerName = Name;
        History = Story;
    }
    /// <summary>
    /// Function calledwhen the player's weapon hits.
    /// </summary>
    /// <param name="collider">Collider of the object in contact with the weapon</param>
    protected override void OnWeaponAttackHit(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Enemy"))
        {
            if (collider.gameObject.TryGetComponent(out Enemy enemy) && IsAttacking())
            {
                if (GetLuckyTry() >= enemy.GetLuckyTry())
                {
                    bool enemyKilled = enemy.ReceiveAttack(
                    value: GetDamage());
                    if (this.Weapon.GetEffectName() != EffectName.Normal && !enemyKilled)
                        enemy.AddEffect(
                            effect: this.Weapon.GetEffectName(),
                            intensity: this.Weapon.GetEffectValue(),
                            duration: this.Weapon.GetEffectDuration());
                }
            }
        }
    }
}
