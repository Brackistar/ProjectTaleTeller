using System.Collections;
using UnityEngine;
using System.Linq;

public class AreaReachedLevelEnd : LevelEndCondition
{
    private bool AreaReached;
    [SerializeField]
    private GameObject[] PossibleAreas;
    protected override bool CheckCondition()
    {
        return AreaReached;
    }

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();
        ConditionType = "Reach area at: " + transform.position.ToString();

        //AreaReached = false;
        //bool AnyEndPoint = false;
        //if (PossibleAreas == null || PossibleAreas.Length == 0)
        //{
        //    AnyEndPoint = false;
        //}
        //else
        //{
        //    if (!PossibleAreas.Any(_ => _.GetComponent<EndPointController>() != null))
        //    {
        //        AnyEndPoint = false;
        //    }
        //    else
        //    {
        //        AnyEndPoint = true;

        //        foreach (GameObject area in PossibleAreas)
        //        {
        //            area.GetComponent<EndPointController>()
        //                .OnTriggered += OnAnyAreaReached;
        //            Debug.Log(
        //                message: name + " posible level end area registered at: " + area.transform.position.ToString());
        //        }
        //    }
        //}

        //if (!AnyEndPoint)
        //{
        //    System.Exception exception = new System.Exception(
        //        message: "No area associated to the level end condition.");

        //    Debug.LogException(exception);
        //    throw exception;
        //}
    }

    protected override void Start()
    {
        base.Start();

        AreaReached = false;
        bool AnyEndPoint = false;
        if (PossibleAreas == null || PossibleAreas.Length == 0)
        {
            AnyEndPoint = false;
        }
        else
        {
            if (!PossibleAreas.Any(_ => _.GetComponent<EndPointController>() != null))
            {
                AnyEndPoint = false;
            }
            else
            {
                AnyEndPoint = true;

                foreach (GameObject area in PossibleAreas)
                {
                    area.GetComponent<EndPointController>()
                        .OnTriggered += OnAnyAreaReached;
                    Debug.Log(
                        message: name + " posible level end area registered at: " + area.transform.position.ToString());
                }
            }
        }

        if (!AnyEndPoint)
        {
            System.Exception exception = new System.Exception(
                message: "No area associated to the level end condition.");

            Debug.LogException(exception);
            throw exception;
        }
    }

    private void OnAnyAreaReached()
    {
        AreaReached = true;

        foreach (GameObject area in PossibleAreas)
        {
            area.GetComponent<EndPointController>()
                .OnTriggered -= OnAnyAreaReached;
        }
    }
}
