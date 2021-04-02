using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(PlatformEffector2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlatformController : MonoBehaviour
    {
        [SerializeField]
        [Range(0.5f, 5)]
        private float Speed = 0.5f;
        public float speed { get => Speed; }
        [SerializeField]
        [Range(0, 100)]
        private float XDistance = 0.5f;
        [SerializeField]
        [Range(0, 100)]
        private float YDistance = 0.5f;
        [SerializeField]
        private PlatformDirection Direction = PlatformDirection.right;
        public bool Activated = true;

        private Vector2 StartPosition;
        private Vector2 StopPosition;
        private Vector2 target;
        private bool firstMovement = true;

        private void Awake()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.sortingLayerName = "Terrain";
            renderer.sortingOrder = 2;

            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        }
        private void Start()
        {
            SetPlatform(transform.position);
        }
        private void Update()
        {
            if (Activated)
            {
                Move();
            }
        }
        private void Move()
        {
            if ((Vector2)transform.position == StartPosition || firstMovement)
            {
                target = StopPosition;
                Debug.Log(
                    message: "Platform \'" + name + "\' reached start position at: " + transform.position.ToString());
            }

            if ((Vector2)transform.position == StopPosition)
            {
                if (firstMovement)
                    firstMovement = false;
                target = StartPosition;
                Debug.Log(
                    message: "Platform \'" + name + "\' reached stop position at: " + transform.position.ToString());
            }


            transform.position = Vector2.MoveTowards(
                current: transform.position,
                target: target,
                maxDistanceDelta: Speed * Time.deltaTime);

            Debug.Log(
                message: "Platform \'" + name + "\' moves " + Speed.ToString() + " towards " + target.ToString() + " position.");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Position">Initial psition of the platform</param>
        public void SetPlatform(Vector2 Position)
        {
            StartPosition = new Vector2(
                x: Mathf.FloorToInt(Position.x),
                y: Mathf.FloorToInt(Position.y));

            switch (Direction)
            {
                case PlatformDirection.up:
                    StopPosition = new Vector2(
                        x: StartPosition.x,
                        y: StartPosition.y + YDistance);
                    break;
                case PlatformDirection.down:
                    StopPosition = new Vector2(
                        x: StartPosition.x,
                        y: StartPosition.y - YDistance);
                    break;
                case PlatformDirection.left:
                    StopPosition = new Vector2(
                        x: StartPosition.x - XDistance,
                        y: StartPosition.y);
                    break;
                case PlatformDirection.right:
                    StopPosition = new Vector2(
                        x: StartPosition.x + XDistance,
                        y: StartPosition.y);
                    break;
                case PlatformDirection.upleft:
                    StopPosition = new Vector2(
                        x: StartPosition.x - XDistance,
                        y: StartPosition.y + YDistance);
                    break;
                case PlatformDirection.upright:
                    StopPosition = new Vector2(
                        x: StartPosition.x + XDistance,
                        y: StartPosition.y + YDistance);
                    break;
                case PlatformDirection.downleft:
                    StopPosition = new Vector2(
                        x: StartPosition.x - XDistance,
                        y: StartPosition.y - YDistance);
                    break;
                case PlatformDirection.downright:
                    StopPosition = new Vector2(
                        x: StartPosition.x + XDistance,
                        y: StartPosition.y - YDistance);
                    break;
            }
        }
        public void SetPlatform(Vector2 Position, PlatformDirection MovementDirection)
        {
            Direction = MovementDirection;
            SetPlatform(Position);
        }
        /// <summary>
        /// Determines the initial movement direction of a platform.
        /// </summary>
        public enum PlatformDirection
        {
            up,
            down,
            left,
            right,
            upleft,
            upright,
            downleft,
            downright
        }
    }
}