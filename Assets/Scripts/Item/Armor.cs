using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Armor : Item
{
    /// <summary>
    /// Percentaje of the damage nullified by this armor.
    /// </summary>
    public float Protection => GetValue();
    /// <summary>
    /// Value applied by the Item's effect.
    /// </summary>
    [SerializeField]
    private float[] EffectValues;
    /// <summary>
    /// Name of the effect applied
    /// </summary>
    [SerializeField]
    private EffectName[] Effects;
    /// <summary>
    /// Returns the value of the "Value" field as a percentaje.
    /// </summary>
    /// <returns>float between o and 1</returns>
    protected override float GetValue()
    {
        return base.GetValue() * 0.01f;
    }
    /// <summary>
    /// Get one of the EffecName in Effects array.
    /// </summary>
    /// <param name="index">Index in the array of the EffectName.</param>
    /// <returns></returns>
    public EffectName GetEffectName(int index)
    {
        return Effects[index];
    }
    /// <summary>
    /// Returns all EffectName of the Effects array.
    /// </summary>
    /// <returns></returns>
    public EffectName[] GetEffectNames()
    {
        return Effects;
    }
    /// <summary>
    /// Get one of the values of the EffectValues array.
    /// </summary>
    /// <param name="index">Index in the array of the effect value.</param>
    /// <returns></returns>
    public float GetEffectValue(int index)
    {
        return EffectValues[index];
    }
    /// <summary>
    /// Get one of the values of the EffectValues array.
    /// </summary>
    /// <param name="effectName">EffectName asociated to the effect value.</param>
    /// <returns></returns>
    public float GetEffectValue(EffectName effectName)
    {
        int index = System.Array.IndexOf(
            array: Effects,
            value: effectName);
        return GetEffectValue(index);
    }

    //public new enum EffectName
    //{
    //    Fire,
    //    Heal,
    //    Strenght,
    //    Resistance,
    //    Agility,
    //    Vitality
    //}
}
