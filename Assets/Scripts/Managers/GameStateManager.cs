using System;
using UnityEngine;

namespace MiniIT.GAMESTATE
{
    public interface IGameStateManager
    {
        /// <summary>
        /// Current game state.
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// Raised every time the game state changes. Passes the new state.
        /// </summary>
        event Action<GameState> OnStateChanged;

        /// <summary>
        /// Changes the current game state.
        /// </summary>
        /// <param name="newState">Target state to switch to.</param>
        void SetState(GameState newState);
    }

    /// <summary>
    /// Global game states used throughout the project.
    /// </summary>
    public enum GameState
    {
        Menu = 0,
        Playing = 1,
        Paused = 2,
        GameOver = 3,
    }

    public class GameStateManager : MonoBehaviour, IGameStateManager
    {
        public GameState CurrentState
        {
            get
            {
                return currentState;
            }
        }

        public event Action<GameState> OnStateChanged;

        private GameState currentState = GameState.Menu;

        private void Awake()
        {
            SetState(GameState.Menu);
        }

        #region IGameStateManager

        public void SetState(GameState newState)
        {
            if (currentState == newState)
            {
                return;
            }

            currentState = newState;

            OnStateChanged?.Invoke(currentState);
        }

        #endregion
    }
}