using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Text TextValue;
    public Slider Slider;
    // Start is called before the first frame update
    void Start()
    {

    }

    void Awake()
    {
        UpdateValues();
    }
    // Update is called once per frame
    void Update()
    {
        UpdateValues();
    }

    void UpdateValues()
    {
        try
        {
            TextValue.text = "" +
            Slider.value.ToString() +
            "/" +
            Slider.maxValue.ToString();
        }
        catch (NullReferenceException)
        {
            return;
        }
    }
}
