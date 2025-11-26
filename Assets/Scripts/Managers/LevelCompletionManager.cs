using System;
using UnityEngine;
using Zenject;
using MiniIT.EVENTS;
using MiniIT.BLOCK;

namespace MiniIT.LEVELS
{
    public interface ILevelCompletion
    {
        /// <summary>
        /// Raised when all blocks in the current level are destroyed.
        /// </summary>
        event Action OnLevelCompleted;

        /// <summary>
        /// Initializes the manager for a new level by counting destroyable blocks.
        /// </summary>
        /// <param name="levelContainer">Root GameObject that contains all blocks of the level.</param>
        void InitializeForLevel(GameObject levelContainer);
    }

    public class LevelCompletionManager : MonoBehaviour, ILevelCompletion
    {
        public event Action OnLevelCompleted;

        [Inject] private IGameEvents gameEvents = null;

        private int remainingBlocks = 0;

        private void Awake()
        {
            if (gameEvents != null)
            {
                gameEvents.BlockDestroyed += HandleBlockDestroyed;
            }
        }

        private void OnDestroy()
        {
            if (gameEvents != null)
            {
                gameEvents.BlockDestroyed -= HandleBlockDestroyed;
            }
        }

        #region ILevelCompletion

        public void InitializeForLevel(GameObject levelContainer)
        {
            if (levelContainer == null)
            {
                Debug.LogError("LevelCompletionManager: Level container is null!");
                remainingBlocks = 0;
                return;
            }

            Block[] blocks = levelContainer.GetComponentsInChildren<Block>(true);
            remainingBlocks = blocks.Length;

            CheckCompletion();
        }

        #endregion

        /// <summary>
        /// Called via GameEvents when any block is destroyed.
        /// </summary>
        private void HandleBlockDestroyed(int scoreValue)
        {
            remainingBlocks--;

            CheckCompletion();
        }

        /// <summary>
        /// Checks if all blocks are destroyed and fires completion event if needed.
        /// </summary>
        private void CheckCompletion()
        {
            if (remainingBlocks <= 0)
            {
                OnLevelCompleted?.Invoke();
            }
        }
    }
}