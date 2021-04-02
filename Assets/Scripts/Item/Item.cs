using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    [SerializeField]
    private Sprite Image;
    [SerializeField]
    private string Name;
    [SerializeField]
    private string Description;
    [SerializeField]
    [Range(0f, 100f)]
    protected float Value;
    [SerializeField]
    [Range(0, 5)]
    private int Rank = 0;
    public int Quality { get => Rank; }

    public string ItemDescription
    {
        private set
        {
            ItemDescription = value.Substring(
            startIndex: 0,
            length: 500).Trim();

            ItemDescription = Regex.Replace(
                input: ItemDescription,
                pattern: @"^[a-z]|(?<=\. |\.)[a-z]",
                evaluator: x => x.ToString().ToUpper(),
                options: RegexOptions.Multiline);

            if (ItemDescription.ElementAt(ItemDescription.Length - 1) != '.')
                ItemDescription += ".";
        }
        get => ItemDescription;
    }

    // Start is called before the first frame update
    void Start()
    {
        ItemDescription = Description;
    }
    /// <summary>
    /// Returns the sprite asociated to the item.
    /// </summary>
    /// <returns></returns>
    public Sprite GetImage()
    {
        return Image;
    }
    /// <summary>
    /// Returns the value of the "Value" field.
    /// </summary>
    /// <returns></returns>
    protected virtual float GetValue()
    {
        return Value;
    }
}
