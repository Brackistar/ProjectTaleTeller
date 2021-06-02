using System.Collections;
using UnityEngine;


public abstract class LevelEndCondition : MonoBehaviour
{
    public delegate void LevelEnded();
    public event LevelEnded OnLevelEnded;
    protected bool levelEnded;
    public string ConditionType { get; protected set; }
    protected virtual void Awake()
    {
        ConditionType = "Undefined";
        levelEnded = false;
    }
    protected void EndLevel()
    {
        Debug.Log(
            message: "Level end fired.");
        levelEnded = true;
        OnLevelEnded?.Invoke();
    }

    protected virtual void Start()
    {
        Debug.Log(
            message: name + " level end condition start.");
    }
    protected virtual void Update()
    {
        if (CheckCondition() && !levelEnded)
        {
            EndLevel();
        }
    }
    protected abstract bool CheckCondition();
}
