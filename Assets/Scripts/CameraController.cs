using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    /// <summary>
    /// Difference between the camera and the player initial positions
    /// </summary>
    private Vector2 offset;
    /// <summary>
    /// When true the camera will move acording with the playerAvatar position while maintaining the offset distance
    /// </summary>
    private bool isMovementAllowed;
    /// <summary>
    /// Player that this camara will follow
    /// </summary>
    public Player playerAvatar;

    private void Awake()
    {
        isMovementAllowed = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - playerAvatar.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMovementAllowed)
            transform.position = new Vector3(
                x: playerAvatar.transform.position.x + offset.x,
                y: playerAvatar.transform.position.y + offset.y,
                z: transform.position.z);
    }
    /// <summary>
    /// Allows or denies the camera from moving following the player
    /// </summary>
    /// <param name="state"></param>
    public void CanMove(bool state)
    {
        isMovementAllowed = state;
    }
}
