using UnityEngine;
using UnityEngine.UI;
using Zenject;
using MiniIT.BALL;
using MiniIT.SCORE;
using MiniIT.LIVES;
using MiniIT.BLOCK;
using MiniIT.GAMEOVER;
using MiniIT.GAMESTATE;
using MiniIT.MOVEMENT;
using MiniIT.AUDIO;

namespace MiniIT.LEVELS
{
    public interface ILevelSelector
    {
        /// <summary>
        /// Restarts the currently loaded level.
        /// </summary>
        void RestartCurrentLevel();

        /// <summary>
        /// Shows the level selection screen and returns the game to menu state.
        /// </summary>
        void ShowLevelSelection();
    }

    /// <summary>
    /// Available level types. Must match the order of containers in the inspector.
    /// </summary>
    public enum LevelType
    {
        Level1 = 0,
        Level2 = 1,
        Level3 = 2,
    }

    /// <summary>
    /// Central level management system.
    /// Handles level selection, activation, reset, and transitions between menu and gameplay.
    /// </summary>
    public class LevelSelector : MonoBehaviour, ILevelSelector
    {
        [Header("Level Containers")]
        [SerializeField] private GameObject[] levelContainers = null;

        [Header("UI Elements")]
        [SerializeField] private GameObject levelSelectionPanel = null;
        [SerializeField] private Button[] levelButtons = null;

        [Inject] private IBall ball = null;
        [Inject] private IAudioManager audioManager = null;
        [Inject] private IScoreManager scoreManager = null;
        [Inject] private ILivesManager livesManager = null;
        [Inject] private ILevelCompletion levelCompletion = null;
        [Inject] private GameOverMenu gameOverMenu = null;
        [Inject] private IGameStateManager gameStateManager = null;
        [Inject] private IPlatformMover platformMover = null;

        private int currentLevelIndex = -1;

        private void Awake()
        {
            SetupLevelButtons();

            if (levelCompletion != null)
            {
                levelCompletion.OnLevelCompleted += HandleLevelCompleted;
            }

            if (gameStateManager == null)
            {
                Debug.LogError("LevelSelector: IGameStateManager injection failed!");
            }

            if (platformMover == null)
            {
                Debug.LogError("LevelSelector: IPlatformMover injection failed!");
            }
        }

        private void OnDestroy()
        {
            if (levelCompletion != null)
            {
                levelCompletion.OnLevelCompleted -= HandleLevelCompleted;
            }
        }

        /// <summary>
        /// Binds each level button to its corresponding level index.
        /// </summary>
        private void SetupLevelButtons()
        {
            if (levelButtons == null || levelButtons.Length == 0)
            {
                return;
            }

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelIndex = i;

                if (levelButtons[i] != null)
                {
                    levelButtons[i].onClick.RemoveAllListeners();
                    levelButtons[i].onClick.AddListener(() => SelectLevel(levelIndex));
                }
            }
        }

        /// <summary>
        /// Loads and starts the level with the given index.
        /// </summary>
        private void SelectLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= levelContainers.Length || levelContainers[levelIndex] == null)
            {
                Debug.LogError($"LevelSelector: Invalid level index {levelIndex} or missing container.");
                return;
            }

            currentLevelIndex = levelIndex;

            DeactivateAllLevels();
            ActivateLevel(levelContainers[levelIndex]);
            ResetGameState();
            HideLevelSelectionPanel();

            gameStateManager?.SetState(GameState.Playing);
        }

        /// <summary>
        /// Disables all level containers in the scene.
        /// </summary>
        private void DeactivateAllLevels()
        {
            if (levelContainers == null)
            {
                return;
            }

            foreach (GameObject container in levelContainers)
            {
                if (container != null)
                {
                    container.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Enables the specified level container and resets all blocks inside.
        /// </summary>
        private void ActivateLevel(GameObject levelContainer)
        {
            levelContainer.SetActive(true);

            Block[] blocks = levelContainer.GetComponentsInChildren<Block>(true);

            foreach (Block block in blocks)
            {
                block.ResetBlock();
            }

            levelCompletion?.InitializeForLevel(levelContainer);
        }

        /// <summary>
        /// Resets all gameplay systems to their initial state.
        /// </summary>
        private void ResetGameState()
        {
            scoreManager?.ResetScore();
            livesManager?.ResetLives();
            platformMover?.ResetPosition();
            ball?.Reset();
        }

        /// <summary>
        /// Hides the level selection UI panel.
        /// </summary>
        private void HideLevelSelectionPanel()
        {
            if (levelSelectionPanel != null)
            {
                levelSelectionPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Called when the current level is completed.
        /// Plays win sound and shows the game over / win screen.
        /// </summary>
        private void HandleLevelCompleted()
        {
            audioManager?.PlaySound("win");
            gameOverMenu?.ShowGameOverMenu();
        }

        #region ILevelSelector

        public void RestartCurrentLevel()
        {
            if (currentLevelIndex >= 0 && currentLevelIndex < levelContainers.Length)
            {
                SelectLevel(currentLevelIndex);
            }
            else
            {
                Debug.LogWarning("LevelSelector: No active level to restart.");
            }
        }

        public void ShowLevelSelection()
        {
            DeactivateAllLevels();
            ResetGameState();

            if (levelSelectionPanel != null)
            {
                levelSelectionPanel.SetActive(true);
            }

            gameStateManager?.SetState(GameState.Menu);
        }

        #endregion
    }
}