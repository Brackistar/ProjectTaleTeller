using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonController : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField]
    private string targetMethodName;
    private Player target;
    private bool isPressed;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (PauseMenuController.isPaused || isPressed)
            return;

        isPressed = true;
        target.SendMessage(
            methodName: targetMethodName,
            SendMessageOptions.RequireReceiver);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindObjectOfType<Player>();
        isPressed = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
