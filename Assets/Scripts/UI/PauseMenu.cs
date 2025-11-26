using UnityEngine;
using UnityEngine.UI;
using Zenject;
using MiniIT.LEVELS;
using MiniIT.GAMESTATE;

namespace MiniIT.PAUSE
{
    /// <summary>
    /// Controls the in-game pause menu.
    /// Allows player to pause, resume or return to level selection.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button pauseButton = null;
        [SerializeField] private Button resumeButton = null;
        [SerializeField] private Button backToMenuButton = null;

        [Inject] private ILevelSelector levelSelector = null;
        [Inject] private IGameStateManager gameStateManager = null;

        private void Awake()
        {
            SetupButtons();

            if (levelSelector == null)
            {
                Debug.LogError("PauseMenu: ILevelSelector injection failed!");
            }

            if (gameStateManager == null)
            {
                Debug.LogError("PauseMenu: IGameStateManager injection failed!");
            }
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Binds all button clicks and removes previous listeners to prevent duplicates.
        /// </summary>
        private void SetupButtons()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveAllListeners();
                pauseButton.onClick.AddListener(ShowPauseMenu);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(OnResumeButtonClicked);
            }

            if (backToMenuButton != null)
            {
                backToMenuButton.onClick.RemoveAllListeners();
                backToMenuButton.onClick.AddListener(OnBackToMenuButtonClicked);
            }
        }

        /// <summary>
        /// Shows the pause menu and freezes the game.
        /// Called by the main pause button in the gameplay UI.
        /// </summary>
        private void ShowPauseMenu()
        {
            Time.timeScale = 0f;
            gameObject.SetActive(true);

            gameStateManager?.SetState(GameState.Paused);
        }

        /// <summary>
        /// Resumes the game from pause.
        /// </summary>
        private void OnResumeButtonClicked()
        {
            Time.timeScale = 1f;
            gameObject.SetActive(false);

            gameStateManager?.SetState(GameState.Playing);
        }

        /// <summary>
        /// Returns player to the level selection screen.
        /// </summary>
        private void OnBackToMenuButtonClicked()
        {
            Time.timeScale = 1f;
            levelSelector?.ShowLevelSelection();
            gameObject.SetActive(false);
        }
    }
}