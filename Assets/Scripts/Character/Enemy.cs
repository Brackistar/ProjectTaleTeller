using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void EnemyDead(Enemy source);
public class Enemy : Character
{
    // Health bar   
    [SerializeField]
    private GameObject HealthBarPrefab;
    private EnemyHealthBar healthBar;
    // A.I. related
    [SerializeField]
    [Range(2, 5)]
    public float FieldOfVision;
    public Character Target;
    // Dead related
    public EnemyDead OnDeath;

    protected override void Start()
    {
        base.Start();

        GameObject HealthBar = Instantiate<GameObject>(HealthBarPrefab);
        RectTransform canvas = GameObject.Find("Canvas").GetComponent<RectTransform>();
        HealthBar.GetComponent<EnemyHealthBar>().SetHealthBarData(
            target: this,
            healthBarPanel: canvas);

        healthBar = HealthBar.GetComponent<EnemyHealthBar>();

        HealthBar.transform.SetParent(
            parent: canvas,
            worldPositionStays: false);

        CurrentXP = BaseXP * CurrentLevel;
    }

    protected override void Update()
    {
        base.Update();

        if (IsDead)
            OnDeath?.Invoke(this);
    }

    public int GetXP()
    {
        if (HasHealth)
            return 0;
        return CurrentXP;
    }

    protected override void Death()
    {
        base.Death();
        gameObject.layer = 9;
    }

    protected override void OnWeaponAttackHit(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            Character player;
            if (collider.gameObject.TryGetComponent<Character>(out player) && IsAttacking())
            {
                player.ReceiveAttack(
                    value: GetDamage());

                if (Weapon.GetEffectName() != EffectName.Normal)
                    player.AddEffect(
                        effect: Weapon.GetEffectName(),
                        intensity: Weapon.GetEffectValue(),
                        duration: Weapon.GetEffectDuration());
            }
        }
    }

    private void OnDestroy()
    {
        Weapon.OnAttackHit -= OnWeaponAttackHit;
        GameObject.Destroy(Weapon.gameObject);
        Destroy(healthBar);
    }
}
