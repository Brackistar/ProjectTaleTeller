using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoController : MonoBehaviour
{
    [SerializeField]
    [Min(1)]
    private float ChangeTime;
    private Slider HealthBar;
    private TextMeshProUGUI HealthBarText;
    private void Awake()
    {
        if (HealthBar == null)
            HealthBar = GetComponentsInChildren<Slider>()
                .FirstOrDefault(slider => slider.name == "Health_Bar");

        //if (HealthBarText == null)
        //{
        //    TextMeshProUGUI[] textMesh = GetComponentsInChildren<TextMeshProUGUI>();

        //    HealthBarText = textMesh.FirstOrDefault(text => text.name == "Indicator");
        //}
        HealthBarText = GetComponentsInChildren<TextMeshProUGUI>()
            .FirstOrDefault(text => text.name == "Indicator");

        if (ChangeTime <= 0)
            ChangeTime = 1;
    }
    // Start is called before the first frame update
    void Start()
    {
        HealthBar.maxValue = LevelController.Player
            .GetTotalHealth();
        HealthBar.value = LevelController.Player
            .GetCurrentHealth();

        SetMaxHealth();
        SetCurrentHealth();
        SetHealtBarText();
    }

    // Update is called once per frame
    void Update()
    {


        SetMaxHealth();
        SetCurrentHealth();
        SetHealtBarText();
    }
    /// <summary>
    /// If the current health bar value or max value is different from the values on the text showed, changes the text value
    /// </summary>
    private void SetHealtBarText()
    {
        string[] values = HealthBarText.text.Split(
            separator: new char[] { '/' },
            options: System.StringSplitOptions.RemoveEmptyEntries);

        int currentHealth = int.Parse(values[0]),
            totalHealth = int.Parse(values[1]);

        if (currentHealth != HealthBar.value || totalHealth != HealthBar.maxValue)
        {
            HealthBarText.text = "" +
                HealthBar.value +
                "/" +
                HealthBar.maxValue;
        }
    }
    /// <summary>
    /// If the player maxHealth value changes, changes the Health bar max value
    /// </summary>
    private void SetMaxHealth()
    {
        int playerMaxHealth = LevelController.Player.GetTotalHealth();

        if (HealthBar.maxValue != playerMaxHealth)
            HealthBar.maxValue = playerMaxHealth;
    }
    /// <summary>
    /// If the value of the Health bar is different of the player's current health value, changes the health bar value
    /// </summary>
    private void SetCurrentHealth()
    {
        int playerHealth = LevelController.Player.GetCurrentHealth();

        if (HealthBar.value != playerHealth)
        {
            StopCoroutine("SmoothHealthChange");
            StartCoroutine(SmoothHealthChange(playerHealth));
        }
    }
    /// <summary>
    /// Changes the current value of the health bar towars a target value over a period of time
    /// </summary>
    /// <param name="newValue">Final value desired for the healt bar value</param>
    /// <returns></returns>
    private IEnumerator SmoothHealthChange(int newValue)
    {
        float time = 0.0f;

        while (time <= ChangeTime)
        {
            HealthBar.value = Mathf.Lerp(
                a: HealthBar.value,
                b: newValue,
                time / ChangeTime);

            time += Time.deltaTime;

            SetHealtBarText();
            yield return null;
        }
    }
}
