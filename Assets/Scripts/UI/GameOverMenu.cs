using UnityEngine;
using UnityEngine.UI;
using Zenject;
using MiniIT.LEVELS;
using MiniIT.LIVES;
using MiniIT.GAMESTATE;

namespace MiniIT.GAMEOVER
{
    /// <summary>
    /// Controls the Game Over / Win screen UI.
    /// Shown when player runs out of lives or completes a level.
    /// </summary>
    public class GameOverMenu : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button restartButton = null;
        [SerializeField] private Button backToMenuButton = null;

        [Inject] private ILevelSelector levelSelector = null;
        [Inject] private ILivesManager livesManager = null;
        [Inject] private IGameStateManager gameStateManager = null;

        private void Awake()
        {
            SetupButtons();

            if (livesManager != null)
            {
                livesManager.OnGameOver += ShowGameOverMenu;
            }
            else
            {
                Debug.LogError("GameOverMenu: ILivesManager injection failed!");
            }

            if (gameStateManager == null)
            {
                Debug.LogError("GameOverMenu: IGameStateManager injection failed!");
            }
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (livesManager != null)
            {
                livesManager.OnGameOver -= ShowGameOverMenu;
            }
        }

        /// <summary>
        /// Binds button clicks to corresponding actions and clears previous listeners.
        /// </summary>
        private void SetupButtons()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }

            if (backToMenuButton != null)
            {
                backToMenuButton.onClick.RemoveAllListeners();
                backToMenuButton.onClick.AddListener(OnBackToMenuButtonClicked);
            }
        }

        /// <summary>
        /// Displays the Game Over / Win screen.
        /// Pauses the game and changes state to GameOver.
        /// </summary>
        public void ShowGameOverMenu()
        {
            Time.timeScale = 0f;
            gameObject.SetActive(true);

            gameStateManager?.SetState(GameState.GameOver);
        }

        /// <summary>
        /// Handler for the Restart button.
        /// Resumes time and restarts the current level.
        /// </summary>
        private void OnRestartButtonClicked()
        {
            Time.timeScale = 1f;
            levelSelector?.RestartCurrentLevel();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Handler for the Back to Menu button.
        /// Resumes time and returns to level selection screen.
        /// </summary>
        private void OnBackToMenuButtonClicked()
        {
            Time.timeScale = 1f;
            levelSelector?.ShowLevelSelection();
            gameObject.SetActive(false);
        }
    }
}