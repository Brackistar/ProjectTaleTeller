using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class LevelController : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField]
    private GameObject GameoverScreen;
    [Header("Audio Settings")]
    [SerializeField]
    private AudioClip WalkSound;
    [SerializeField]
    private AudioClip FallSound;
    [SerializeField]
    private AudioClip JumpSound;
    [SerializeField]
    private AudioClip RunSound;
    [SerializeField]
    private AudioSource audioSource;
    [Header("Level Settings")]
    [SerializeField]
    private Weapon initialWeapon;
    [SerializeField]
    private Armor initialArmor;
    [SerializeField]
    private Shield initialShield;
    //private LevelEndCondition levelEndCondition;
    public static Player Player { get; private set; }
    public static string LevelName { get; private set; }
    private int totalLevelXP;

    private CameraController mainCameraController;
    private void Awake()
    {
#if UNITY_EDITOR
        if (!GameObject.Find("GameManager"))
        {
            GameObject GameManager = Instantiate(new GameObject(), null);
            GameManager.name = "GameManager";
            GameManager.AddComponent<GameManager>();
        }
#endif

        mainCameraController = Camera.main.GetComponent<CameraController>();
        audioSource = gameObject.GetComponent<AudioSource>();
        Player = FindObjectOfType<Player>();
        LevelName = SceneManager.GetActiveScene().name;
        //levelEndCondition = gameObject.GetComponent<LevelEndCondition>();
        //levelEndCondition.OnLevelEnded += OnLevelEnd;
        //foreach (LevelEndCondition endCondition in GameObject.FindObjectsOfType<LevelEndCondition>())
        //{
        //    endCondition.OnLevelEnded += OnLevelEnd;

        //    Debug.Log(
        //        message: name + " level end condition registered. Condition: \'" + endCondition.ConditionType + "\'");
        //}

    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(
            message: name + " Level start.");

        Player.SetWalkSound(
            sound: WalkSound);
        Player.SetJumpSound(
            sound: JumpSound);
        Player.SetFallSound(
            sound: FallSound);
        Player.SetRunSound(
            sound: RunSound);

        audioSource.time = 3f;
        audioSource.Play();

        short totalEnemies = 0;
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Enemy _ = enemy.GetComponent<Enemy>();
            int xp = _.GetXP();
            _.OnDeath += OnEnemyDeath;
            totalLevelXP += xp;

            totalEnemies++;

            Debug.Log(
                message: name + " enemy XP added:" + xp + ", new level xp: " + totalLevelXP.ToString());
        }

        Debug.Log(
            message: name + " total enemy count: " + totalEnemies.ToString());
        Debug.Log(
            message: name + " total XP: " + totalLevelXP.ToString());
        if (Time.timeScale == 0)
            Resume();

        foreach (LevelEndCondition endCondition in GameObject.FindObjectsOfType<LevelEndCondition>())
        {
            endCondition.OnLevelEnded += OnLevelEnd;

            Debug.Log(
                message: name + " level end condition registered. Condition: \'" + endCondition.ConditionType + "\'");
        }
    }

    private void OnEnemyDeath(Enemy enemy)
    {
        enemy.OnDeath -= OnEnemyDeath;

        int XP = enemy.GetXP();

        Player.AddXP(XP);

        totalLevelXP -= XP;

        //GameObject.Destroy(enemy.gameObject, 3);
        Debug.Log(
            message: name + " enemy \'" + enemy.name + "\' dead. Total level XP: " + totalLevelXP.ToString());
    }

    private void OnLevelEnd()
    {
        foreach (LevelEndCondition endCondition in GameObject.FindObjectsOfType<LevelEndCondition>())
            endCondition.OnLevelEnded -= OnLevelEnd;

        Player.AddXP(totalLevelXP);
        Debug.Log(
            message: "Level finished.");
        EndLevel();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Player)
            return;
        if (!Player.HasHealth)
        {
            mainCameraController.CanMove(false);
        }

        if (Player.IsDead)
            //GameoverScreen.SetActive(true);
            EndLevel();
    }

    public static void Pause()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;

        Debug.Log(
            message: "game paused.");
    }
    public static void Resume()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;

        Debug.Log(
            message: "game resumed.");
    }

    private void EndLevel()
    {
        GameoverScreen.SetActive(true);
    }

    public Weapon GetInitialWeapon() => initialWeapon;
    public Armor GetInitialArmor() => initialArmor;
    public Shield GetInitialShield() => initialShield;

    public static Vector2 AngleToVector2(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector2(
            x: Mathf.Cos(radian),
            y: Mathf.Sin(radian));
    }
    public static Vector2 AngleToVector2(float angle, float magnitude)
    {
        return AngleToVector2(angle) * magnitude;
    }
}
