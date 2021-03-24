using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : Item
{
    /// <summary>
    /// Percentaje of the damage nullified by this shield.
    /// </summary>
    public float Protection => GetValue();
    /// <summary>
    /// Returns the value of the "Value" field as a percentaje.
    /// </summary>
    /// <returns>float between o and 1</returns>
    protected override float GetValue()
    {
        return base.GetValue() * 0.01f;
    }
}
