using UnityEngine;
using UnityEngine.U2D;
using Zenject;
using MiniIT.EVENTS;

namespace MiniIT.BLOCK
{
    /// <summary>
    /// Types of destructible blocks in the game.
    /// </summary>
    public enum BlockType
    {
        Sand = 0,
        Stone = 1,
    }

    /// <summary>
    /// Destructible block that changes appearance on hit and awards score when fully destroyed.
    /// Uses SpriteShapeController for dynamic shape rendering.
    /// </summary>
    public class Block : MonoBehaviour
    {
        [Header("Block Configuration")]
        [SerializeField] private BlockType blockType = BlockType.Sand;

        [Header("Sand Block Shape")]
        [SerializeField] private SpriteShape sandShape = null;

        [Header("Stone Block Shapes")]
        [SerializeField] private SpriteShape intactStoneShape = null;
        [SerializeField] private SpriteShape crackedStoneShape = null;

        [Header("Score Values")]
        [SerializeField] private int sandDestroyScore = 100;
        [SerializeField] private int stoneDestroyScore = 200;

        [Inject] private IGameEvents gameEvents = null;

        private SpriteShape[] hitShapes = null;

        private SpriteShapeController spriteShapeController = null;
        private int currentHitCount = 0;

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes the block: finds SpriteShapeController, configures hit shapes and sets initial state.
        /// </summary>
        private void Initialize()
        {
            spriteShapeController = GetComponent<SpriteShapeController>();

            if (spriteShapeController == null)
            {
                Debug.LogError($"Block: SpriteShapeController not found on {gameObject.name}");
                return;
            }

            ConfigureHitShapes();

            if (hitShapes == null || hitShapes.Length == 0)
            {
                Debug.LogError($"Block: Hit shapes array is empty on {gameObject.name}");
                return;
            }

            currentHitCount = 0;
            spriteShapeController.spriteShape = hitShapes[0];
            spriteShapeController.RefreshSpriteShape();
        }

        /// <summary>
        /// Builds the hitShapes array depending on block type.
        /// </summary>
        private void ConfigureHitShapes()
        {
            if (blockType == BlockType.Sand)
            {
                if (sandShape == null)
                {
                    Debug.LogError($"Block: Sand shape is missing on {gameObject.name}");
                    return;
                }

                hitShapes = new SpriteShape[] { sandShape };
            }
            else
            {
                if (intactStoneShape == null || crackedStoneShape == null)
                {
                    Debug.LogError($"Block: Stone shapes are incomplete on {gameObject.name}");
                    return;
                }

                hitShapes = new SpriteShape[] { intactStoneShape, crackedStoneShape };
            }
        }

        /// <summary>
        /// Returns score value awarded when this block is fully destroyed.
        /// </summary>
        private int GetDestroyScore()
        {
            return blockType == BlockType.Sand ? sandDestroyScore : stoneDestroyScore;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ball"))
            {
                Hit();
            }
        }

        /// <summary>
        /// Processes a single hit from the ball.
        /// Changes visual state or destroys the block if hit limit is reached.
        /// </summary>
        public void Hit()
        {
            currentHitCount++;

            if (currentHitCount >= hitShapes.Length)
            {
                gameEvents?.OnBlockDestroyed(GetDestroyScore());
                gameObject.SetActive(false);
                return;
            }

            // Update visual to next damage state
            spriteShapeController.spriteShape = hitShapes[currentHitCount];
            spriteShapeController.RefreshSpriteShape();
        }

        /// <summary>
        /// Resets block to its initial undamaged state.
        /// Called when restarting or loading a level.
        /// </summary>
        public void ResetBlock()
        {
            gameObject.SetActive(true);
            currentHitCount = 0;

            if (spriteShapeController != null && hitShapes != null && hitShapes.Length > 0)
            {
                spriteShapeController.spriteShape = hitShapes[0];
                spriteShapeController.RefreshSpriteShape();
            }
        }
    }
}