using System;
using UnityEngine;
using Zenject;
using MiniIT.GAMESTATE;

namespace MiniIT.MOVEMENT
{
    public interface IPlatformMover
    {
        /// <summary>
        /// Moves platform by a horizontal delta (in screen units).
        /// </summary>
        void Move(float horizontalDelta);

        /// <summary>
        /// Moves platform by a world-space vector delta.
        /// </summary>
        void Move(Vector3 delta);

        /// <summary>
        /// Raised every time the platform moves horizontally. Passes the raw input delta.
        /// </summary>
        event Action<float> OnMove;

        /// <summary>
        /// Resets platform to its initial position (usually centered).
        /// </summary>
        void ResetPosition();
    }

    /// <summary>
    /// Base class for all platform movement implementations.
    /// Handles boundary clamping, speed, sensitivity and common logic.
    /// </summary>
    public abstract class AbstractMover : MonoBehaviour, IPlatformMover
    {
        public event Action<float> OnMove;

        [Header("Movement Settings")]
        [SerializeField] private float sensitivity = 0.02f;
        [SerializeField] private float speed = 12f;

        [Header("Screen Boundaries")]
        [SerializeField] private bool useScreenBounds = true;
        [SerializeField] private Camera targetCamera = null;

        [Inject] protected IGameStateManager gameStateManager = null;

        private float leftBoundary = float.MinValue;
        private float rightBoundary = float.MaxValue;
        private float halfWidth = 1f;

        public float Sensitivity => sensitivity;
        public float Speed => speed;
        public float LeftBoundary => leftBoundary;
        public float RightBoundary => rightBoundary;

        protected virtual void Awake()
        {
            CalculateBoundaries();

            if (gameStateManager == null)
            {
                Debug.LogError("AbstractMover: IGameStateManager injection failed!");
            }
        }

        /// <summary>
        /// Recalculates screen boundaries based on camera and platform collider.
        /// Called automatically on Awake and can be called again if needed.
        /// </summary>
        private void CalculateBoundaries()
        {
            if (!useScreenBounds)
            {
                return;
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                return;
            }

            float halfHeight = targetCamera.orthographicSize;
            float halfScreenWidth = halfHeight * targetCamera.aspect;

            Collider2D collider = GetComponent<Collider2D>();
            halfWidth = (collider != null) ? collider.bounds.extents.x : 1f;

            leftBoundary  = -halfScreenWidth + halfWidth;
            rightBoundary =  halfScreenWidth - halfWidth;
        }

        public virtual void Move(float horizontalDelta)
        {
            if (horizontalDelta == 0f)
            {
                return;
            }

            Vector3 delta = new Vector3(horizontalDelta * speed * Time.deltaTime, 0f, 0f);
            Move(delta);

            OnMove?.Invoke(horizontalDelta);
        }

        public virtual void Move(Vector3 delta)
        {
            Vector3 newPosition = transform.position + delta;

            if (useScreenBounds)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, leftBoundary, rightBoundary);
            }

            transform.position = newPosition;
        }

        /// <summary>
        /// Forces boundary recalculation (useful after resolution change or platform resize).
        /// </summary>
        public void RecalculateBoundaries()
        {
            CalculateBoundaries();
        }

        protected abstract void HandleInput();

        public abstract void ResetPosition();
    }

    /// <summary>
    /// Touch-based platform movement (drag to move).
    /// Works with single-finger touch or mouse in editor.
    /// </summary>
    public class PlatformMovement : AbstractMover
    {
        private Vector2 startTouchPosition = Vector2.zero;
        private bool isTouchActive = false;
        private Vector3 initialPosition = Vector3.zero;

        protected override void Awake()
        {
            base.Awake();
            initialPosition = transform.position;
        }

        private void Update()
        {
            if (gameStateManager.CurrentState != GameState.Playing)
            {
                return;
            }

            HandleInput();
        }

        protected override void HandleInput()
        {
            if (Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }

        private void HandleTouchInput()
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    isTouchActive = true;
                    break;

                case TouchPhase.Moved:
                    if (isTouchActive)
                    {
                        float deltaX = (touch.position.x - startTouchPosition.x) * Sensitivity;
                        Move(deltaX);
                        startTouchPosition = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isTouchActive = false;
                    break;
            }
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                startTouchPosition = Input.mousePosition;
                isTouchActive = true;
            }
            else if (Input.GetMouseButton(0) && isTouchActive)
            {
                float deltaX = (Input.mousePosition.x - startTouchPosition.x) * Sensitivity;
                Move(deltaX);
                startTouchPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isTouchActive = false;
            }
        }

        public override void ResetPosition()
        {
            transform.position = initialPosition;
        }
    }
}