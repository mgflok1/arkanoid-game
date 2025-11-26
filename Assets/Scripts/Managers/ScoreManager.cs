using System;
using UnityEngine;
using Zenject;
using MiniIT.EVENTS;

namespace MiniIT.SCORE
{
    public interface IScoreManager
    {
        event Action<int> OnScoreChanged;

        int CurrentScore { get; }

        /// <param name="value">The score value to add.</param>
        void AddScore(int value);

        void ResetScore();
    }

    #region IScoreManager
    public class ScoreManager : MonoBehaviour, IScoreManager
    {
        #region IScoreManager

        public event Action<int> OnScoreChanged;

        public int CurrentScore
        {
            get
            {
                return currentScore;
            }
        }

        #endregion

        [Inject]
        private IGameEvents gameEvents = null;

        private int currentScore = 0;

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

        /// <param name="value">The score value from the destroyed block.</param>
        private void HandleBlockDestroyed(int value)
        {
            AddScore(value);
        }

        /// <param name="value">The score value to add.</param>
        public void AddScore(int value)
        {
            currentScore += value;
            OnScoreChanged?.Invoke(currentScore);
        }

        public void ResetScore()
        {
            currentScore = 0;
            OnScoreChanged?.Invoke(currentScore);
        }
    }
    #endregion
}