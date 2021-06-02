using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EndPointController : MonoBehaviour
{
    public delegate void Triggered();
    public event Triggered OnTriggered;
    // Use this for initialization
    void Start()
    {
        BoxCollider2D collider = gameObject.GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
    }

    // Update is called once per frame
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(
            message: name + " end point area reached at: " + transform.position.ToString());

        if (collision.tag.Equals("Player") || collision.transform.parent.tag.Equals("Player"))
            OnTriggered?.Invoke();
    }
}