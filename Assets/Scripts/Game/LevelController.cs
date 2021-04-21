using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class LevelController : MonoBehaviour
{
    [SerializeField]
    private AudioClip WalkSound,
        FallSound,
        JumpSound,
        RunSound;
    [SerializeField]
    private GameObject GameoverScreen;

    private AudioSource audioSource;

    public static Player Player { get; private set; }

    [SerializeField]
    private Weapon initialWeapon;
    [SerializeField]
    private Armor initialArmor;
    [SerializeField]
    private Shield initialShield;

    private CameraController mainCameraController;
    private void Awake()
    {
        mainCameraController = Camera.main.GetComponent<CameraController>();
        audioSource = GetComponent<AudioSource>();
        Player = GameObject.FindObjectOfType<Player>();
        if (!GameObject.Find("SceneManager"))
        {
            GameObject SceneManager = Instantiate(new GameObject(), null);
            SceneManager.name = "SceneManager";
            SceneManager.AddComponent<LoadingManager>();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
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

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<Enemy>().OnDeath += OnEnemyDeath;
        }
        if (Time.timeScale == 0)
            Resume();
    }

    private void OnEnemyDeath(Enemy enemy)
    {
        Player.AddXP(enemy.GetXP());
        GameObject.Destroy(enemy.gameObject, 3);
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
            GameoverScreen.SetActive(true);
    }

    public static void Pause()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
    }
    public static void Resume()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
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
