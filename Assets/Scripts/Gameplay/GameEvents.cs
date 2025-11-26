using System;
using UnityEngine;

namespace MiniIT.EVENTS
{
    /// <summary>
    /// Central event hub for gameplay-related events.
    /// Allows loose coupling between systems (blocks, score, level completion, etc.).
    /// </summary>
    public interface IGameEvents
    {
        /// <summary>
        /// Raised when any block is destroyed.
        /// Passes the score value that should be added.
        /// </summary>
        event Action<int> BlockDestroyed;

        /// <summary>
        /// Triggers the BlockDestroyed event.
        /// </summary>
        /// <param name="scoreValue">Score points awarded for destroying the block.</param>
        void OnBlockDestroyed(int scoreValue);
    }

    /// <summary>
    /// Global singleton implementation of IGameEvents.
    /// Injected via Zenject as a single instance across the whole game.
    /// </summary>
    public class GameEvents : IGameEvents
    {
        public event Action<int> BlockDestroyed;

        /// <summary>
        /// Fires the BlockDestroyed event for all subscribers.
        /// </summary>
        public void OnBlockDestroyed(int scoreValue)
        {
            BlockDestroyed?.Invoke(scoreValue);
        }
    }
}