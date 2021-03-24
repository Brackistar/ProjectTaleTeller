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
    private GameObject EnemyHealthBarPrefab;

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
    }

    private void OnEnemyDeath(Enemy enemy)
    {
        Player.AddXP(enemy.GetXP());
        GameObject.Destroy(enemy.gameObject, 3);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Player.HasHealth)
        {
            mainCameraController.CanMove(false);

        }

        if (Player.IsDead)
            GameObject.Find("GameOver_Container")
                .SetActive(true);
    }

    public Weapon GetInitialWeapon() => initialWeapon;
    public Armor GetInitialArmor() => initialArmor;
    public Shield GetInitialShield() => initialShield;
}
