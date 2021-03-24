﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    [Header("Player Info")]
    [SerializeField]
    private string PlayerName;
    public string Name { get => PlayerName; }
    [TextArea]
    [SerializeField]
    private string History;
    public int NextLevelXP { get; private set; }
    public bool isLevelUp { get; private set; }

    protected override void Start()
    {
        if (gameObject.layer != 8)
            gameObject.layer = 8;

        base.Start();

        gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
        gameObject.GetComponent<CapsuleCollider2D>().enabled = true;
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
        if (CurrentXP == NextLevelXP)
            return;
        CurrentXP += XP;
        if (CurrentXP >= NextLevelXP)
        {
            int leftXP = CurrentXP - NextLevelXP;
            CurrentXP = 0 + leftXP;
            isLevelUp = true;
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
    }
    /// <summary>
    /// Sets the initial status of a new Character.
    /// </summary>
    /// <param name="Name">Name displayed on the status screen.</param>
    /// <param name="Story">Story displayed on the status screen.</param>
    public void SetIdentiry(string Name, string Story)
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
            Enemy enemy;
            if (collider.gameObject.TryGetComponent<Enemy>(out enemy) && IsAttacking())
            {
                bool enemyKilled = enemy.ReceiveAttack(
                    value: GetDamage());
                if (Weapon.GetEffectName() != EffectName.Normal && !enemyKilled)
                    enemy.AddEffect(
                        effect: Weapon.GetEffectName(),
                        intensity: Weapon.GetEffectValue(),
                        duration: Weapon.GetEffectDuration());
            }
        }
    }
}