using System;
using UnityEngine;
using Zenject;
using MiniIT.BALL;

namespace MiniIT.LIVES
{
    public interface ILivesManager
    {
        /// <summary>
        /// Raised when the number of lives changes. Passes the new amount.
        /// </summary>
        event Action<int> OnLivesChanged;

        /// <summary>
        /// Raised once when lives reach zero.
        /// </summary>
        event Action OnGameOver;

        /// <summary>
        /// Current remaining lives.
        /// </summary>
        int CurrentLives { get; }

        /// <summary>
        /// Decreases lives by one. Triggers reset of the ball or GameOver if needed.
        /// </summary>
        void LoseLife();

        /// <summary>
        /// Restores full lives (usually called on level start/restart).
        /// </summary>
        void ResetLives();
    }

    /// <summary>
    /// Manages player lives, reacts to ball loss and notifies UI / game over systems.
    /// </summary>
    public class LivesManager : MonoBehaviour, ILivesManager
    {
        public event Action<int> OnLivesChanged;
        public event Action OnGameOver;

        public int CurrentLives
        {
            get
            {
                return currentLives;
            }
        }

        [Inject] private IBall ball = null;

        private const int StartingLives = 3;

        private int currentLives = StartingLives;
        private bool isGameOver = false;

        private void Awake()
        {
            ResetLives();

            if (ball != null)
            {
                ball.OnLost += LoseLife;
            }
            else
            {
                Debug.LogError("LivesManager: IBall injection failed!");
            }
        }

        private void OnDestroy()
        {
            if (ball != null)
            {
                ball.OnLost -= LoseLife;
            }
        }

        #region ILivesManager

        public void LoseLife()
        {
            if (isGameOver)
            {
                return;
            }

            currentLives--;

            OnLivesChanged?.Invoke(currentLives);

            if (currentLives > 0)
            {
                ball?.Reset();
            }
            else
            {
                isGameOver = true;
                OnGameOver?.Invoke();
            }
        }

        public void ResetLives()
        {
            currentLives = StartingLives;
            isGameOver = false;

            OnLivesChanged?.Invoke(currentLives);
        }

        #endregion
    }
}