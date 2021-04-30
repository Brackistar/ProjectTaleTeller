using System;
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

    Image XPBar;
    GameObject[] LevelUpButtons;
    private void Awake()
    {
        if (HealthBar == null)
            HealthBar = GetComponentsInChildren<Slider>()
                .FirstOrDefault(slider => slider.name == "Health_Bar");
        if (!XPBar)
            XPBar = GetComponentsInChildren<Image>()
                .FirstOrDefault(_ => _.name == "XP_Bar");
        if (!HealthBarText)
            HealthBarText = GetComponentsInChildren<TextMeshProUGUI>()
                .FirstOrDefault(text => text.name == "Indicator");

        LevelUpButtons = new GameObject[2];
        LevelUpButtons[0] = transform.Find("Level Up Container")
            .gameObject;
        LevelUpButtons[1] = transform.parent
            .Find("Pause_Menu_Container/Pause_Menu/InnerMenuSpace/Character_Sheet/Character_Attrib_Container/Level_Up_Button")
            .gameObject;

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

        LevelController.Player.OnLevelUp += OnLevelUp;
        LevelController.Player.OnLevelUpDone += OnLevelUpDone;

        SetMaxHealth();
        SetCurrentHealth();
        SetHealtBarText();

        ChangeXPBar();
    }

    // Update is called once per frame
    void Update()
    {
        SetMaxHealth();
        SetCurrentHealth();
        SetHealtBarText();

        ChangeXPBar();
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
    /// <summary>
    /// Updates teh value of the player's XP bar.
    /// </summary>
    private void ChangeXPBar()
    {
        int MaxXP = LevelController.Player.NextLevelXP,
            CurrentXP = LevelController.Player.XP,
            CurrentPercent = CurrentXP / MaxXP;

        if (XPBar.fillAmount != CurrentPercent)
            XPBar.fillAmount = CurrentPercent;
    }
    /// <summary>
    /// Shows the level up menu.
    /// </summary>
    /// <param name="source"></param>
    private void OnLevelUp(Player source)
    {
        //GameObject LevelUpContainer = GameObject.Find("Level Up Container");
        TextMeshProUGUI IndicatorText = LevelUpButtons[0]
            .GetComponentInChildren<TextMeshProUGUI>();

        IndicatorText.text = (source.Level + 1) + "!";
        //LevelUpContainer.GetComponent<Button>()
        //    .interactable = true;

        //LevelUpContainer.SetActive(true);
        //GameObject.Find("Character_Attrib_Container/Level_Up_Button")
        //    .SetActive(true);
        for (int i = 0; i < LevelUpButtons.Length; i++)
        {
            LevelUpButtons[i].SetActive(true);
            LevelUpButtons[i].GetComponent<Button>()
            .interactable = true;
        }
    }
    /// <summary>
    /// Hides the level up menu and icon.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    private void OnLevelUpDone(object source, EventArgs e)
    {
        //GameObject LevelUpContainer = GameObject.Find("Level Up Container");
        //TextMeshProUGUI IndicatorText = LevelUpContainer
        //    .GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI IndicatorText = LevelUpButtons[0]
            .GetComponentInChildren<TextMeshProUGUI>();

        IndicatorText.text = "";
        //LevelUpContainer.GetComponent<Button>()
        //    .interactable = false;

        //LevelUpContainer.SetActive(false);
        //GameObject.Find("Character_Sheet/Character_Attrib_Container/Level_Up_Button")
        //    .SetActive(false);
        for (int i = 0; i < LevelUpButtons.Length; i++)
        {
            LevelUpButtons[i].SetActive(false);
            LevelUpButtons[i].GetComponent<Button>()
            .interactable = false;
        }
    }
}
