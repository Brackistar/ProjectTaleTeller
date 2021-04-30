using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public delegate void ValueChanged(int change);
[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class StatModifierController : MonoBehaviour
{
    private int InitialValue;
    public int Value { get; private set; }

    public ValueChanged OnValueChanged;
    private TMPro.TextMeshProUGUI ValueText;
    // Use this for initialization
    void Awake()
    {
        if (!ValueText)
        {
            ValueText = transform.Find("Value")
            .GetComponent<TMPro.TextMeshProUGUI>();
            Reset();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Add()
    {
        Value++;
        UpdateValue();

        if (!transform.Find("Minus").GetComponent<Button>().interactable)
            AllowSubstract();

        OnValueChanged?.Invoke(1);
    }
    public void Substract()
    {
        Value--;
        UpdateValue();

        if (Value == InitialValue)
            DenySubstract();

        OnValueChanged(-1);
    }
    public void SetValue(int value)
    {
        //Initialize();
        Value = InitialValue = value;
        ValueText.text = value.ToString();

        Debug.Log(
            message: name + " value initially set to: " + value.ToString());
    }
    public void Reset()
    {
        ValueText.text = "";
        Debug.Log(
            message: name + " values reset.");
    }
    public void AllowAdd()
    {
        transform.Find("Plus")
            .GetComponent<Button>()
            .interactable = true;
        Debug.Log(
            message: name + " Add allowed.");
    }
    public void DenyAdd()
    {
        transform.Find("Plus")
            .GetComponent<Button>()
            .interactable = false;
        Debug.Log(
            message: name + " Add denied.");
    }
    public void DenySubstract()
    {
        transform.Find("Minus")
            .GetComponent<Button>()
            .interactable = false;
        Debug.Log(
            message: name + " Substract denied.");
    }
    private void AllowSubstract()
    {
        transform.Find("Minus")
            .GetComponent<Button>()
            .interactable = true;
        Debug.Log(
            message: name + " Substract allowed.");
    }
    private void UpdateValue()
    {
        ValueText.text = Value.ToString();
        Debug.Log(
            name + " value changed to: " + Value.ToString());
    }
}
