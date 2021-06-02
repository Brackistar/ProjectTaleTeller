using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void EnemyDead(Enemy source);
public class Enemy : Character
{
    // Health bar   
    [SerializeField]
    private GameObject HealthBarPrefab;
    //private EnemyHealthBar healthBar;
    [SerializeField]
    private EnemyStatusController EnemySpotedAlert;
    private EnemyHealthBar HealthBar;
    // A.I. related
    [SerializeField]
    [Range(2, 5)]
    public float VisionFieldLength = 2;
    [SerializeField]
    [Range(45, 180)]
    public float FieldOfVision = 80;
    public Character Target;
    // Dead related
    public event EnemyDead OnDeath;
    [SerializeField]
    [Min(0)]
    protected int BaseXP;

    protected override void Start()
    {
        base.Start();

        //GameObject healthBar = Instantiate(HealthBarPrefab);
        HealthBar = Instantiate(HealthBarPrefab)
            .GetComponent<EnemyHealthBar>();
        RectTransform canvas = GameObject.Find("HealthBars_Container").GetComponent<RectTransform>();

        //healthBar.GetComponent<EnemyHealthBar>().SetHealthBarData(
        //    target: this,
        //    healthBarPanel: canvas);
        HealthBar.SetHealthBarData(
            target: this,
            healthBarPanel: canvas);



        HealthBar.transform.SetParent(
            parent: canvas,
            worldPositionStays: false);

        EnemySpotedAlert = Instantiate(
            original: EnemySpotedAlert.gameObject)
            .GetComponent<EnemyStatusController>();

        EnemySpotedAlert.SetInitialData(
            target: this,
            canvas: canvas);

        EnemySpotedAlert.gameObject.transform.SetParent(
            parent: canvas,
            worldPositionStays: false);

        CurrentXP = BaseXP * CurrentLevel;
    }

    protected override void Update()
    {
        base.Update();
        if (!HasHealth)
            return;

        Debug.Log(
            message: name + " enemy updating.");

        EnemySpotedAlert.Reposition();

        HealthBar.Reposition();
        HealthBar.Fill();
        //if (IsDead)
        //    OnDeath?.Invoke(this);
    }

    public int GetXP()
    {
        //if (HasHealth)
        //    return 0;
        return CurrentXP;
    }

    public void AlertStatus(AIState state)
    {
        Debug.Log(
            message: name + " alert state \'" + state.ToString() + "\'");

        switch (state)
        {
            case AIState.EnemySpoted:
                if (!HasHealth)
                    break;
                EnemySpotedAlert.EnableAlert();
                break;
        }
    }

    protected override void Death()
    {
        if (!HasHealth)
            return;
        base.Death();
        OnDeath?.Invoke(this);
        gameObject.layer = 9;

        Destroy(gameObject, 1);
        Destroy(EnemySpotedAlert);
        Destroy(HealthBar);
    }

    protected override void OnWeaponAttackHit(Collider2D collider)
    {
        if (!HasHealth)
            return;
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
        Destroy(HealthBar);
    }
}
