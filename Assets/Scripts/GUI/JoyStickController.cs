using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class JoyStickController : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    //[SerializeField]
    //private Player PlayerAvatar;

    private Image InnerCircle,
        OuterCircle;
    [Range(1, 30)]
    public int MaxJoyStickFreedom = 10;
    [Range(10, 20)]
    public int ControlSensitivity = 10;

    private Vector2 InputDirection;

    // Start is called before the first frame update
    void Start()
    {
        InnerCircle = GetComponentsInChildren<Image>()
            .SingleOrDefault(image => image.name == "InnerCircle");
        OuterCircle = GetComponentsInChildren<Image>()
            .SingleOrDefault(image => image.name == "OuterCircle");
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseMenuController.isPaused)
            return;

        LevelController.Player.Move(
            Direction: new Vector2(
                x: InputDirection.x * ControlSensitivity,
                y: InputDirection.y * ControlSensitivity)
            );
    }

    public void Jump()
    {
        if (PauseMenuController.isPaused)
            return;

        LevelController.Player.Jump();
    }

    public void Attack()
    {
        if (PauseMenuController.isPaused)
            return;

        LevelController.Player.Attack();
    }

    public void Defend()
    {
        if (PauseMenuController.isPaused)
            return;

        LevelController.Player.Defend();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (PauseMenuController.isPaused)
            return;

        Vector2 position = Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect: OuterCircle.rectTransform,
            screenPoint: eventData.position,
            cam: eventData.pressEventCamera,
            out position);

        position.x = (position.x / OuterCircle.rectTransform.sizeDelta.x);
        position.y = (position.y / OuterCircle.rectTransform.sizeDelta.y);


        float x = (OuterCircle.rectTransform.pivot.x == 1f) ? position.x * 2 + 1 : position.x * 2 - 1;
        float y = (OuterCircle.rectTransform.pivot.y == 1f) ? position.y * 2 + 1 : position.y * 2 - 1;

        InputDirection = new Vector3(
            x: x,
            y: y,
            z: 0);

        InputDirection = (InputDirection.magnitude > 1) ? InputDirection.normalized : InputDirection;

        InnerCircle.rectTransform.anchoredPosition = new Vector3(
            x: InputDirection.x * (OuterCircle.rectTransform.sizeDelta.x / MaxJoyStickFreedom),
            y: InputDirection.y * (OuterCircle.rectTransform.sizeDelta.y) / MaxJoyStickFreedom);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (PauseMenuController.isPaused)
            return;

        InputDirection = Vector3.zero;
        InnerCircle.rectTransform.anchoredPosition = Vector3.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (PauseMenuController.isPaused)
            return;
        OnDrag(eventData);
    }
}
