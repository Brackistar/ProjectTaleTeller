using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class DeveloperOptionBoolController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Name of the function of the initial state. Must use a ReturnArgs parameter.")]
    private string InitialStateFunction;
    [SerializeField]
    private string ChangeStateFunction;

    private Toggle Toggle;
    // Use this for initialization
    void Start()
    {
        ReturnArgs InitialState = new ReturnArgs();
        SendMessageUpwards(
            methodName: InitialStateFunction,
            value: InitialState);
        Debug.Log(
            message: name + " initial value set to: \'" + InitialState.value.ToString() + "\'");


        Toggle = GetComponentInChildren<Toggle>();
        Toggle.onValueChanged.AddListener(OnValueChanged);

        Toggle.isOn = InitialState.value;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    private void OnValueChanged(bool value)
    {
        SendMessageUpwards(
            methodName: ChangeStateFunction,
            value: value);
        Debug.Log(
            message: name + " value changed to: \'" + value.ToString() + "\'");
    }

    public class ReturnArgs
    {
        public bool value;
    }
}