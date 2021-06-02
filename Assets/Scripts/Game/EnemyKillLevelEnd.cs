using System.Collections;
using UnityEngine;


public class EnemyKillLevelEnd : LevelEndCondition
{
    [SerializeField]
    [Range(1, 250)]
    private short TotalEnemyKill;
    private short ActualEnemyKill;

    protected override void Awake()
    {
        base.Awake();

        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
            enemy.OnDeath += OnEnemyDeath;

        TotalEnemyKill = (short)enemies.Length;
        ActualEnemyKill = 0;

        ConditionType = "Enemy kill (" + TotalEnemyKill + ")";
    }
    protected override bool CheckCondition()
    {
        return TotalEnemyKill == ActualEnemyKill;
    }

    private void OnEnemyDeath(Enemy enemy)
    {
        enemy.OnDeath -= OnEnemyDeath;
        ActualEnemyKill++;
    }
}
