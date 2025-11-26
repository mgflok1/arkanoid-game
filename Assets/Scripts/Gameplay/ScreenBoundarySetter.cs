using UnityEngine;

namespace MiniIT.BOUNDARIES
{
    /// <summary>
    /// Configures screen boundary colliders (left, right, top) and game over area
    /// based on the main camera viewport. Works in Awake/Start on scene load.
    /// </summary>
    public class ScreenBoundariesSetter : MonoBehaviour
    {
        [Header("Boundary GameObjects")]
        [SerializeField] private GameObject leftBoundary = null;
        [SerializeField] private GameObject rightBoundary = null;
        [SerializeField] private GameObject topBoundary = null;

        [Header("Game Over Area")]
        [SerializeField] private GameObject gameOverArea = null;

        [Header("Configuration")]
        [SerializeField] private float boundaryOffset = 0.0f;
        [SerializeField] private float sideBoundaryThickness = 0.1f;
        [SerializeField] private float topBoundaryHeightPercentage = 0.1f;

        private Camera mainCamera = null;

        private void Awake()
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("ScreenBoundariesSetter: Main Camera not found in the scene!");
            }
        }

        private void Start()
        {
            if (mainCamera == null)
            {
                return;
            }

            Vector2 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
            Vector2 topRight   = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

            float screenHeight      = topRight.y - bottomLeft.y;
            float topBoundaryHeight = screenHeight * topBoundaryHeightPercentage;
            float topYCenter        = topRight.y - (topBoundaryHeight * 0.5f) + boundaryOffset;

            ConfigureVerticalBoundary(leftBoundary,  bottomLeft.x - boundaryOffset, bottomLeft.y, topRight.y);
            ConfigureVerticalBoundary(rightBoundary, topRight.x + boundaryOffset, bottomLeft.y, topRight.y);
            ConfigureHorizontalBoundary(topBoundary, bottomLeft.x, topRight.x, topYCenter, topBoundaryHeight);

            ConfigureGameOverArea(bottomLeft.x, topRight.x);
        }

        /// <summary>
        /// Configures a vertical side boundary (left or right wall).
        /// </summary>
        private void ConfigureVerticalBoundary(GameObject boundary, float xPosition, float bottomY, float topY)
        {
            if (boundary == null)
            {
                return;
            }

            float height   = topY - bottomY;
            float centerY  = (bottomY + topY) * 0.5f;

            boundary.transform.position = new Vector3(xPosition, centerY, 0f);

            BoxCollider2D collider = boundary.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = boundary.AddComponent<BoxCollider2D>();
            }

            collider.size = new Vector2(sideBoundaryThickness, height);
        }

        /// <summary>
        /// Configures the horizontal top boundary.
        /// </summary>
        private void ConfigureHorizontalBoundary(GameObject boundary, float leftX, float rightX, float yCenter, float height)
        {
            if (boundary == null)
            {
                return;
            }

            float width   = rightX - leftX;
            float centerX = (leftX + rightX) * 0.5f;

            boundary.transform.position = new Vector3(centerX, yCenter, 0f);

            BoxCollider2D collider = boundary.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = boundary.AddComponent<BoxCollider2D>();
            }

            collider.size = new Vector2(width, height);
        }

        /// <summary>
        /// Configures the game over trigger area below the screen.
        /// Keeps its current Y position and height, adjusts only width and X center.
        /// </summary>
        private void ConfigureGameOverArea(float leftX, float rightX)
        {
            if (gameOverArea == null)
            {
                return;
            }

            float width   = rightX - leftX;
            float centerX = (leftX + rightX) * 0.5f;
            float currentY = gameOverArea.transform.position.y;

            gameOverArea.transform.position = new Vector3(centerX, currentY, 0f);

            BoxCollider2D collider = gameOverArea.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameOverArea.AddComponent<BoxCollider2D>();
            }

            float currentHeight = collider.size.y;
            collider.size = new Vector2(width, currentHeight);
        }
    }
}