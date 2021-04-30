using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelUp_Menu_Controller : MonoBehaviour
{
    [SerializeField]
    [Range(1, 5)]
    private int skillPointByLevel = 2;
    private int totalSkillPoints = 0;

    private StatModifierController AgilityModifier,
        StrengthModifier,
        ResistanceModifier,
        VitalityModifier;

    private TMPro.TextMeshProUGUI SkillPointsText;
    private void Awake()
    {
        AgilityModifier = transform.Find("LevelUp_Menu/InnerSpace/ModifiersSpace/AgilityModifier")
            .GetComponent<StatModifierController>();
        StrengthModifier = transform.Find("LevelUp_Menu/InnerSpace/ModifiersSpace/StrengthModifier")
            .GetComponent<StatModifierController>();
        ResistanceModifier = transform.Find("LevelUp_Menu/InnerSpace/ModifiersSpace/ResistanceModifier")
            .GetComponent<StatModifierController>();
        VitalityModifier = transform.Find("LevelUp_Menu/InnerSpace/ModifiersSpace/VitalityModifier")
            .GetComponent<StatModifierController>();

        SkillPointsText = transform.Find("LevelUp_Menu/InnerSpace/Indicator Space/Value")
            .GetComponentInChildren<TMPro.TextMeshProUGUI>();

        AgilityModifier.OnValueChanged += OnStatLevelChange;
        StrengthModifier.OnValueChanged += OnStatLevelChange;
        ResistanceModifier.OnValueChanged += OnStatLevelChange;
        VitalityModifier.OnValueChanged += OnStatLevelChange;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="changeValue"></param>
    private void OnStatLevelChange(int changeValue)
    {
        if (changeValue == 0)
            return;

        totalSkillPoints -= changeValue;
        if (totalSkillPoints == 0)
        {
            AgilityModifier.DenyAdd();
            StrengthModifier.DenyAdd();
            ResistanceModifier.DenyAdd();
            VitalityModifier.DenyAdd();

            transform.Find("LevelUp_Menu/InnerSpace/Level Up Button")
                .GetComponent<Button>()
                .interactable = true;
        }
        else if (totalSkillPoints == 1 && changeValue < 0)
        {
            AgilityModifier.AllowAdd();
            StrengthModifier.AllowAdd();
            ResistanceModifier.AllowAdd();
            VitalityModifier.AllowAdd();

            transform.Find("LevelUp_Menu/InnerSpace/Level Up Button")
                .GetComponent<Button>()
                .interactable = false;
        }

        SkillPointsText.text = totalSkillPoints.ToString();

        Debug.Log(
            message: name + " remaining skill points: " + totalSkillPoints);
    }
    public void SetValues()
    {
        //Initialize();
        Player player = LevelController.Player;

        totalSkillPoints = skillPointByLevel;
        SkillPointsText.text = skillPointByLevel.ToString();

        AgilityModifier.SetValue(Mathf.FloorToInt(player.GetAgility()));
        StrengthModifier.SetValue(Mathf.FloorToInt(player.GetStrength()));
        ResistanceModifier.SetValue(Mathf.FloorToInt(player.GetResistance()));
        VitalityModifier.SetValue(Mathf.FloorToInt(player.GetVitality()));
    }
    public void ResetValues()
    {
        AgilityModifier.Reset();
        StrengthModifier.Reset();
        ResistanceModifier.Reset();
        VitalityModifier.Reset();
    }

    public void LevelUp()
    {
        LevelController.Player.ChangeStatValues(
            agility: AgilityModifier.Value,
            strength: StrengthModifier.Value,
            resistance: ResistanceModifier.Value,
            vitality: VitalityModifier.Value);

        LevelController.Player.LevelUp();

        Debug.Log(
            message: "Level up button pressed.");
    }
}
