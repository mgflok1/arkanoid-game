using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using MiniIT.GAMESTATE;
using MiniIT.AUDIO;

namespace MiniIT.BALL
{
    public interface IBall
    {
        /// <summary>
        /// Raised when the ball is launched for the first time.
        /// </summary>
        event Action OnLaunched;

        /// <summary>
        /// Raised when the ball falls into the lose zone.
        /// </summary>
        event Action OnLost;

        /// <summary>
        /// Current movement speed of the ball.
        /// </summary>
        float CurrentSpeed { get; }

        /// <summary>
        /// Launches the ball in the specified direction.
        /// </summary>
        /// <param name="initialDirection">Initial direction vector (will be normalized).</param>
        void Launch(Vector2 initialDirection);

        /// <summary>
        /// Returns the ball to its initial state on the platform.
        /// </summary>
        void Reset();
    }

    public class Ball : MonoBehaviour, IBall
    {
        public event Action OnLaunched;
        public event Action OnLost;

        public float CurrentSpeed
        {
            get
            {
                return currentSpeed;
            }
        }

        [Header("Ball Physics")]
        [SerializeField] private float initialSpeed = 8f;
        [SerializeField] private float accelerationPerSecond = 1.5f;
        [SerializeField] private float maxSpeed = 18f;
        [SerializeField] private float minVerticalSpeedFraction = 0.2f;

        [Header("Platform Attachment")]
        [SerializeField] private Transform platformParent = null;
        [SerializeField] private Vector3 localOffsetOnPlatform = Vector3.zero;

        [Header("Lose Zone")]
        [SerializeField] private Collider2D loseZone = null;

        [Header("Components")]
        [SerializeField] private Rigidbody2D rb = null;

        [Inject] private IGameStateManager gameStateManager = null;
        [Inject] private IAudioManager audioManager = null;

        private float currentSpeed = 0f;
        private bool isLaunched = false;
        private float acceleration = 0f;

        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }

            if (rb != null)
            {
                rb.gravityScale = 0f;
            }

            if (platformParent != null)
            {
                transform.SetParent(platformParent);
                transform.localPosition = localOffsetOnPlatform;
            }

            acceleration = accelerationPerSecond;

            if (gameStateManager == null)
            {
                Debug.LogError("Ball: IGameStateManager injection failed!");
            }
        }

        private void Update()
        {
            if (gameStateManager.CurrentState != GameState.Playing)
            {
                return;
            }

            if (isLaunched)
            {
                return;
            }

            // Touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        return;
                    }

                    Launch(Vector2.up);
                }

                return;
            }

            // Mouse input
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                Launch(Vector2.up);
            }
        }

        private void FixedUpdate()
        {
            if (!isLaunched || rb.velocity.magnitude < 0.1f)
            {
                return;
            }

            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, maxSpeed);

            Vector2 normalizedVelocity = rb.velocity.normalized;
            rb.velocity = normalizedVelocity * currentSpeed;

            EnsureMinimumVerticalComponent();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            audioManager?.PlaySound("hit");

            if (collision.transform == platformParent)
            {
                ReflectFromPlatform(collision);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other != loseZone)
            {
                return;
            }

            audioManager?.PlaySound("lose");
            OnLost?.Invoke();
        }

        private void EnsureMinimumVerticalComponent()
        {
            Vector2 velocity = rb.velocity;
            float absVy = Mathf.Abs(velocity.y);

            if (absVy >= minVerticalSpeedFraction * currentSpeed)
            {
                return;
            }

            float newAbsVy = minVerticalSpeedFraction * currentSpeed;
            float newAbsVx = Mathf.Sqrt(currentSpeed * currentSpeed - newAbsVy * newAbsVy);

            velocity.y = velocity.y >= 0f ? newAbsVy : -newAbsVy;
            velocity.x = velocity.x >= 0f ? newAbsVx : -newAbsVx;

            rb.velocity = velocity;
        }

        private void ReflectFromPlatform(Collision2D collision)
        {
            Bounds platformBounds = collision.collider.bounds;
            ContactPoint2D contact = collision.GetContact(0);
            float hitPercent = (contact.point.x - platformBounds.min.x) / platformBounds.size.x;

            float xComponent = (hitPercent - 0.5f) * currentSpeed * 1.8f;
            float yComponent = Mathf.Sqrt(currentSpeed * currentSpeed - xComponent * xComponent);

            Vector2 newVelocity = new Vector2(xComponent, yComponent);
            rb.velocity = newVelocity;
        }

        #region IBall

        public void Launch(Vector2 initialDirection)
        {
            transform.SetParent(null);

            rb.velocity = initialDirection.normalized * initialSpeed;
            currentSpeed = initialSpeed;
            isLaunched = true;

            OnLaunched?.Invoke();
        }

        public void Reset()
        {
            isLaunched = false;
            currentSpeed = 0f;

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            if (platformParent != null)
            {
                transform.SetParent(platformParent);
                transform.localPosition = localOffsetOnPlatform;
            }
        }

        #endregion
    }
}